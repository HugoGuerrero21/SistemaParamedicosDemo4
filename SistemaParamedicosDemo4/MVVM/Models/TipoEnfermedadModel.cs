using SQLite;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    [Table("CASE_TIPOENFERMEDAD")]
    public class TipoEnfermedadModel
    {
        [PrimaryKey]
        [Column("ID_TIPO_ENFERMEDAD")]
        public int IdTipoEnfermedad { get; set; }

        [MaxLength(50)]
        [Column("NOMBRE_DE_ENFERMEDAD")]
        public string NombreEnfermedad { get; set; }

        [MaxLength(25)]
        [Column("ID_USUARIO_ACC")]
        public string IdUsuarioAcc { get; set; } // ⭐ PascalCase consistente
    }
}