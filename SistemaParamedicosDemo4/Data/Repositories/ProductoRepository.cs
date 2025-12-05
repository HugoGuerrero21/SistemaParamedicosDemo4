using SistemaParamedicosDemo4.MVVM.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SistemaParamedicosDemo4.Data.Repositories
{
    public class ProductoRepository
    {
        private SQLiteConnection Connection;
        public string StatusMessage { get; set; }

        public ProductoRepository()
        {
            try
            {
                // Usar la conexión compartida del DatabaseManager
                Connection = DatabaseManager.Instance.Connection;
                StatusMessage = "Repositorio de movimientos inicializado";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al inicializar la tabla{ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
            }
        }

        public List<ProductoModel> GetAllProductos()
        {
            try
            {
                return Connection.Table<ProductoModel>().ToList();
            }
            catch (Exception ex)
            {
                StatusMessage = "Imposible extraer todos los productos";
                return new List<ProductoModel>();
            }
        }

        public List<ProductoModel> GetProductoConsStock()
        {
            try
            {

                return Connection.Table<ProductoModel>().Where(stock => stock.CantidadDisponible > 0).ToList();
            }

            catch (Exception ex)
            {
                StatusMessage = "No hay productos";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return new List<ProductoModel>();
            }
        }

        // Solo la parte del método GetProductosById con mejor debugging

        public ProductoModel GetProductosById(string id)
        {
            try
            {
                var producto = Connection.Find<ProductoModel>(id);

                if (producto != null)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ Producto encontrado: {id} -> {producto.Nombre} {producto.Model}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Producto NO encontrado con ID: {id}");
                }

                return producto;
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo encontrar el producto{ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        public ProductoModel InsertarProducto(ProductoModel producto)
        {
            try
            {
                if (producto == null)
                {
                    StatusMessage = "El producto no puede ser null";
                    return null;
                }

                int result = Connection.Insert(producto);

                if (result > 0)
                {
                    StatusMessage = $"Producto insertado correctamente (ID: {producto.ProductoId})";

                    return producto;
                }
                else
                {
                    StatusMessage = "Error no se pudo ingresar el producto";
                    System.Diagnostics.Debug.WriteLine(StatusMessage);
                    return null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"No se pudo ingresar el producto{ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        public ProductoModel ActualizarProducto(ProductoModel producto)
        {
            try
            {
                if (producto == null)
                {
                    StatusMessage = "El producto no puede ser nulo";
                    return null;
                }

                int result = Connection.Update(producto);

                if (result > 0)
                {
                    StatusMessage = "Producto actualizado correctamente";
                    return producto; // Devuelves el producto actualizado
                }
                else
                {
                    StatusMessage = "No se encontró el producto a actualizar";
                    return null;
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar producto: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return null;
            }
        }

        public bool ActualizarStock(string claveProducto, double cantidadCambio)
        {
            try
            {
                var producto = Connection.Table<ProductoModel>()
                    .FirstOrDefault(p => p.ProductoId == claveProducto);

                if (producto == null)
                {
                    StatusMessage = $"Producto con clave {claveProducto} no encontrado.";
                    return false;
                }

                producto.CantidadDisponible += cantidadCambio; // sumar o restar según el caso
                Connection.Update(producto);
                StatusMessage = $"Stock actualizado correctamente para {producto.Nombre}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al actualizar stock: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }



        public bool InsertarProductosIniciales(List<ProductoModel> productos)
        {
            try
            {
                // Verificar si ya existen datos
                var existentes = GetAllProductos();
                if (existentes.Count > 0)
                {
                    StatusMessage = "Ya existen productos en la BD";
                    return false;
                }

                Connection.InsertAll(productos);
                StatusMessage = $"{productos.Count} productos insertados";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al insertar productos iniciales: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(StatusMessage);
                return false;
            }
        }

        public bool SincronizarProductosDesdeInventario(List<ProductoModel> productos)
        {
            try
            {
                if (productos == null || productos.Count == 0)
                {
                    StatusMessage = "No hay productos para sincronizar";
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"🔄 Sincronizando {productos.Count} productos del inventario...");

                foreach (var producto in productos)
                {
                    var existente = Connection.Find<ProductoModel>(producto.ProductoId);

                    if (existente != null)
                    {
                        // ⭐ ACTUALIZAR producto existente (stock, nombre, etc.)
                        existente.Nombre = producto.Nombre;
                        existente.Marca = producto.Marca;
                        existente.Descripcion = producto.Descripcion;
                        existente.NumeroPieza = producto.NumeroPieza;
                        existente.Foto = producto.Foto;
                        existente.CantidadDisponible = producto.CantidadDisponible;

                        Connection.Update(existente);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Actualizado: {producto.ProductoId}");
                    }
                    else
                    {
                        // ⭐ INSERTAR nuevo producto
                        Connection.Insert(producto);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Insertado: {producto.ProductoId}");
                    }
                }

                StatusMessage = $"{productos.Count} productos sincronizados";
                System.Diagnostics.Debug.WriteLine($"✅ {StatusMessage}");
                return true;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error al sincronizar productos: {ex.Message}";
                System.Diagnostics.Debug.WriteLine($"❌ {StatusMessage}");
                return false;
            }
        }
    }
}