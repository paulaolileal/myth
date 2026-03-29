using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Myth.Morph.Generator.Models;

namespace Myth.Morph.Generator.Helpers;

/// <summary>
/// Provides pure Roslyn semantic analysis helpers for discovering and building
/// <see cref="MorphableTypeModel"/> instances from type symbols.
/// </summary>
internal static class TypeSymbolHelper {

    private const string IMorphableToMetadataName = "Myth.Interfaces.IMorphableTo`1";
    private const string IMorphableFromMetadataName = "Myth.Interfaces.IMorphableFrom`1";

    /// <summary>
    /// Returns all <see cref="MorphableTypeModel"/> pairs produced by the interfaces
    /// <c>IMorphableTo&lt;TDest&gt;</c> and <c>IMorphableFrom&lt;TSrc&gt;</c> implemented by
    /// <paramref name="typeSymbol"/>.
    /// Returns an empty array when neither interface is found in the compilation or the
    /// type does not implement them.
    /// </summary>
    public static ImmutableArray<MorphableTypeModel> GetMorphablePairs(
        INamedTypeSymbol typeSymbol,
        Compilation compilation ) {

        var morphableToIface = compilation.GetTypeByMetadataName( IMorphableToMetadataName );
        var morphableFromIface = compilation.GetTypeByMetadataName( IMorphableFromMetadataName );

        // Neither interface found in the compilation — generator is not applicable.
        if ( morphableToIface is null && morphableFromIface is null )
            return ImmutableArray<MorphableTypeModel>.Empty;

        var builder = ImmutableArray.CreateBuilder<MorphableTypeModel>( );

        foreach ( var iface in typeSymbol.AllInterfaces ) {
            if ( !iface.IsGenericType ) continue;

            var genericDefinition = iface.OriginalDefinition;

            if ( morphableToIface is not null &&
                 SymbolEqualityComparer.Default.Equals( genericDefinition, morphableToIface ) ) {
                // typeSymbol : IMorphableTo<TDest>  →  source = typeSymbol, dest = TDest
                var destType = iface.TypeArguments[ 0 ] as INamedTypeSymbol;
                var model = TryBuildModel( typeSymbol, destType, compilation );
                if ( model is not null ) builder.Add( model );
            }
            else if ( morphableFromIface is not null &&
                      SymbolEqualityComparer.Default.Equals( genericDefinition, morphableFromIface ) ) {
                // typeSymbol : IMorphableFrom<TSrc>  →  source = TSrc, dest = typeSymbol
                var srcType = iface.TypeArguments[ 0 ] as INamedTypeSymbol;
                var model = TryBuildModel( srcType, typeSymbol, compilation );
                if ( model is not null ) builder.Add( model );
            }
        }

        return builder.ToImmutable( );
    }

    // ─── Private helpers ─────────────────────────────────────────────────────────

    private static MorphableTypeModel? TryBuildModel(
        INamedTypeSymbol? sourceType,
        INamedTypeSymbol? destinationType,
        Compilation compilation ) {

        if ( sourceType is null || destinationType is null )
            return null;

        // Skip generic types — generated code cannot know the concrete type arguments at build time.
        if ( sourceType.IsGenericType || destinationType.IsGenericType )
            return null;

        // Skip abstract sources and interface destinations (can't instantiate / assign).
        if ( sourceType.IsAbstract || destinationType.IsAbstract )
            return null;

        // Skip file-local types (C# 11 `file` modifier) — they cannot be referenced
        // from generated code even within the same assembly.
        if ( sourceType.IsFileLocal || destinationType.IsFileLocal )
            return null;

        // Only generate for types that are at least internal — private/nested-private types
        // cannot be referenced from generated code.
        if ( !IsAtLeastInternal( sourceType ) || !IsAtLeastInternal( destinationType ) )
            return null;

        var properties = GetMappableProperties( sourceType, destinationType, compilation );

        return new MorphableTypeModel(
            SourceTypeFullName: sourceType.ToDisplayString( SymbolDisplayFormat.FullyQualifiedFormat ),
            SourceTypeSafeName: BuildSafeIdentifier( sourceType ),
            DestinationTypeFullName: destinationType.ToDisplayString( SymbolDisplayFormat.FullyQualifiedFormat ),
            DestinationTypeSafeName: BuildSafeIdentifier( destinationType ),
            Properties: new EquatableArray<PropertyMappingModel>( properties )
        );
    }

    private static ImmutableArray<PropertyMappingModel> GetMappableProperties(
        INamedTypeSymbol sourceType,
        INamedTypeSymbol destinationType,
        Compilation compilation ) {

        var sourceProps = GetPublicReadableProperties( sourceType );
        var destPropsByName = GetPublicWritableProperties( destinationType )
            .ToDictionary( p => p.Name, StringComparer.Ordinal );

        var builder = ImmutableArray.CreateBuilder<PropertyMappingModel>( );

        foreach ( var srcProp in sourceProps ) {
            if ( !destPropsByName.TryGetValue( srcProp.Name, out var destProp ) )
                continue;

            var (isCompatible, requiresNullCheck) =
                CheckTypeCompatibility( srcProp.Type, destProp.Type, compilation );

            if ( !isCompatible ) continue;

            builder.Add( new PropertyMappingModel(
                Name: srcProp.Name,
                SourcePropertyType: srcProp.Type.ToDisplayString( SymbolDisplayFormat.FullyQualifiedFormat ),
                DestinationPropertyType: destProp.Type.ToDisplayString( SymbolDisplayFormat.FullyQualifiedFormat ),
                RequiresNullCheck: requiresNullCheck
            ) );
        }

        return builder.ToImmutable( );
    }

    /// <summary>
    /// Returns public, non-indexer, readable properties including those declared in base types.
    /// </summary>
    private static IEnumerable<IPropertySymbol> GetPublicReadableProperties( INamedTypeSymbol type ) {
        var seen = new HashSet<string>( StringComparer.Ordinal );
        var current = type;

        while ( current is not null && current.SpecialType != SpecialType.System_Object ) {
            foreach ( var member in current.GetMembers( ).OfType<IPropertySymbol>( ) ) {
                if ( member.DeclaredAccessibility != Accessibility.Public ) continue;
                if ( member.IsIndexer ) continue;
                if ( member.IsWriteOnly ) continue;
                if ( member.GetMethod is null ) continue;
                if ( !seen.Add( member.Name ) ) continue; // override already added by derived type
                yield return member;
            }

            current = current.BaseType;
        }
    }

    /// <summary>
    /// Returns public, non-indexer, non-init-only writable properties including those in base types.
    /// <c>init</c>-only setters are excluded because they cannot be called after object construction.
    /// </summary>
    private static IEnumerable<IPropertySymbol> GetPublicWritableProperties( INamedTypeSymbol type ) {
        var seen = new HashSet<string>( StringComparer.Ordinal );
        var current = type;

        while ( current is not null && current.SpecialType != SpecialType.System_Object ) {
            foreach ( var member in current.GetMembers( ).OfType<IPropertySymbol>( ) ) {
                if ( member.DeclaredAccessibility != Accessibility.Public ) continue;
                if ( member.IsIndexer ) continue;
                if ( member.IsReadOnly ) continue;
                if ( member.SetMethod is null ) continue;
                if ( member.SetMethod.DeclaredAccessibility != Accessibility.Public ) continue;
                if ( member.SetMethod.IsInitOnly ) continue; // init-only: not settable after ctor
                if ( !seen.Add( member.Name ) ) continue;
                yield return member;
            }

            current = current.BaseType;
        }
    }

    /// <summary>
    /// Checks whether <paramref name="sourceType"/> can be assigned to <paramref name="destinationType"/>,
    /// and whether a null-guard should be emitted in the generated code.
    /// </summary>
    /// <returns>
    /// (<c>isCompatible</c>, <c>requiresNullCheck</c>).
    /// Complex types that are not directly assignable (would require nested Morph call) return <c>false</c>.
    /// </returns>
    private static (bool IsCompatible, bool RequiresNullCheck) CheckTypeCompatibility(
        ITypeSymbol sourceType,
        ITypeSymbol destinationType,
        Compilation compilation ) {

        // ── Nullable value type handling ──────────────────────────────────────
        var srcUnderlyingNullable = GetNullableUnderlyingType( sourceType );
        var destUnderlyingNullable = GetNullableUnderlyingType( destinationType );

        // T? → T  (nullable value type to its non-nullable counterpart)
        if ( srcUnderlyingNullable is not null && destUnderlyingNullable is null &&
             SymbolEqualityComparer.Default.Equals( srcUnderlyingNullable, destinationType ) )
            return (IsCompatible: true, RequiresNullCheck: true);

        // T → T?  (non-nullable value type to nullable wrapper — always safe)
        if ( srcUnderlyingNullable is null && destUnderlyingNullable is not null &&
             SymbolEqualityComparer.Default.Equals( sourceType, destUnderlyingNullable ) )
            return (IsCompatible: true, RequiresNullCheck: false);

        // ── Direct assignment check via CSharpCompilation ────────────────────
        // ClassifyConversion is only available on CSharpCompilation.
        if ( compilation is not CSharpCompilation csharpCompilation )
            return (IsCompatible: false, RequiresNullCheck: false);

        var conversion = csharpCompilation.ClassifyConversion( sourceType, destinationType );

        if ( !conversion.IsImplicit )
            return (IsCompatible: false, RequiresNullCheck: false);

        // Emit a null-guard when source is a nullable-annotated reference type
        // and the destination is non-nullable, to avoid compiler warnings in the generated code.
        var requiresNull = sourceType.IsReferenceType &&
                           sourceType.NullableAnnotation == NullableAnnotation.Annotated &&
                           destinationType.NullableAnnotation == NullableAnnotation.NotAnnotated;

        return (IsCompatible: true, RequiresNullCheck: requiresNull);
    }

    /// <summary>
    /// Returns the underlying type when <paramref name="type"/> is <c>Nullable&lt;T&gt;</c>,
    /// or <c>null</c> if it is not a nullable value type.
    /// </summary>
    private static ITypeSymbol? GetNullableUnderlyingType( ITypeSymbol type ) {
        if ( type is INamedTypeSymbol { ConstructedFrom.SpecialType: SpecialType.System_Nullable_T } named )
            return named.TypeArguments[ 0 ];
        return null;
    }

    /// <summary>
    /// Returns <c>true</c> when the type is accessible from generated code in the same assembly,
    /// i.e. its effective accessibility is <c>internal</c> or <c>public</c>.
    /// </summary>
    private static bool IsAtLeastInternal( INamedTypeSymbol type ) {
        var accessibility = type.DeclaredAccessibility;
        return accessibility is Accessibility.Public or
                                Accessibility.Internal or
                                Accessibility.ProtectedOrInternal;
    }

    /// <summary>
    /// Builds a C# identifier-safe string from a type's namespace-qualified name.
    /// Dots are replaced with underscores (e.g. <c>MyApp.Entities.User</c> → <c>MyApp_Entities_User</c>).
    /// </summary>
    private static string BuildSafeIdentifier( INamedTypeSymbol type ) {
        var fullName = type.ToDisplayString( SymbolDisplayFormat.CSharpErrorMessageFormat );
        return fullName.Replace( '.', '_' ).Replace( '<', '_' ).Replace( '>', '_' ).Replace( ',', '_' )
                       .Replace( ' ', '_' ).Trim( '_' );
    }
}
