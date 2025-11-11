using System;
using System.Net;
using Bogus;
using Myth.Constants;
using Myth.Interfaces;
using Myth.Rest.Test.Models;

namespace Myth.Rest.Test.Base;

public abstract class BaseTests {
	protected readonly IRestRequest _restClient;
	public readonly Faker _faker;
	public readonly Faker<Error> _errorFaker;

	protected BaseTests( ) {
		_faker = new Faker( "pt_BR" );

		_errorFaker = new Faker<Error>( "pt_BR" )
			.RuleFor( e => e.ErrorCode, f => f.Random.Int( 1000, 9999 ) )
			.RuleFor( e => e.Message, f => f.Lorem.Sentence( ) );

		_restClient = Rest
			.Create( )
			.Configure( config => config
				.WithContentType( "application/json" )
				.WithBodySerialization( CaseStrategy.CamelCase )
				.WithRetry( 3, TimeSpan.FromSeconds( 10 ), HttpStatusCode.InternalServerError )
				.WithTypeConverter<IPost, Post>( ) );
	}
}
