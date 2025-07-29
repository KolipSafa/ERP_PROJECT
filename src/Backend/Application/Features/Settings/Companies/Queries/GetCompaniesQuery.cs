using Application.DTOs;
using MediatR;

namespace Application.Features.Settings.Companies.Queries
{
    public class GetCompaniesQuery : IRequest<IEnumerable<CompanyDto>>
    {
        public bool? IsActive { get; set; } // Opsiyonel filtre
        public string? SearchTerm { get; set; } // Arama kelimesi
    }
}
