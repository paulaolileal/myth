# Validation Error

**Type:** `https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md`

**Status Codes:** 400 (Bad Request), 409 (Conflict), 422 (Unprocessable Entity), or custom codes

## Description

Validation errors occur when input data fails to meet the defined validation rules. This error type follows [RFC 9457 (Problem Details for HTTP APIs)](https://www.rfc-editor.org/rfc/rfc9457.html), which supersedes RFC 7807.

## Response Format

```json
{
  "type": "https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md",
  "title": "One or more validation errors occurred",
  "status": 400,
  "instance": "/api/users",
  "traceId": "00-abc123...",
  "errors": {
    "email": [
      "Email is required"
    ],
    "age": [
      "Value must be greater than 0",
      "Value must be less than 150"
    ]
  }
}
```

## Fields

### Standard Fields (RFC 9457)

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `type` | string | Yes | URI reference identifying the error type |
| `title` | string | Yes | Human-readable summary of the problem type |
| `status` | integer | Yes | HTTP status code |
| `instance` | string | No | URI reference identifying the specific occurrence |
| `errors` | object | Yes | Dictionary mapping field names to arrays of error messages |

### Extension Fields

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `traceId` | string | No | Trace identifier for request correlation (distributed tracing) |
| `options` | object | No | Valid options for fields with enum/constant validation |

## Examples

### Simple Validation Error

```json
{
  "type": "https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md",
  "title": "One or more validation errors occurred",
  "status": 400,
  "instance": "/api/users",
  "traceId": "00-fb27f88950d75be0ee3c0787c3bcb772-f52eb8f38d98b38f-00",
  "errors": {
    "name": [
      "Name is required"
    ]
  }
}
```

### Multiple Errors Per Field

```json
{
  "type": "https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md",
  "title": "One or more validation errors occurred",
  "status": 400,
  "instance": "/api/users",
  "traceId": "00-abc123...",
  "errors": {
    "password": [
      "Password must be at least 8 characters long",
      "Password must contain at least one uppercase letter",
      "Password must contain at least one digit"
    ]
  }
}
```

### With Enum/Constant Options

```json
{
  "type": "https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md",
  "title": "One or more validation errors occurred",
  "status": 400,
  "instance": "/api/orders",
  "traceId": "00-abc123...",
  "errors": {
    "status": [
      "Value 'X' is not valid. Valid options are: A: Active | I: Inactive | P: Pending"
    ]
  },
  "options": {
    "status": [
      "A: Active",
      "I: Inactive",
      "P: Pending"
    ]
  }
}
```

### Conflict Status (409)

When a validation error represents a conflict (e.g., duplicate email):

```json
{
  "type": "https://github.com/paulaolileal/myth/blob/main/docs/errors/validation.md",
  "title": "One or more validation errors occurred",
  "status": 409,
  "instance": "/api/users",
  "traceId": "00-abc123...",
  "errors": {
    "email": [
      "Email already exists"
    ]
  }
}
```

## HTTP Status Codes

The `status` field will contain the highest status code among all validation errors:

- **400 (Bad Request)**: Default for general validation failures
- **409 (Conflict)**: Resource conflicts (duplicates, constraints)
- **422 (Unprocessable Entity)**: Semantic validation failures (configurable as default)
- **404 (Not Found)**: Referenced resource doesn't exist

## Configuration

### Global Default Status Code

```csharp
builder.Services.AddGuard(config => config
    .UseDefaultStatusCode(422) // UnprocessableEntity for all validation errors
);
```

### Per-Rule Status Code

```csharp
public void Validate(ValidationBuilder<UserDto> builder, ValidationContextKey? context = null)
{
    builder.For(Email, x => x
        .NotEmpty()
        .Email()
        .RespectAsync(async (email, ct, sp) =>
        {
            var userService = sp.GetRequiredService<IUserService>();
            return await userService.IsEmailAvailableAsync(email, ct);
        })
        .WithMessage("Email already exists")
        .WithStatusCode(HttpStatusCode.Conflict) // 409
    );
}
```

## Client Handling

### TypeScript/JavaScript

```typescript
interface ValidationProblemDetails {
  type: string;
  title: string;
  status: number;
  instance?: string;
  traceId?: string;
  errors: Record<string, string[]>;
  options?: Record<string, string[]>;
}

// Handle validation errors
try {
  await api.post('/users', userData);
} catch (error) {
  if (error.response?.status === 400 || error.response?.status === 422) {
    const problem: ValidationProblemDetails = error.response.data;

    // Display errors per field
    Object.entries(problem.errors).forEach(([field, messages]) => {
      console.error(`${field}: ${messages.join(', ')}`);
    });

    // Use options if available
    if (problem.options?.status) {
      console.log('Valid options:', problem.options.status);
    }
  }
}
```

### C# Client

```csharp
using Microsoft.AspNetCore.Mvc;

try
{
    await httpClient.PostAsJsonAsync("/users", userData);
}
catch (HttpRequestException ex) when (ex.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.UnprocessableEntity)
{
    var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

    foreach (var (field, errors) in problem.Errors)
    {
        Console.WriteLine($"{field}: {string.Join(", ", errors)}");
    }
}
```

## See Also

- [RFC 9457 - Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457.html)
- [Myth.Guard Documentation](https://github.com/paulaolileal/myth/tree/main/Myth.Guard)
- [ASP.NET Core ProblemDetails](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails)
