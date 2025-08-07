namespace Core.Domain.Enums
{
    public enum InvoiceStatus
    {
        Draft,      // Taslak
        Sent,       // Gönderildi
        Paid,       // Ödendi
        Overdue     // Vadesi Geçti
    }
}
