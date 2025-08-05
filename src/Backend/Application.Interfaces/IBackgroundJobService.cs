using System;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBackgroundJobService
    {
        string Enqueue(Expression<Func<Task>> methodCall);
    }
}
