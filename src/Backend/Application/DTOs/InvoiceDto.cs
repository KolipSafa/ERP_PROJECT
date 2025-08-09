using System;

namespace Application.DTOs
{
    public class InvoiceDto
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public Guid TeklifId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public int Status { get; set; }
    }
}


