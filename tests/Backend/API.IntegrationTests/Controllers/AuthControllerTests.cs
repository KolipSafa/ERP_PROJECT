using API.Web;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Application.Features.Auth.Commands;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Text.Json;
using System.Text;

namespace API.IntegrationTests.Controllers
{
    public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory<Program> _factory;

        public AuthControllerTests(CustomWebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact(Skip = "Bu test, WebApplicationFactory'deki bir sorun nedeniyle geçici olarak devre dışı bırakıldı.")]
        public async Task SetPassword_WithValidUserAndToken_ReturnsOkAndSetsPassword()
        {
            // Arrange: Test için gerekli servisleri ve veriyi hazırla
            using var scope = _factory.Services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            
            // Veritabanının oluşturulduğundan emin ol
            await dbContext.Database.EnsureCreatedAsync();

            // 1. Test kullanıcısını oluştur
            var user = new ApplicationUser { UserName = "testuser@example.com", Email = "testuser@example.com", EmailConfirmed = true };
            var createUserResult = await userManager.CreateAsync(user);
            Assert.True(createUserResult.Succeeded, "Test kullanıcısı oluşturulamadı.");

            // 2. Kullanıcı için şifre belirleme token'ı oluştur
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            Assert.NotNull(token);

            // 3. API'ye gönderilecek komutu oluştur
            var command = new SetPasswordForCustomerCommand
            {
                UserId = user.Id.ToString(),
                Token = token,
                NewPassword = "NewStrongPassword123!"
            };

            // Act: API endpoint'ini çağır
            var content = new StringContent(JsonSerializer.Serialize(command), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/set-password", content);

            // Assert: Sonuçları doğrula
            // 1. HTTP yanıtının başarılı olduğunu kontrol et
            response.EnsureSuccessStatusCode(); // Status Code 200-299
            var responseString = await response.Content.ReadAsStringAsync();
            Assert.Contains("Password set successfully", responseString);

            // 2. Veritabanında şifrenin gerçekten ayarlandığını doğrula
            var updatedUser = await dbContext.Users.FindAsync(user.Id);
            Assert.NotNull(updatedUser.PasswordHash);

            // 3. Kullanıcının yeni şifreyle giriş yapabildiğini doğrula (opsiyonel ama daha güçlü bir test)
            var canSignIn = await userManager.CheckPasswordAsync(updatedUser, command.NewPassword);
            Assert.True(canSignIn, "Kullanıcının şifresi doğru şekilde ayarlanmamış.");
        }
    }
}