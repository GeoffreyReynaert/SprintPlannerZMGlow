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
            builder.ConfigureSprintlokaalReservatie();
            builder.ConfigureLeerlingverdeling();
        }

        private static void ConfigureLeerling(this ModelBuilder builder)
        {
            builder.Entity<Leerling>()
                .HasOne(a => a.Klas)
                .WithMany(u => u.Leerlingen)
                .HasForeignKey(a => a.KlasID);
            builder.Entity<Leerling>()
                .HasOne(a => a.hulpleerling)
                .WithOne(u => u.Leerling)
                .HasPrincipalKey<Leerling>(a => a.leerlingID)
                .HasForeignKey<Hulpleerling>(u => u.leerlingID);
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
                .HasOne(a => a.klas)
                .WithMany(u => u.Vakken)
                .HasForeignKey(a => a.klasID);
            builder.Entity<Vak>()
                .HasMany(a => a.Sprintvakkeuzes)
                .WithOne(a=>a.Vak)
                .HasForeignKey(a => a.vakID);
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
                .HasForeignKey<Leerling>(u => u.hulpleerlingID);
            builder.Entity<Hulpleerling>()
                .HasMany(a => a.Sprintvakkeuzes)
                .WithOne(u => u.Hulpleerling)
                .HasForeignKey(a => a.sprintvakkeuzeID);

        }

        private static void ConfigureSprintvak(this ModelBuilder builder)
        {
            builder.Entity<Sprintvakkeuze>()
                .HasOne(a => a.Vak)
                .WithMany(u => u.Sprintvakkeuzes)
                .HasForeignKey(a => a.vakID);
            builder.Entity<Sprintvakkeuze>()
                .HasOne(a => a.Hulpleerling)
                .WithMany(u => u.Sprintvakkeuzes)
                .HasForeignKey(a => a.hulpleerlingID);
        }

        private static void ConfigureSprintlokaalReservatie(this ModelBuilder builder)
        {
            builder.Entity<Sprintlokaalreservatie>()
                .HasOne(a => a.Leerkracht)
                .WithMany(u => u.Sprintlokaalreservaties)
                .HasForeignKey(a => a.leerkrachtID);
            builder.Entity<Sprintlokaalreservatie>()
                .HasOne(a => a.Examen)
                .WithMany(u => u.Sprintlokaalreservaties)
                .HasForeignKey(a => a.examenID);
            builder.Entity<Sprintlokaalreservatie>()
                .HasOne(a => a.Lokaal)
               .WithMany(a=>a.Sprintlokaalreservaties)
                .HasForeignKey(a => a.lokaalID);
        }

        private static void ConfigureLeerlingverdeling(this ModelBuilder builder)
        {
            builder.Entity<Leerlingverdeling>()
                .HasOne(a => a.Hulpleerling)
                .WithMany(u => u.Leerlingverdelingen)
                .HasForeignKey(a => a.hulpleerlingID);
            builder.Entity<Leerlingverdeling>()
                .HasOne(a => a.Sprintlokaalreservatie)
                .WithMany(u => u.Leerlingverdelingen)
                .HasForeignKey(a => a.sprintlokaalreservatieID);
            builder.Entity<Leerlingverdeling>()
                .HasOne(a => a.Examenrooster)
                .WithMany(u => u.Leerlingverdelingen)
                .HasForeignKey(a => a.examenID);
        }
    }
}