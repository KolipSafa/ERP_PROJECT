using Application.Features.Teklifler.Commands;
using Application.Features.Teklifler.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TekliflerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TekliflerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = new GetAllTekliflerQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetTeklifByIdQuery(id);
            var result = await _mediator.Send(query);
            return result != null ? Ok(result) : NotFound($"ID'si {id} olan teklif bulunamadı.");
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTeklifCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeklifCommand command)
        {
            if (command.Id != Guid.Empty && command.Id != id)
            {
                return BadRequest("URL'deki ID ile gövdedeki ID uyuşmuyor.");
            }
            command.Id = id; // URL'den gelen ID'yi komuta atayarak tutarlılığı sağlıyoruz.
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteTeklifCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
