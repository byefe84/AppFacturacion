using FacturacionDobleEje.Models;
using FacturacionDobleEje.Models.FacturacionConstruccion.Models;
using Microsoft.EntityFrameworkCore;

namespace FacturacionDobleEje
{
    public class AppDbContext : DbContext
    {
        public DbSet<Material> Materials { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                "Host=100.116.189.105;Port=5432;Database=dobleejeconstruccion;Username=dobleejeconstruccion;Password=decadmin"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Material>(entity =>
            {
                entity.ToTable("MATERIAL");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasColumnName("ID");

                entity.Property(e => e.Code)
                      .HasColumnName("CODE");

                entity.Property(e => e.Name)
                      .HasColumnName("NAME");

                entity.Property(e => e.UnitPrice)
                      .HasColumnName("UNIT_PRICE");

                entity.Property(e => e.Unit)
                      .HasColumnName("UNIT");

                entity.Property(e => e.CreatedOn)
                      .HasColumnName("CREATED_ON");
            });
            modelBuilder.Entity<Client>(entity =>
            {
                entity.ToTable("CLIENT");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasColumnName("ID");

                entity.Property(e => e.Name)
                      .HasColumnName("NAME");

                entity.Property(e => e.Address)
                      .HasColumnName("ADDRESS");

                entity.Property(e => e.DocType)
                      .HasColumnName("DOCTYPE");

                entity.Property(e => e.Document)
                      .HasColumnName("DOCUMENT");

                entity.Property(e => e.PhoneNumber)
                      .HasColumnName("PHONENUMBER");

                entity.Property(e => e.Email)
                      .HasColumnName("EMAIL");

                entity.Property(e => e.CreatedOn)
                      .HasColumnName("CREATED_ON");
            });
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("COMPANY");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                      .HasColumnName("ID");

                entity.Property(e => e.Name)
                      .HasColumnName("NAME");

                entity.Property(e => e.Address)
                      .HasColumnName("ADDRESS");

                entity.Property(e => e.DocType)
                      .HasColumnName("DOCTYPE");

                entity.Property(e => e.Document)
                      .HasColumnName("DOCUMENT");

                entity.Property(e => e.PhoneNumber)
                      .HasColumnName("PHONENUMBER");

                entity.Property(e => e.Email)
                      .HasColumnName("EMAIL");

                entity.Property(e => e.LogoPath)
                      .HasColumnName("LOGOPATH");

                entity.Property(e => e.Account)
                      .HasColumnName("ACCOUNT");

                entity.Property(e => e.CreatedOn)
                      .HasColumnName("CREATED_ON");
            });
        }
    }
}
