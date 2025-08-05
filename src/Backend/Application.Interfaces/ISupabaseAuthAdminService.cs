using Supabase.Gotrue;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISupabaseAuthAdminService
    {
        Task DeleteUser(string userId, CancellationToken cancellationToken = default);
        Task<User?> UpdateUserAppMetadata(string userId, Dictionary<string, object> metadata);
    }
}