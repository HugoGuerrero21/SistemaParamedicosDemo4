using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("MOAD_MOVALMACEN")]
    public class MovimientoModel
    {
        [Key]
        [Column("ID_MOVIMIENTO")]
        [MaxLength(25)]
        public string IdMovimiento { get; set; }

        [Required]
        [Column("ID_TIPO_MOVIMIENTO")]
        [MaxLength(45)]
        public string IdTipoMovimiento { get; set; }

        [Required]
        [Column("ID_ALMACEN")]
        [MaxLength(45)]
        public string IdAlmacen { get; set; }

        [Required]
        [Column("FECHA_MOVIMIENTO")]
        public DateTime FechaMovimiento { get; set; }

        [Column("ID_EMPLEADO")]
        [MaxLength(30)]
        public string? IdEmpleado { get; set; }

        [Column("ID_UNIDAD")]
        [MaxLength(45)]
        public string? IdUnidad { get; set; }

        [Column("ID_AREA")]
        [MaxLength(45)]
        public string? IdArea { get; set; }

        [Column("OTROS_SALIDA")]
        [MaxLength(50)]
        public string? OtrosSalida { get; set; }

        [Required]
        [Column("ES_TRASPASO")]
        public sbyte? EsTraspaso { get; set; }

        [Required]
        [Column("STATUS")]
        public int Status { get; set; }

        [Required]
        [Column("ID_USUARIO")]
        [MaxLength(45)]
        public string IdUsuario { get; set; }

        [Column("MOTIVO_DEVOLUCION")]
        [MaxLength(200)]
        public string? MotivoDevolucion { get; set; }

        // Relaciones   
        [ForeignKey("IdTipoMovimiento")]
        public TipoMovimientoModel? TipoMovimiento { get; set; }

        [ForeignKey("IdEmpleado")]
        public EmpleadoModel? Empleado { get; set; }

        public ICollection<MovimientoDetalleModel>? Detalles { get; set; }
    }
}