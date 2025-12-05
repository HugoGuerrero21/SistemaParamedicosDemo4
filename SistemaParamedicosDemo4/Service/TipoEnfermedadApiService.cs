using System.Net.Http.Json;
using System.Text.Json;
using SistemaParamedicosDemo4.DTOS;
using SistemaParamedicosDemo4.Services;

namespace SistemaParamedicosDemo4.Service
{
    public class TipoEnfermedadApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TipoEnfermedadApiService()
        {
            // ⭐ USAR ApiConfiguration
            _httpClient = ApiConfiguration.CreateHttpClient();
            _baseUrl = ApiConfiguration.BaseUrl;

            System.Diagnostics.Debug.WriteLine($"✓ TipoEnfermedadApiService inicializado con URL: {_baseUrl}");
        }

        /// <summary>
        /// Obtiene todos los tipos de enfermedad desde la API
        /// </summary>
        public async Task<List<TipoEnfermedadDto>> ObtenerTiposEnfermedadAsync()
        {
            try
            {
                var url = $"{_baseUrl}/TipoEnfermedad";
                System.Diagnostics.Debug.WriteLine($"📡 Llamando a URL: {url}");

                var response = await _httpClient.GetAsync(url);

                System.Diagnostics.Debug.WriteLine($"📡 Status Code: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"📡 Reason: {response.ReasonPhrase}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📡 Respuesta recibida (primeros 300 chars): {content.Substring(0, Math.Min(300, content.Length))}...");

                    var tipos = await response.Content.ReadFromJsonAsync<List<TipoEnfermedadDto>>();
                    System.Diagnostics.Debug.WriteLine($"✓ {tipos?.Count ?? 0} tipos deserializados correctamente");

                    return tipos ?? new List<TipoEnfermedadDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"❌ Contenido: {errorContent}");
                    return new List<TipoEnfermedadDto>();
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ InnerException: {ex.InnerException?.Message}");
                return new List<TipoEnfermedadDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error general: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return new List<TipoEnfermedadDto>();
            }
        }

        /// <summary>
        /// Crea un nuevo tipo de enfermedad
        /// </summary>
        public async Task<(bool exito, string mensaje, TipoEnfermedadDto tipo)> CrearTipoEnfermedadAsync(
            CrearTipoEnfermedadDto dto)
        {
            try
            {
                var url = $"{_baseUrl}/TipoEnfermedad";
                System.Diagnostics.Debug.WriteLine($"📡 POST: {url}");
                System.Diagnostics.Debug.WriteLine($"📤 Datos: {JsonSerializer.Serialize(dto)}");

                var response = await _httpClient.PostAsJsonAsync(url, dto);

                System.Diagnostics.Debug.WriteLine($"📡 Status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📥 Respuesta: {content}");

                    var tipoCreado = await response.Content.ReadFromJsonAsync<TipoEnfermedadDto>();
                    System.Diagnostics.Debug.WriteLine($"✅ Tipo creado con ID: {tipoCreado?.IdTipoEnfermedad}");

                    return (true, "Tipo de enfermedad creado correctamente", tipoCreado);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                {
                    return (false, "Ya existe un tipo de enfermedad con ese nombre", null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error: {errorContent}");
                    return (false, $"Error del servidor: {response.StatusCode}", null);
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ InnerException: {ex.InnerException?.Message}");
                return (false, $"Error de conexión: {ex.Message}", null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error general: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return (false, $"Error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Verifica si existe un tipo de enfermedad con el nombre dado
        /// </summary>
        public async Task<bool> VerificarNombreExistenteAsync(string nombre)
        {
            try
            {
                var url = $"{_baseUrl}/TipoEnfermedad/verificar/{Uri.EscapeDataString(nombre)}";
                System.Diagnostics.Debug.WriteLine($"📡 Verificando nombre: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<Dictionary<string, bool>>();
                    var existe = resultado.ContainsKey("existe") && resultado["existe"];
                    System.Diagnostics.Debug.WriteLine($"✓ Nombre '{nombre}' {(existe ? "YA EXISTE" : "disponible")}");
                    return existe;
                }

                System.Diagnostics.Debug.WriteLine($"❌ Error al verificar: {response.StatusCode}");
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al verificar nombre: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Prueba la conexión con la API
        /// </summary>
        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                var url = $"{_baseUrl}/TipoEnfermedad/test";
                System.Diagnostics.Debug.WriteLine($"📡 Probando conexión: {url}");

                var response = await _httpClient.GetAsync(url);
                var isSuccess = response.IsSuccessStatusCode;

                System.Diagnostics.Debug.WriteLine(isSuccess
                    ? "✓ Conexión exitosa con API de tipos de enfermedad"
                    : $"❌ No se pudo conectar: {response.StatusCode}");

                return isSuccess;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al probar conexión: {ex.Message}");
                return false;
            }
        }
    }
}