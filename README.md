[![](https://img.shields.io/nuget/v/soenneker.atomics.valuebools.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.atomics.valuebools/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.atomics.valuebools/publish-package.yml?style=for-the-badge)](https://github.com/soenneker/soenneker.atomics.valuebools/actions/workflows/publish-package.yml)
[![](https://img.shields.io/nuget/dt/soenneker.atomics.valuebools.svg?style=for-the-badge)](https://www.nuget.org/packages/soenneker.atomics.valuebools/)
[![](https://img.shields.io/github/actions/workflow/status/soenneker/soenneker.atomics.valuebools/codeql.yml?label=CodeQL&style=for-the-badge)](https://github.com/soenneker/soenneker.atomics.valuebools/actions/workflows/codeql.yml)

# Soenneker.Atomics.ValueBools

A lightweight, allocation-free atomic boolean struct implemented on top of an inline `ValueAtomicInt`. This type provides atomic read, write, and compare-and-set semantics for boolean values using a single integer backing field (0 = false, 1 = true).

## Install

```bash
dotnet add package Soenneker.Atomics.ValueBools
```

## What you get

- `ValueAtomicBool` — A lightweight, allocation-free atomic boolean struct implemented on top of an inline `ValueAtomicInt`. This type provides atomic read, write, and compare-and-set semantics for boolean values using a single integer backing field (0 = false, 1 = true).

## API at a glance

| API | What it does | Result / important behavior |
| --- | --- | --- |
| `ValueAtomicBool.Read()` | Reads the current value of the atomic boolean. | `true` if the current value is true; otherwise `false`. |
| `ValueAtomicBool.Write(value)` | Writes a new value to the atomic boolean. | This operation performs an atomic write with release semantics. |
| `ValueAtomicBool.Exchange(value)` | Atomically replaces the current value with `value` and returns the previous value. | The value that was stored prior to the exchange. |
| `ValueAtomicBool.CompareAndSet(expected, newValue)` | Atomically sets the value to `newValue` if the current value equals `expected`. | `true` if the value was updated; otherwise `false`. |
| `ValueAtomicBool.Value` | Gets or sets the current value of the atomic boolean. | The getter performs an atomic read with acquire semantics. The setter performs an atomic write with release semantics. |
| `ValueAtomicBool.TrySetTrue()` | Attempts to atomically transition the value from `false` to `true`. | `true` if the value was updated; `false` if it was already `true`. |
| `ValueAtomicBool.TrySetFalse()` | Attempts to atomically transition the value from `true` to `false`. | `true` if the value was updated; `false` if it was already `false`. |
| `ValueAtomicBool.ToString()` | Returns a string representation of the current value. | Returns `string`. |

## Important behavior

- `ValueAtomicBool`: Reads establish acquire semantics and writes establish release semantics, making this type suitable for visibility signaling and safe publication between threads. This is a mutable `struct` intended for use as a private field or inline synchronization primitive. Avoid copying this type, returning it from properties, or using it through interfaces, as doing so will create independent copies of the atomic state.
- `ValueAtomicBool.Write(value)`: This operation performs an atomic write with release semantics.
- `ValueAtomicBool.Value`: The getter performs an atomic read with acquire semantics. The setter performs an atomic write with release semantics.
- `ValueAtomicBool.TrySetTrue()`: This method performs a single compare-and-exchange operation and does not spin or retry.
- `ValueAtomicBool.TrySetFalse()`: This method performs a single compare-and-exchange operation and does not spin or retry.
