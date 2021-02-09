using System.Collections.Generic;

namespace SprintPlannerZM.Model
{
    public class Hulpleerling
    {
        public int hulpleerlingID { get; set; }
        public int klasID { get; set; }
        public bool sprint { get; set; }
        public bool typer { get; set; }
        public bool mklas { get; set; }
        public long leerlingID { get; set; }
        public Klas Klas { get; set; }
        public Leerling Leerling { get; set; }
        public IList<Sprintvak> Sprintvakken { get; set; }
        public IList<Leerlingverdeling> Leerlingverdelingen { get; set; }
    }
}
