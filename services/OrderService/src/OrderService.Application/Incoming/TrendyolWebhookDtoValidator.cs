using FluentValidation;

namespace OrderService.Application.Incoming;

public sealed class TrendyolWebhookDtoValidator : AbstractValidator<TrendyolWebhookDto>
{
    public TrendyolWebhookDtoValidator()
    {
        RuleFor(x => x.ChannelOrderId).NotEmpty().MaximumLength(64);
        RuleFor(x => x.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleForEach(x => x.Items).SetValidator(new TrendyolItemDtoValidator());
    }
}

public sealed class TrendyolItemDtoValidator : AbstractValidator<TrendyolItemDto>
{
    public TrendyolItemDtoValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
    }
}
