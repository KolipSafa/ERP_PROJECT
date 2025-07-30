using Application.DTOs;
using Application.Validators.Settings.Companies;
using MediatR;

namespace Application.Features.Settings.Companies.Commands;

public record CreateCompanyCommand(
    string Name,
    string? TaxNumber,
    string? Address) : IRequest<CompanyDto>, ICompanyName;