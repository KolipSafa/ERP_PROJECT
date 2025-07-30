using Application.DTOs;
using MediatR;

namespace Application.Features.Settings.Companies.Queries;

public record GetCompaniesQuery : IRequest<IEnumerable<CompanyDto>>;