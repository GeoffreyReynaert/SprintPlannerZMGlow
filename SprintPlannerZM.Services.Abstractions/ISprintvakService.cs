using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ISprintvakService
    {
        Sprintvak Get(int id);
        IList<Sprintvak> Find();
        Sprintvak Create(Sprintvak sprintvak);
        Sprintvak Update(int id, Sprintvak sprintvak);
        bool Delete(int id);
    }
}
