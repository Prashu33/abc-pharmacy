using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmacyTracker.Api.Repositories
{
    public interface IJsonRepository<T> where T : class
    {
        Task<List<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Func<T, bool> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(Func<T, bool> predicate, Action<T> updateAction);
        Task SaveAllAsync(List<T> items);
    }
}
