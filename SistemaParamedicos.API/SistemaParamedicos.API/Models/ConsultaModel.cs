using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("MOSE_CONSULTAS")]
    public class ConsultaModel
    {
        [Key]
        [Column("ID_CONSULTA")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdConsulta { get; set; }

        [Required]
        [Column("ID_EMPLEADO")]
        [MaxLength(30)]
        public string IdEmpleado { get; set; }

        [Column("ID_USUARIO_ACC")]
        [MaxLength(25)]
        public string IdUsuarioAcc { get; set; }

        [Required]
        [Column("ID_TIPO_ENFERMEDAD")]
        public int IdTipoEnfermedad { get; set; }

        [Column("ID_MOVIMIENTO")]
        [MaxLength(25)]
        public string IdMovimiento { get; set; }

        [Required]
        [Column("MOTIVO_CONSULTA")]
        [MaxLength(500)]
        public string MotivoConsulta { get; set; }

        [Required]
        [Column("FECHA_CONSULTA")]
        public DateTime FechaConsulta { get; set; }

        [Column("FRECUENCIA_RESPIRATORIA")]
        public byte? FrecuenciaRespiratoria { get; set; }

        [Column("FRECUENCIA_CARDIACA")] 
        public short? FrecuenciaCardiaca { get; set; }

        [Column("TEMPERATURA")]
        public decimal? Temperatura { get; set; } // decimal(10,0) en tu BD

        [Column("PRESION_ARTERIAL")]
        [MaxLength(30)]
        public string PresionArterial { get; set; }

        [Column("OBSERVACIONES")]
        [MaxLength(500)]
        public string Observaciones { get; set; }

        [Column("ULTIMA_COMIDA")]
        [MaxLength(500)]
        public string UltimaComida { get; set; }

        [Required]
        [Column("DIAGNOSTICO")]
        [MaxLength(150)]
        public string Diagnostico { get; set; }

        // Relaciones de navegación
        [ForeignKey("IdEmpleado")]
        public virtual EmpleadoModel Empleado { get; set; }

        [ForeignKey("IdTipoEnfermedad")]
        public virtual TipoEnfermedadModel TipoEnfermedad { get; set; }

        [ForeignKey("IdUsuarioAcc")]
        public virtual UsuarioAccesoModel Usuario { get; set; }

        [ForeignKey("IdMovimiento")]
        public virtual MovimientoModel Movimiento { get; set; }
    }
}