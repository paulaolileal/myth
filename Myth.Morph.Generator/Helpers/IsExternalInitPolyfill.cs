// Polyfill required for `record` types when targeting netstandard2.0.
// The `init` accessor and positional records depend on this type, which was
// introduced in .NET 5. Defining it here allows the compiler to use records
// in a netstandard2.0 assembly without any runtime dependency change.
namespace System.Runtime.CompilerServices;

internal static class IsExternalInit { }
