using Microsoft.EntityFrameworkCore;
using SistemaParamedicos.API.Models;

namespace SistemaParamedicos.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UsuarioAccesoModel> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurar la tabla de usuarios
            modelBuilder.Entity<UsuarioAccesoModel>()
                .ToTable("COSI_USUARIOS"); // ← Nombre de tu tabla

            modelBuilder.Entity<UsuarioAccesoModel>()
                .HasKey(u => u.IdUsuarioAcc);
        }
    }
}