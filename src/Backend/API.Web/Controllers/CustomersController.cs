// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\API.Web\Controllers\CustomersController.cs

using Application.Features.Customers.Commands;
using Application.Features.Customers.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] bool includeInactive = false,
            [FromQuery] string? searchTerm = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] bool isDescending = false)
        {
            var query = new GetAllCustomersQuery(includeInactive, searchTerm, sortBy, isDescending);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetCustomerByIdQuery(id);
            var result = await _mediator.Send(query);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerCommand command)
        {
            // Komutun ID'sini her zaman URL'den gelen ID ile ayarla.
            // Bu, PATCH isteğinde body'de ID olmasa bile komutun doğru ID'ye sahip olmasını sağlar.
            if (command.Id != Guid.Empty && command.Id != id)
            {
                return BadRequest("ID mismatch");
            }
            command.Id = id;
            
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteCustomerCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
