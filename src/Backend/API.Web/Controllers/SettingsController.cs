using Application.Features.Settings.Currencies.Commands;
using Application.Features.Settings.Currencies.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // --- Para Birimi (Currency) Endpoint'leri ---

        [HttpGet("currencies")]
        public async Task<IActionResult> GetCurrencies()
        {
            var currencies = await _mediator.Send(new GetCurrenciesQuery());
            return Ok(currencies);
        }

        [HttpPost("currencies")]
        public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyCommand command)
        {
            var createdCurrency = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetCurrencies), new { id = createdCurrency.Id }, createdCurrency);
        }

        [HttpPut("currencies/{id}")]
        public async Task<IActionResult> UpdateCurrency(int id, [FromBody] object currencyDto)
        {
            // TODO: UpdateCurrencyCommand'ı implement et
            await Task.CompletedTask;
            return Ok($"Para birimi {id} güncellenecek.");
        }

        [HttpDelete("currencies/{id}")]
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            // TODO: DeleteCurrencyCommand'ı implement et
            await Task.CompletedTask;
            return Ok($"Para birimi {id} silinecek (soft delete).");
        }


        // --- Firma (Company) Endpoint'leri ---

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            // TODO: GetCompaniesQuery'i implement et
            await Task.CompletedTask;
            return Ok("Firmalar listelenecek.");
        }

        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompany([FromBody] object companyDto)
        {
            // TODO: CreateCompanyCommand'ı implement et
            await Task.CompletedTask;
            return Ok("Firma oluşturulacak.");
        }

        [HttpPut("companies/{id}")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] object companyDto)
        {
            // TODO: UpdateCompanyCommand'ı implement et
            await Task.CompletedTask;
            return Ok($"Firma {id} güncellenecek.");
        }

        [HttpDelete("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            // TODO: DeleteCompanyCommand'ı implement et
            await Task.CompletedTask;
            return Ok($"Firma {id} silinecek (soft delete).");
        }
    }
}
