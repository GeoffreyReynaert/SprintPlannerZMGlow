using Microsoft.EntityFrameworkCore;
using SprintPlannerZM.Model;

namespace SprintPlannerZM.Repository.Extensions
{
    public static class RelationshipsExtension
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (var entity in modelBuilder.Model.GetEntityTypes()) entity.SetTableName(entity.DisplayName());
        }
    }

    public static class RelationshipsExtensions
    {
        public static void ConfigureRelationships(this ModelBuilder builder)
        {
            builder.ConfigureLeerling();
            builder.ConfigureKlas();
            builder.ConfigureVak();
            builder.ConfigureExamenrooster();
            builder.ConfigureHulpleerling();
            builder.ConfigureSprintvak();
            builder.ConfigureSprintlokaal();
            builder.ConfigureLeerlingverdeling();
        }

        private static void ConfigureLeerling(this ModelBuilder builder)
        {
            builder.Entity<Leerling>()
                .HasOne(a => a.Klas)
                .WithMany(u => u.Leerlingen)
                .HasForeignKey(a => a.KlasID);
        }

        private static void ConfigureKlas(this ModelBuilder builder)
        {
            builder.Entity<Klas>()
                .HasOne(a => a.Leerkracht)
                .WithMany(u => u.Klassen)
                .HasForeignKey(a => a.titularisID);
        }

        private static void ConfigureVak(this ModelBuilder builder)
        {
            builder.Entity<Vak>()
                .HasOne(a => a.Leerkracht)
                .WithMany(u => u.Vakken)
                .HasForeignKey(a => a.leerkrachtID);
            builder.Entity<Vak>()
                .HasOne(a => a.Klas)
                .WithMany(u => u.Vakken)
                .HasForeignKey(a => a.klasID);
        }

        private static void ConfigureExamenrooster(this ModelBuilder builder)
        {
            builder.Entity<Examenrooster>()
                .HasOne(a => a.Vak)
                .WithMany(u => u.Examenroosters)
                .HasForeignKey(a => a.vakID);
        }

        private static void ConfigureHulpleerling(this ModelBuilder builder)
        {
            builder.Entity<Hulpleerling>()
                .HasOne(a => a.Klas)
                .WithMany(u => u.Hulpleerlingen)
                .HasForeignKey(a => a.klasID);
            builder.Entity<Hulpleerling>()
                .HasOne(a => a.Leerling)
                .WithOne(u => u.hulpleerling)
                .HasPrincipalKey<Hulpleerling>(a=>a.leerlingID)
                .HasForeignKey<Leerling>(u => u.leerlingID);

        }

        private static void ConfigureSprintvak(this ModelBuilder builder)
        {
            builder.Entity<Sprintvak>()
                .HasOne(a => a.Vak)
                .WithMany(u => u.Sprintvakken)
                .HasForeignKey(a => a.vakID);
            builder.Entity<Sprintvak>()
                .HasOne(a => a.Hulpleerling)
                .WithMany(u => u.Sprintvakken)
                .HasForeignKey(a => a.hulpleerlingID);
        }

        private static void ConfigureSprintlokaal(this ModelBuilder builder)
        {
            builder.Entity<Sprintlokaal>()
                .HasOne(a => a.Leerkracht)
                .WithMany(u => u.Sprintlokalen)
                .HasForeignKey(a => a.leerkrachtID);
            builder.Entity<Sprintlokaal>()
                .HasOne(a => a.Lokaal)
                .WithMany(u => u.Sprintlokalen)
                .HasForeignKey(a => a.lokaalID);
        }

        private static void ConfigureLeerlingverdeling(this ModelBuilder builder)
        {
            builder.Entity<Leerlingverdeling>()
                .HasOne(a => a.Hulpleerling)
                .WithMany(u => u.Leerlingverdelingen)
                .HasForeignKey(a => a.hulpleerlingID);
            builder.Entity<Leerlingverdeling>()
                .HasOne(a => a.Sprintlokaal)
                .WithMany(u => u.Leerlingverdelingen)
                .HasForeignKey(a => a.sprintlokaalID);
            builder.Entity<Leerlingverdeling>()
                .HasOne(a => a.Examenrooster)
                .WithMany(u => u.Leerlingverdelingen)
                .HasForeignKey(a => a.examenID);
        }
    }
}