namespace NPVCalculator.Domain.Models;

public class NpvRequest
{
    public List<decimal> CashFlows { get; set; } = [];
    public decimal LowerBoundRate { get; set; }
    public decimal UpperBoundRate { get; set; }
    public decimal Increment { get; set; }
}