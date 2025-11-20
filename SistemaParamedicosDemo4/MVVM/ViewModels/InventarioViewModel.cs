using PropertyChanged;
using SistemaParamedicosDemo4.DTOS;
using SistemaParamedicosDemo4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class InventarioViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Servicios
        private InventarioApiService _inventarioApiService;
        #endregion

        #region Properties
        // Colecciones
        public ObservableCollection<InventarioDTO> ProductosInventario { get; set; }
        private List<InventarioDTO> _todosLosProductos;

        // Búsqueda y filtros
        public string TextoBusqueda { get; set; }
        public string FiltroSeleccionado { get; set; }
        public ObservableCollection<string> OpcionesFiltro { get; set; }

        // Estados
        public bool IsCargando { get; set; } = false;
        public string MensajeEstado { get; set; }
        public bool TieneProductos { get; set; }

        // Estadísticas
        public int TotalProductos { get; set; }
        public int ProductosDisponibles { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosAgotados { get; set; }

        // Control de cancelación
        private CancellationTokenSource _cancellationTokenSource;
        private bool _datosCargados = false;
        #endregion

        #region Commands
        public ICommand CargarInventarioCommand { get; }
        public ICommand BuscarCommand { get; }
        public ICommand RefrescarCommand { get; }
        public ICommand AplicarFiltroCommand { get; }
        public ICommand LimpiarBusquedaCommand { get; }
        #endregion

        public InventarioViewModel()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                // Inicializar servicio
                _inventarioApiService = new InventarioApiService();

                // Inicializar colecciones
                ProductosInventario = new ObservableCollection<InventarioDTO>();
                _todosLosProductos = new List<InventarioDTO>();

                // Opciones de filtro
                OpcionesFiltro = new ObservableCollection<string>
                {
                    "Todos",
                    "Disponibles",
                    "Stock Bajo",
                    "Agotados"
                };
                FiltroSeleccionado = "Todos";

                // Inicializar comandos
                CargarInventarioCommand = new Command(async () => await CargarInventarioAsync());
                BuscarCommand = new Command(Buscar);
                RefrescarCommand = new Command(async () => await RefrescarInventarioAsync());
                AplicarFiltroCommand = new Command<string>(AplicarFiltro);
                LimpiarBusquedaCommand = new Command(LimpiarBusqueda);

                // Suscribirse a cambios de propiedades
                PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(TextoBusqueda))
                    {
                        Buscar();
                    }
                };

                System.Diagnostics.Debug.WriteLine("✓ InventarioViewModel inicializado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en constructor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Se ejecuta cuando la vista aparece
        /// </summary>
        public async Task InicializarVistaAsync()
        {
            System.Diagnostics.Debug.WriteLine("🚀 InicializarVistaAsync EJECUTÁNDOSE");

            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            try
            {
                if (!_datosCargados)
                {
                    System.Diagnostics.Debug.WriteLine("🚀 Inventario no cargado, procediendo...");

                    // Probar conexión primero
                    var conexionExitosa = await _inventarioApiService.ProbarConexionAsync();
                    if (!conexionExitosa)
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ No hay conexión con la API");
                        MensajeEstado = "Sin conexión con el servidor";
                    }

                    await CargarInventarioAsync(_cancellationTokenSource.Token);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Inventario ya estaba cargado, saltando...");
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Inicialización cancelada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en InicializarVistaAsync: {ex.Message}");
            }
        }

        /// <summary>
        /// Carga el inventario desde la API
        /// </summary>
        private async Task CargarInventarioAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsCargando = true;
                MensajeEstado = "Cargando inventario desde la API...";

                // Cargar desde la API
                var inventarioDto = await _inventarioApiService.ObtenerExistenciasAsync();

                if (inventarioDto == null || inventarioDto.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ API sin datos");
                    MensajeEstado = "No se encontraron productos en el inventario";
                    TieneProductos = false;
                    return;
                }

                // Guardar en memoria
                _todosLosProductos = inventarioDto;
                TotalProductos = _todosLosProductos.Count;
                _datosCargados = true;

                // Calcular estadísticas
                CalcularEstadisticas();

                // Actualizar UI
                if (!cancellationToken.IsCancellationRequested)
                {
                    await ActualizarListaAsync(_todosLosProductos);
                    MensajeEstado = $"{TotalProductos} productos cargados";
                    TieneProductos = true;
                }

                System.Diagnostics.Debug.WriteLine($"✓ {TotalProductos} productos cargados");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Carga de inventario cancelada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                MensajeEstado = "Error al cargar inventario";
                TieneProductos = false;
            }
            finally
            {
                IsCargando = false;
                System.Diagnostics.Debug.WriteLine("🏁 CargarInventarioAsync finalizado");
            }
        }

        /// <summary>
        /// Refresca el inventario desde la API
        /// </summary>
        private async Task RefrescarInventarioAsync()
        {
            _datosCargados = false;
            await CargarInventarioAsync();
        }

        /// <summary>
        /// Busca productos en el inventario
        /// </summary>
        private async void Buscar()
        {
            if (_todosLosProductos == null || _todosLosProductos.Count == 0)
            {
                MensajeEstado = "Cargando inventario...";
                return;
            }

            try
            {
                IsCargando = true;

                if (string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    AplicarFiltro(FiltroSeleccionado);
                    return;
                }

                var busqueda = TextoBusqueda.ToLower();
                var productosFiltrados = _todosLosProductos.Where(p =>
                    p.NombreDelProducto.ToLower().Contains(busqueda) ||
                    p.Producto.ToLower().Contains(busqueda) ||
                    (p.Marca?.ToLower().Contains(busqueda) ?? false) ||
                    (p.Descripcion?.ToLower().Contains(busqueda) ?? false)
                ).ToList();

                // Aplicar también el filtro seleccionado
                productosFiltrados = AplicarFiltroALista(productosFiltrados, FiltroSeleccionado);

                await ActualizarListaAsync(productosFiltrados);

                MensajeEstado = productosFiltrados.Count == 0
                    ? "No se encontraron productos"
                    : $"{productosFiltrados.Count} productos encontrados";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en búsqueda: {ex.Message}");
                MensajeEstado = "Error al buscar";
            }
            finally
            {
                IsCargando = false;
            }
        }

        /// <summary>
        /// Aplica un filtro a la lista de productos
        /// </summary>
        private void AplicarFiltro(string filtro)
        {
            if (_todosLosProductos == null || _todosLosProductos.Count == 0)
                return;

            try
            {
                IsCargando = true;
                FiltroSeleccionado = filtro;

                var productosFiltrados = AplicarFiltroALista(_todosLosProductos, filtro);

                // Si hay búsqueda activa, filtrar también por texto
                if (!string.IsNullOrWhiteSpace(TextoBusqueda))
                {
                    var busqueda = TextoBusqueda.ToLower();
                    productosFiltrados = productosFiltrados.Where(p =>
                        p.NombreDelProducto.ToLower().Contains(busqueda) ||
                        p.Producto.ToLower().Contains(busqueda) ||
                        (p.Marca?.ToLower().Contains(busqueda) ?? false)
                    ).ToList();
                }

                MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    ProductosInventario.Clear();
                    foreach (var producto in productosFiltrados)
                    {
                        ProductosInventario.Add(producto);
                    }
                });

                MensajeEstado = $"{productosFiltrados.Count} productos ({filtro})";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al aplicar filtro: {ex.Message}");
            }
            finally
            {
                IsCargando = false;
            }
        }

        /// <summary>
        /// Método auxiliar para aplicar filtros
        /// </summary>
        private List<InventarioDTO> AplicarFiltroALista(List<InventarioDTO> productos, string filtro)
        {
            return filtro switch
            {
                "Disponibles" => productos.Where(p => p.Existencia > 10).ToList(),
                "Stock Bajo" => productos.Where(p => p.StockBajo && !p.Agotado).ToList(),
                "Agotados" => productos.Where(p => p.Agotado).ToList(),
                _ => productos // "Todos"
            };
        }

        /// <summary>
        /// Limpia la búsqueda
        /// </summary>
        private void LimpiarBusqueda()
        {
            TextoBusqueda = string.Empty;
            AplicarFiltro(FiltroSeleccionado);
        }

        /// <summary>
        /// Actualiza la lista de productos en la UI
        /// </summary>
        private async Task ActualizarListaAsync(List<InventarioDTO> productos)
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ProductosInventario.Clear();
                    foreach (var producto in productos)
                    {
                        ProductosInventario.Add(producto);
                    }
                    OnPropertyChanged(nameof(ProductosInventario));
                    System.Diagnostics.Debug.WriteLine($"✓ UI actualizada con {productos.Count} productos");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar UI: {ex.Message}");
            }
        }

        /// <summary>
        /// Calcula las estadísticas del inventario
        /// </summary>
        private void CalcularEstadisticas()
        {
            ProductosDisponibles = _todosLosProductos.Count(p => p.Existencia > 10);
            ProductosStockBajo = _todosLosProductos.Count(p => p.StockBajo && !p.Agotado);
            ProductosAgotados = _todosLosProductos.Count(p => p.Agotado);

            System.Diagnostics.Debug.WriteLine($"📊 Estadísticas:");
            System.Diagnostics.Debug.WriteLine($"  Total: {TotalProductos}");
            System.Diagnostics.Debug.WriteLine($"  Disponibles: {ProductosDisponibles}");
            System.Diagnostics.Debug.WriteLine($"  Stock Bajo: {ProductosStockBajo}");
            System.Diagnostics.Debug.WriteLine($"  Agotados: {ProductosAgotados}");
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            try
            {
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = null;
            }
            catch (ObjectDisposedException)
            {
                // Ya fue disposed, ignorar
            }

            _inventarioApiService = null;
        }
        #endregion
    }
}