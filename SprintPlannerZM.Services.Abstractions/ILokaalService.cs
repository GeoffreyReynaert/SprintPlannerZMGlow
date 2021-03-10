using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILokaalService
    {
        Task<Lokaal> GetAsync(int id);
        Task<Lokaal> GetByNameAsync(string lokaalnaam);
        Task<IList<Lokaal>> FindAsync();
        IQueryable<Lokaal> FindAsyncPagingQueryable();
        Task<IList<Lokaal>> FindForSprintAsync();
        Task<IList<Lokaal>> FindForTyperAsync();
        Task<Lokaal> CreateAsync(Lokaal lokaal);
        Task<Lokaal> UpdateAsync(int id, Lokaal lokaal);
        Task<bool> Delete(int id);
    }
}
