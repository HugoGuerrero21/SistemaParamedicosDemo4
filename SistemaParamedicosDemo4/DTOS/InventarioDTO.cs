using SistemaParamedicosDemo4.MVVM.Models;
using System.Text.Json.Serialization;

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

        public string FotoUrl
        {
            get
            {
                if (string.IsNullOrEmpty(Foto))
                    return null;

                if (Foto.StartsWith("http://") || Foto.StartsWith("https://"))
                    return Foto;

                const string BASE_URL = "https://localhost:7285";
                var rutaLimpia = Foto.StartsWith("/") ? Foto : $"/{Foto}";
                return $"{BASE_URL}{rutaLimpia}";
            }
        }

        // ⭐ AGREGA ESTA PROPIEDAD QUE TE FALTABA
        public bool TieneFoto => !string.IsNullOrEmpty(Foto);

        [JsonPropertyName("entrada")]
        public double Entrada { get; set; }

        [JsonPropertyName("salida")]
        public double Salida { get; set; }

        [JsonPropertyName("existencia")]
        public double Existencia { get; set; }

        public string NombreCompleto => !string.IsNullOrEmpty(Marca)
            ? $"{NombreDelProducto} - {Marca}"
            : NombreDelProducto;

        public bool StockBajo => Existencia < 10;
        public bool Agotado => Existencia <= 0;

        public string ColorStock
        {
            get
            {
                if (Agotado) return "#DC3545";
                if (StockBajo) return "#FFC107";
                return "#28A745";
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

        public ProductoModel ToProductoModel()
        {
            return new ProductoModel
            {
                ProductoId = this.Producto,
                Nombre = this.NombreDelProducto,
                Marca = this.Marca ?? string.Empty,
                Model = string.Empty, // No viene en la vista
                Descripcion = this.Descripcion ?? string.Empty,
                NumeroPieza = this.NumeroDePieza ?? string.Empty,
                Foto = this.Foto,
                CantidadDisponible = this.Existencia
            };
        }
    }
}