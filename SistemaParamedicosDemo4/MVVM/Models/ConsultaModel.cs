using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    [Table("Consultas")]
    public  class ConsultaModel
    {
        [PrimaryKey, AutoIncrement]
        public int IdConsulta { get; set; }
        [MaxLength(30)]
        public string IdEmpleado { get; set; }

        // Propiedad de navegación - NO se guarda en BD
        [Ignore]
        public EmpleadoModel Empleado { get; set; } //Traemos el modelo del empleado

        [MaxLength(25)]
        public string IdUsuarioAcceso { get; set; }

        // Propiedad de navegación - NO se guarda en BD
        [Ignore]
        public UsuariosAccesoModel UsuariosAcceso { get; set; } //Traemos el objetos del usuario

        public int IdTipoEnfermedad { get; set; }

        // Propiedad de navegación - NO se guarda en BD
        [Ignore]
        public TipoEnfermedadModel TipoEnfermedad { get; set; } //Traemos objeto de tipo enfermedad

        [MaxLength(25)]
        public string IdMovimiento { get; set; }


        public byte FrecuenciaRespiratoria { get; set; }
        public short FrecuenciaCardiaca { get; set; }
        [MaxLength(15)]
        public string Temperatura { get; set; }
        [MaxLength(40)]
        public string PresionArterial { get; set; }
        [MaxLength(255)]
        public string Observaciones { get; set; }
        [MaxLength(255)]
        public string UltimaComida { get; set; }
        [MaxLength(30)]
        public string MotivoConsulta { get; set; }
        public DateTime FechaConsulta { get; set; }
        [MaxLength(255)]

        public string Diagnostico { get; set; }
    }
}
