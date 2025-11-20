using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaParamedicos.API.Models
{
    public class ExistenciaParamedicoViewModel
    {
        [Column("PRODUCTO")]
        public string Producto { get; set; }

        [Column("NOMBRE DEL PRODUCTO")]
        public string NombreDelProducto { get; set; }

        [Column("DESCRIPCION")]
        public string? Descripcion { get; set; }

        [Column("MARCA")]
        public string? Marca { get; set; }

        [Column("NUMERO DE PIEZA")]
        public string? NumeroDePieza { get; set; }

        [Column("ENTRADA")]
        public double Entrada { get; set; }

        [Column("SALIDA")]
        public double Salida { get; set; }

        [Column("EXISTENCIA")]
        public double Existencia { get; set; }
        [Column("FOTO")]
        public string? Foto { get; set; }
    }
}