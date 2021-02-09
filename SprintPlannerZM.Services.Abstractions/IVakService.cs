using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IVakService
    {
        Vak Get(int id);
        IList<Vak> Find();
        Vak Create(Vak vak);
        Vak Update(int id, Vak vak);
        bool Delete(int id);
    }
}
