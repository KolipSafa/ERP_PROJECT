namespace Application.DTOs
{
    public class CompanyDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}
