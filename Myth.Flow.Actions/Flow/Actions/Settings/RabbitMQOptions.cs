/// <summary>
/// RabbitMQ configuration options
/// </summary>
public sealed class RabbitMQOptions {
	public string HostName { get; set; }
	public int Port { get; set; } = 5672;
	public string UserName { get; set; }
	public string Password { get; set; }
	public string VirtualHost { get; set; } = "/";
	public string ExchangeName { get; set; } = "flow.actions";
	public string ExchangeType { get; set; } = "topic";
	public bool ExchangeDurable { get; set; } = true;
	public bool QueueDurable { get; set; } = true;
	public bool QueueAutoDelete { get; set; } = false;
	public ushort PrefetchCount { get; set; } = 10;
	public int ConnectionTimeout { get; set; } = 30000;
}