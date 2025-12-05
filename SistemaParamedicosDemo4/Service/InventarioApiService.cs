using SistemaParamedicosDemo4.DTOS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using SistemaParamedicosDemo4.Services;

namespace SistemaParamedicosDemo4.Service
{
    public class InventarioApiService
    {
        private readonly string _baseUrl;

        public string StatusMessage { get; set; }

        public InventarioApiService()
        {
            // ⭐ USAR HTTPCLIENT SINGLETON
            _baseUrl = ApiConfiguration.BaseUrl;
            System.Diagnostics.Debug.WriteLine($"✓ InventarioApiService inicializado con URL: {_baseUrl}");
        }

        //Obtener todas las existencias.

        public async Task<List<InventarioDTO>> ObtenerExistenciasAsync()
        {
            try
            {
                var url = $"{_baseUrl}/inventario/existencias";
                System.Diagnostics.Debug.WriteLine($"📡 Llamando a URL: {url}");

                // ⭐ USAR REINTENTOS
                var inventario = await ApiConfiguration.EjecutarConReintentos(async () =>
                {
                    var httpClient = ApiConfiguration.GetHttpClient();
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"📡 Respuesta recibida (primeros 300 chars): {content.Substring(0, Math.Min(300, content.Length))}...");
                        return await response.Content.ReadFromJsonAsync<List<InventarioDTO>>();
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"❌ Error HTTP: {response.StatusCode} - {errorContent}");
                        throw new HttpRequestException($"HTTP {response.StatusCode}: {errorContent}");
                    }
                });

                System.Diagnostics.Debug.WriteLine($"✓ {inventario?.Count ?? 0} productos deserializados correctamente");
                return inventario ?? new List<InventarioDTO>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error final: {ex.Message}");
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
                    idEmpleado,
                    idUsuario,
                    productos = productos.Select(p => new { idProducto = p.IdProducto, cantidad = (float)p.Cantidad }).ToList()
                };

                var jsonContent = System.Text.Json.JsonSerializer.Serialize(request);
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");

                System.Diagnostics.Debug.WriteLine($"📤 Registrando salida para empleado: {idEmpleado}");
                System.Diagnostics.Debug.WriteLine($"📦 Productos: {productos.Count}");

                var url = $"{_baseUrl}/inventario/salida";

                var resultado = await ApiConfiguration.EjecutarConReintentos(async () =>
                {
                    var httpClient = ApiConfiguration.GetHttpClient();
                    var response = await httpClient.PostAsync(url, content);

                    if (response.IsSuccessStatusCode)
                    {
                        var resultado = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"✅ Salida registrada");
                        return true;
                    }
                    else
                    {
                        var error = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"❌ Error: {error}");
                        throw new HttpRequestException($"HTTP {response.StatusCode}: {error}");
                    }
                });

                return resultado;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en RegistrarSalidaAsync: {ex.Message}");
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
                var url = $"{_baseUrl}/inventario/existencias";
                System.Diagnostics.Debug.WriteLine($"📡 Probando conexión: {url}");

                var isSuccess = await ApiConfiguration.EjecutarConReintentos(async () =>
                {
                    var httpClient = ApiConfiguration.GetHttpClient();
                    var response = await httpClient.GetAsync(url);
                    return response.IsSuccessStatusCode;
                }, maxIntentos: 2);

                System.Diagnostics.Debug.WriteLine(isSuccess
                    ? "✓ Conexión exitosa con API de inventario"
                    : "❌ No se pudo conectar");

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
