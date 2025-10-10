using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    public  class ConsultaModel
    {
        public int IdConsulta { get; set; }


        public string IdEmpleado { get; set; }
        public EmpleadoModel Empleado { get; set; } //Traemos el modelo del empleado

        public string IdUsuarioAcceso { get; set; }
        public UsuariosAccesoModel UsuariosAcceso { get; set; } //Traemos el objetos del usuario

        public int IdTipoEnfermedad { get; set; }
        public TipoEnfermedadModel TipoEnfermedad { get; set; } //Traemos objeto de tipo enfermedad

        public string IdMovimiento { get; set; }


        public byte FrecuenciaRespiratoria { get; set; }
        public short FrecuenciaCardiaca { get; set; }
        public string Temperatura { get; set; }
        public string PresionArterial { get; set; }
        public string Observaciones { get; set; }
        public string UltimaComida { get; set; }
        public string MotivoConsulta { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string Diagnostico { get; set; }
    }
}
