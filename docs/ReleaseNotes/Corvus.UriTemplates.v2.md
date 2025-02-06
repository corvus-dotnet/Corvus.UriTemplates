# Release notes for Corvus.UriTemplates v2.

## v2.3
* `ParameterValue.GetValue` now offers an overload that works with `ReadOnlyMemory<char>`. (The existing method uses `ReadOnlySpan<char>`)

## v2.2
* Update `Corvus.HighPerformance` reference from 0.2.0 to 1.0.0
* Update `Microsoft.Extensions.ObjectPool` reference from 8.0.0 to 8.0.8

## v2.1
* The `NullableAttribute` we add for targets that don't have that built in had been accidentally made `public`. This release makes it private.

## v2.0

The main changes are:

* .NET Standard 2.0 support
* `ValueStringBuilder` replaces `IBufferWriter<char>`
* Low-allocation range-based parameter reporting in `ParseUri`

