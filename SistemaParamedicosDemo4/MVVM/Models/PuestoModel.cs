using SQLite;
using System;

namespace SistemaParamedicosDemo4.MVVM.Models
{
    [Table("Puestos")]
    public class PuestoModel
    {
        [PrimaryKey, MaxLength(25)]
        public string IdPuesto { get; set; }

        [MaxLength(45)]
        public string IdDepartamento { get; set; }

        [MaxLength(100)]
        public string Nombre { get; set; }

        public DateTime Fecha { get; set; }
    }
}