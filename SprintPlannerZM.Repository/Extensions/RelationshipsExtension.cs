using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SprintPlannerZM.Model;


namespace SprintPlannerZM.Repository.Extensions
{
    public static class RelationshipsExtension
    {
        public static void RemovePluralizingTableNameConvention(this ModelBuilder modelBuilder)
        {
            foreach (IMutableEntityType entity in modelBuilder.Model.GetEntityTypes())
            {
                entity.SetTableName(entity.DisplayName());
            }
        }
    }

    public static class RelationshipsExtensions
    {
        public static void ConfigureRelationships(this ModelBuilder builder)
        {
            builder.ConfigureLeerling();
            builder.ConfigureKlas();
            builder.ConfigureVak();
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
    }
}