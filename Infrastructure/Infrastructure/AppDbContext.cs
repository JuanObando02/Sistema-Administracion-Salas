using Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Milk> Milks { get; set; }
        public DbSet<Cow> Cows { get; set; }
        public DbSet<Farm> Farms { get; set; }

        // proyecto
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Reserva> Reservas { get; set; }
        public DbSet<Reporte> Reportes { get; set; }
        public DbSet<Asesoria> Asesorias { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Llamamos al método base para que configure las tablas de Identity
            base.OnModelCreating(modelBuilder);

            // --- 2. Arreglo para Ciclos de Eliminación en Cascada ---
            // Le decimos a EF que use "DeleteBehavior.Restrict" (no borrar en cascada)
            // para todas las relaciones que se conectan a AppUser.

            // Relaciones de Reserva <-> AppUser
            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.UsuarioSolicitante)
                .WithMany(u => u.ReservasHechas) //
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Reserva>()
                .HasOne(r => r.UsuarioAprobador)
                .WithMany(u => u.ReservasAprobadas) //
                .OnDelete(DeleteBehavior.Restrict);

            // Relaciones de Asesoria <-> AppUser
            modelBuilder.Entity<Asesoria>()
                .HasOne(a => a.UsuarioSolicitante)
                .WithMany(u => u.AsesoriasSolicitadas) //
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Asesoria>()
                .HasOne(a => a.CoordinadorAsignado)
                .WithMany(u => u.AsesoriasAtendidas) //
                .OnDelete(DeleteBehavior.Restrict);

            // Relación de Reporte <-> AppUser
            modelBuilder.Entity<Reporte>()
                .HasOne(r => r.UsuarioCreador)
                .WithMany(u => u.ReportesCreados) //
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Reserva>()
            .HasOne(r => r.Sala)
            .WithMany(s => s.Reservas) //
            .HasForeignKey(r => r.SalaId)
            .OnDelete(DeleteBehavior.Restrict); // Evita problemas si se borra una Sala

            modelBuilder.Entity<Reporte>()
                .HasOne(r => r.SalaReportada)
                .WithMany(s => s.Reportes) //
                .HasForeignKey(r => r.SalaId)
                .OnDelete(DeleteBehavior.Restrict); // Evita problemas si se borra una Sala

        }
    }

}
