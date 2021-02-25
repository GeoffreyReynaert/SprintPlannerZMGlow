using System;
using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IExamenroosterService
    {
        Examenrooster Get(int id);
        IList<Examenrooster> Find();
        IList<Examenrooster> FindByDatum(DateTime date);
        IList<Examenrooster> FindDistinct();
        Examenrooster Create(Examenrooster examenrooster);
        Examenrooster Update(int id, Examenrooster examenrooster);
        bool Delete(int id);
    }
}
