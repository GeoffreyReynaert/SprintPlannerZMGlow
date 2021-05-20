using System.Collections.Generic;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Ui.Mvc.Models
{
    public class LeerlingVerdelingWijzegingsModel
    {
        public Leerlingverdeling leerlingeverdeling { get; set; }
        public IList<Sprintlokaalreservatie> sprintlokaalreservaties { get; set; }

    }
}


