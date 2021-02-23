using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Vak
    {
        public int vakID { get; set; }
        public string vaknaam { get; set; }
        public int klasID { get; set; }
        public long leerkrachtID { get; set; }
        public Klas klas { get; set; }
        public Leerkracht Leerkracht { get; set; }
        public IList<Examenrooster> Examenroosters { get; set; }
        public IList<Sprintvak> Sprintvakken { get; set; }
    }
}
