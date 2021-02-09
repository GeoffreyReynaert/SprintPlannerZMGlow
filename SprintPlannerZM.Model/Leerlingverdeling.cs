namespace SprintPlannerZM.Model
{
    public class Leerlingverdeling
    {
        public int leerlingverdelingID { get; set; }
        public int hulpleerlingID { get; set; }
        public int sprintlokaalID { get; set; }
        public int examenID { get; set; }
        public Hulpleerling Hulpleerling { get; set; }
        public Sprintlokaal Sprintlokaal { get; set; }
        public Examenrooster Examenrooster { get; set; }
    }
}
