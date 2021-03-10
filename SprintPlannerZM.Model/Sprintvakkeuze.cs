namespace SprintPlannerZM.Model
{
    public class Sprintvakkeuze
    {
        public int sprintvakkeuzeID { get; set; }
        public int vakID { get; set; }
        public bool sprint { get; set; }
        public bool typer { get; set; }
        public bool mklas { get; set; }
        public long hulpleerlingID { get; set; }
        public Vak Vak { get; set; }
        public Hulpleerling Hulpleerling { get; set; }
    }
}
