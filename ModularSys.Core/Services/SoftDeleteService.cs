using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Core.Services;

public interface ISoftDeleteService
{
    Task SoftDeleteAsync<T>(T entity, string? deletedBy = null) where T : class, ISoftDeletable;
    Task SoftDeleteAsync<T>(int id, DbContext context, string? deletedBy = null) where T : class, ISoftDeletable;
    Task RestoreAsync<T>(T entity) where T : class, ISoftDeletable;
    IQueryable<T> IncludeDeleted<T>(IQueryable<T> query) where T : class, ISoftDeletable;
    IQueryable<T> OnlyDeleted<T>(IQueryable<T> query) where T : class, ISoftDeletable;
}

public class SoftDeleteService : ISoftDeleteService
{
    public Task SoftDeleteAsync<T>(T entity, string? deletedBy = null) where T : class, ISoftDeletable
    {
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = deletedBy;
        return Task.CompletedTask;
    }

    public async Task SoftDeleteAsync<T>(int id, DbContext context, string? deletedBy = null) where T : class, ISoftDeletable
    {
        var entity = await context.Set<T>().FindAsync(id);
        if (entity != null)
        {
            await SoftDeleteAsync(entity, deletedBy);
        }
    }

    public Task RestoreAsync<T>(T entity) where T : class, ISoftDeletable
    {
        entity.IsDeleted = false;
        entity.DeletedAt = null;
        entity.DeletedBy = null;
        return Task.CompletedTask;
    }

    public IQueryable<T> IncludeDeleted<T>(IQueryable<T> query) where T : class, ISoftDeletable
    {
        return query.IgnoreQueryFilters();
    }

    public IQueryable<T> OnlyDeleted<T>(IQueryable<T> query) where T : class, ISoftDeletable
    {
        return query.IgnoreQueryFilters().Where(x => x.IsDeleted);
    }
}
