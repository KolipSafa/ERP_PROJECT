using Application.DTOs;
using Supabase.Gotrue; // Supabase User nesnesi için

namespace Application.DTOs
{
    public class ActivationResponseDto
    {
        public CustomerDto? Customer { get; set; }
        public User? SupabaseUser { get; set; }
    }
}
