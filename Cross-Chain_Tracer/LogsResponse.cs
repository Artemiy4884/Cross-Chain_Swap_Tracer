using System.Text.Json.Serialization;

namespace Cross_Chain_Tracer;

public class LogsResponse{
    [JsonPropertyName("result")]
    public List<TransactionLog> Result { get; set; }
}