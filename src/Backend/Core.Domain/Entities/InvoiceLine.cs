using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class InvoiceLine
    {
        public Guid Id { get; set; }

        public Guid InvoiceId { get; set; }
        [ForeignKey("InvoiceId")]
        public Invoice Invoice { get; set; } = null!;

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public string Description { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
