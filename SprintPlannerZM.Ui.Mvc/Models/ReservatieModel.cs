using System.Collections.Generic;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Ui.Mvc.Models
{
    public class ReservatieModel
    {
        public Sprintlokaalreservatie reservatie { get; set; }
        public IList<Leerkracht> Toezichters { get; set; }

    }
}
