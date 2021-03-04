using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SprintPlannerZM.Model
{
    public class Hulpleerling
    {

        public Hulpleerling()
        {
            Sprintvakken = new List<Sprintvak>();
            Leerlingverdelingen = new List<Leerlingverdeling>();
        }
        [Key]
        public int hulpleerlingID { get; set; }
        public int klasID { get; set; }
        public long leerlingID { get; set; }
        public Klas Klas { get; set; }
        public Leerling Leerling { get; set; }
        public IList<Sprintvak> Sprintvakken { get; set; }
        public IList<Leerlingverdeling> Leerlingverdelingen { get; set; }
    }
}
