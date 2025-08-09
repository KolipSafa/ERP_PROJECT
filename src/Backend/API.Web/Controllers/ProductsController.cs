using Application.Features.Products.Commands;
using Application.Features.Products.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllProductsQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetProductByIdQuery { Id = id };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"ID'si {id} olan ürün bulunamadı.");
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var productDto = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = productDto.Id }, productDto);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
        {
            command.Id = id;
            
            var productDto = await _mediator.Send(command);
            return Ok(productDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _mediator.Send(new DeleteProductCommand(id));
            return NoContent();
        }

        [HttpDelete("hard/{id}")]
        public async Task<IActionResult> HardDelete(int id)
        {
            await _mediator.Send(new HardDeleteProductCommand { Id = id });
            return NoContent();
        }
    }
}
