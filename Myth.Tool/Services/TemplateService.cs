using Myth.Tool.Models;
using Scriban;
using Scriban.Runtime;

namespace Myth.Tool.Services;

/// <summary>
/// Service for template processing
/// </summary>
public class TemplateService {
	private readonly Dictionary<string, string> _templates;

	/// <summary>
	/// Initializes a new instance of the <see cref="TemplateService"/> class
	/// </summary>
	public TemplateService( ) {
		_templates = LoadTemplates( );
	}

	/// <summary>
	/// Renders a template with context
	/// </summary>
	/// <param name="templateName">Template name</param>
	/// <param name="context">Generation context</param>
	/// <returns>Rendered content</returns>
	public string RenderTemplate( string templateName, GenerationContext context ) {
		if ( !_templates.TryGetValue( templateName, out var templateContent ) ) {
			throw new InvalidOperationException( $"Template '{templateName}' not found" );
		}

		var template = Template.Parse( templateContent );
		var scriptObject = new ScriptObject( );

		// Add context data
		scriptObject.Add( "aggregate", context.Aggregate );
		scriptObject.Add( "name", context.Name );
		scriptObject.Add( "namespace", context.Namespace );
		scriptObject.Add( "properties", context.Properties );
		scriptObject.Add( "return_type", context.ReturnType );
		scriptObject.Add( "has_validation", context.HasValidation );
		scriptObject.Add( "publishes_events", context.PublishesEvents );
		scriptObject.Add( "events", context.Events );
		scriptObject.Add( "repository_type", context.RepositoryType );

		// Add helper functions
		var helperObject = new ScriptObject( );
		helperObject.Add( "pascal_case", new Func<string, string>( ToPascalCase ) );
		helperObject.Add( "camel_case", new Func<string, string>( ToCamelCase ) );
		helperObject.Add( "lower", new Func<string, string>( s => s.ToLowerInvariant( ) ) );
		helperObject.Add( "replace", new Func<string, string, string, string>( ( s, old, @new ) => s.Replace( old, @new ) ) );

		scriptObject.Add( "string", helperObject );

		// Add global helper functions
		scriptObject.Add( "lower", new Func<string, string>( s => s.ToLowerInvariant( ) ) );

		var templateContext = new TemplateContext( );
		templateContext.PushGlobal( scriptObject );

		// Register custom filters
		templateContext.PushGlobal( new ScriptObject {
			[ "lower" ] = new Func<string, string>( s => s.ToLowerInvariant( ) )
		} );

		return template.Render( templateContext );
	}

	/// <summary>
	/// Gets available templates
	/// </summary>
	/// <returns>List of template names</returns>
	public List<string> GetAvailableTemplates( ) => _templates.Keys.ToList( );

	private static Dictionary<string, string> LoadTemplates( ) {
		return new Dictionary<string, string> {
			[ "command" ] = GetCommandTemplate( ),
			[ "command-handler" ] = GetCommandHandlerTemplate( ),
			[ "query" ] = GetQueryTemplate( ),
			[ "query-handler" ] = GetQueryHandlerTemplate( ),
			[ "event" ] = GetEventTemplate( ),
			[ "event-handler" ] = GetEventHandlerTemplate( ),
			[ "dto" ] = GetDtoTemplate( ),
			[ "entity" ] = GetEntityTemplate( ),
			[ "repository" ] = GetRepositoryTemplate( ),
			[ "repository-interface" ] = GetRepositoryInterfaceTemplate( ),
			[ "controller" ] = GetControllerTemplate( )
		};
	}

	private static string GetCommandTemplate( ) {
		return """
            using Myth.Flow.Actions;
            {{~ if has_validation ~}}
            using Myth.Guard;
            {{~ end ~}}

            namespace {{ namespace }};

            /// <summary>
            /// Command for {{ name }}
            /// </summary>
            public record {{ name }} : ICommand{{~ if return_type && return_type != "" ~}}<{{ return_type }}>{{~ end ~}}{{~ if has_validation ~}}, IValidatable<{{ name }}>{{~ end ~}} {
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

	private static string GetCommandHandlerTemplate( ) {
		return """
            using Myth.Flow.Actions;
            using Myth.Flow.Actions.Models;

            namespace {{ namespace }};

            /// <summary>
            /// Handler for {{ name }} command
            /// </summary>
            public class {{ name }}Handler : ICommandHandler<{{ name }}{{~ if return_type && return_type != "" ~}}, {{ return_type }}{{~ end ~}}> {
            {{~ if publishes_events ~}}
                private readonly IDispatcher _dispatcher;

                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Handler"/> class
                /// </summary>
                public {{ name }}Handler( IDispatcher dispatcher ) {
                    _dispatcher = dispatcher;
                }
            {{~ else ~}}
                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Handler"/> class
                /// </summary>
                public {{ name }}Handler() {
                }
            {{~ end ~}}

                /// <summary>
                /// Handles the command
                /// </summary>
                /// <param name="command">Command to handle</param>
                /// <param name="cancellationToken">Cancellation token</param>
                /// <returns>Command result</returns>
                public async Task<{{~ if return_type && return_type != "" ~}}CommandResult<{{ return_type }}>{{~ else ~}}CommandResult{{~ end ~}}> HandleAsync(
                    {{ name }} command,
                    CancellationToken cancellationToken = default ) {
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
            {{~ if return_type && return_type != "" ~}}
                        // TODO: Return appropriate result
                        var result = default({{ return_type }});
                        return CommandResult<{{ return_type }}>.Success( result! );
            {{~ else ~}}
                        return CommandResult.Success();
            {{~ end ~}}
                    } catch( Exception ex ) {
                        return {{~ if return_type && return_type != "" ~}}CommandResult<{{ return_type }}>{{~ else ~}}CommandResult{{~ end ~}}.Failure( $"Error handling {{ name }}: {ex.Message}", ex );
                    }
                }
            }
            """;
	}

	private static string GetQueryTemplate( ) {
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

	private static string GetQueryHandlerTemplate( ) {
		return """
            using Myth.Flow.Actions;
            using Myth.Flow.Actions.Models;

            namespace {{ namespace }};

            /// <summary>
            /// Handler for {{ name }} query
            /// </summary>
            public class {{ name }}Handler : IQueryHandler<{{ name }}, {{ return_type }}> {
                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Handler"/> class
                /// </summary>
                public {{ name }}Handler() {
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

	private static string GetEventTemplate( ) {
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

	private static string GetDtoTemplate( ) {
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

	private static string GetEventHandlerTemplate( ) {
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

	private static string GetEntityTemplate( ) {
		return """
            namespace {{ namespace }};

            /// <summary>
            /// {{ name }} entity
            /// </summary>
            public class {{ name }} {
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
                }
            }
            """;
	}

	private static string GetRepositoryInterfaceTemplate( ) {
		return """
            using Myth.Interfaces.Repositories.Base;

            namespace {{ namespace }};

            /// <summary>
            /// Repository interface for {{ name }} entity
            /// </summary>
            {{~ if repository_type == "read" ~}}
            public interface I{{ name }}Repository : IReadRepositoryAsync<{{ name }}> {
            {{~ else if repository_type == "write" ~}}
            public interface I{{ name }}Repository : IWriteRepositoryAsync<{{ name }}> {
            {{~ else ~}}
            public interface I{{ name }}Repository : IReadWriteRepositoryAsync<{{ name }}> {
            {{~ end ~}}
                // TODO: Add custom methods specific to {{ name }} entity here
                // All basic CRUD operations are inherited from the base interface
            }
            """;
	}

	private static string GetRepositoryTemplate( ) {
		return """
            using Myth.Contexts;
            using Myth.Repositories.EntityFramework;

            namespace {{ namespace }};

            /// <summary>
            /// Repository implementation for {{ name }} entity
            /// </summary>
            {{~ if repository_type == "read" ~}}
            public class {{ name }}Repository : ReadRepositoryAsync<{{ name }}>, I{{ name }}Repository {
            {{~ else if repository_type == "write" ~}}
            public class {{ name }}Repository : WriteRepositoryAsync<{{ name }}>, I{{ name }}Repository {
            {{~ else ~}}
            public class {{ name }}Repository : ReadWriteRepositoryAsync<{{ name }}>, I{{ name }}Repository {
            {{~ end ~}}
                /// <summary>
                /// Initializes a new instance of the <see cref="{{ name }}Repository"/> class
                /// </summary>
                /// <param name="context">Database context</param>
                public {{ name }}Repository( BaseContext context ) : base( context ) {
                }

                // TODO: Add custom methods specific to {{ name }} entity here
                // All basic CRUD operations are inherited from the base class
            }
            """;
	}

	private static string GetControllerTemplate( ) {
		return """
            using Microsoft.AspNetCore.Mvc;
            using Myth.Extensions;
            using Myth.Flow.Actions.Extensions;
            using Myth.Interfaces;
            using Myth.Interfaces.Results;
            using Myth.ValueObjects;

            namespace {{ namespace }};

            /// <summary>
            /// RESTful API controller for managing {{ aggregate }} data.
            /// This controller demonstrates Clean Architecture principles, CQRS pattern,
            /// and advanced pipeline processing using the Myth framework.
            ///
            /// Features demonstrated:
            /// - Pipeline-based request processing with validation, caching, and logging
            /// - CQRS implementation with separate command and query handlers
            /// - Event-driven architecture with domain event publishing
            /// - Comprehensive error handling and validation
            /// - Async/await patterns with cancellation token support
            /// </summary>
            /// <remarks>
            /// This controller demonstrates best practices for:
            /// - Request validation using the built-in validation pipeline
            /// - Response caching for improved performance
            /// - Structured logging with contextual information
            /// - Event publishing for loose coupling between bounded contexts
            /// - RESTful API design with proper HTTP status codes
            /// </remarks>
            [Tags( "{{ aggregate }}" )]
            [ApiController]
            [Route( "api/v1/[controller]" )]
            public class {{ name }}( IValidator validator, ILogger<{{ name }}> logger ) : ControllerBase {

                /// <summary>
                /// Retrieves a paginated collection of {{ aggregate }}s with optional filtering capabilities.
                /// </summary>
                /// <param name="pagination">Pagination parameters including page number and page size for result limiting.</param>
                /// <param name="cancellationToken">Token to monitor for cancellation requests during the asynchronous operation.</param>
                /// <response code="200">Successfully retrieved the paginated {{ aggregate }} collection</response>
                /// <response code="400">Invalid request parameters or validation errors</response>
                /// <response code="500">Internal server error occurred during processing</response>
                /// <remarks>
                /// This endpoint demonstrates advanced querying with pagination, caching,
                /// and comprehensive logging throughout the pipeline execution.
                /// </remarks>
                [HttpGet]
                public async Task<IActionResult> GetAsync(
                    [FromQuery] Pagination pagination,
                    CancellationToken cancellationToken = default ) {
                    var result = await PipelineExtensions
                        .Start( new GetAll{{ aggregate }}Query( pagination ) )
                        .TapAsync( pipeline => validator.ValidateAsync( pipeline.CurrentRequest! ) )
                        .Tap( pipeline => logger.LogDebug( "Filters validated with success!" ) )
                        .Query<GetAll{{ aggregate }}Query, IPaginated<Get{{ aggregate }}Response>>( ( query, conf ) => conf.UseCache() )
                        .Tap( pipeline => logger.LogInformation( "{{ aggregate }} queried with `{Amount}` results", pipeline.CurrentRequest!.Items.Count() ) )
                        .ExecuteAsync( cancellationToken );

                    return Ok( result.Value );
                }

                /// <summary>
                /// Retrieves a specific {{ aggregate }} by its unique identifier.
                /// </summary>
                /// <param name="id">The unique identifier (GUID) of the {{ aggregate }} to retrieve.</param>
                /// <param name="cancellationToken">Token to monitor for cancellation requests during the asynchronous operation.</param>
                /// <response code="200">Successfully retrieved the {{ aggregate }}</response>
                /// <response code="400">Invalid identifier format provided</response>
                /// <response code="404">{{ aggregate }} not found</response>
                /// <response code="500">Internal server error occurred during processing</response>
                /// <remarks>
                /// This endpoint demonstrates simple entity retrieval with validation and caching capabilities.
                /// </remarks>
                [HttpGet( "{id}", Name = "GetByIdAsync" )]
                public async Task<IActionResult> GetByIdAsync( [FromRoute] Guid id, CancellationToken cancellationToken = default ) {
                    var result = await PipelineExtensions
                        .Start( new Get{{ aggregate }}ByIdQuery( id ) )
                        .TapAsync( pipeline => validator.ValidateAsync( pipeline.CurrentRequest! ) )
                        .Tap( pipeline => logger.LogDebug( "Request validated with success!" ) )
                        .Query<Get{{ aggregate }}ByIdQuery, Get{{ aggregate }}Response>()
                        .Tap( pipeline => logger.LogInformation( "{{ aggregate }} queried for identifier `{Id}` results", pipeline.CurrentRequest!.Id ) )
                        .ExecuteAsync( cancellationToken );

                    return Ok( result.Value );
                }

                /// <summary>
                /// Creates a new {{ aggregate }} entry in the system.
                /// </summary>
                /// <param name="request">The request object containing the {{ aggregate }} data to be created.</param>
                /// <param name="cancellationToken">Token to monitor for cancellation requests during the asynchronous operation.</param>
                /// <response code="201">{{ aggregate }} successfully created</response>
                /// <response code="400">Invalid request data or validation errors</response>
                /// <response code="409">{{ aggregate }} with the specified identifier already exists</response>
                /// <response code="500">Internal server error occurred during processing</response>
                /// <remarks>
                /// This endpoint demonstrates the complete command processing pipeline including validation,
                /// business logic execution, event publishing, and proper RESTful response creation.
                /// </remarks>
                [HttpPost]
                public async Task<IActionResult> PostAsync( [FromBody] Create{{ aggregate }}Request request, CancellationToken cancellationToken = default ) {
                    var result = await PipelineExtensions
                        .Start( request.To<Create{{ aggregate }}Command>() )
                        .TapAsync( pipeline => validator.ValidateAsync( pipeline.CurrentRequest! ) )
                        .Tap( pipeline => logger.LogDebug( "Request validated with success!" ) )
                        .Process<Create{{ aggregate }}Command, Guid>()
                        .Tap( pipeline => logger.LogInformation( "Command runned with success" ) )
                        .Transform( result => new {{ aggregate }}CreatedEvent( result ) )
                        .Publish()
                        .Tap( pipeline => logger.LogDebug( "Event published with success!" ) )
                        .ExecuteAsync( cancellationToken );

                    return CreatedAtRoute(
                        nameof( GetByIdAsync ),
                        new {
                            id = result.Value.Id
                        },
                        result.Value );
                }

                /// <summary>
                /// Updates an existing {{ aggregate }} with new data.
                /// </summary>
                /// <param name="request">The request object containing the updated {{ aggregate }} data including the identifier.</param>
                /// <param name="cancellationToken">Token to monitor for cancellation requests during the asynchronous operation.</param>
                /// <response code="204">{{ aggregate }} successfully updated</response>
                /// <response code="400">Invalid request data or validation errors</response>
                /// <response code="404">{{ aggregate }} not found for update</response>
                /// <response code="500">Internal server error occurred during processing</response>
                /// <remarks>
                /// This endpoint demonstrates the command processing pipeline for entity modifications,
                /// including validation and business rule enforcement.
                /// </remarks>
                [HttpPut]
                public async Task<IActionResult> PutAsync( [FromBody] Update{{ aggregate }}Request request, CancellationToken cancellationToken = default ) {
                    var result = await PipelineExtensions
                        .Start( request.To<Update{{ aggregate }}Command>() )
                        .TapAsync( pipeline => validator.ValidateAsync( pipeline.CurrentRequest! ) )
                        .Tap( pipeline => logger.LogDebug( "Request validated with success!" ) )
                        .Process()
                        .Tap( pipeline => logger.LogInformation( "Command runned with success" ) )
                        .ExecuteAsync( cancellationToken );

                    return NoContent();
                }

                /// <summary>
                /// Deletes an existing {{ aggregate }} from the system.
                /// </summary>
                /// <param name="request">The request object containing the identifier of the {{ aggregate }} to be deleted.</param>
                /// <param name="cancellationToken">Token to monitor for cancellation requests during the asynchronous operation.</param>
                /// <response code="204">{{ aggregate }} successfully deleted</response>
                /// <response code="400">Invalid request data or validation errors</response>
                /// <response code="404">{{ aggregate }} not found for deletion</response>
                /// <response code="500">Internal server error occurred during processing</response>
                /// <remarks>
                /// This endpoint handles the deletion of a {{ aggregate }} entity identified by its unique identifier.
                /// </remarks>
                [HttpDelete]
                public async Task<IActionResult> DeleteAsync( [FromBody] Delete{{ aggregate }}Request request, CancellationToken cancellationToken = default ) {
                    var result = await PipelineExtensions
                        .Start( request.To<Delete{{ aggregate }}Command>() )
                        .TapAsync( pipeline => validator.ValidateAsync( pipeline.CurrentRequest! ) )
                        .Tap( pipeline => logger.LogDebug( "Request validated with success!" ) )
                        .Process()
                        .Tap( pipeline => logger.LogInformation( "Command runned with success" ) )
                        .ExecuteAsync( cancellationToken );

                    return NoContent();
                }
            }
            """;
	}

	private static string ToPascalCase( string input ) {
		if ( string.IsNullOrEmpty( input ) )
			return input;

		return char.ToUpper( input[ 0 ] ) + input[ 1.. ];
	}

	private static string ToCamelCase( string input ) {
		if ( string.IsNullOrEmpty( input ) )
			return input;

		return char.ToLower( input[ 0 ] ) + input[ 1.. ];
	}

}
