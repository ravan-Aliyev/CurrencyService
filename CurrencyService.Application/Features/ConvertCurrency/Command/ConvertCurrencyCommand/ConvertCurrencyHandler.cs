using System;
using CurrencyService.Domain.Constants;
using CurrencyService.Domain.Exceptions;
using CurrencyService.Domain.Interfaces;
using Mapster;
using MapsterMapper;
using MediatR;

namespace CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;

public class ConvertCurrencyHandler : IRequestHandler<ConvertCurrencyRequest, ConvertCurrencyResponse>
{
    private readonly ICurrencyService _service;
    private readonly IMapper _mapper;

    public ConvertCurrencyHandler(ICurrencyService service, IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public async Task<ConvertCurrencyResponse> Handle(ConvertCurrencyRequest request, CancellationToken ct)
    {
        var result = await new ConvertCurrencyValidator().ValidateAsync(request);

        if (!result.IsValid)
            throw new CustomValidationException(result.Errors);

        var convertedAmount = await _service.ConvertAsync(request.From, request.To, request.Amount);

        var response = _mapper.Map<ConvertCurrencyResponse>(convertedAmount);

        return response;
    }
}

