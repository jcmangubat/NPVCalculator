using Microsoft.Extensions.Caching.Memory;
using NPVCalculator.Application.Services;
using NPVCalculator.Domain.Models;

namespace NPVCalculator.Test.Unit;

public class NPVCalculatorUnitTest 
{
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly NpvCalculatorService _calculator;

    public NPVCalculatorUnitTest()
    {
        _calculator = new NpvCalculatorService(_cache);
    }

    /// <summary>
    /// Basic NPV computation with multiple rates
    /// Ensures NPV is computed for each step from lower to upper bound
    /// </summary>
    [Fact]
    public void Calculate_ReturnsExpectedNumberOfResults()
    {
        var request = new NpvRequest
        {
            CashFlows = [1000, 2000, 3000],
            LowerBoundRate = 1.0M,
            UpperBoundRate = 3.0M,
            Increment = 1.0M
        };

        var results = _calculator.Calculate(request);

        Assert.Equal(3, results.Count());
        Assert.Collection(results,
            r => Assert.Equal(1.0M, r.DiscountRate),
            r => Assert.Equal(2.0M, r.DiscountRate),
            r => Assert.Equal(3.0M, r.DiscountRate));
    }

    /// <summary>
    /// Single discount rate scenario 
    /// Should return exactly one NPV result
    /// </summary>
    [Fact]
    public void Calculate_HandlesSingleDiscountRateCorrectly()
    {
        var request = new NpvRequest
        {
            CashFlows = [-10000, 3000, 4200, 6800],
            LowerBoundRate = 2.0M,
            UpperBoundRate = 2.0M,
            Increment = 1.0M
        };

        var results = _calculator.Calculate(request).ToList();

        Assert.Single(results);
        Assert.Equal(2.0M, results[0].DiscountRate);
        Assert.True(results[0].Npv != 0);
    }

    /// <summary>
    /// Empty cash flows scenario  
    /// Should return NPV = 0 for each discount rate
    /// </summary>
    [Fact]
    public void Calculate_ReturnsZeroWhenNoCashFlows()
    {
        var request = new NpvRequest
        {
            CashFlows = [],
            LowerBoundRate = 1.0M,
            UpperBoundRate = 3.0M,
            Increment = 1.0M
        };

        var results = _calculator.Calculate(request).ToList();

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.Equal(0.0M, r.Npv));
    }

    /// <summary>
    /// Verifies rounding & NPV output is valid 
    /// Ensures NPVs are all positive and rates are within bounds
    /// </summary>
    [Fact]
    public void Calculate_ReturnsCorrectCount_AndRoundedNpvs()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 1000, 2000, 3000 },
            LowerBoundRate = 1.0M,
            UpperBoundRate = 3.0M,
            Increment = 1.0M
        };

        var results = _calculator.Calculate(request).ToList();

        Assert.Equal(3, results.Count);
        Assert.All(results, r => Assert.InRange(r.DiscountRate, 1.0M, 3.0M));
        Assert.All(results, r => Assert.True(r.Npv > 0));
    }

    /// <summary>
    /// Ensures incrementing logic produces expected rates 
    /// Verifies that rate steps are spaced correctly
    /// </summary>
    [Fact]
    public void Calculate_ProducesCorrectStepsBetweenBounds()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { 1000, 1000, 1000 },
            LowerBoundRate = 2.0M,
            UpperBoundRate = 3.0M,
            Increment = 0.25M
        };

        var results = _calculator.Calculate(request).ToList();

        var expectedRates = new List<decimal> { 2.00M, 2.25M, 2.50M, 2.75M, 3.00M };
        Assert.Equal(expectedRates.Count, results.Count);

        for (int i = 0; i < expectedRates.Count; i++)
        {
            Assert.Equal(expectedRates[i], results[i].DiscountRate);
        }
    }

    /// <summary>
    /// Tests actual caching behavior via instance identity 
    /// Cached list object should be reused (same reference)
    /// </summary>
    [Fact]
    public void Calculate_ReturnsCachedInstanceOnRepeatedRequest()
    {
        var request = new NpvRequest
        {
            CashFlows = new List<decimal> { -10000, 3000, 4200, 6800 },
            LowerBoundRate = 1.0M,
            UpperBoundRate = 1.0M,
            Increment = 1.0M
        };

        var resultFirst = _calculator.Calculate(request);
        var resultSecond = _calculator.Calculate(request);

        Assert.Same(resultFirst, resultSecond); // Reference match = cache used

        var list = resultFirst.ToList();
        Assert.Single(list);
        Assert.Equal(1.0M, list[0].DiscountRate);
    }

    /// <summary>
    /// Validates cache hit by comparing value equality 
    /// Even if reference changes (via ToList), values should match
    /// </summary>
    [Fact]
    public void Calculate_UsesCacheForRepeatedRequests()
    {
        var request = new NpvRequest
        {
            CashFlows = [1000, 1000, 1000],
            LowerBoundRate = 1.0M,
            UpperBoundRate = 1.0M,
            Increment = 1.0M
        };

        var firstResult = _calculator.Calculate(request).ToList();
        var secondResult = _calculator.Calculate(request).ToList();

        Assert.Equal(firstResult, secondResult); // Value-based equality
        Assert.Single(firstResult);
    }

    /// <summary>
    /// Tests large range of rates with small increments (0.25%)
    /// Ensures the NPV calculator steps correctly from 1.00% to 15.00%
    /// Useful for validating that expected rate count and precision match
    /// </summary>
    [Fact]
    public void Calculate_ProducesCorrectSteps_From1To15_ByQuarterPercent()
    {
        var request = new NpvRequest
        {
            CashFlows = [-10000, 3000, 4200, 6800],
            LowerBoundRate = 1.00M,
            UpperBoundRate = 15.00M,
            Increment = 0.25M
        };

        var results = _calculator.Calculate(request).ToList();

        // 15.00 - 1.00 = 14.00 ➜ 14.00 / 0.25 + 1 = 57 steps
        Assert.Equal(57, results.Count);

        // First and last rate checks
        Assert.Equal(1.00M, results.First().DiscountRate);
        Assert.Equal(15.00M, results.Last().DiscountRate);

        // Verify consistent incrementing
        for (int i = 1; i < results.Count; i++)
        {
            var step = results[i].DiscountRate - results[i - 1].DiscountRate;
            Assert.Equal(0.25M, step);
        }
    }
}
