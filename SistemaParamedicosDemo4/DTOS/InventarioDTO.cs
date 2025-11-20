using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.DTOS
{
    public class InventarioDTO
    {
        [JsonPropertyName("producto")]
        public string Producto { get; set; }

        [JsonPropertyName("nombreDelProducto")]
        public string NombreDelProducto { get; set; }

        [JsonPropertyName("descripcion")]
        public string Descripcion { get; set; }

        [JsonPropertyName("marca")]
        public string Marca { get; set; }

        [JsonPropertyName("numeroDePieza")]
        public string NumeroDePieza { get; set; }

        [JsonPropertyName("foto")]
        public string Foto { get; set; }

        [JsonPropertyName("entrada")]
        public double Entrada { get; set; }

        [JsonPropertyName("salida")]
        public double Salida { get; set; }

        [JsonPropertyName("existencia")]
        public double Existencia { get; set; }

        public string NombreCompleto => !string.IsNullOrEmpty(Marca)
         ? $"{NombreDelProducto} - {Marca}"
         : NombreDelProducto;

        //Indicador de si existen inventario bajo (menor a 10 productos)
        public bool StockBajo => Existencia < 10;


        //Indicador de producto agotado
        public bool Agotado => Existencia <= 0;

        public string ColorStock
        {
            get
            {
                if (Agotado) return "#DC3545"; // Rojo
                if (StockBajo) return "#FFC107"; // Amarillo
                return "#28A745"; // Verde
            }
        }

        public string EstadoStock
        {
            get
            {
                if (Agotado) return "AGOTADO";
                if (StockBajo) return "STOCK BAJO";
                return "DISPONIBLE";
            }
        }
    }
}
