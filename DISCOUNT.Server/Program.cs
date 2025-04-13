using DISCOUNT.Server.Services;
using DISCOUNT.Server.Helpers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information); 

var redisServerAddress = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING") ?? "redis:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisServerAddress));

builder.Services.AddScoped<IDatabaseAsync>(sp => sp.GetRequiredService<IConnectionMultiplexer>().GetDatabase());
builder.Services.AddScoped<DiscountCodeGenerator>();

builder.Services.AddGrpc();

var app = builder.Build();

app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
