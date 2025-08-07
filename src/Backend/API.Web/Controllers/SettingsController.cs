using Application.Features.Settings.Companies.Commands;
using Application.Features.Settings.Companies.Queries;
using Application.Features.Settings.Currencies.Commands;
using Application.Features.Settings.Currencies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

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
        public async Task<IActionResult> UpdateCurrency(int id, [FromBody] UpdateCurrencyCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("URL'deki ID ile komuttaki ID uyuşmuyor.");
            }
            var updatedCurrency = await _mediator.Send(command);
            return Ok(updatedCurrency);
        }

        [HttpDelete("currencies/{id}")]
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            await _mediator.Send(new DeleteCurrencyCommand(id));
            return NoContent();
        }

        // --- Firma (Company) Endpoint'leri ---

        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            var companies = await _mediator.Send(new GetCompaniesQuery());
            return Ok(companies);
        }

        [HttpPost("companies")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyCommand command)
        {
            var createdCompany = await _mediator.Send(command);
            // Şimdilik GetCompanies'e yönlendiriyoruz, GetCompanyById implemente edilince o kullanılabilir.
            return CreatedAtAction(nameof(GetCompanies), new { id = createdCompany.Id }, createdCompany);
        }

        [HttpPut("companies/{id}")]
        public async Task<IActionResult> UpdateCompany(Guid id, [FromBody] UpdateCompanyCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("URL'deki ID ile komuttaki ID uyuşmuyor.");
            }
            var updatedCompany = await _mediator.Send(command);
            return Ok(updatedCompany);
        }

        [HttpDelete("companies/{id}")]
        public async Task<IActionResult> DeleteCompany(Guid id)
        {
            await _mediator.Send(new DeleteCompanyCommand(id));
            return NoContent();
        }
    }
}
