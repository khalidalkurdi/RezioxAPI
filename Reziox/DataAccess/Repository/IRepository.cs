using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace Rezioxgithub.DataAccess.Repository
{
    public interface IRepository <T> where T : class
    {
        IEnumerable<T> GetAll(string? includeProperties = null);
        T Get(Expression<Func<T, bool>> function);
        void Add(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entity);
    }
}
