using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Asegúrate de que este namespace coincida con la ubicación en tu proyecto
namespace SistemaParamedicos.API.Models
{
    [Table("CORH_EMPLEADOS")]
    public class EmpleadoModel
    {
        [Key]
        [Column("ID_EMPLEADO")]
        [MaxLength(20)]
        public string IdEmpleado { get; set; }

        [Column("RFID")]
        [MaxLength(20)]
        public string? Rfid { get; set; }

        [Required] // Este campo es "No Nulo" en tu tabla
        [Column("NOMBRE")]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [Required] // Este campo es "No Nulo" en tu tabla
        [Column("SEXO")]
        [MaxLength(15)]
        public string Sexo { get; set; }

        [Column("TELEFONO")]
        [MaxLength(12)]
        public string? Telefono { get; set; }

        [Column("ALERGIAS")]
        [MaxLength(100)]
        public string? Alergias { get; set; }

        [Column("TIPO_SANGRE")]
        [MaxLength(5)]
        public string? TipoSangre { get; set; }

        [Required] // Este campo es "No Nulo" en tu tabla
        [Column("ID_PUESTO")]
        [MaxLength(30)]
        public string IdPuesto { get; set; }

        [Column("ID_DEPARTAMENTO")]
        [MaxLength(45)]
        public string? IdDepartamento { get; set; }

        [Column("ID_AREA")]
        [MaxLength(45)]
        public string? IdArea { get; set; }

        [Column("NACIMIENTO")]
        public DateTime? Nacimiento { get; set; } // El tipo 'date' de SQL se mapea a DateTime

        [Column("FOTO")]
        public string? Foto { get; set; } // El tipo 'text' de SQL se mapea a string
    }
}