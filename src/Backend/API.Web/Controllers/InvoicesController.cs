using Application.Features.Invoices.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Core.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace API.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Customer")]
    public class InvoicesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IUnitOfWork _unitOfWork;

        public InvoicesController(IMediator mediator, IUnitOfWork unitOfWork)
        {
            _mediator = mediator;
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] Guid? customerId)
        {
            var query = new GetInvoicesQuery(customerId, User);
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpGet("{id}/pdf")]
        [Authorize]
        public async Task<IActionResult> DownloadPdf(Guid id)
        {
            var invoice = await _unitOfWork.InvoiceRepository.GetByIdAsync(id);
            if (invoice == null)
                return NotFound();

            // Customer rolü için sahiplik kontrolü
            if (User.IsInRole("Customer"))
            {
                var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(sub, out var appUserId)) return Forbid();
                var customers = await _unitOfWork.CustomerRepository.GetAllAsync();
                var me = customers.FirstOrDefault(c => c.ApplicationUserId == appUserId);
                if (me == null || invoice.CustomerId != me.Id) return Forbid();
            }

            // Basit PDF şablonu
            QuestPDF.Settings.License = LicenseType.Community;
            var bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(20);
                    page.Header().Text($"FATURA #{invoice.InvoiceNumber}").SemiBold().FontSize(20);
                    page.Content().Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Tarih: {invoice.InvoiceDate:dd.MM.yyyy}");
                        col.Item().Text($"Vade: {invoice.DueDate:dd.MM.yyyy}");
                        col.Item().Text($"Müşteri: {invoice.Customer?.FirstName} {invoice.Customer?.LastName}");
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(6);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });
                            table.Header(header =>
                            {
                                header.Cell().Text("Ürün").SemiBold();
                                header.Cell().Text("Miktar").SemiBold();
                                header.Cell().Text("Birim Fiyat").SemiBold();
                                header.Cell().Text("Toplam").SemiBold();
                            });
                            foreach (var l in invoice.InvoiceLines)
                            {
                                table.Cell().Text(l.Description);
                                table.Cell().Text(l.Quantity.ToString());
                                table.Cell().Text(l.UnitPrice.ToString("N2"));
                                table.Cell().Text(l.Total.ToString("N2"));
                            }
                        });
                        col.Item().AlignRight().Text($"Genel Toplam: {invoice.TotalAmount:N2}").SemiBold();
                    });
                    page.Footer().AlignCenter().Text("Bu belge otomatik oluşturulmuştur.").FontSize(9);
                });
            }).GeneratePdf();

            return File(bytes, "application/pdf", $"invoice-{invoice.InvoiceNumber}.pdf");
        }
    }
}


