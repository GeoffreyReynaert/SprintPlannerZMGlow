using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface ILeerkrachtService

    {
    Leerkracht Get(int id);
    IList<Leerkracht> Find();
    Leerkracht Create(Leerkracht leerkracht);
    Leerkracht Update(int id, Leerkracht leerkracht);
    bool Delete(int id);
    }
}
