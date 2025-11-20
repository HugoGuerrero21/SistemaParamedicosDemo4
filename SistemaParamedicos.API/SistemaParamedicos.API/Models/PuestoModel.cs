using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("CARH_PUESTOS")]
    public class PuestoModel
    {
        [Key]
        [Column("ID_PUESTO")]
        [MaxLength(25)]
        public string IdPuesto { get; set; }

        [Column("ID_DEPARTAMENTO")]
        [MaxLength(45)]
        public string? IdDepartamento { get; set; }

        [Required]
        [Column("NOMBRE")]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Required]
        [Column("FECHA")]
        public DateTime Fecha { get; set; }

        // Relación: Un puesto puede tener muchos empleados
        public ICollection<EmpleadoModel>? Empleados { get; set; }
    }
}