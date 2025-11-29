using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("MOAD_TRASPALMACEN")]
    public class MovimientoTraspaso
    {
        [Key]
        [Column("ID_TRASPASO")]
        [MaxLength(25)]
        public string IdTraspaso { get; set; }

        [Required]
        [Column("ID_USUARIOO")]
        [MaxLength(45)]
        public string IdUsuarioo { get; set; }

        [Required]
        [Column("ALMACEN_ORIGEN")]
        [MaxLength(45)]
        public string AlmacenOrigen { get; set; }

        [Required]
        [Column("ALMACEN_DESTINO")]
        [MaxLength(45)]
        public string AlmacenDestino { get; set; }

        [Column("ID_EMPLEADO")]
        [MaxLength(30)]
        public string? IdEmpleado { get; set; }

        [Required]
        [Column("FECHA_ENVIO")]
        public DateTime FechaEnvio { get; set; }

        [Column("FECHA_RECEPCION")]
        public DateTime? FechaRecepcion { get; set; }

        [Column("FECHA_COMPLETADO")]
        public DateTime? FechaCompletado { get; set; }

        [Column("ID_USUARIOD")]
        [MaxLength(45)]
        public string? IdUsuarioD { get; set; }

        [Required]
        [Column("STATUS")]
        public byte Status { get; set; }

        [Column("ID_UNIDAD")]
        [MaxLength(45)]
        public string? IdUnidad { get; set; }

        [Column("FECHA_CANCELACION")]
        public DateTime? FechaCancelacion { get; set; }

        [Column("ID_USUARIO_CANCELA")]
        [MaxLength(45)]
        public string? IdUsuarioCancela { get; set; }

        // Relaciones
        [ForeignKey("IdEmpleado")]
        public EmpleadoModel? Empleado { get; set; }

        [ForeignKey("IdUsuarioo")]
        public UsuarioAccesoModel? UsuarioOrigen { get; set; }

        [ForeignKey("IdUsuarioD")]
        public UsuarioAccesoModel? UsuarioDestino { get; set; }

        public ICollection<MovimientoTraspasoDetalle>? Detalles { get; set; }
    }
}