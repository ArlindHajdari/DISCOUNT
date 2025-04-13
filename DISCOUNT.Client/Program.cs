using Grpc.Net.Client;
using DISCOUNT.Client;
using DISCOUNT.Client.Services;

var builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((hostContext, services) =>
{   
    var grpcServerAddress = Environment.GetEnvironmentVariable("GRPC_SERVER_ADDRESS") ?? "http://discountserver:5001";

    var handler = new HttpClientHandler()
    {
        ServerCertificateCustomValidationCallback = 
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    var channel = GrpcChannel.ForAddress(grpcServerAddress, new GrpcChannelOptions { HttpHandler = handler });
    var discountClient = new DiscountService.DiscountServiceClient(channel);

    services.AddSingleton(discountClient);
    services.AddHostedService<TCPBridge>();
})
.ConfigureLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Information);
});



await builder.Build().RunAsync();