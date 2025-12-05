using SistemaParamedicosDemo4.DTOS;
using SistemaParamedicosDemo4.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Service
{
    public class EmpleadoApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public string StatusMessage { get; set; }

        public EmpleadoApiService()
        {
            _httpClient = ApiConfiguration.CreateHttpClient();
            _baseUrl = ApiConfiguration.BaseUrl;

            System.Diagnostics.Debug.WriteLine($"✓ EmpleadoApiService inicializado con URL: {_baseUrl}");
        }

        /// <summary>
        /// Obtiene todos los empleados activos desde la API
        /// </summary>
        public async Task<List<EmpleadoDto>> ObtenerEmpleadosActivosAsync()
        {
            try
            {
                var url = $"{_baseUrl}/Empleados/activos";
                System.Diagnostics.Debug.WriteLine($"📡 Llamando a URL: {url}");

                var response = await _httpClient.GetAsync(url);

                System.Diagnostics.Debug.WriteLine($"📡 Status Code: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"📡 Reason: {response.ReasonPhrase}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📡 Respuesta recibida (primeros 300 chars): {content.Substring(0, Math.Min(300, content.Length))}...");

                    var empleados = await response.Content.ReadFromJsonAsync<List<EmpleadoDto>>();
                    System.Diagnostics.Debug.WriteLine($"✓ {empleados?.Count ?? 0} empleados deserializados correctamente");

                    return empleados ?? new List<EmpleadoDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"❌ Contenido: {errorContent}");
                    return new List<EmpleadoDto>();
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ InnerException: {ex.InnerException?.Message}");
                return new List<EmpleadoDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error general: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return new List<EmpleadoDto>();
            }
        }

        /// <summary>
        /// ⭐ NUEVO: Obtiene todos los puestos desde la API
        /// </summary>
        public async Task<List<PuestoDTO>> ObtenerPuestosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/Puestos");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PuestoDTO>>();
                }

                return new List<PuestoDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error API Puestos: {ex.Message}");
                return new List<PuestoDTO>();
            }
        }

        /// <summary>
        /// Busca empleados por texto
        /// </summary>
        public async Task<List<EmpleadoDto>> BuscarEmpleadosAsync(string textoBusqueda)
        {
            try
            {
                // ⭐ CORREGIDO: Usar query parameter en lugar de ruta
                var url = $"{_baseUrl}/Empleados/buscar?texto={Uri.EscapeDataString(textoBusqueda)}";
                System.Diagnostics.Debug.WriteLine($"📡 Buscando en: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var empleados = await response.Content.ReadFromJsonAsync<List<EmpleadoDto>>();
                    System.Diagnostics.Debug.WriteLine($"✓ {empleados?.Count ?? 0} empleados encontrados");
                    return empleados ?? new List<EmpleadoDto>();
                }

                System.Diagnostics.Debug.WriteLine($"❌ Error al buscar: {response.StatusCode}");
                return new List<EmpleadoDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al buscar: {ex.Message}");
                return new List<EmpleadoDto>();
            }
        }

        /// <summary>
        /// Obtiene un empleado por ID
        /// </summary>
        public async Task<EmpleadoDto> ObtenerEmpleadoPorIdAsync(string idEmpleado)
        {
            try
            {
                var url = $"{_baseUrl}/Empleados/{idEmpleado}";
                System.Diagnostics.Debug.WriteLine($"📡 Obteniendo empleado: {url}");

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var empleado = await response.Content.ReadFromJsonAsync<EmpleadoDto>();
                    System.Diagnostics.Debug.WriteLine($"✓ Empleado {idEmpleado} obtenido");
                    return empleado;
                }

                System.Diagnostics.Debug.WriteLine($"❌ Error: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Prueba la conexión con la API
        /// </summary>
        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                var url = $"{_baseUrl}/Empleados/test";
                System.Diagnostics.Debug.WriteLine($"📡 Probando conexión: {url}");

                var response = await _httpClient.GetAsync(url);
                var isSuccess = response.IsSuccessStatusCode;

                System.Diagnostics.Debug.WriteLine(isSuccess
                    ? "✓ Conexión exitosa con API de empleados"
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