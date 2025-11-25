using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("CASE_TIPOENFERMEDAD")]
    public class TipoEnfermedadModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("ID_TIPO_ENFERMEDAD")]
        public int IdTipoEnfermedad { get; set; }

        [Required]
        [Column("NOMBRE_DE_ENFERMEDAD")]
        [MaxLength(50)]
        public string NombreEnfermedad { get; set; }

        [Column("ID_USUARIO_ACC")]
        [MaxLength(10)]
        public string IdUsuarioAcc { get; set; }

    }
}
