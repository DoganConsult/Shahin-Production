using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GrcMvc.Data.Repositories
{
    /// <summary>
    /// Generic repository interface following Unit of Work pattern.
    /// IMPORTANT: Add/Update/Delete operations do NOT auto-save.
    /// Use IUnitOfWork.SaveChangesAsync() to persist all changes.
    /// </summary>
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(Guid id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Adds an entity to the change tracker. Does NOT auto-save.
        /// </summary>
        Task<T> AddAsync(T entity);

        /// <summary>
        /// Adds multiple entities to the change tracker. Does NOT auto-save.
        /// </summary>
        Task AddRangeAsync(IEnumerable<T> entities);

        /// <summary>
        /// Marks an entity as modified. Does NOT auto-save.
        /// </summary>
        Task UpdateAsync(T entity);

        /// <summary>
        /// Marks an entity for deletion. Does NOT auto-save.
        /// </summary>
        Task DeleteAsync(T entity);

        /// <summary>
        /// Marks multiple entities for deletion. Does NOT auto-save.
        /// </summary>
        Task DeleteRangeAsync(IEnumerable<T> entities);

        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> Query();

        /// <summary>
        /// Returns a queryable with no tracking for read-only operations.
        /// </summary>
        IQueryable<T> QueryNoTracking();
    }
}