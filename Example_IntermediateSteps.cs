using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Myth.Flow.Actions;
using Myth.Flow.Actions.Extensions;
using Myth.Flow.Actions.Settings;
using Myth.Interfaces;

// ===============================================
// EXEMPLO: Steps Intermediários no Pipeline
// ===============================================

// Comando de exemplo
public record CreateUserCommand : ICommand<Guid>
{
    public required string Email { get; init; }
    public required string Name { get; init; }
}

// Handler do comando
public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, Guid>
{
    private readonly ILogger<CreateUserCommandHandler> _logger;

    public CreateUserCommandHandler(ILogger<CreateUserCommandHandler> logger)
    {
        _logger = logger;
    }

    public async Task<CommandResult<Guid>> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user: {Email}", command.Email);

        // Simular criação do usuário
        await Task.Delay(100, cancellationToken);
        var userId = Guid.NewGuid();

        return CommandResult<Guid>.Success(userId);
    }
}

// Serviços de exemplo para os steps intermediários
public interface IValidationService
{
    Task ValidateEmailAsync(string email);
}

public interface ISecurityService
{
    Task LogSecurityEventAsync(string eventName, object data);
}

public interface INotificationService
{
    Task SendWelcomeEmailAsync(string email);
}

public class ValidationService : IValidationService
{
    public async Task ValidateEmailAsync(string email)
    {
        Console.WriteLine($"✓ Validating email: {email}");
        await Task.Delay(50); // Simular validação
        if (!email.Contains("@"))
            throw new ArgumentException("Invalid email format");
    }
}

public class SecurityService : ISecurityService
{
    public async Task LogSecurityEventAsync(string eventName, object data)
    {
        Console.WriteLine($"🔒 Security event logged: {eventName}");
        await Task.Delay(25);
    }
}

public class NotificationService : INotificationService
{
    public async Task SendWelcomeEmailAsync(string email)
    {
        Console.WriteLine($"📧 Welcome email sent to: {email}");
        await Task.Delay(100);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        // Configurar DI
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        services.AddFlow();
        services.AddFlowActions(options =>
        {
            options.UseInMemory()
                   .ScanAssemblies(typeof(Program).Assembly);
        });

        // Registrar serviços para os steps intermediários
        services.AddTransient<IValidationService, ValidationService>();
        services.AddTransient<ISecurityService, SecurityService>();
        services.AddTransient<INotificationService, NotificationService>();

        var serviceProvider = services.BuildServiceProvider();

        Console.WriteLine("🚀 Demonstração: Steps Intermediários no Pipeline\n");

        // ==========================================
        // EXEMPLO 1: Pipeline com validação prévia
        // ==========================================
        Console.WriteLine("📋 Exemplo 1: Validação e logging antes do comando");

        var result1 = await Pipeline
            .Start(new CreateUserCommand { Email = "user@example.com", Name = "John Doe" }, serviceProvider)
            // Step 1: Validar email antes do processamento
            .StepAsync<IValidationService>(async (validator, state) =>
            {
                await validator.ValidateEmailAsync(state.CurrentRequest!.Email);
                return state;
            })
            // Step 2: Log evento de segurança
            .TapAsync<ISecurityService>(async (security, state) =>
                await security.LogSecurityEventAsync("UserCreationStarted", state.CurrentRequest!))
            // Step 3: Processar o comando
            .Process<CreateUserCommand, Guid>()
            // Step 4: Enviar notificação após sucesso
            .TapAsync<INotificationService>(async (notification, state) =>
                await notification.SendWelcomeEmailAsync(state.CurrentRequest!.Email))
            .ExecuteAsync();

        if (result1.IsSuccess)
        {
            Console.WriteLine($"✅ Usuário criado com ID: {result1.Value}\n");
        }

        // ==========================================
        // EXEMPLO 2: Pipeline com execução condicional
        // ==========================================
        Console.WriteLine("🔀 Exemplo 2: Execução condicional baseada no email");

        var result2 = await Pipeline
            .Start(new CreateUserCommand { Email = "admin@company.com", Name = "Admin User" }, serviceProvider)
            // Validação sempre executada
            .StepAsync<IValidationService>(async (validator, state) =>
            {
                await validator.ValidateEmailAsync(state.CurrentRequest!.Email);
                return state;
            })
            // Log adicional apenas para emails administrativos
            .When(state => state.CurrentRequest!.Email.Contains("admin"), builder =>
                builder.TapAsync<ISecurityService>(async (security, state) =>
                    await security.LogSecurityEventAsync("AdminUserCreation", state.CurrentRequest!)))
            .Process<CreateUserCommand, Guid>()
            .ExecuteAsync();

        if (result2.IsSuccess)
        {
            Console.WriteLine($"✅ Admin criado com ID: {result2.Value}\n");
        }

        // ==========================================
        // EXEMPLO 3: Pipeline com retry e telemetria
        // ==========================================
        Console.WriteLine("🔄 Exemplo 3: Pipeline com retry e telemetria");

        var result3 = await Pipeline
            .Start(new CreateUserCommand { Email = "retry@example.com", Name = "Retry User" }, serviceProvider)
            // Adicionar telemetria
            .WithTelemetry("CreateUserWorkflow")
            // Configurar retry para operações externas
            .WithRetry(maxAttempts: 3, backoffMs: 500)
            .StepAsync<IValidationService>(async (validator, state) =>
            {
                await validator.ValidateEmailAsync(state.CurrentRequest!.Email);
                return state;
            })
            .Process<CreateUserCommand, Guid>()
            .ExecuteAsync();

        if (result3.IsSuccess)
        {
            Console.WriteLine($"✅ Usuário com retry criado com ID: {result3.Value}\n");
        }

        Console.WriteLine("🎉 Demonstração concluída!");
        Console.WriteLine("\n📚 Métodos intermediários disponíveis:");
        Console.WriteLine("  • Step<TService>() - Operações síncronas");
        Console.WriteLine("  • StepAsync<TService>() - Operações assíncronas");
        Console.WriteLine("  • Tap<TService>() - Efeitos colaterais");
        Console.WriteLine("  • TapAsync<TService>() - Efeitos colaterais assíncronos");
        Console.WriteLine("  • When(predicate, configure) - Execução condicional");
        Console.WriteLine("  • WithRetry(attempts, backoff) - Políticas de retry");
        Console.WriteLine("  • WithTelemetry(name) - Rastreamento distribuído");
    }
}