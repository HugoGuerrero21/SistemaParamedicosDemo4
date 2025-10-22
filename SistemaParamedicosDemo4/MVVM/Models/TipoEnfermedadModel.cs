using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public class TipoEnfermedadModel
    {
        [PrimaryKey, AutoIncrement]
        public int IdTipoEnfermedad { get; set; }
        [MaxLength(50)]
        public string NombreEnfermedad { get; set; }
    }
}
