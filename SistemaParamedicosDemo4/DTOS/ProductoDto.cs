using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.DTOS
{
    public class ProductoDto
    {
        public string IdProducto { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Marca { get; set; } = string.Empty;
        public string NumeroPieza { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? MinStock { get; set; }
        public int? MaxStock { get; set; }
        public string? Foto { get; set; }
        public double? Precio { get; set; }
        public double? PrecioFinal { get; set; }
        public int? Estado { get; set; }
        public string? IdCategoria { get; set; }
        public string? IdFamilia { get; set; }
    }
}
