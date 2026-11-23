using FleetManager.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace FleetManager.Data
{
    public class FleetManagerDbContext : DbContext
    {
        public DbSet<Munkavallalo> Munkavallalok { get; set; }

        public DbSet<Gepjarmu> Gepjarmuvek { get; set; }

        public DbSet<Szervizeles> Szervizelesek { get; set; }

        public DbSet<Kilometerallas> Kilometerallasok { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                string connectionString =
                    configuration.GetConnectionString("FleetManagerDB")
                    ?? throw new InvalidOperationException(
                        "Az adatbázis kapcsolati karakterlánca nem található.");

                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Munkavallalo>(entity =>
            {
                entity.ToTable("Munkavallalok");
                entity.HasKey(e => e.DolgozoID);
                entity.Property(e => e.Nev).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Reszleg).HasMaxLength(100);
                entity.Property(e => e.Email).HasMaxLength(150);
                entity.Property(e => e.Telefonszam).HasMaxLength(30);
            });

            modelBuilder.Entity<Gepjarmu>(entity =>
            {
                entity.ToTable("Gepjarmuvek");
                entity.HasKey(e => e.GepjarmuID);

                entity.Property(e => e.Rendszam)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.HasIndex(e => e.Rendszam).IsUnique();

                entity.Property(e => e.Marka).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Modell).HasMaxLength(50).IsRequired();
                entity.Property(e => e.Alvazszam).HasMaxLength(17);
                entity.Property(e => e.Uzemanyag).HasMaxLength(30);
                entity.Property(e => e.Allapot).HasMaxLength(30).IsRequired();

                entity.HasOne(e => e.Munkavallalo)
                    .WithMany()
                    .HasForeignKey(e => e.DolgozoID);
            });

            modelBuilder.Entity<Szervizeles>(entity =>
            {
                entity.ToTable("Szervizelesek");
                entity.HasKey(e => e.SzervizID);

                entity.Property(e => e.SzervizTipus)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.Leiras).HasMaxLength(500);
                entity.Property(e => e.Koltseg).HasPrecision(12, 2);

                entity.HasOne(e => e.Gepjarmu)
                    .WithMany()
                    .HasForeignKey(e => e.GepjarmuID);
            });

            modelBuilder.Entity<Kilometerallas>(entity =>
            {
                entity.ToTable("Kilometerallasok");
                entity.HasKey(e => e.KilometerID);

                entity.HasOne(e => e.Gepjarmu)
                    .WithMany()
                    .HasForeignKey(e => e.GepjarmuID);
            });
        }
    }
}