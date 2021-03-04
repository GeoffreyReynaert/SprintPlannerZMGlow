using System;
using SprintPlannerZM.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IExamenroosterService
    {
        Task<Examenrooster> Get(int id);
        Task<IList<Examenrooster>> Find();
        Task<IList<Examenrooster>> FindByDatum(DateTime date);
        Task<IList<Examenrooster>> FindDistinct();
        Task<Examenrooster> Create(Examenrooster examenrooster);
        Task<Examenrooster> Update(int id, Examenrooster examenrooster);
        Task<bool> Delete(int id);
    }
}
