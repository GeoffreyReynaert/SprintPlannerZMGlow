using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Klas
    {
        public Klas()
        {
            Leerlingen = new List<Leerling>();
            Vakken = new List<Vak>();
            Hulpleerlingen = new List<Hulpleerling>();
        }
        public int klasID { get; set; }
        public string klasnaam { get; set; }
        public long titularisID { get; set; }
        public Leerkracht Leerkracht { get; set; }
        public IList<Leerling> Leerlingen { get; set; }
        public IList<Vak> Vakken { get; set; }
        public IList<Hulpleerling> Hulpleerlingen { get; set; }
    }
}
