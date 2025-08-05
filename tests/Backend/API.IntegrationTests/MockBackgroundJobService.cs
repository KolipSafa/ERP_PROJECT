using Application.Interfaces;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace API.IntegrationTests
{
    public class MockBackgroundJobService : IBackgroundJobService
    {
        public string Enqueue(Expression<Func<Task>> methodCall)
        {
            // Hiçbir şey yapma, sadece bir iş ID'si döndür
            return "mock-job-id";
        }
    }
}
