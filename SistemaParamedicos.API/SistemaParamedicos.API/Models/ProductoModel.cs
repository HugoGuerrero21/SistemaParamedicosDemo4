using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("CAAD_PRODUCTOS")]
    public class ProductoModel
    {
        [Key]
        [Column("ID_PRODUCTO")]
        [MaxLength(45)]
        public string IdProducto { get; set; }

        [Required]
        [Column("NOMBRE")]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Column("MARCA")]
        [MaxLength(45)]
        public string? Marca { get; set; }

        [Column("NUMERO_PIEZA")]
        [MaxLength(45)]
        public string? NumeroPieza { get; set; }

        [Column("DESCRIPCION")]
        [MaxLength(1000)]
        public string? Descripcion { get; set; }

        // Relaciones
        public ICollection<MovimientoDetalleModel>? MovimientoDetalles { get; set; }
    }
}