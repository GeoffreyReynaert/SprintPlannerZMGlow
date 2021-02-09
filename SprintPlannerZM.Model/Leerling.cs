using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Leerling
    {
        public int leerlingID { get; set; }
        public string voornaam { get; set; }
        public string familienaam { get; set; }
        public string email { get; set; }
        public int klasID { get; set; }
        public Klas Klas { get; set; }
        public IList<Hulpleerling> Hulpleerlingen { get; set; }
    }
}
