using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SprintPlannerZM.Model
{
    public class Leerling
    {
        public int leerlingID { get; set; }
        public string voorNaam { get; set; }
        public string familieNaam { get; set; }
        public string email { get; set; }
        public Klas Klas { get; set; }
    }
}
