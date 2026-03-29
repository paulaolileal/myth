using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Myth.Morph.Generator.Emitters;
using Myth.Morph.Generator.Helpers;
using Myth.Morph.Generator.Models;

namespace Myth.Morph.Generator;

/// <summary>
/// Roslyn incremental source generator for <c>Myth.Morph</c>.
/// Scans all class and record declarations that implement <c>IMorphableTo&lt;TDest&gt;</c>
/// or <c>IMorphableFrom&lt;TSrc&gt;</c> and generates:
/// <list type="bullet">
///   <item>One <c>IMorphAutoMapper</c> class per source → destination pair.</item>
///   <item>One <c>MorphBootstrap</c> + <c>[ModuleInitializer]</c> that wires everything
///         into the Myth.Morph runtime at assembly-load time.</item>
/// </list>
/// When this generator is referenced by a project, assembly scanning is skipped entirely
/// inside <c>AddMorph()</c> and all auto-mapping uses direct property assignments instead of reflection.
/// </summary>
[Generator]
public sealed class MorphIncrementalGenerator : IIncrementalGenerator {

    /// <inheritdoc/>
    public void Initialize( IncrementalGeneratorInitializationContext context ) {
        // ── Step 1: Cheap syntax filter ──────────────────────────────────────
        // Only consider classes and records that declare a base type list.
        // Structs are excluded — Morph targets reference types only.
        var morphableCandidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: IsTypeCandidateSyntax,
                transform: TransformToModels )
            .Where( static models => !models.IsDefaultOrEmpty )
            .SelectMany( static ( models, _ ) => models );

        // ── Step 2: Collect all models across the compilation ────────────────
        var allModels = morphableCandidates.Collect( );

        // ── Step 3: Generate source ──────────────────────────────────────────
        context.RegisterSourceOutput( allModels, GenerateSource );
    }

    // ─── Pipeline stages ─────────────────────────────────────────────────────────

    /// <summary>
    /// Syntax predicate — cheap check that runs on every syntax node in the compilation.
    /// Returns <c>true</c> only for class or record declarations that have a base list,
    /// avoiding semantic analysis for the vast majority of types.
    /// </summary>
    private static bool IsTypeCandidateSyntax( SyntaxNode node, CancellationToken _ ) =>
        node is ClassDeclarationSyntax { BaseList: not null } or
        RecordDeclarationSyntax { BaseList: not null };

    /// <summary>
    /// Semantic transform — resolves the type symbol and extracts all <see cref="MorphableTypeModel"/>
    /// instances for each <c>IMorphableTo</c> / <c>IMorphableFrom</c> interface the type implements.
    /// Returns an empty array when the type does not implement either interface.
    /// </summary>
    private static ImmutableArray<MorphableTypeModel> TransformToModels(
        GeneratorSyntaxContext context,
        CancellationToken cancellationToken ) {

        cancellationToken.ThrowIfCancellationRequested( );

        if ( context.SemanticModel.GetDeclaredSymbol( context.Node, cancellationToken )
             is not INamedTypeSymbol typeSymbol )
            return ImmutableArray<MorphableTypeModel>.Empty;

        return TypeSymbolHelper.GetMorphablePairs( typeSymbol, context.SemanticModel.Compilation );
    }

    // ─── Code generation ─────────────────────────────────────────────────────────

    /// <summary>
    /// Generates the auto-mapper source files and the single bootstrap file.
    /// Duplicate pairs (same source + destination) are deduplicated before generation —
    /// this can happen when both <c>IMorphableTo</c> and <c>IMorphableFrom</c> describe the same pair.
    /// </summary>
    private static void GenerateSource(
        SourceProductionContext context,
        ImmutableArray<MorphableTypeModel> allModels ) {

        if ( allModels.IsDefaultOrEmpty ) return;

        var uniqueModels = DeduplicateModels( allModels );

        // Generate one file per mapping pair
        foreach ( var model in uniqueModels ) {
            context.CancellationToken.ThrowIfCancellationRequested( );

            var source = AutoMapperEmitter.Emit( model );
            context.AddSource( $"{model.AutoMapperClassName}.g.cs", source );
        }

        // Generate a single bootstrap file covering all pairs
        if ( uniqueModels.Length > 0 ) {
            var bootstrapSource = BootstrapEmitter.Emit( uniqueModels );
            context.AddSource( "MorphBootstrap.g.cs", bootstrapSource );
        }
    }

    /// <summary>
    /// Deduplicates models by (SourceTypeFullName, DestinationTypeFullName) key.
    /// The first occurrence wins — both directions of the same pair produce identical property lists,
    /// so the choice is arbitrary.
    /// </summary>
    private static ImmutableArray<MorphableTypeModel> DeduplicateModels(
        ImmutableArray<MorphableTypeModel> models ) {

        var seen = new HashSet<(string, string)>( );
        var builder = ImmutableArray.CreateBuilder<MorphableTypeModel>( models.Length );

        foreach ( var model in models ) {
            var key = (model.SourceTypeFullName, model.DestinationTypeFullName);
            if ( seen.Add( key ) )
                builder.Add( model );
        }

        return builder.ToImmutable( );
    }
}
