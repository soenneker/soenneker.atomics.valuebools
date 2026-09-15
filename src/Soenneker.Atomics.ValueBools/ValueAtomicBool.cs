using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Soenneker.Atomics.ValueBools;

/// <summary>
/// A lightweight, allocation-free atomic boolean struct backed by a single byte.
/// <para/>
/// This type provides atomic read, write, and compare-and-set semantics for boolean
/// values using <see cref="Volatile"/> and <see cref="Interlocked"/> (0 = false, 1 = true).
/// </summary>
/// <remarks>
/// <para>
/// Reads establish acquire semantics and writes establish release semantics, making this
/// type suitable for visibility signaling and safe publication between threads.
/// </para>
/// <para>
/// This is a mutable <see langword="struct"/> intended for use as a <b>private field</b>
/// or inline synchronization primitive. Avoid copying this type, returning it from
/// properties, or using it through interfaces, as doing so will create independent copies
/// of the atomic state.
/// </para>
/// </remarks>
[DebuggerDisplay("{Value}")]
public struct ValueAtomicBool
{
    private const byte _false = 0;
    private const byte _true = 1;

    // Every write stores 0 or 1, so reads can reinterpret the byte as a normalized
    // bool without an additional comparison. This matches a plain bool load.
    private byte _value;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ValueAtomicBool(bool initialValue = false) => _value = initialValue ? _true : _false;

    /// <summary>
    /// Reads the current value of the atomic boolean.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the current value is true; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Read() => Unsafe.BitCast<byte, bool>(Volatile.Read(ref _value));

    /// <summary>
    /// Writes a new value to the atomic boolean.
    /// </summary>
    /// <param name="value">
    /// The value to assign.
    /// </param>
    /// <remarks>
    /// This operation uses an interlocked exchange and provides a full memory fence.
    /// Use <see cref="VolatileWrite"/> when only release ordering is required.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(bool value) => Interlocked.Exchange(ref _value, value ? _true : _false);

    /// <summary>
    /// Writes a value with release semantics, publishing preceding writes to readers
    /// that observe it through <see cref="Read"/> or <see cref="Value"/>.
    /// </summary>
    /// <param name="value">The value to store.</param>
    /// <remarks>
    /// This does not perform a read-modify-write operation or provide the full memory fence of <see cref="Write"/>.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void VolatileWrite(bool value) => Volatile.Write(ref _value, value ? _true : _false);

    /// <summary>
    /// Atomically replaces the current value with <paramref name="value"/> and
    /// returns the previous value.
    /// </summary>
    /// <param name="value">
    /// The value to assign.
    /// </param>
    /// <returns>
    /// The value that was stored prior to the exchange.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Exchange(bool value) => Unsafe.BitCast<byte, bool>(Interlocked.Exchange(ref _value, value ? _true : _false));

    /// <summary>
    /// Atomically sets the value to <paramref name="newValue"/> if the current value
    /// equals <paramref name="expected"/>.
    /// </summary>
    /// <param name="expected">
    /// The value expected to be currently stored.
    /// </param>
    /// <param name="newValue">
    /// The value to assign if the comparison succeeds.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if the value was updated; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CompareAndSet(bool expected, bool newValue) =>
        Interlocked.CompareExchange(ref _value, newValue ? _true : _false, expected ? _true : _false) == (expected ? _true : _false);

    /// <summary>
    /// Gets or sets the current value of the atomic boolean.
    /// </summary>
    /// <remarks>
    /// The getter performs an atomic read with acquire semantics.
    /// The setter uses an interlocked exchange and provides a full memory fence.
    /// </remarks>
    public bool Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Read();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Write(value);
    }

    /// <summary>
    /// Attempts to atomically transition the value from <see langword="false"/> to
    /// <see langword="true"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the value was updated; <see langword="false"/> if it
    /// was already <see langword="true"/>.
    /// </returns>
    /// <remarks>
    /// This method performs a single compare-and-exchange operation and does not
    /// spin or retry.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySetTrue() => Interlocked.CompareExchange(ref _value, _true, _false) == _false;

    /// <summary>
    /// Attempts to atomically transition the value from <see langword="true"/> to
    /// <see langword="false"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the value was updated; <see langword="false"/> if it
    /// was already <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This method performs a single compare-and-exchange operation and does not
    /// spin or retry.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TrySetFalse() => Interlocked.CompareExchange(ref _value, _false, _true) == _true;

    /// <summary>
    /// Returns a string representation of the current value.
    /// </summary>
    public override string ToString() => Read() ? "true" : "false";
}
