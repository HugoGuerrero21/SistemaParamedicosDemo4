using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("MDAD_TRASPALMACEN")]
    public class MovimientoTraspasoDetalle
    {
        [Key]
        [Column("ID_TRASPASODETALLE")]
        [MaxLength(25)]
        public string IdTraspasoDetalle { get; set; }

        [Required]
        [Column("ID_TRASPASO")]
        [MaxLength(25)]
        public string IdTraspaso { get; set; }

        [Required]
        [Column("ID_PRODUCTO")]
        [MaxLength(45)]
        public string IdProducto { get; set; }

        [Required]
        [Column("CANTIDAD")]
        public decimal Cantidad { get; set; }  // Cambiado de float a decimal

        [Column("ID_PROVEEDOR")]
        [MaxLength(45)]
        public string? IdProveedor { get; set; }

        [Column("CANTIDAD_RECIBIDA")]
        public decimal? CantidadRecibida { get; set; }  // Cambiado de float a decimal

        [Column("SALIDA_PADRE")]
        [MaxLength(45)]
        public string? SalidaPadre { get; set; }

        [Column("ENTRADA_HIJA")]
        [MaxLength(1000)]
        public string? EntradaHija { get; set; }

        [Required]
        [Column("COMPLETADA")]
        public byte Completada { get; set; }

        [Column("FECHA_COMPLETADO")]
        public DateTime? FechaCompletado { get; set; }

        [Column("MOTIVO_CANCELACION")]
        [MaxLength(200)]
        public string? MotivoCancelacion { get; set; }

        // Relaciones
        [ForeignKey("IdTraspaso")]
        public MovimientoTraspaso? Traspaso { get; set; }

        [ForeignKey("IdProducto")]
        public ProductoModel? Producto { get; set; }
    }
}