using SQLite;
using System;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    [Table("MOSE_CONSULTAS")]
    public class ConsultaModel
    {
        [PrimaryKey, AutoIncrement]
        [Column("ID_CONSULTA")]
        public int IdConsulta { get; set; }

        [MaxLength(30)]
        [Column("ID_EMPLEADO")]
        public string IdEmpleado { get; set; }

        [Ignore]
        public EmpleadoModel Empleado { get; set; }

        // ⭐ CAMBIO CRÍTICO: Renombrar propiedad para coincidir con la API
        [MaxLength(25)]
        [Column("ID_USUARIO_ACC")]
        public string IdUsuarioAcc { get; set; } // Antes era "IdUsuarioAcceso"

        [Ignore]
        public UsuariosAccesoModel UsuariosAcceso { get; set; }

        [Column("ID_TIPO_ENFERMEDAD")]
        public int IdTipoEnfermedad { get; set; }

        [Ignore]
        public TipoEnfermedadModel TipoEnfermedad { get; set; }

        [MaxLength(25)]
        [Column("ID_MOVIMIENTO")]
        public string IdMovimiento { get; set; }

        [Column("FRECUENCIA_RESPIRATORIA")]
        public byte FrecuenciaRespiratoria { get; set; }

        [Column("FRECUENCIA_CARDIACA")]
        public short FrecuenciaCardiaca { get; set; }

        [MaxLength(15)]
        [Column("TEMPERATURA")]
        public string Temperatura { get; set; }

        [MaxLength(40)]
        [Column("PRESION_ARTERIAL")]
        public string PresionArterial { get; set; }

        [MaxLength(255)]
        [Column("OBSERVACIONES")]
        public string Observaciones { get; set; }

        [MaxLength(255)]
        [Column("ULTIMA_COMIDA")]
        public string UltimaComida { get; set; }

        [MaxLength(500)]
        [Column("MOTIVO_CONSULTA")]
        public string MotivoConsulta { get; set; }

        [Column("FECHA_CONSULTA")]
        public DateTime FechaConsulta { get; set; }

        [MaxLength(255)]
        [Column("DIAGNOSTICO")]
        public string Diagnostico { get; set; }
    }
}