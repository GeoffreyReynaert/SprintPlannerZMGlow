using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Klas
    {
        public int klasID { get; set; }
        public string klasnaam { get; set; }
        public int titularisID { get; set; }
        public IList<Leerling> Leerlingen { get; set; }
        public Leerkracht Leerkracht { get; set; }
        public IList<Vak> Vakken { get; set; }
        public IList<Hulpleerling> Hulpleerlingen { get; set; }
    }
}
