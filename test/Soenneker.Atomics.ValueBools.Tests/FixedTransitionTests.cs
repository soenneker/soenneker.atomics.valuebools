using System;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Atomics.ValueBools.Tests;

public sealed class FixedTransitionTests
{
    [Test]
    public async ValueTask TrySetTrue_has_one_winner_under_contention(CancellationToken cancellationToken)
    {
        await AssertSingleWinner(true, cancellationToken);
    }

    [Test]
    public async ValueTask TrySetFalse_has_one_winner_under_contention(CancellationToken cancellationToken)
    {
        await AssertSingleWinner(false, cancellationToken);
    }

    private static async Task AssertSingleWinner(bool target, CancellationToken cancellationToken)
    {
        const int workerCount = 8;
        const int rounds = 64;
        var flags = new ValueAtomicBool[rounds];
        var winners = new int[rounds];
        for (int round = 0; round < rounds; round++)
            flags[round] = new ValueAtomicBool(!target);

        using var barrier = new Barrier(workerCount);
        var workers = new Task[workerCount];
        for (int worker = 0; worker < workerCount; worker++)
        {
            workers[worker] = Task.Factory.StartNew(() =>
            {
                for (int round = 0; round < rounds; round++)
                {
                    barrier.SignalAndWait(cancellationToken);
                    bool changed = target ? flags[round].TrySetTrue() : flags[round].TrySetFalse();
                    if (changed)
                        Interlocked.Increment(ref winners[round]);
                }
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        await Task.WhenAll(workers).WaitAsync(TimeSpan.FromSeconds(15), cancellationToken);
        for (int round = 0; round < rounds; round++)
        {
            await Assert.That(winners[round]).IsEqualTo(1);
            await Assert.That(flags[round].Read()).IsEqualTo(target);
            bool changed = target ? flags[round].TrySetTrue() : flags[round].TrySetFalse();
            await Assert.That(changed).IsFalse();
            await Assert.That(flags[round].Read()).IsEqualTo(target);
        }
    }
}
