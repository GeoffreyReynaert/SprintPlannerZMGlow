using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SprintPlannerZM.Model
{
    public class Hulpleerling
    {

        public Hulpleerling()
        {
            Sprintvakkeuzes = new List<Sprintvakkeuze>();
            Leerlingverdelingen = new List<Leerlingverdeling>();
        }
      
        public long hulpleerlingID { get; set; }
        public int klasID { get; set; }
        public long leerlingID { get; set; } 
        public Leerling Leerling { get; set; }
        public Klas Klas { get; set; }
        public IList<Sprintvakkeuze> Sprintvakkeuzes { get; set; }
        public IList<Leerlingverdeling> Leerlingverdelingen { get; set; }
    }
}
