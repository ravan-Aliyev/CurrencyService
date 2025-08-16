using System;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using MapsterMapper;
using MediatR;

namespace CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;

public class GetHistoricalCurrencyQueryHandler : IRequestHandler<GetHistoricalCurrencyQueryRequest, GetHistoricalCurrencyQueryResponse>
{
    readonly ICurrencyService _currencyService;
    readonly IMapper _mapper;
    public GetHistoricalCurrencyQueryHandler(ICurrencyService currencyService, IMapper mapper)
    {
        _mapper = mapper;
        _currencyService = currencyService;
    }

    public async Task<GetHistoricalCurrencyQueryResponse> Handle(GetHistoricalCurrencyQueryRequest request, CancellationToken cancellationToken)
    {
        var result = await new GetHistoricalCurrencyValidation().ValidateAsync(request, cancellationToken);

        if (!result.IsValid)
            throw new CustomValidationException(result.Errors);

        var historicalData = await _currencyService.GetHistoricalRatesAsync(request.BaseCurrency, request.StartDate, request.EndDate);

        if (historicalData == null || historicalData.Rates.Count == 0)
            throw new UnsupportedCurrencyException(request.BaseCurrency);

        var response = _mapper.Map<GetHistoricalCurrencyQueryResponse>(historicalData);
        response.TotalCount = historicalData.Rates.Count;
        response.Page = request.Page;
        response.PageSize = request.PageSize;

        if (response.TotalCount > 0)
        {
            response.Items = [.. response.Items
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)];
        }

        return response;
    }
}
