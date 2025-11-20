using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("MDAD_MOVALMACEN")]
    public class MovimientoDetalleModel
    {
        [Key]
        [Column("ID_MOVIMIENTODETALLES")]
        [MaxLength(25)]
        public string IdMovimientoDetalles { get; set; }

        [Required]
        [Column("ID_MOVIMIENTO")]
        [MaxLength(25)]
        public string IdMovimiento { get; set; }

        [Required]
        [Column("ID_PRODUCTO")]
        [MaxLength(25)]
        public string IdProducto { get; set; }

        [Required]
        [Column("CANTIDAD")]
        public float Cantidad { get; set; }

        [Column("ID_LOCACION")]
        [MaxLength(45)]
        public string? IdLocacion { get; set; }

        [Column("PRECIO_FINAL")]
        [MaxLength(45)]
        public string? PrecioFinal { get; set; }

        [Column("ID_PROVEEDOR")]
        [MaxLength(45)]
        public string? IdProveedor { get; set; }

        [Required]
        [Column("STATUS")]
        public sbyte Status { get; set; }

        [Column("CANTIDAD_UTILIZADA")]
        public float? CantidadUtilizada { get; set; }

        [Column("ID_DETALLE_PADRE")]
        [MaxLength(45)]
        public string? IdDetallePadre { get; set; }

        // Relaciones
        [ForeignKey("IdMovimiento")]
        public MovimientoModel? Movimiento { get; set; }

        [ForeignKey("IdProducto")]
        public ProductoModel? Producto { get; set; }

        [ForeignKey("IdDetallePadre")]
        public MovimientoDetalleModel? DetallePadre { get; set; }
    }
}