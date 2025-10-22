using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public class EmpleadoModel
    {
        [PrimaryKey, MaxLength(30)]
        public string IdEmpleado { get; set; }
        [MaxLength(150)]
        public string Nombre { get; set; }
        [MaxLength(5)]
        public string TipoSangre { get; set; }
        [MaxLength(15)]
        public string Sexo { get; set; }
        [MaxLength(255)]
        public string AlergiasSangre { get; set; }
        [MaxLength(12)]
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        [MaxLength(25)]
        public string IdPuesto { get; set; }
    }
}
