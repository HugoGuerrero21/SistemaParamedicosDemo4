using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public class EmpleadoModel
    {
        public string IdEmpleado { get; set; }
        public string Nombre { get; set; }
        public string TipoSangre { get; set; }
        public string SexoSangre { get; set; }
        public string AlergiasSangre { get; set; }
        public string Telefono { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string IdPuesto { get; set; }
    }
}
