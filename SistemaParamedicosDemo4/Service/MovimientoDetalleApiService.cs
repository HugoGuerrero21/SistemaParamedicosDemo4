using System.Net.Http.Json;
using SistemaParamedicosDemo4.MVVM.Models;
using SistemaParamedicosDemo4.Services;

namespace SistemaParamedicosDemo4.Service
{
    public class MovimientoDetalleApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public MovimientoDetalleApiService()
        {
            _httpClient = ApiConfiguration.GetHttpClient();
            _baseUrl = ApiConfiguration.BaseUrl;
        }

        /// <summary>
        /// Obtiene los detalles de un movimiento específico desde la API
        /// </summary>
        public async Task<List<MovimientoDetalleDto>> ObtenerDetallesPorMovimientoAsync(string idMovimiento)
        {
            try
            {
                var url = $"{_baseUrl}/Movimientos/{idMovimiento}/detalles";
                System.Diagnostics.Debug.WriteLine($"📦 Llamando a: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var detalles = await response.Content.ReadFromJsonAsync<List<MovimientoDetalleDto>>();
                    System.Diagnostics.Debug.WriteLine($"✅ {detalles?.Count ?? 0} detalles obtenidos para movimiento {idMovimiento}");
                    return detalles ?? new List<MovimientoDetalleDto>();
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Error al obtener detalles: {response.StatusCode}");
                    return new List<MovimientoDetalleDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener detalles del movimiento: {ex.Message}");
                return new List<MovimientoDetalleDto>();
            }
        }
    }

    // DTO para recibir detalles desde la API
    public class MovimientoDetalleDto
    {
        public string IdMovimientoDetalle { get; set; }
        public string IdMovimiento { get; set; }
        public string IdProducto { get; set; }
        public float Cantidad { get; set; }
        public float CantidadUtilizada { get; set; }
        public int Status { get; set; }

        // Info del producto
        public string NombreProducto { get; set; }
        public string DescripcionProducto { get; set; }

        /// <summary>
        /// Convierte el DTO a un modelo de SQLite
        /// </summary>
        public MovimientoDetalleModel ToModel()
        {
            return new MovimientoDetalleModel
            {
                IdMovimientoDetalle = IdMovimientoDetalle,
                IdMovimiento = IdMovimiento,
                ClaveProducto = IdProducto,
                Cantidad = Cantidad,
                CantidadUtilizada = CantidadUtilizada,
                Status = (byte)Status
            };
        }
    }
}