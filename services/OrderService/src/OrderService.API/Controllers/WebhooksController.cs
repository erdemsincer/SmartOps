using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrderService.Application.Incoming;
using OrderService.Application.UseCases;

namespace OrderService.API.Controllers;

[ApiController]
[Route("orders/inbound")]
public sealed class WebhooksController : ControllerBase
{
    private readonly ICreateOrderFromTrendyol _useCase;
    private readonly IValidator<TrendyolWebhookDto> _validator;

    public WebhooksController(ICreateOrderFromTrendyol useCase, IValidator<TrendyolWebhookDto> validator)
    {
        _useCase = useCase;
        _validator = validator;
    }

    [HttpPost("trendyol")]
    public async Task<IActionResult> TrendyolAsync([FromBody] TrendyolWebhookDto dto, CancellationToken ct)
    {
        var vr = await _validator.ValidateAsync(dto, ct);
        if (!vr.IsValid)
            return ValidationProblem(new ValidationProblemDetails(vr.ToDictionary()));

        var id = await _useCase.HandleAsync(dto, ct);
        if (id == Guid.Empty)
            return Ok(new { message = "duplicate_ignored" });

        return Ok(new { orderId = id });
    }
}
