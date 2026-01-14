using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace GrcMvc.Data.Repositories
{
    /// <summary>
    /// Generic repository implementation following proper Unit of Work pattern.
    /// IMPORTANT: This repository does NOT auto-save changes. Use IUnitOfWork.SaveChangesAsync()
    /// to persist all changes in a single transaction.
    /// </summary>
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        protected readonly GrcDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public GenericRepository(GrcDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = context.Set<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Adds an entity to the change tracker. Call IUnitOfWork.SaveChangesAsync() to persist.
        /// </summary>
        public virtual Task<T> AddAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Add(entity);
            return Task.FromResult(entity);
        }

        /// <summary>
        /// Adds multiple entities to the change tracker. Call IUnitOfWork.SaveChangesAsync() to persist.
        /// </summary>
        public virtual Task AddRangeAsync(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            _dbSet.AddRange(entities);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Marks an entity as modified. Call IUnitOfWork.SaveChangesAsync() to persist.
        /// </summary>
        public virtual Task UpdateAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Update(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Marks an entity for deletion. Call IUnitOfWork.SaveChangesAsync() to persist.
        /// </summary>
        public virtual Task DeleteAsync(T entity)
        {
            ArgumentNullException.ThrowIfNull(entity);
            _dbSet.Remove(entity);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Marks multiple entities for deletion. Call IUnitOfWork.SaveChangesAsync() to persist.
        /// </summary>
        public virtual Task DeleteRangeAsync(IEnumerable<T> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);
            _dbSet.RemoveRange(entities);
            return Task.CompletedTask;
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
        {
            return predicate == null
                ? await _dbSet.CountAsync()
                : await _dbSet.CountAsync(predicate);
        }

        public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public virtual IQueryable<T> Query()
        {
            return _dbSet.AsQueryable();
        }

        /// <summary>
        /// Returns a queryable with no tracking for read-only operations.
        /// </summary>
        public virtual IQueryable<T> QueryNoTracking()
        {
            return _dbSet.AsNoTracking();
        }
    }
}