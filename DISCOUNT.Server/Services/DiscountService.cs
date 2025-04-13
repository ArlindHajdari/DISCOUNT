using DISCOUNT.Server.Helpers;
using Grpc.Core;

namespace DISCOUNT.Server.Services
{
    public class DiscountService : Server.DiscountService.DiscountServiceBase
{
    private readonly DiscountCodeGenerator _generator;

    private readonly ILogger<DiscountService> _logger;

    public DiscountService(DiscountCodeGenerator generator, ILogger<DiscountService> logger)
    {
        _logger = logger;
        _generator = generator;
    }

    public override async Task<GenerationResponse> GenerateDiscountCodes(GenerationRequest request, ServerCallContext context)
    {
        if (request.Count <= 0 || request.Count > 2000)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Count must be between 1 and 2000."));
        }

        if (request.Length < 7 || request.Length > 8)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Length must be 7 or 8."));
        }

        try
        {
            var codes = await _generator.GenerateUniqueDiscountCodesAsync(request.Count, request.Length);
            _logger.LogInformation($"Received GenerateUniqueDiscountCodesAsync response with Count={codes.Count}");
            var response = new GenerationResponse() { Result = codes.Count == request.Count};
            return response;
        }
        catch (Exception ex)
        {
            throw new RpcException(new Status(StatusCode.Internal, ex.Message));
        }
    }

    public override async Task<UseCodeResponse> UseDiscountCode(UseCodeRequest request, ServerCallContext context)
    {
        var discountCodeStatus = await _generator.UseDiscountCodeAsync(request.Code);
        _logger.LogInformation($"Received UseDiscountCode request with status={discountCodeStatus}");
        return new UseCodeResponse { Result = (uint)discountCodeStatus };
    }
}
}