using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;
using SprintPlannerZM.Repository.Extensions;

namespace SprintPlannerZM.Repository
{
    public class TihfDbContext : DbContext
    {
        public TihfDbContext(DbContextOptions<TihfDbContext> options) : base(options)
        {

        }

        public DbSet<Leerling> Leerling { get; set; }
        public DbSet<Klas> Klas { get; set; }
        public DbSet<Leerkracht> Leerkracht { get; set; }
        public DbSet<Vak> Vak { get; set; }
        public DbSet<Examenrooster> Examenrooster { get; set; }
        public DbSet<Lokaal> Lokaal { get; set; }
        public DbSet<Hulpleerling> Hulpleerling { get; set; }
        public DbSet<Sprintvak> Sprintvak { get; set; }
        public DbSet<Sprintlokaal> Sprintlokaal { get; set; }
        public DbSet<Leerlingverdeling> Leerlingverdeling { get; set; }
        public DbSet<Dagdeel> Dagdeel { get; set; }
        public DbSet<Examentijdspanne> Examentijdspanne { get; set; }
        public DbSet<Deadline> Deadline { get; set; }
        public DbSet<Beheerder> Beheerder { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ConfigureRelationships();
            modelBuilder.RemovePluralizingTableNameConvention();
            base.OnModelCreating(modelBuilder);
        }
    }
}
