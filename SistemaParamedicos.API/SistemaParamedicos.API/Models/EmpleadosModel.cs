using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("CARH_EMPLEADOS")]
    public class EmpleadosModel
    {
        [Key]
        [Column("ID_EMPLEADO")]
        [MaxLength(30)]
        public string IdEmpleado { get; set; }

    }
}
