using MediatR;

namespace Application.Features.Settings.Companies.Commands
{
    public class DeleteCompanyCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
    }
}
