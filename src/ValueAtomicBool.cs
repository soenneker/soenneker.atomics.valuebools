using System.Runtime.CompilerServices;
using Soenneker.Atomics.ValueInts;

namespace Soenneker.Atomics.ValueBools;

/// <summary>
/// A lightweight, allocation-free atomic boolean implemented on top of an inline
/// <see cref="ValueAtomicInt"/>.
/// <para/>
/// This type provides atomic read, write, and compare-and-set semantics for boolean
/// values using a single integer backing field (0 = false, 1 = true).
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
public struct AtomicBool
{
    private const int _false = 0;
    private const int _true = 1;

    private ValueAtomicInt _value;

    /// <summary>
    /// Initializes a new <see cref="AtomicBool"/> with the specified initial value.
    /// </summary>
    /// <param name="initialValue">
    /// The initial boolean value.
    /// </param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public AtomicBool(bool initialValue = false) => _value = new ValueAtomicInt(initialValue ? _true : _false);

    /// <summary>
    /// Reads the current value of the atomic boolean.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> if the current value is true; otherwise <see langword="false"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Read() => _value.Read() != _false;

    /// <summary>
    /// Writes a new value to the atomic boolean.
    /// </summary>
    /// <param name="value">
    /// The value to assign.
    /// </param>
    /// <remarks>
    /// This operation performs an atomic write with release semantics.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(bool value) => _value.Write(value ? _true : _false);

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
    public bool Exchange(bool value) => _value.Exchange(value ? _true : _false) == _true;

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
        _value.CompareExchange(newValue ? _true : _false, expected ? _true : _false) == (expected ? _true : _false);

    /// <summary>
    /// Gets or sets the current value of the atomic boolean.
    /// </summary>
    /// <remarks>
    /// The getter performs an atomic read with acquire semantics.
    /// The setter performs an atomic write with release semantics.
    /// </remarks>
    public bool Value
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => _value.Read() != _false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => _value.Write(value ? _true : _false);
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
    public bool TrySetTrue() => _value.TrySet(_true, _false);

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
    public bool TrySetFalse() => _value.TrySet(_false, _true);

    /// <summary>
    /// Returns a string representation of the current value.
    /// </summary>
    public override string ToString() => Read() ? "true" : "false";
}