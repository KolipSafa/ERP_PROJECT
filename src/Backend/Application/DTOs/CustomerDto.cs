// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Application\DTOs\CustomerDto.cs

namespace Application.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}"; // Frontend'in işini kolaylaştıran birleşik bir özellik.
        public string? CompanyName { get; set; }
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
    }
}
