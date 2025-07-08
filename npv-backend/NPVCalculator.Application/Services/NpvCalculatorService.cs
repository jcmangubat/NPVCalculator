using Microsoft.Extensions.Caching.Memory;
using NPVCalculator.Application.Interfaces;
using NPVCalculator.Domain.Models;

namespace NPVCalculator.Application.Services;

public class NpvCalculatorService(IMemoryCache cache) : INpvCalculatorService
{
    private readonly IMemoryCache _cache = cache;

    public IEnumerable<NpvResult> Calculate(NpvRequest request)
    {
        var cacheKey = GenerateCacheKey(request);

        if (_cache.TryGetValue(cacheKey, out List<NpvResult>? cachedResults))
            return cachedResults!;
        
        var results = new List<NpvResult>();
        for (decimal rate = request.LowerBoundRate; rate <= request.UpperBoundRate; rate += request.Increment)
        {
            decimal npv = 0;
            for (int t = 0; t < request.CashFlows.Count; t++)
            {
                npv += request.CashFlows[t] / (decimal)Math.Pow((double)(1 + rate / 100), t + 1);
            }

            results.Add(new NpvResult { DiscountRate = rate, Npv = Math.Round(npv, 2) });
        }

        // Store in cache for 10 minutes
        _cache.Set(cacheKey, results, TimeSpan.FromMinutes(10));

        return results;
    }

    private static string GenerateCacheKey(NpvRequest request)
    {
        var cashFlowsKey = string.Join(",", request.CashFlows);
        return $"{cashFlowsKey}|{request.LowerBoundRate}|{request.UpperBoundRate}|{request.Increment}";
    }
}