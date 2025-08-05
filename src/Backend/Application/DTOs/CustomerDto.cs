// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\Application\DTOs\CustomerDto.cs

namespace Application.DTOs
{
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public Guid CompanyId { get; set; }
        public string? CompanyName { get; set; } // Şirket adını DTO'da göstermek kullanışlıdır.
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public decimal Balance { get; set; }
        public bool IsActive { get; set; }
        public bool IsAccountActive { get; set; } // Eklendi
    }
}
