using Application.DTOs;
using MediatR;

namespace Application.Features.Settings.Companies.Commands
{
    public class CreateCompanyCommand : IRequest<CompanyDto>
    {
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
    }
}
