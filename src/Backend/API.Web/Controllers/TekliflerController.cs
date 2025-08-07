using Application.Features.Teklifler.Commands;
using Application.Features.Teklifler.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
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
        
        private Guid CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                {
                    throw new UnauthorizedAccessException("User ID could not be found in the token.");
                }
                return userId;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllTekliflerQuery query)
        {
            if (User.IsInRole("Customer"))
            {
                query.MusteriId = CurrentUserId;
            }
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetTeklifByIdQuery(id);
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound($"ID'si {id} olan teklif bulunamadı.");
            }

            // If the user is a customer, ensure they can only access their own quotes.
            if (User.IsInRole("Customer") && result.MusteriId != CurrentUserId)
            {
                return Forbid();
            }
            
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTeklifCommand command)
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateTeklifCommand command)
        {
            if (command.Id != Guid.Empty && command.Id != id)
            {
                return BadRequest("URL'deki ID ile gövdedeki ID uyuşmuyor.");
            }
            command.Id = id;
            
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

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var command = new RestoreTeklifCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }
        
        // --- Müşteri Aksiyonları ---

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> Approve(Guid id)
        {
            var command = new ApproveTeklifCommand(id, CurrentUserId);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(Guid id)
        {
            var command = new RejectTeklifCommand(id, CurrentUserId);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/request-change")]
        public async Task<IActionResult> RequestChange(Guid id, [FromBody] TeklifChangeRequestDto changeRequest)
        {
            var command = new RequestTeklifChangeCommand(id, CurrentUserId, changeRequest);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
