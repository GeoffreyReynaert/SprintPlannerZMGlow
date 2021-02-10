using SprintPlannerZM.Model;
using System.Collections.Generic;

namespace SprintPlannerZM.Services.Abstractions
{
    public interface IHulpleerlingService
    {
        Hulpleerling Get(int id);
        Hulpleerling GetbyLeerlingId(long id);
        IList<Hulpleerling> Find();
        Hulpleerling Create(Hulpleerling hulpleerling);
        Hulpleerling Update(int id, Hulpleerling hulpleerling);
        bool Delete(int id);
    }
}
