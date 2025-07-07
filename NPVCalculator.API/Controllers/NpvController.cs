using Microsoft.AspNetCore.Mvc;
using NPVCalculator.Application.Interfaces;
using NPVCalculator.Domain.Models;
using System.Text;

namespace NPVCalculator.API.Controllers;

[ApiController]
[Route("api/npv")]
public class NpvController(INpvCalculatorService service) : ControllerBase
{
    private readonly INpvCalculatorService _service = service;

    /// <summary>
    /// Calculates a list of Net Present Value (NPV) results across a range of discount rates.
    /// </summary>
    /// <remarks>
    /// Computes NPV values for a series of cash flows given a range of discount rates 
    /// (from LowerBoundRate to UpperBoundRate, increasing by the specified Increment).
    /// 
    /// Example input:
    /// 
    ///     {
    ///         "cashFlows": [-10000, 3000, 4200, 6800],
    ///         "lowerBoundRate": 1.00,
    ///         "upperBoundRate": 5.00,
    ///         "increment": 1.00
    ///     }
    /// 
    /// This will calculate NPV for 1.00%, 2.00%, 3.00%, 4.00%, and 5.00%.
    /// </remarks>
    /// <param name="request">The input parameters for the NPV calculation including cash flows and rate bounds.</param>
    /// <returns>A list of NPV results for each discount rate increment in the specified range.</returns>
    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] NpvRequest request)
    {
        if (request.CashFlows == null || request.CashFlows.Count == 0)
            return BadRequest("CashFlows are required.");

        if (request.LowerBoundRate < 0 || request.UpperBoundRate < request.LowerBoundRate || request.Increment <= 0)
            return BadRequest("Invalid rate bounds or increment.");

        var results = _service.Calculate(request);
        return Ok(results);
    }

    /// <summary>
    /// Returns the NPV results as a downloadable CSV file.
    /// </summary>
    /// <remarks>
    /// Accepts a POST request with NPV input parameters and returns a CSV-formatted file containing
    /// discount rates and corresponding NPV values. This is useful for financial analysts or auditors
    /// who want to import results into Excel or reporting tools.
    ///
    /// Example request:
    ///
    ///     {
    ///         "cashFlows": [-10000, 3000, 4200, 6800],
    ///         "lowerBoundRate": 1.00,
    ///         "upperBoundRate": 5.00,
    ///         "increment": 1.00
    ///     }
    ///
    /// The response will prompt download of a `text/csv` file named `npv-results-{timestamp}.csv`.
    /// </remarks>
    /// <param name="request">The input parameters for the NPV calculation.</param>
    /// <returns>A downloadable CSV file containing DiscountRate and NPV columns.</returns>
    [HttpPost("calculate/csv")]
    public IActionResult CalculateCsv([FromBody] NpvRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var results = _service.Calculate(request).ToList();

        var csv = new StringBuilder();
        csv.AppendLine("DiscountRate,NPV");

        foreach (var result in results)
        {
            csv.AppendLine($"{result.DiscountRate:0.##},{result.Npv:0.##}");
        }

        var csvBytes = Encoding.UTF8.GetBytes(csv.ToString());
        var filename = $"npv-results-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(csvBytes, "text/csv", filename);
    }

    /// <summary>
    /// Returns a sample <see cref="NpvRequest"/> object with dummy data.
    /// </summary>
    /// <remarks>
    /// Useful for frontend developers and API consumers to understand the structure and data format expected by the NPV calculator.
    /// </remarks>
    /// <returns>A populated <see cref="NpvRequest"/> object with sample values.</returns>
    [HttpGet("sample-request")]
    public ActionResult<NpvRequest> GetSampleRequest()
    {
        return Ok(new NpvRequest
        {
            CashFlows = [-10000, 3000, 4200, 6800],
            LowerBoundRate = 1.0M,
            UpperBoundRate = 5.0M,
            Increment = 1.0M
        });
    }

    /// <summary>
    /// Returns valid parameter constraints and metadata for the NPV calculator.
    /// </summary>
    /// <remarks>
    /// Can be used by frontend applications to guide form validations or limit user inputs (e.g., discount rate range, max number of cash flows).
    /// </remarks>
    /// <returns>An object containing min/max discount rates and max allowable cash flows.</returns>
    [HttpGet("range-info")]
    public ActionResult<object> GetRangeInfo()
    {
        return Ok(new
        {
            MinRate = 0.0,
            MaxRate = 100.0,
            MaxCashFlows = 100
        });
    }


    /// <summary>
    /// Calculates NPV using a single discount rate from the input request.
    /// </summary>
    /// <remarks>
    /// This is a lightweight, single-rate version of the full NPV calculation. Useful for testing, previewing, or when only one rate needs to be evaluated (e.g., when <c>Increment</c> is 0).
    /// </remarks>
    /// <param name="request">The NPV input parameters including cash flows and a single discount rate.</param>
    /// <returns>A single <see cref="NpvResult"/> containing the discount rate and corresponding NPV.</returns>
    [HttpPost("preview")]
    public ActionResult<NpvResult> Preview([FromBody] NpvRequest request)
    {
        //if (!ModelState.IsValid) return BadRequest(ModelState); --> will be taken care of by FluentValidation

        decimal rate = Convert.ToDecimal(request.LowerBoundRate);
        decimal npv = 0m;

        for (int t = 0; t < request.CashFlows.Count; t++)
        {
            decimal cashFlow = Convert.ToDecimal(request.CashFlows[t]);
            decimal discountFactor = (decimal)Math.Pow(1 + (double)(rate / 100), t + 1);
            npv += cashFlow / discountFactor;
        }

        return Ok(new NpvResult
        {
            DiscountRate = request.LowerBoundRate,
            Npv = Math.Round(npv, 2)
        });
    }

    /// <summary>
    /// API health check endpoint.
    /// </summary>
    /// <remarks>
    /// Use this endpoint to verify if the API service is running and reachable. Can be integrated with CI/CD health probes or DevOps monitoring tools.
    /// </remarks>
    /// <returns>String response indicating operational status.</returns>
    [HttpGet("health")]
    public IActionResult Health() => Ok("API is running");
}