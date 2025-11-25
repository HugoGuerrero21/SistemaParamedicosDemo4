using SistemaParamedicosDemo4.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Service
{
    public class InventarioApiService
    {
        private readonly HttpClient _httpClient;
        private const string BASE_URL = "https://localhost:7285/api";
        public string StatusMessage { get; set; }

        // ⭐ USA ESTA SI ESTÁS EN EMULADOR ANDROID
        // private const string BASE_URL = "https://10.0.2.2:7285/api";
        public InventarioApiService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            System.Diagnostics.Debug.WriteLine($"✓ InventarioApiService inicializado con URL: {BASE_URL}");
        }

        //Obtener todas las existencias.

        public async Task<List<InventarioDTO>> ObtenerExistenciasAsync()
        {
            try
            {
                var url = $"{BASE_URL}/inventario/existencias";
                System.Diagnostics.Debug.WriteLine($"📡 Llamando a URL: {url}");

                var response = await _httpClient.GetAsync(url);

                System.Diagnostics.Debug.WriteLine($"📡 Status Code: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"📡 Reason: {response.ReasonPhrase}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"📡 Respuesta recibida (primeros 300 chars): {content.Substring(0, Math.Min(300, content.Length))}...");

                    var inventario = await response.Content.ReadFromJsonAsync<List<InventarioDTO>>();
                    System.Diagnostics.Debug.WriteLine($"✓ {inventario?.Count ?? 0} productos deserializados correctamente");

                    return inventario ?? new List<InventarioDTO>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error HTTP: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"❌ Contenido: {errorContent}");
                    return new List<InventarioDTO>();
                }
            }
            catch (HttpRequestException ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ InnerException: {ex.InnerException?.Message}");
                return new List<InventarioDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error general: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return new List<InventarioDTO>();
            }
        }

        //Buscar productos por texto
        public async Task<List<InventarioDTO>> BuscarProductosAsync(string textoBusqueda)
        {
            try
            {
                // Obtener todos los productos y filtrar localmente
                var todosLosProductos = await ObtenerExistenciasAsync();

                if (string.IsNullOrWhiteSpace(textoBusqueda))
                    return todosLosProductos;

                var busqueda = textoBusqueda.ToLower();
                var productosFiltrados = todosLosProductos
                    .Where(p =>
                        p.NombreDelProducto.ToLower().Contains(busqueda) ||
                        p.Producto.ToLower().Contains(busqueda) ||
                        (p.Marca?.ToLower().Contains(busqueda) ?? false) ||
                        (p.Descripcion?.ToLower().Contains(busqueda) ?? false))
                    .ToList();

                System.Diagnostics.Debug.WriteLine($"✓ {productosFiltrados.Count} productos encontrados");
                return productosFiltrados;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al buscar: {ex.Message}");
                return new List<InventarioDTO>();
            }
        }


        //Registrar salidas del servidor
        public async Task<bool> RegistrarSalidaAsync(string idEmpleado, string idUsuario, List<ProductoSalidaDTO> productos)
        {
            try
            {
                var request = new
                {
                    idEmpleado = idEmpleado,
                    idUsuario = idUsuario,
                    productos = productos.Select(p => new
                    {
                        idProducto = p.IdProducto,
                        cantidad = (float)p.Cantidad  // ← Cast a float
                    }).ToList()
                };

                var jsonContent = JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"📤 Registrando salida en API para empleado: {idEmpleado}");
                System.Diagnostics.Debug.WriteLine($"📦 Productos: {productos.Count}");
                System.Diagnostics.Debug.WriteLine($"📡 JSON enviado: {jsonContent}");  

                var url = $"{BASE_URL}/inventario/salida";  
                System.Diagnostics.Debug.WriteLine($"📡 URL: {url}");  

                var response = await _httpClient.PostAsync(url, content);  

                System.Diagnostics.Debug.WriteLine($"📡 Status: {response.StatusCode}");  

                if (response.IsSuccessStatusCode)
                {
                    var resultado = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"✅ Salida registrada: {resultado}");
                    return true;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"❌ Error al registrar salida: {error}");
                    StatusMessage = $"Error al registrar salida: {error}";
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Excepción: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");  // ← AÑADE ESTO
                StatusMessage = $"Error: {ex.Message}";
                return false;
            }
        }

        // ⭐ CLASE AUXILIAR PARA LA SALIDA
        public class ProductoSalidaDTO
        {
            public string IdProducto { get; set; }
            public double Cantidad { get; set; }
        }

        //Probar conexión
        public async Task<bool> ProbarConexionAsync()
        {
            try
            {
                var url = $"{BASE_URL}/inventario/existencias";
                System.Diagnostics.Debug.WriteLine($"📡 Probando conexión: {url}");

                var response = await _httpClient.GetAsync(url);
                var isSuccess = response.IsSuccessStatusCode;

                System.Diagnostics.Debug.WriteLine(isSuccess
                    ? "✓ Conexión exitosa con API de inventario"
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
