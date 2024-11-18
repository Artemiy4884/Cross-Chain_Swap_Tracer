using System.Text.Json;
using Cross_Chain_Tracer;

class Tracer
{
    private static readonly string ETHEREUM_API_KEY = "YOUR_API_KEY"; 
    private static readonly string BASE_API_KEY = "YOUR_API_KEY"; 

    private static async Task Main(){
        Console.WriteLine("Enter start block for Base:");
        string baseStartBlock = Console.ReadLine();

        Console.WriteLine("Enter start block for Ethereum:");
        string ethStartBlock = Console.ReadLine();

        Console.WriteLine("Press 'Q' to stop the application.");

        var baseToEthTask = TraceCrossChainSwap(baseStartBlock, ethStartBlock, true);
        var ethToBaseTask = TraceCrossChainSwap(baseStartBlock, ethStartBlock, false);

        await Task.WhenAll(baseToEthTask, ethToBaseTask);
    }

    private static async Task TraceCrossChainSwap(string baseStartBlock, string ethStartBlock, bool isBaseToEthereum){
        
        string baseFunctionSignatureHash = "0xa123dc29aebf7d0c3322c8eeb5b999e859f39937950ed31056532713d0de396f"; // V3FundsDeposited
        string ethFunctionSignatureHash = "0x571749edf1d5c9599318cdbc4e28a6475d65e87fd3b2ddbe1e9a8d5e7a0f0ff7"; // FilledV3Relay

        uint currentBaseBlock = uint.Parse(baseStartBlock);
        uint currentEthBlock = uint.Parse(ethStartBlock);

        while (!Console.KeyAvailable || Console.ReadKey(true).Key != ConsoleKey.Q){
            List<TransactionLog> baseLogs, ethLogs;

            if (isBaseToEthereum){
                baseLogs = await FetchTransactionLogs("Base", baseFunctionSignatureHash, currentBaseBlock.ToString());
                ethLogs = await FetchTransactionLogs("Ethereum", ethFunctionSignatureHash, currentEthBlock.ToString());
            }else{
                baseLogs = await FetchTransactionLogs("Ethereum", baseFunctionSignatureHash, currentEthBlock.ToString());
                ethLogs = await FetchTransactionLogs("Base", ethFunctionSignatureHash, currentBaseBlock.ToString());
            }

            if (baseLogs != null && ethLogs != null){
                foreach (var baseLog in baseLogs){
                    foreach (var ethLog in ethLogs){
                        if (baseLog.Topics[2] == ethLog.Topics[2]){
                            Console.WriteLine("Match Found: ");
                            Console.WriteLine($"Input Chain: {(isBaseToEthereum ? "Base" : "Ethereum")}, Output Chain: {(isBaseToEthereum ? "Ethereum" : "Base")}");
                            Console.WriteLine($"Sender Chain ID: {baseLog.Topics[1]}");
                            Console.WriteLine($"Receiver Chain ID: {ethLog.Topics[1]}");
                            Console.WriteLine($"Sender Account ID: {baseLog.Topics[3]}");
                            Console.WriteLine($"Receiver Account ID: {ethLog.Topics[3]}");
                            Console.WriteLine($"Input Transaction Hash: {baseLog.TransactionHash}");
                            Console.WriteLine($"Output Transaction Hash: {ethLog.TransactionHash}");
                            Console.WriteLine("----------------------------------------------------------");
                        }
                    }
                }
            }
            currentBaseBlock++;
            currentEthBlock++;
        }
        Console.WriteLine("Real-time tracing stopped.");
    }
    
    public static async Task<List<TransactionLog>> FetchTransactionLogs(string chain, string functionSignatureHash, string startBlock){
        string apiUrl;
        switch (chain){
            case "Ethereum":
                apiUrl = $"https://api.etherscan.io/api?module=logs&action=getLogs&fromBlock={startBlock}&toBlock=latest&topic0={functionSignatureHash}&apikey={ETHEREUM_API_KEY}";
                break;
            case "Base":
                apiUrl = $"https://api.basescan.org/api?module=logs&action=getLogs&fromBlock={startBlock}&toBlock=latest&topic0={functionSignatureHash}&apikey={BASE_API_KEY}";
                break;
            default:
                throw new ArgumentException("Unsupported chain.");
        }

        Console.WriteLine($"Fetching transaction logs from {apiUrl}");
        using HttpClient client = new();
        try{
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode){
                string jsonResponse = await response.Content.ReadAsStringAsync();
                var logsResponse = JsonSerializer.Deserialize<LogsResponse>(jsonResponse);
                return logsResponse?.Result;
            }
        }catch (Exception ex){
            Console.WriteLine($"Exception while fetching logs: {ex.Message}");
        }
        return null;
    }
}