using MediatR;

namespace Application.Features.Products.Commands
{
    public class HardDeleteProductCommand : IRequest<Unit>
    {
        public int Id { get; set; }
    }
}