namespace SprintPlannerZM.Model
{
    public class Sprintvak
    {
        public int sprintvakID { get; set; }
        public int vakID { get; set; }
        public bool sprint { get; set; }
        public bool typer { get; set; }
        public bool mklas { get; set; }
        public int hulpleerlingID { get; set; }
        public Vak Vak { get; set; }
        public Hulpleerling Hulpleerling { get; set; }
    }
}
