using System;

namespace CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;

public class ConvertCurrencyResponse
{
    public string FromCurrency { get; set; }
    public string ToCurrency { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal ConvertedAmount { get; set; }
    public decimal ConversionRate { get; set; }
    
}
