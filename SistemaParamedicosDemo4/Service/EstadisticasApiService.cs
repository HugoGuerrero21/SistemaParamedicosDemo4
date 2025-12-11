using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SistemaParamedicosDemo4.DTOS;
using SistemaParamedicosDemo4.Services;

namespace SistemaParamedicosDemo4.Service
{
    public class EstadisticasApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public EstadisticasApiService()
        {
            _httpClient = ApiConfiguration.GetHttpClient();
            _baseUrl = ApiConfiguration.BaseUrl;
        }

        // ⭐ MÉTODO ACTUALIZADO: Recibe rango de fechas
        public async Task<EstadisticasResponseDto> ObtenerEstadisticasAsync(DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                // Formato de fecha para URL: yyyy-MM-dd
                string fInicio = fechaInicio.ToString("yyyy-MM-dd");
                string fFin = fechaFin.ToString("yyyy-MM-dd");

                string url = $"{_baseUrl}/Consultas/estadisticas?fechaInicio={fInicio}&fechaFin={fFin}";

                System.Diagnostics.Debug.WriteLine($"📊 Solicitando estadísticas: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var estadisticas = await response.Content.ReadFromJsonAsync<EstadisticasResponseDto>();
                    return estadisticas;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error API: {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                return null;
            }
        }
    }
}