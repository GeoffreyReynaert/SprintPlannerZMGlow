using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IExamenroosterService
    {
        Examenrooster Get(int id);
        IList<Examenrooster> Find();
        Examenrooster Create(Examenrooster examenrooster);
        Examenrooster Update(int id, Examenrooster examenrooster);
        bool Delete(int id);
    }
}
