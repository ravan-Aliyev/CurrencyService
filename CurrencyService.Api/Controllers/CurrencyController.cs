using CurrencyService.Application.Features.ConvertCurrency.Command.ConvertCurrencyCommand;
using CurrencyService.Application.Features.ConvertCurrency.Query.GetHistoricalCurrencyQuery;
using CurrencyService.Application.Features.ConvertCurrency.Query.GetLatestCurrencyQuery;
using CurrencyService.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyService.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurrencyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("latest-rates/{baseCurrency}")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<ExchangeRate>> GetLatestRates(string baseCurrency)
        {
            var request = new GetLatestCurrencyQueryRequest { BaseCurrency = baseCurrency };
            var result = await _mediator.Send(request);

            return Ok(result);
        }

        [HttpPost("convert")]
        [Authorize(Roles = "User,Admin")]
        public async Task<ActionResult<ConvertCurrencyResponse>> ConvertCurrency([FromBody] ConvertCurrencyRequest request)
        {
            var result = await _mediator.Send(request);

            return Ok(result);
        }

        [HttpGet("historical-rates")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<GetHistoricalCurrencyQueryResponse>> GetHistoricalRates(
            [FromQuery] GetHistoricalCurrencyQueryRequest request)
        {
            var result = await _mediator.Send(request);

            return Ok(result);
        }
    }
}
