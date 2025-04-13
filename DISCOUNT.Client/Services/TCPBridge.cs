using System.Net.Sockets;
using System.Net;
using System.Text.Json;
using System.Text;

namespace DISCOUNT.Client.Services
{
    public class TCPBridge : BackgroundService
    {
       private readonly DiscountService.DiscountServiceClient _discountClient;
        private readonly ILogger<TCPBridge> _logger;
        private readonly int _port = 6000;

        public TCPBridge(DiscountService.DiscountServiceClient discountClient, ILogger<TCPBridge> logger)
        {
            _discountClient = discountClient;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();
            _logger.LogInformation("TCP Bridge Server listening on port {Port}", _port);

            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(stoppingToken);
                _ = HandleClientAsync(client, stoppingToken);
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken token)
        {
            _logger.LogInformation("TCP client connected.");
            try
            {
                using var networkStream = client.GetStream();
                var buffer = new byte[4096];
                int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length, token);
                var requestJson = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                _logger.LogInformation("Received TCP Request: {RequestJson}", requestJson);

                var baseRequest = JsonSerializer.Deserialize<BaseRequest>(requestJson);

                if (baseRequest != null)
                {
                    string responseJson = "{}";

                    switch (baseRequest.MethodName)
                    {
                        case "GenerateDiscountCode":
                            var generateRequest = JsonSerializer.Deserialize<GenerationRequest>(baseRequest.Payload);
                            if (generateRequest != null)
                            {
                                _logger.LogInformation("Calling gRPC GenerateDiscountCode with Count: {Count}", generateRequest.Count);
                                var grpcResponse = await _discountClient.GenerateDiscountCodesAsync(generateRequest, cancellationToken: token);
                                responseJson = JsonSerializer.Serialize(new GenerationResponse{ Result = grpcResponse.Result });
                                _logger.LogInformation($"Generated result: {grpcResponse.Result}");
                            }
                            break;

                        case "UseDiscountCode":
                            var useRequest = JsonSerializer.Deserialize<UseCodeRequest>(baseRequest.Payload);
                            if (useRequest != null)
                            {
                                _logger.LogInformation("Calling gRPC UseDiscountCode with Code: {Code}", useRequest.Code);
                                var grpcResponse = await _discountClient.UseDiscountCodeAsync(useRequest, cancellationToken: token);
                                responseJson = JsonSerializer.Serialize(new UseCodeResponse{ Result = grpcResponse.Result });
                                _logger.LogInformation("UseDiscountCode result: {Result}", grpcResponse.Result);
                            }
                            break;

                        default:
                            _logger.LogWarning("Unknown method received: {MethodName}", baseRequest.MethodName);
                            responseJson = JsonSerializer.Serialize(new { Error = "Unknown method." });
                            break;
                    }

                    var responseBytes = Encoding.UTF8.GetBytes(responseJson);
                    await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length, token);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling TCP client");
            }
            finally
            {
                client.Close();
                _logger.LogInformation("TCP client disconnected.");
            }
        }

        public class BaseRequest
        {
            public string MethodName { get; set; } = string.Empty;
            public string Payload { get; set; } = string.Empty;
        }
    }
}