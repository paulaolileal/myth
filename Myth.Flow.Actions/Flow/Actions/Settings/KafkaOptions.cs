/// <summary>
/// Kafka configuration options
/// </summary>
public sealed class KafkaOptions {
	public string BootstrapServers { get; set; }
	public string GroupId { get; set; }
	public string? ClientId { get; set; }
	public bool EnableAutoCommit { get; set; } = false;
	public int SessionTimeoutMs { get; set; } = 30000;
	public int MaxPollIntervalMs { get; set; } = 300000;
	public string AutoOffsetReset { get; set; } = "earliest";
	public int ProducerLingerMs { get; set; } = 10;
	public int ProducerBatchSize { get; set; } = 16384;
	public string CompressionType { get; set; } = "snappy";
	public Dictionary<string, string> AdditionalConfig { get; set; } = new( );
}