using Microsoft.EntityFrameworkCore;
using Reziox.DataAccess;
using System.Linq.Expressions;

namespace Rezioxgithub.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _db;
        private readonly DbSet<T> _dbset;
        public Repository(AppDbContext db) 
        {
            _db = db;
            _dbset=_db.Set<T>();
        }
        public void Add(T entity)
        {
            _dbset.Add(entity);
        }

        public T Get(Expression<Func<T, bool>> function)
        {
            
           return _dbset.FirstOrDefault(function);
        }

        public IEnumerable<T> GetAll(string? includeProperties = null)
        {
            return _dbset.ToList();
        }

        public void Remove(T entity)
        {
           _dbset.Remove(entity);
        }

        public void RemoveRange(IEnumerable<T> entity)
        {
            _dbset.RemoveRange(entity);
        }
    }
}
