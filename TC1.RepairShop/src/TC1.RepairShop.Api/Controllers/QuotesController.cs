using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TC1.RepairShop.Application.Quotes.UseCases;

namespace TC1.RepairShop.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/quotes")]
public class QuotesController : ControllerBase
{
    private readonly ApproveQuoteUseCase _approve;
    private readonly RejectQuoteUseCase _reject;

    public QuotesController(ApproveQuoteUseCase approve, RejectQuoteUseCase reject)
    {
        _approve = approve;
        _reject = reject;
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        try
        {
            await _approve.ExecuteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        try
        {
            await _reject.ExecuteAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
