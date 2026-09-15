using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Soenneker.Atomics.ValueBools.Tests;

public sealed class ByteStorageTests
{
    [Test]
    public async Task Storage_is_one_byte_and_default_is_false()
    {
        await Assert.That(Unsafe.SizeOf<ValueAtomicBool>()).IsEqualTo(1);
        await Assert.That(Unsafe.SizeOf<AdjacentFields>()).IsEqualTo(3);
        await Assert.That(default(ValueAtomicBool).Read()).IsFalse();
        await Assert.That(new ValueAtomicBool().Value).IsFalse();
        await Assert.That(new ValueAtomicBool(true).Read()).IsTrue();
    }

    [Test]
    public async Task Writes_exchanges_and_conditional_transitions_preserve_results()
    {
        ValueAtomicBool flag = default;
        await Assert.That(flag.Exchange(true)).IsFalse();
        await Assert.That(flag.Exchange(false)).IsTrue();
        await Assert.That(flag.CompareAndSet(true, false)).IsFalse();
        await Assert.That(flag.CompareAndSet(false, true)).IsTrue();
        await Assert.That(flag.TrySetTrue()).IsFalse();
        await Assert.That(flag.TrySetFalse()).IsTrue();
        await Assert.That(flag.TrySetFalse()).IsFalse();
        await Assert.That(flag.TrySetTrue()).IsTrue();
        flag.Write(false);
        await Assert.That(flag.Value).IsFalse();
        flag.Value = true;
        await Assert.That(flag.Read()).IsTrue();
        flag.VolatileWrite(false);
        await Assert.That(flag.ToString()).IsEqualTo("false");
        flag.VolatileWrite(true);
        await Assert.That(flag.ToString()).IsEqualTo("true");
    }

    [Test]
    public async Task Byte_atomics_do_not_modify_adjacent_storage()
    {
        var values = new AdjacentFields[32];
        for (int i = 0; i < values.Length; i++)
        {
            values[i].Before = 0xA5;
            values[i].After = 0x5A;
        }
        int violations = 0;
        Parallel.For(0, values.Length, i =>
        {
            for (int iteration = 0; iteration < 10000; iteration++)
            {
                if (!values[i].Flag.TrySetTrue() || !values[i].Flag.Exchange(false))
                    Interlocked.Increment(ref violations);
            }
        });
        await Assert.That(violations).IsEqualTo(0);
        for (int i = 0; i < values.Length; i++)
        {
            await Assert.That(values[i].Before).IsEqualTo((byte)0xA5);
            await Assert.That(values[i].After).IsEqualTo((byte)0x5A);
            await Assert.That(values[i].Flag.Read()).IsFalse();
        }
    }

    [Test]
    public async Task Compare_and_set_has_one_owner_under_contention()
    {
        var holder = new Holder();
        int inside = 0, violations = 0;
        await Task.Run(() => Parallel.For(0, 8, _ =>
        {
            for (int iteration = 0; iteration < 5000; iteration++)
            {
                var spin = new SpinWait();
                while (!holder.Flag.TrySetTrue())
                    spin.SpinOnce();
                if (Interlocked.Increment(ref inside) != 1)
                    Interlocked.Increment(ref violations);
                holder.Value++;
                Interlocked.Decrement(ref inside);
                holder.Flag.VolatileWrite(false);
            }
        })).WaitAsync(TimeSpan.FromSeconds(15));
        await Assert.That(holder.Value).IsEqualTo(40000);
        await Assert.That(violations).IsEqualTo(0);
    }

    [Test]
    public async Task Volatile_write_publishes_payload_to_acquire_readers()
    {
        var holder = new Holder();
        int violations = 0;
        Task writer = Task.Factory.StartNew(() =>
        {
            for (int i = 1; i <= 10000; i++)
            {
                var spin = new SpinWait();
                while (holder.Flag.Read())
                    spin.SpinOnce();
                holder.Value = i;
                holder.Complement = ~i;
                holder.Flag.VolatileWrite(true);
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        Task reader = Task.Factory.StartNew(() =>
        {
            for (int i = 1; i <= 10000; i++)
            {
                var spin = new SpinWait();
                while (!holder.Flag.Read())
                    spin.SpinOnce();
                if (holder.Value != i || holder.Complement != ~i)
                    violations++;
                holder.Flag.VolatileWrite(false);
            }
        }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        await Task.WhenAll(writer, reader).WaitAsync(TimeSpan.FromSeconds(15));
        await Assert.That(violations).IsEqualTo(0);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct AdjacentFields
    {
        public byte Before;
        public ValueAtomicBool Flag;
        public byte After;
    }

    private sealed class Holder
    {
        public ValueAtomicBool Flag;
        public int Value;
        public int Complement;
    }
}
