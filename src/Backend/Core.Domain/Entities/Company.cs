using System;
using System.Collections.Generic;

namespace Core.Domain.Entities
{
    /// <summary>
    /// Müşterilerin bağlı olduğu firmaları temsil eder.
    /// </summary>
    public class Company
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? TaxNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;

        // Bir firmanın birden çok müşterisi olabilir.
        public ICollection<Customer> Customers { get; set; } = new List<Customer>();
    }
}
