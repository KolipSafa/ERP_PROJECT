using Application.Features.Teklifler.Commands;
using Application.Features.Teklifler.Queries;
using Application.DTOs;
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
                // sub yoksa NameIdentifier veya user_id claim'lerini de dene
                var userIdClaim = User.FindFirstValue("sub")
                                   ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                                   ?? User.FindFirstValue("user_id");
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
                query.ApplicationUserId = CurrentUserId;
            }
            
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var isCustomer = User.IsInRole("Customer");
            Guid? currentUserId = null;
            if (isCustomer)
            {
                var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (Guid.TryParse(sub, out var parsed)) currentUserId = parsed;
            }

            var query = new GetTeklifByIdQuery(id, currentUserId, isCustomer);
            TeklifDto? result;
            try
            {
                result = await _mediator.Send(query);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            
            if (result == null)
            {
                return NotFound($"ID'si {id} olan teklif bulunamadı.");
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

        [HttpDelete("hard/{id}")]
        public async Task<IActionResult> HardDelete(Guid id)
        {
            await _mediator.Send(new HardDeleteTeklifCommand(id));
            return NoContent();
        }

        [HttpPost("{id}/restore")]
        public async Task<IActionResult> Restore(Guid id)
        {
            var command = new RestoreTeklifCommand(id);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/resend")]
        public async Task<IActionResult> Resend(Guid id)
        {
            await _mediator.Send(new ResendTeklifCommand(id));
            return NoContent();
        }
        
        // --- Müşteri Aksiyonları ---

        [HttpPost("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> Approve(Guid id)
        {
            var command = new ApproveTeklifCommand(id, CurrentUserId);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/reject")]
        [Authorize]
        public async Task<IActionResult> Reject(Guid id)
        {
            var command = new RejectTeklifCommand(id, CurrentUserId);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPost("{id}/request-change")]
        [Authorize]
        public async Task<IActionResult> RequestChange(Guid id, [FromBody] TeklifChangeRequestDto changeRequest)
        {
            var command = new RequestTeklifChangeCommand(id, CurrentUserId, changeRequest);
            await _mediator.Send(command);
            return NoContent();
        }
    }
}
