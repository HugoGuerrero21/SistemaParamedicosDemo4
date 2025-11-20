using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("CAAD_TIPOMOVIMIENTO")]
    public class TipoMovimientoModel
    {
        [Key]
        [Column("ID_TIPO_MOVIMIENTO")]
        [MaxLength(10)]
        public string IdTipoMovimiento { get; set; }

        [Required]
        [Column("NOMBRE")]
        [MaxLength(50)]
        public string Nombre { get; set; }

        // Relaciones
        public ICollection<MovimientoModel>? Movimientos { get; set; }
    }
}