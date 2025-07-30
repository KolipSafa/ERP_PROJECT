using Application.DTOs;
using Application.Validators.Settings.Companies;
using MediatR;
using System;

namespace Application.Features.Settings.Companies.Commands;

public record UpdateCompanyCommand : IRequest<CompanyDto>, ICompanyName
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? TaxNumber { get; set; }
    public string? Address { get; set; }
    public bool? IsActive { get; set; }
}