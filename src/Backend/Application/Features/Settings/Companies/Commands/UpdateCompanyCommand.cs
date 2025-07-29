using MediatR;

namespace Application.Features.Settings.Companies.Commands
{
    public class UpdateCompanyCommand : IRequest<Unit>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}
