using DISCOUNT.Server.Enums;
using StackExchange.Redis;

namespace DISCOUNT.Server.Helpers
{
    public class DiscountCodeGenerator
{
    private static readonly char[] _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789".ToCharArray();
    private readonly Random _random = new Random();
    private readonly IDatabaseAsync _redisDb;
    private const string RedisKey = "discount-codes";

    public DiscountCodeGenerator(IDatabaseAsync redisDb)
    {
        _redisDb = redisDb;
    }

    public async Task<List<string>> GenerateUniqueDiscountCodesAsync(uint count, uint length, int maxRetriesPerCode = 5)
    {
        var codes = new List<string>((int)count);

        for (int i = 0; i < count; i++)
        {
            string code = await GenerateUniqueCodeWithRetryAsync(length, maxRetriesPerCode);
            codes.Add(code);
        }

        return codes;
    }

    private async Task<string> GenerateUniqueCodeWithRetryAsync(uint length, int maxRetries)
    {
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var code = GenerateRandomCode(length);

            if (await _redisDb.StringSetAsync($"{RedisKey}:{code}", "unused", when: When.NotExists))
            {
                return code;
            }
        }

        throw new Exception($"Failed to generate a unique discount code after {maxRetries} retries.");
    }

    private string GenerateRandomCode(uint length)
    {
        return new string(Enumerable.Range(0, (int)length)
            .Select(_ => _chars[_random.Next(_chars.Length)]).ToArray());
    }

    public async Task<UseCodeResult> UseDiscountCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));

        var currentKey = $"{RedisKey}:{code}";
        var currentValue = await _redisDb.StringGetAsync(currentKey);

        if (!currentValue.HasValue)
        {
            return UseCodeResult.CodeNotFound;
        }

        if (currentValue == "used")
        {
            return UseCodeResult.CodeAlreadyUsed;
        }

        await _redisDb.StringSetAsync(currentKey, "used");

        return UseCodeResult.Success;
    }
}
}