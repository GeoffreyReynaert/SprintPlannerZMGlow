using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IBeheerderService
    {
        Beheerder Get(int id);
        IList<Beheerder> Find();
        Beheerder Create(Beheerder beheerder);
        Beheerder Update(int id, Beheerder beheerder);
        bool Delete(int id);
    }
}
