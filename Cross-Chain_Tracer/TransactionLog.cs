using System.Text.Json.Serialization;

namespace Cross_Chain_Tracer;

public class TransactionLog{
    [JsonPropertyName("transactionHash")]
    public string TransactionHash { get; set; }

    [JsonPropertyName("blockNumber")]
    public string BlockNumber { get; set; }

    [JsonPropertyName("data")]
    public string Data { get; set; }

    [JsonPropertyName("topics")]
    public List<string> Topics { get; set; }
}