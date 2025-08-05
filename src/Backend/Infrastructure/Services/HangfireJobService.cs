using Application.Interfaces;
using Hangfire;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class HangfireJobService : IBackgroundJobService
    {
        public string Enqueue(Expression<Func<Task>> methodCall)
        {
            return BackgroundJob.Enqueue(methodCall);
        }
    }
}
