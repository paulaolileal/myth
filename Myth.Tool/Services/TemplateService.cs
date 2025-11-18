using Scriban;
using Scriban.Runtime;
using Myth.Tool.Models;

namespace Myth.Tool.Services;

/// <summary>
/// Service for template processing
/// </summary>
public class TemplateService {
    private readonly Dictionary<string, string> _templates;

    /// <summary>
    /// Initializes a new instance of the <see cref="TemplateService"/> class
    /// </summary>
    public TemplateService() {
        _templates = LoadTemplates();
    }

    /// <summary>
    /// Renders a template with context
    /// </summary>
    /// <param name="templateName">Template name</param>
    /// <param name="context">Generation context</param>
    /// <returns>Rendered content</returns>
    public string RenderTemplate( string templateName, GenerationContext context ) {
        if( !_templates.TryGetValue( templateName, out var templateContent ) ) {
            throw new InvalidOperationException( $"Template '{templateName}' not found" );
        }

        var template = Template.Parse( templateContent );
        var scriptObject = new ScriptObject();

        // Add context data
        scriptObject.Add( "aggregate", context.Aggregate );
        scriptObject.Add( "name", context.Name );
        scriptObject.Add( "namespace", context.Namespace );
        scriptObject.Add( "properties", context.Properties );
        scriptObject.Add( "return_type", context.ReturnType );
        scriptObject.Add( "has_validation", context.HasValidation );
        scriptObject.Add( "publishes_events", context.PublishesEvents );
        scriptObject.Add( "events", context.Events );

        // Add helper functions
        var helperObject = new ScriptObject();
        helperObject.Add( "pascal_case", new Func<string, string>( ToPascalCase ) );
        helperObject.Add( "camel_case", new Func<string, string>( ToCamelCase ) );
        helperObject.Add( "lower", new Func<string, string>( s => s.ToLowerInvariant() ) );
        helperObject.Add( "replace", new Func<string, string, string, string>( ( s, old, @new ) => s.Replace( old, @new ) ) );

        scriptObject.Add( "string", helperObject );

        var templateContext = new TemplateContext();
        templateContext.PushGlobal( scriptObject );

        return template.Render( templateContext );
    }

    /// <summary>
    /// Gets available templates
    /// </summary>
    /// <returns>List of template names</returns>
    public List<string> GetAvailableTemplates() => _templates.Keys.ToList();

    private static Dictionary<string, string> LoadTemplates() {
        return new Dictionary<string, string>
        {
            ["command"] = GetCommandTemplate(),
            ["command-handler"] = GetCommandHandlerTemplate(),
            ["query"] = GetQueryTemplate(),
            ["query-handler"] = GetQueryHandlerTemplate(),
            ["event"] = GetEventTemplate(),
            ["event-handler"] = GetEventHandlerTemplate(),
            ["dto"] = GetDtoTemplate(),
            ["entity"] = GetEntityTemplate(),
            ["repository"] = GetRepositoryTemplate(),
            ["repository-interface"] = GetRepositoryInterfaceTemplate(),
            ["controller"] = GetControllerTemplate()
        };
    }

    private static string GetCommandTemplate() {
        return """
            using Myth.Flow.Actions;
            {{~ if has_validation ~}}
            using Myth.Guard;
            {{~ end ~}}

            namespace {{ namespace }};

            /// <summary>
            /// Command for {{ name }}
            /// </summary>
            public record {{ name }} : ICommand{{~ if return_type ~}}<{{ return_type }}>{{~ end ~}}{{~ if has_validation ~}}, IValidatable<{{ name }}>{{~ end ~}} {
            {{~ for prop in properties ~}}
                /// <summary>
                /// Gets or sets {{ prop.name }}
                /// </summary>
                public{{ if prop.is_required }} required{{ end }} {{ prop.type }} {{ prop.name }} { get; init; }

            {{~ end ~}}
            {{~ if has_validation ~}}

                /// <summary>
                /// Validates the command
                /// </summary>
                /// <param name="builder">Validation builder</param>
                /// <param name="context">Validation context</param>
                public void Validate( ValidationBuilder<{{ name }}> builder, ValidationContextKey? context = null ) {
            {{~ for prop in properties ~}}
            {{~ if prop.type == "string" ~}}
                    builder.For( {{ prop.name }}, x => x.NotEmpty().MaximumLength( 100 ) );
            {{~ else if prop.type == "Guid" ~}}
                    builder.For( {{ prop.name }}, x => x.NotEmpty() );
            {{~ else if prop.type == "int" || prop.type == "decimal" || prop.type == "double" ~}}
                    builder.For( {{ prop.name }}, x => x.GreaterThan( 0 ) );
            {{~ else if prop.type == "DateTime" || prop.type == "DateOnly" ~}}
                    builder.For( {{ prop.name }}, x => x.NotEmpty() );
            {{~ end ~}}
            {{~ end ~}}
                }
            {{~ end ~}}
            }
            """;
    }

    private static string GetCommandHandlerTemplate() {
        return """
            using Myth.Flow.Actions;
            using Myth.Flow.Actions.Models;
            {{~ if has_validation ~}}
            using Myth.Guard;
            {{~ end ~}}

            namespace {{ namespace }};

            /// <summary>
            /// Handler for {{ name }} command
            /// </summary>
            public class {{ name }}Handler : ICommandHandler<{{ name }}{{~ if return_type ~}}, {{ return_type }}{{~ end ~}}> {
            {{~ if has_validation ~}}
                private readonly IValidator _validator;
            {{~ end ~}}
            {{~ if publishes_events ~}}
                private readonly IDispatcher _dispatcher;
            {{~ end ~}}

                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Handler"/> class
                /// </summary>
                public {{ name }}Handler(
            {{~ if has_validation ~}}IValidator validator{{~ end ~}}{{~ if publishes_events && has_validation ~}},{{~ end ~}}
            {{~ if publishes_events ~}}IDispatcher dispatcher{{~ end ~}} ) {
            {{~ if has_validation ~}}
                    _validator = validator;
            {{~ end ~}}
            {{~ if publishes_events ~}}
                    _dispatcher = dispatcher;
            {{~ end ~}}
                }

                /// <summary>
                /// Handles the command
                /// </summary>
                /// <param name="command">Command to handle</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Command result</returns>
                public async Task<{{~ if return_type ~}}CommandResult<{{ return_type }}>{{~ else ~}}CommandResult{{~ end ~}}> HandleAsync(
                    {{ name }} command,
                    CancellationToken cancellationToken = default ) {
            {{~ if has_validation ~}}
                    // Validate command
                    await _validator.ValidateAsync( command, ValidationContextKey.Create, cancellationToken );

            {{~ end ~}}
                    try {
                        // TODO: Implement command logic

            {{~ if publishes_events ~}}
                        // Publish events
            {{~ for event in events ~}}
                        await _dispatcher.PublishEventAsync( new {{ event }} {
                            // TODO: Set event properties
                        }, cancellationToken );
            {{~ end ~}}

            {{~ end ~}}
            {{~ if return_type ~}}
                        // TODO: Return appropriate result
                        var result = default({{ return_type }});
                        return CommandResult<{{ return_type }}>.Success( result! );
            {{~ else ~}}
                        return CommandResult.Success();
            {{~ end ~}}
                    } catch( Exception ex ) {
                        return {{~ if return_type ~}}CommandResult<{{ return_type }}>{{~ else ~}}CommandResult{{~ end ~}}.Failure( $"Error handling {{ name }}: {ex.Message}", ex );
                    }
                }
            }
            """;
    }

    private static string GetQueryTemplate() {
        return """
            using Myth.Flow.Actions;
            {{~ if has_validation ~}}
            using Myth.Guard;
            {{~ end ~}}

            namespace {{ namespace }};

            /// <summary>
            /// Query for {{ name }}
            /// </summary>
            public record {{ name }} : IQuery<{{ return_type }}>{{~ if has_validation ~}}, IValidatable<{{ name }}>{{~ end ~}} {
            {{~ for prop in properties ~}}
                /// <summary>
                /// Gets or sets {{ prop.name }}
                /// </summary>
                public {{ prop.type }} {{ prop.name }} { get; init; }

            {{~ end ~}}
            {{~ if has_validation ~}}

                /// <summary>
                /// Validates the query
                /// </summary>
                /// <param name="builder">Validation builder</param>
                /// <param name="context">Validation context</param>
                public void Validate( ValidationBuilder<{{ name }}> builder, ValidationContextKey? context = null ) {
            {{~ for prop in properties ~}}
            {{~ if prop.type == "Guid" && prop.name == "Id" ~}}
                    builder.For( {{ prop.name }}, x => x.NotEmpty() );
            {{~ else if prop.type == "string" ~}}
                    builder.For( {{ prop.name }}, x => x.MaximumLength( 100 ) );
            {{~ end ~}}
            {{~ end ~}}
                }
            {{~ end ~}}
            }
            """;
    }

    private static string GetQueryHandlerTemplate() {
        return """
            using Myth.Flow.Actions;
            using Myth.Flow.Actions.Models;
            {{~ if has_validation ~}}
            using Myth.Guard;
            {{~ end ~}}

            namespace {{ namespace }};

            /// <summary>
            /// Handler for {{ name }} query
            /// </summary>
            public class {{ name }}Handler : IQueryHandler<{{ name }}, {{ return_type }}> {
            {{~ if has_validation ~}}
                private readonly IValidator _validator;
            {{~ end ~}}

                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Handler"/> class
                /// </summary>
                public {{ name }}Handler({{~ if has_validation ~}}IValidator validator{{~ end ~}} ) {
            {{~ if has_validation ~}}
                    _validator = validator;
            {{~ end ~}}
                }

                /// <summary>
                /// Handles the query
                /// </summary>
                /// <param name="query">Query to handle</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Query result</returns>
                public async Task<QueryResult<{{ return_type }}>> HandleAsync(
                    {{ name }} query,
                    CancellationToken cancellationToken = default ) {
            {{~ if has_validation ~}}
                    // Validate query
                    await _validator.ValidateAsync( query, ValidationContextKey.Search, cancellationToken );

            {{~ end ~}}
                    try {
                        // TODO: Implement query logic
                        var result = default({{ return_type }});

                        return QueryResult<{{ return_type }}>.Success( result! );
                    } catch( Exception ex ) {
                        return QueryResult<{{ return_type }}>.Failure( $"Error handling {{ name }}: {ex.Message}", ex );
                    }
                }
            }
            """;
    }

    private static string GetEventTemplate() {
        return """
            using Myth.Flow.Actions;

            namespace {{ namespace }};

            /// <summary>
            /// Event for {{ name }}
            /// </summary>
            public record {{ name }} : DomainEvent {
            {{~ for prop in properties ~}}
                /// <summary>
                /// Gets or sets {{ prop.name }}
                /// </summary>
                public{{ if prop.is_required }} required{{ end }} {{ prop.type }} {{ prop.name }} { get; init; }

            {{~ end ~}}
            }
            """;
    }

    private static string GetDtoTemplate() {
        return """
            namespace {{ namespace }};

            /// <summary>
            /// {{ name }} DTO
            /// </summary>
            public record {{ name }} {
            {{~ for prop in properties ~}}
                /// <summary>
                /// Gets or sets {{ prop.name }}
                /// </summary>
                public{{ if prop.is_required }} required{{ end }} {{ prop.type }} {{ prop.name }} { get; init; }

            {{~ end ~}}
            }
            """;
    }

    private static string GetEventHandlerTemplate() {
        return """
            using Myth.Flow.Actions;

            namespace {{ namespace }};

            /// <summary>
            /// Handler for {{ name }} event
            /// </summary>
            public class {{ name }}Handler : IEventHandler<{{ name }}> {
                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Handler"/> class
                /// </summary>
                public {{ name }}Handler() {
                }

                /// <summary>
                /// Handles the event
                /// </summary>
                /// <param name="eventData">Event to handle</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Task</returns>
                public async Task HandleAsync( {{ name }} eventData, CancellationToken cancellationToken = default ) {
                    // TODO: Implement event handling logic

                    await Task.CompletedTask;
                }
            }
            """;
    }

    private static string GetEntityTemplate() {
        return """
            using Myth.Commons;

            namespace {{ namespace }};

            /// <summary>
            /// {{ name }} entity
            /// </summary>
            public class {{ name }} : BaseEntity<Guid> {
            {{~ for prop in properties ~}}
                /// <summary>
                /// Gets or sets {{ prop.name }}
                /// </summary>
                public{{ if prop.is_required }} required{{ end }} {{ prop.type }} {{ prop.name }} { get; set; }

            {{~ end ~}}
                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}"/> class
                /// </summary>
                public {{ name }}() {
                    Id = Guid.NewGuid();
                    CreatedAt = DateTime.UtcNow;
                }
            }
            """;
    }

    private static string GetRepositoryInterfaceTemplate() {
        return """
            using Myth.Repository;

            namespace {{ namespace }};

            /// <summary>
            /// Repository interface for {{ name }} entity
            /// </summary>
            public interface I{{ name }}Repository : IRepository<{{ name }}> {
                /// <summary>
                /// Gets {{ name }} by name
                /// </summary>
                /// <param name="name">The name to search for</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>The {{ name }} if found</returns>
                Task<{{ name }}?> GetByNameAsync( string name, CancellationToken cancellationToken = default );

                /// <summary>
                /// Gets active {{ name }} entities
                /// </summary>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>List of active entities</returns>
                Task<List<{{ name }}>> GetActiveAsync( CancellationToken cancellationToken = default );
            }
            """;
    }

    private static string GetRepositoryTemplate() {
        return """
            using Microsoft.EntityFrameworkCore;
            using Myth.Repository.EntityFramework;

            namespace {{ namespace }};

            /// <summary>
            /// Repository implementation for {{ name }} entity
            /// </summary>
            public class {{ name }}Repository : RepositoryBase<{{ name }}>, I{{ name }}Repository {
                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Repository"/> class
                /// </summary>
                /// <param name="context">Database context</param>
                public {{ name }}Repository( DbContext context ) : base( context ) {
                }

                /// <summary>
                /// Gets {{ name }} by name
                /// </summary>
                /// <param name="name">The name to search for</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>The {{ name }} if found</returns>
                public async Task<{{ name }}?> GetByNameAsync( string name, CancellationToken cancellationToken = default ) {
                    return await DbSet
                        .Where( x => x.Name == name )
                        .FirstOrDefaultAsync( cancellationToken );
                }

                /// <summary>
                /// Gets active {{ name }} entities
                /// </summary>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>List of active entities</returns>
                public async Task<List<{{ name }}>> GetActiveAsync( CancellationToken cancellationToken = default ) {
                    return await DbSet
                        .Where( x => x.IsActive )
                        .ToListAsync( cancellationToken );
                }
            }
            """;
    }

    private static string GetControllerTemplate() {
        return """
            using Microsoft.AspNetCore.Mvc;
            using Myth.Flow.Actions;
            using Myth.Guard;

            namespace {{ namespace }};

            /// <summary>
            /// Controller for {{ aggregate }} operations
            /// </summary>
            [ApiController]
            [Route("api/[controller]")]
            public class {{ name }} : ControllerBase {
                private readonly IDispatcher _dispatcher;
                private readonly IValidator _validator;

                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}"/> class
                /// </summary>
                /// <param name="dispatcher">The command/query dispatcher</param>
                /// <param name="validator">The validator service</param>
                public {{ name }}( IDispatcher dispatcher, IValidator validator ) {
                    _dispatcher = dispatcher;
                    _validator = validator;
                }

                /// <summary>
                /// Creates a new {{ aggregate }}
                /// </summary>
                /// <param name="request">Create {{ aggregate }} request</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Created {{ aggregate }} ID</returns>
                [HttpPost]
                public async Task<IActionResult> Create( Create{{ aggregate }}Request request, CancellationToken cancellationToken = default ) {
                    // Validate request
                    await _validator.ValidateAsync( request, ValidationContextKey.Create, cancellationToken );

                    var command = new Create{{ aggregate }}Command {
            {{~ for prop in properties ~}}
                        {{ prop.name }} = request.{{ prop.name }},
            {{~ end ~}}
                    };

                    var result = await _dispatcher.DispatchCommandAsync( command, cancellationToken );

                    if ( result.IsSuccess ) {
                        return CreatedAtAction( nameof( GetById ), new { id = result.Value }, result.Value );
                    }

                    return BadRequest( result.ErrorMessage );
                }

                /// <summary>
                /// Gets {{ aggregate }} by ID
                /// </summary>
                /// <param name="id">{{ aggregate }} ID</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>{{ aggregate }} details</returns>
                [HttpGet("{id}")]
                public async Task<IActionResult> GetById( Guid id, CancellationToken cancellationToken = default ) {
                    var query = new Get{{ aggregate }}Query { Id = id };
                    var result = await _dispatcher.DispatchQueryAsync( query, cancellationToken );

                    if ( result.IsSuccess ) {
                        return Ok( result.Value );
                    }

                    return NotFound( result.ErrorMessage );
                }

                /// <summary>
                /// Gets all {{ aggregate }}s
                /// </summary>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>List of {{ aggregate }}s</returns>
                [HttpGet]
                public async Task<IActionResult> GetAll( CancellationToken cancellationToken = default ) {
                    var query = new GetAll{{ aggregate }}Query();
                    var result = await _dispatcher.DispatchQueryAsync( query, cancellationToken );

                    if ( result.IsSuccess ) {
                        return Ok( result.Value );
                    }

                    return BadRequest( result.ErrorMessage );
                }

                /// <summary>
                /// Updates a {{ aggregate }}
                /// </summary>
                /// <param name="id">{{ aggregate }} ID</param>
                /// <param name="request">Update {{ aggregate }} request</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Updated {{ aggregate }}</returns>
                [HttpPut("{id}")]
                public async Task<IActionResult> Update( Guid id, Update{{ aggregate }}Request request, CancellationToken cancellationToken = default ) {
                    // Validate request
                    await _validator.ValidateAsync( request, ValidationContextKey.Update, cancellationToken );

                    var command = new Update{{ aggregate }}Command {
                        Id = id,
            {{~ for prop in properties ~}}
                        {{ prop.name }} = request.{{ prop.name }},
            {{~ end ~}}
                    };

                    var result = await _dispatcher.DispatchCommandAsync( command, cancellationToken );

                    if ( result.IsSuccess ) {
                        return NoContent();
                    }

                    return BadRequest( result.ErrorMessage );
                }

                /// <summary>
                /// Deletes a {{ aggregate }}
                /// </summary>
                /// <param name="id">{{ aggregate }} ID</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Delete confirmation</returns>
                [HttpDelete("{id}")]
                public async Task<IActionResult> Delete( Guid id, CancellationToken cancellationToken = default ) {
                    var command = new Delete{{ aggregate }}Command { Id = id };
                    var result = await _dispatcher.DispatchCommandAsync( command, cancellationToken );

                    if ( result.IsSuccess ) {
                        return NoContent();
                    }

                    return BadRequest( result.ErrorMessage );
                }
            }
            """;
    }

    private static string ToPascalCase( string input ) {
        if( string.IsNullOrEmpty( input ) )
            return input;

        return char.ToUpper( input[0] ) + input[1..];
    }

    private static string ToCamelCase( string input ) {
        if( string.IsNullOrEmpty( input ) )
            return input;

        return char.ToLower( input[0] ) + input[1..];
    }

}