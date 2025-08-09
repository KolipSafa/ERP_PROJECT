// D:\yazilim_projelerim\ERP_PROJECT\src\Backend\API.Web\Configuration\JwtSettings.cs
namespace API.Web.Configuration
{
    public class JwtSettings
    {
        public const string SectionName = "Jwt";
        public string Authority { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        // Supabase projelerinde çoğunlukla HS256 imzası kullanılır. Bu durumda JWT Secret burada tutulur.
        // Eğer RS256 (JWKS) kullanıyorsanız bu alanı boş bırakabilirsiniz.
        public string SigningKey { get; set; } = string.Empty;
    }
}