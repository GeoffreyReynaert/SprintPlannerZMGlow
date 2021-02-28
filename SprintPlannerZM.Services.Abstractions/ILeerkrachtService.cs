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
        Leerkracht Get(long id);
        IList<Leerkracht> Find();
        Task<IQueryable<Leerkracht>> FindAsyncPagingQueryable();
        Leerkracht Create(Leerkracht leerkracht);
        Leerkracht Update(long id, Leerkracht leerkracht);
        bool Delete(long id);
    }
}
