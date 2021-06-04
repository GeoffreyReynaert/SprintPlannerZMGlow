using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerkrachtService

    {
        Task<Leerkracht> Get(long id);
        Task<Leerkracht> GetBasic(long id);
        Task<IList<Leerkracht>> Find();
        Task<IList<Leerkracht>> FindBasic();
        Task<IList<Leerkracht>> FindToezichters();
        Task<IList<Leerkracht>> FindOverzicht();
        Task<IQueryable<Leerkracht>> FindAsyncPagingQueryable();
        Task<Leerkracht> Create(Leerkracht leerkracht);
        Task<Leerkracht> Update(long id, Leerkracht leerkracht);
        Task<bool> Delete(long id);
    }
}
