using System.Linq.Expressions;
using System.Reflection;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : BaseEntity
{
    protected DefaultContext Context { get; }
    protected DbSet<TEntity> DbSet { get; }

    protected BaseRepository(DefaultContext context)
    {
        Context = context;
        DbSet = context.Set<TEntity>();
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public virtual async Task<TEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAsync(
        IEnumerable<Expression<Func<TEntity, bool>>> filters,
        string? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default,
        params string[] includes)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (includes is { Length: > 0 })
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filters != null)
        {
            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }
        }

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = ApplyOrderBy(query, orderBy, ascending);
        }

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<(IEnumerable<TEntity> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        IEnumerable<Expression<Func<TEntity, bool>>>? filters = null,
        string? orderBy = null,
        bool ascending = true,
        CancellationToken cancellationToken = default,
        params string[] includes)
    {
        IQueryable<TEntity> query = DbSet.AsNoTracking();

        if (includes is { Length: > 0 })
        {
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
        }

        if (filters != null)
        {
            foreach (var filter in filters)
            {
                query = query.Where(filter);
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(orderBy))
        {
            query = ApplyOrderBy(query, orderBy, ascending);
        }

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        var result = await DbSet.AddAsync(entity, cancellationToken);
        return result.Entity;
    }

    public virtual Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        DbSet.AddRangeAsync(entities, cancellationToken);
        return Task.CompletedTask;
    }

    public virtual Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        Context.Entry(entity).State = EntityState.Modified;
        return Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity is not null)
        {
            await DeleteAsync(entity, cancellationToken);
        }
    }

    public virtual Task DeleteRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        foreach (var entity in entities)
        {
            entity.DeletedAt = now;
            entity.UpdatedAt = now;
            Context.Entry(entity).State = EntityState.Modified;
        }
        return Task.CompletedTask;
    }

    public virtual Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<TEntity> ApplyOrderBy(IQueryable<TEntity> query, string orderBy, bool ascending)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = typeof(TEntity).GetProperty(orderBy, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        if (property is null)
        {
            return query;
        }

        var body = Expression.Property(parameter, property);
        var lambda = Expression.Lambda(body, parameter);

        var methodName = ascending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending);
        var result = typeof(Queryable)
            .GetMethods()
            .Single(m => m.Name == methodName && m.GetParameters().Length == 2)
            .MakeGenericMethod(typeof(TEntity), property.PropertyType)
            .Invoke(null, new object[] { query, lambda });

        return (IQueryable<TEntity>)result!;
    }
}
