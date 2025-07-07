using NPVCalculator.Domain.Models;

namespace NPVCalculator.Application.Interfaces;

public interface INpvCalculatorService
{
    IEnumerable<NpvResult> Calculate(NpvRequest request);
}