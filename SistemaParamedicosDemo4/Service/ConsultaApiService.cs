using SistemaParamedicosDemo4.DTOS;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SistemaParamedicosDemo4.Service
{
    public class ConsultaApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public ConsultaApiService()
        {
            // ⭐⭐⭐ CAMBIO CRÍTICO: Usar HTTPS y puerto 7285 (igual que TipoEnfermedadApiService)
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // ⭐ USAR LA MISMA URL QUE TipoEnfermedadApiService
            _baseUrl = "https://localhost:7285/api"; // Era http://localhost:5269/api ❌

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            System.Diagnostics.Debug.WriteLine($"✓ ConsultaApiService inicializado con URL: {_baseUrl}");
        }

        /// <summary>
        /// Envía una consulta completa al servidor
        /// </summary>
        public async Task<ConsultaResponseDto> CrearConsultaAsync(CrearConsultaDto dto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📤 Enviando consulta al servidor...");
                System.Diagnostics.Debug.WriteLine($"URL: {_baseUrl}/Consultas");
                System.Diagnostics.Debug.WriteLine($"Empleado: {dto.IdEmpleado}");
                System.Diagnostics.Debug.WriteLine($"Medicamentos: {dto.Medicamentos?.Count ?? 0}");

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_baseUrl}/Consultas",
                    dto,
                    _jsonOptions
                );

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<ConsultaResponseDto>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ Consulta creada exitosamente. ID: {resultado?.IdConsulta}");
                    return resultado;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error del servidor: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Detalle: {error}");
                    return null;
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        /// <summary>
        /// Obtiene todas las consultas
        /// </summary>
        public async Task<List<ConsultaResumenDto>> ObtenerConsultasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Consultas");

                if (response.IsSuccessStatusCode)
                {
                    var consultas = await response.Content.ReadFromJsonAsync<List<ConsultaResumenDto>>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ {consultas?.Count ?? 0} consultas obtenidas");
                    return consultas ?? new List<ConsultaResumenDto>();
                }

                return new List<ConsultaResumenDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener consultas: {ex.Message}");
                return new List<ConsultaResumenDto>();
            }
        }

        /// <summary>
        /// Obtiene consultas de un empleado específico
        /// </summary>
        public async Task<List<ConsultaResumenDto>> ObtenerConsultasPorEmpleadoAsync(string idEmpleado)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Consultas/empleado/{idEmpleado}");

                if (response.IsSuccessStatusCode)
                {
                    var consultas = await response.Content.ReadFromJsonAsync<List<ConsultaResumenDto>>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ {consultas?.Count ?? 0} consultas del empleado {idEmpleado}");
                    return consultas ?? new List<ConsultaResumenDto>();
                }

                return new List<ConsultaResumenDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener consultas del empleado: {ex.Message}");
                return new List<ConsultaResumenDto>();
            }
        }

        /// <summary>
        /// Prueba la conexión con el endpoint de consultas
        /// </summary>
        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Consultas/test");
                var isSuccess = response.IsSuccessStatusCode;

                System.Diagnostics.Debug.WriteLine(isSuccess
                    ? "✅ Conexión exitosa con API de Consultas"
                    : "❌ No se pudo conectar con API de Consultas");

                return isSuccess;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                return false;
            }
        }
    }
}