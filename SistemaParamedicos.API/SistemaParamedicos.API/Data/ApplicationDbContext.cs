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

        // Tablas existentes
        public DbSet<UsuarioAccesoModel> Usuarios { get; set; }
        public DbSet<EmpleadoModel> Empleados { get; set; }
        public DbSet<PuestoModel> Puestos { get; set; }

        // Nuevas tablas de inventario
        public DbSet<ProductoModel> Productos { get; set; }
        public DbSet<TipoMovimientoModel> TiposMovimiento { get; set; }
        public DbSet<MovimientoModel> Movimientos { get; set; }
        public DbSet<MovimientoDetalleModel> MovimientosDetalle { get; set; }

        // Vista de existencias
        public DbSet<ExistenciaParamedicoViewModel> ExistenciasParamedicos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de Usuarios
            modelBuilder.Entity<UsuarioAccesoModel>()
                .ToTable("COSI_USUARIOS");
            modelBuilder.Entity<UsuarioAccesoModel>()
                .HasKey(u => u.IdUsuarioAcc);

            // Configuración de Empleados
            modelBuilder.Entity<EmpleadoModel>()
                .ToTable("CARH_EMPLEADOS");
            modelBuilder.Entity<EmpleadoModel>()
                .HasKey(e => e.IdEmpleado);

            // Configuración de Puestos
            modelBuilder.Entity<PuestoModel>()
                .ToTable("CARH_PUESTOS");
            modelBuilder.Entity<PuestoModel>()
                .HasKey(p => p.IdPuesto);

            // Relación Empleado -> Puesto
            modelBuilder.Entity<EmpleadoModel>()
                .HasOne(e => e.Puesto)
                .WithMany(p => p.Empleados)
                .HasForeignKey(e => e.IdPuesto)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de Productos
            modelBuilder.Entity<ProductoModel>()
                .ToTable("CAAD_PRODUCTOS");
            modelBuilder.Entity<ProductoModel>()
                .HasKey(p => p.IdProducto);

            // Configuración de Tipos de Movimiento
            modelBuilder.Entity<TipoMovimientoModel>()
                .ToTable("CAAD_TIPOMOVIMIENTO");
            modelBuilder.Entity<TipoMovimientoModel>()
                .HasKey(t => t.IdTipoMovimiento);

            // Configuración de Movimientos
            modelBuilder.Entity<MovimientoModel>()
                .ToTable("MOAD_MOVALMACEN");
            modelBuilder.Entity<MovimientoModel>()
                .HasKey(m => m.IdMovimiento);

            modelBuilder.Entity<MovimientoModel>()
                .Property(m => m.Status)
                .HasConversion<sbyte>();

            modelBuilder.Entity<MovimientoModel>()
                .Property(m => m.EsTraspaso)
                .HasConversion<sbyte?>();

            // Relación Movimiento -> TipoMovimiento
            modelBuilder.Entity<MovimientoModel>()
                .HasOne(m => m.TipoMovimiento)
                .WithMany(t => t.Movimientos)
                .HasForeignKey(m => m.IdTipoMovimiento)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Movimiento -> Empleado
            modelBuilder.Entity<MovimientoModel>()
                .HasOne(m => m.Empleado)
                .WithMany()
                .HasForeignKey(m => m.IdEmpleado)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de Movimientos Detalle
            modelBuilder.Entity<MovimientoDetalleModel>()
                .ToTable("MDAD_MOVALMACEN");
            modelBuilder.Entity<MovimientoDetalleModel>()
                .HasKey(md => md.IdMovimientoDetalles);

            // Relación MovimientoDetalle -> Movimiento
            modelBuilder.Entity<MovimientoDetalleModel>()
                .HasOne(md => md.Movimiento)
                .WithMany(m => m.Detalles)
                .HasForeignKey(md => md.IdMovimiento)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación MovimientoDetalle -> Producto
            modelBuilder.Entity<MovimientoDetalleModel>()
                .HasOne(md => md.Producto)
                .WithMany(p => p.MovimientoDetalles)
                .HasForeignKey(md => md.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación MovimientoDetalle -> DetallePadre (auto-referencia)
            modelBuilder.Entity<MovimientoDetalleModel>()
                .HasOne(md => md.DetallePadre)
                .WithMany()
                .HasForeignKey(md => md.IdDetallePadre)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de la Vista de Existencias
            modelBuilder.Entity<ExistenciaParamedicoViewModel>()
                .ToView("V_EXISTENCIAS_PARAMEDICOS")
                .HasNoKey();
        }
    }
}