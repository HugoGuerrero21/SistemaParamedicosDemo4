using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    [Table("COSI_USUARIOS")]
    public class UsuarioAccesoModel
    {
        [Key]
        [Column("ID_USUARIO_ACC")]
        [MaxLength(25)]
        public string IdUsuarioAcc { get; set; }

        [Column("ID_EMPLEADO")]
        [MaxLength(25)]
        public string? IdEmpleado { get; set; }

        [Required]
        [Column("NOMBRE")]
        [MaxLength(100)]
        public string Nombre { get; set; }

        [Column("ID_AREA")]
        [MaxLength(25)]
        public string? IdArea { get; set; }

        [Column("ID_PUESTO")]
        [MaxLength(25)]
        public string? IdPuesto { get; set; }

        [Column("AREA")]
        [MaxLength(35)]
        public string? Area { get; set; }

        [Column("DEPARTAMENTO")]
        [MaxLength(50)]
        public string? Departamento { get; set; }

        [Column("PUESTO")]
        [MaxLength(50)]
        public string? Puesto { get; set; }

        [Column("ALMACEN_ASIGNADO")]
        [MaxLength(45)]
        public string? AlmacenAsignado { get; set; }

        [Required]
        [Column("USUARIO")]
        [MaxLength(25)]
        public string Usuario { get; set; }

        [Required]
        [Column("PASSWORD")]
        [MaxLength(15)]
        public string Password { get; set; }
    }
}