using System;

namespace Myth.Flow.Test.Models;

public record CreateUserResponse( Guid Id, string Email, string Role, DateTime CreatedAt );
