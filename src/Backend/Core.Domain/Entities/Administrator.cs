using System;

namespace Core.Domain.Entities
{
    /// <summary>
    /// Represents an internal administrator user of the system.
    /// </summary>
    public class Administrator
    {
        /// <summary>
        /// The unique identifier for the administrator.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// The first name of the administrator.
        /// </summary>
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The last name of the administrator.
        /// </summary>
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// The email address of the administrator, used for login and communication.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The corresponding user ID from the Supabase Authentication service.
        /// This links the database record to the authentication user.
        /// </summary>
        public Guid ApplicationUserId { get; set; }
    }
}
