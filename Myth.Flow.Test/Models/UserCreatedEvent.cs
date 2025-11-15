using System;

namespace Myth.Flow.Test.Models;

public record UserCreatedEvent( Guid UserId, string Email, string Role, DateTime CreatedAt );
