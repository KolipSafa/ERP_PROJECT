using MediatR;
using System;

namespace Application.Features.Settings.Companies.Commands;

public record DeleteCompanyCommand(Guid Id) : IRequest;