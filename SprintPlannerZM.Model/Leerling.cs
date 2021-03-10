using System;

namespace SprintPlannerZM.Model
{
    public class Leerling : IEquatable<Leerling>
    {
        public long leerlingID { get; set; }
        public string voorNaam { get; set; }
        public string familieNaam { get; set; }
        public string email { get; set; }
        public int KlasID { get; set; }
        public string? groep { get; set; }
        public bool sprinter { get; set; }
        public bool typer { get; set; }
        public bool mklas { get; set; }
        public Klas Klas { get; set; }
        public long? hulpleerlingID { get; set; }
        public Hulpleerling hulpleerling { get; set; }
        

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj is Leerling objAsPart && Equals(objAsPart);
        }

        public override int GetHashCode()
        {
            return (int) leerlingID;
        }

        public bool Equals(Leerling other)
        {
            if (other == null) return false;
            return (this.leerlingID.Equals(other.leerlingID));
        }
    }
    
}

