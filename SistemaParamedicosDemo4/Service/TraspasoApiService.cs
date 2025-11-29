using SistemaParamedicosDemo4.DTOS;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Service
{
    public class TraspasoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public string StatusMessage { get; set; }

        public TraspasoApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            _baseUrl = "https://localhost:7285/api";

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            System.Diagnostics.Debug.WriteLine($"✓ TraspasoApiService inicializado con URL: {_baseUrl}");
        }

        /// <summary>
        /// Obtiene todos los traspasos pendientes para el almacén de paramédicos
        /// </summary>
        public async Task<List<TraspasoPendienteDto>> ObtenerTraspasosPendientesAsync()
        {
            try
            {
                var url = $"{_baseUrl}/Traspasos/pendientes";
                System.Diagnostics.Debug.WriteLine($"📡 Obteniendo traspasos pendientes desde: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var traspasos = await response.Content.ReadFromJsonAsync<List<TraspasoPendienteDto>>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ {traspasos?.Count ?? 0} traspasos pendientes obtenidos");
                    StatusMessage = $"{traspasos?.Count ?? 0} traspasos pendientes";
                    return traspasos ?? new List<TraspasoPendienteDto>();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"❌ Detalle: {error}");
                    StatusMessage = $"Error: {response.StatusCode}";
                    return new List<TraspasoPendienteDto>();
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                StatusMessage = "Sin conexión con el servidor";
                return new List<TraspasoPendienteDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                StatusMessage = $"Error: {ex.Message}";
                return new List<TraspasoPendienteDto>();
            }
        }

        /// <summary>
        /// Obtiene un traspaso específico por ID
        /// </summary>
        public async Task<TraspasoPendienteDto> ObtenerTraspasoPorIdAsync(string idTraspaso)
        {
            try
            {
                var url = $"{_baseUrl}/Traspasos/{idTraspaso}";
                System.Diagnostics.Debug.WriteLine($"📡 Obteniendo traspaso {idTraspaso}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var traspaso = await response.Content.ReadFromJsonAsync<TraspasoPendienteDto>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ Traspaso {idTraspaso} obtenido");
                    return traspaso;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error: {error}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al obtener traspaso: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Completa un detalle individual de un traspaso
        /// </summary>
        public async Task<TraspasoResultadoDto> CompletarDetalleAsync(CompletarDetalleDto dto)
        {
            try
            {
                var url = $"{_baseUrl}/Traspasos/completar-detalle";
                System.Diagnostics.Debug.WriteLine($"📤 Completando detalle: {dto.IdTraspasoDetalle}");
                System.Diagnostics.Debug.WriteLine($"📦 Cantidad a recibir: {dto.CantidadRecibida}");

                var response = await _httpClient.PostAsJsonAsync(url, dto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<TraspasoResultadoDto>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ Detalle completado: {resultado?.Mensaje}");
                    StatusMessage = resultado?.Mensaje ?? "Detalle completado exitosamente";
                    return resultado;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error al completar detalle: {error}");

                    // Intentar parsear el error como TraspasoResultadoDto
                    try
                    {
                        var errorDto = await response.Content.ReadFromJsonAsync<TraspasoResultadoDto>(_jsonOptions);
                        StatusMessage = errorDto?.Mensaje ?? "Error al completar detalle";
                        return errorDto;
                    }
                    catch
                    {
                        StatusMessage = "Error al completar detalle";
                        return new TraspasoResultadoDto
                        {
                            Exito = false,
                            Mensaje = error
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
                return new TraspasoResultadoDto
                {
                    Exito = false,
                    Mensaje = ex.Message
                };
            }
        }

        /// <summary>
        /// Completa múltiples detalles de un traspaso
        /// </summary>
        public async Task<TraspasoResultadoDto> CompletarTraspasoAsync(CompletarTraspasoDto dto)
        {
            try
            {
                var url = $"{_baseUrl}/Traspasos/completar-traspaso";
                System.Diagnostics.Debug.WriteLine($"📤 Completando traspaso: {dto.IdTraspaso}");
                System.Diagnostics.Debug.WriteLine($"📦 Detalles a completar: {dto.Detalles?.Count ?? 0}");

                var response = await _httpClient.PostAsJsonAsync(url, dto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<TraspasoResultadoDto>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ Traspaso completado: {resultado?.Mensaje}");
                    StatusMessage = resultado?.Mensaje ?? "Traspaso completado exitosamente";
                    return resultado;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error al completar traspaso: {error}");

                    try
                    {
                        var errorDto = await response.Content.ReadFromJsonAsync<TraspasoResultadoDto>(_jsonOptions);
                        StatusMessage = errorDto?.Mensaje ?? "Error al completar traspaso";
                        return errorDto;
                    }
                    catch
                    {
                        StatusMessage = "Error al completar traspaso";
                        return new TraspasoResultadoDto
                        {
                            Exito = false,
                            Mensaje = error
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
                return new TraspasoResultadoDto
                {
                    Exito = false,
                    Mensaje = ex.Message
                };
            }
        }

        /// <summary>
        /// Prueba la conexión con el endpoint de traspasos
        /// </summary>
        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                var url = $"{_baseUrl}/Traspasos/test";
                System.Diagnostics.Debug.WriteLine($"📡 Probando conexión: {url}");

                var response = await _httpClient.GetAsync(url);
                var isSuccess = response.IsSuccessStatusCode;

                System.Diagnostics.Debug.WriteLine(isSuccess
                    ? "✅ Conexión exitosa con API de Traspasos"
                    : $"❌ No se pudo conectar: {response.StatusCode}");

                return isSuccess;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al probar conexión: {ex.Message}");
                return false;
            }
        }

        public async Task<TraspasoResultadoDto> RechazarDetalleAsync(RechazarDetalleDto dto)
        {
            try
            {
                var url = $"{_baseUrl}/Traspasos/rechazar-detalle";
                System.Diagnostics.Debug.WriteLine($"📤 Rechazando detalle: {dto.IdTraspasoDetalle}");

                var response = await _httpClient.PostAsJsonAsync(url, dto, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadFromJsonAsync<TraspasoResultadoDto>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ Detalle rechazado: {resultado?.Mensaje}");
                    StatusMessage = resultado?.Mensaje ?? "Detalle rechazado exitosamente";
                    return resultado;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error al rechazar detalle: {error}");

                    try
                    {
                        var errorDto = await response.Content.ReadFromJsonAsync<TraspasoResultadoDto>(_jsonOptions);
                        StatusMessage = errorDto?.Mensaje ?? "Error al rechazar detalle";
                        return errorDto;
                    }
                    catch
                    {
                        StatusMessage = "Error al rechazar detalle";
                        return new TraspasoResultadoDto
                        {
                            Exito = false,
                            Mensaje = error
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
                StatusMessage = $"Error: {ex.Message}";
                return new TraspasoResultadoDto
                {
                    Exito = false,
                    Mensaje = ex.Message
                };
            }
        }

        public async Task<List<TraspasoPendienteDto>> ObtenerTodosTraspasos()
        {
            try
            {
                var url = $"{_baseUrl}/Traspasos/todos";
                System.Diagnostics.Debug.WriteLine($"📡 Obteniendo todos los traspasos desde: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var traspasos = await response.Content.ReadFromJsonAsync<List<TraspasoPendienteDto>>(_jsonOptions);
                    System.Diagnostics.Debug.WriteLine($"✅ {traspasos?.Count ?? 0} traspasos obtenidos");
                    return traspasos ?? new List<TraspasoPendienteDto>();
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                    return new List<TraspasoPendienteDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
                return new List<TraspasoPendienteDto>();
            }
        }
    }
}