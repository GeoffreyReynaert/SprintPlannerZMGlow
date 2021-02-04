using System.Collections.Generic;
using Microsoft.AspNetCore.Routing.Constraints;

namespace SprintPlannerZM.Model
{
    public class Vak
    {
        public int VakID { get; set; }
        public string vaknaam { get; set; }
        public int klasID { get; set; }
        public long leerkrachtID { get; set; }
        public Klas Klas { get; set; }
        public Leerkracht Leerkracht { get; set; }
    }
}
