using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    [Table("CASE_TIPOENFERMEDAD")]
    public class TipoEnfermedadModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID_TIPO_ENFERMEDAD")]
        public int IdTipoEnfermedad { get; set; }
        [MaxLength(50)]
        [Column("NOMBRE_DE_ENFERMEDAD")]
        public string NombreEnfermedad { get; set; }

        [MaxLength(10)]
        [Column("ID_USUARIO_ACC")]
        public string ID_USUARIO_ACC {  get; set; } //Sirve para saber que usuario creó ese tipo de enfermedad
    }
}
