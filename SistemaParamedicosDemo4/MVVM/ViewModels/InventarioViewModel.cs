using PropertyChanged;
using SistemaParamedicosDemo4.DTOS;
using SistemaParamedicosDemo4.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json; // Importante
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class InventarioViewModel : INotifyPropertyChanged, IDisposable
    {
        private InventarioApiService _inventarioApiService;
        public ObservableCollection<InventarioDTO> ProductosInventario { get; set; }
        private List<InventarioDTO> _todosLosProductos;

        // Control de cambios
        private string _ultimoHashInventario = string.Empty;

        public string TextoBusqueda { get; set; }
        public string FiltroSeleccionado { get; set; } = "Todos";

        public bool IsCargando { get; set; }
        public string MensajeEstado { get; set; }

        // Estadísticas
        public int TotalProductos { get; set; }
        public int ProductosDisponibles { get; set; }
        public int ProductosStockBajo { get; set; }
        public int ProductosAgotados { get; set; }

        private IDispatcherTimer _autoRefreshTimer;
        private CancellationTokenSource _cts;

        // Paginación
        private const int PRODUCTOS_POR_PAGINA = 50;
        public int PaginaActual { get; set; } = 1;
        public bool HayMasPaginas { get; set; }

        public ICommand CargarInventarioCommand { get; }
        public ICommand BuscarCommand { get; }
        public ICommand AplicarFiltroCommand { get; }
        public ICommand CargarMasCommand { get; }

        public InventarioViewModel()
        {
            _inventarioApiService = new InventarioApiService();
            ProductosInventario = new ObservableCollection<InventarioDTO>();
            _todosLosProductos = new List<InventarioDTO>();

            CargarInventarioCommand = new Command(async () => await CargarInventarioAsync(true));
            BuscarCommand = new Command(ActualizarListaVisual);
            AplicarFiltroCommand = new Command<string>((f) => { FiltroSeleccionado = f; ActualizarListaVisual(); });
            CargarMasCommand = new Command(CargarMas);
        }

        public async Task InicializarVistaAsync()
        {
            await CargarInventarioAsync(true);
            IniciarTimer();
        }

        private void IniciarTimer()
        {
            DetenerTimer();
            _autoRefreshTimer = Application.Current.Dispatcher.CreateTimer();
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(15);
            _autoRefreshTimer.Tick += async (s, e) => await CargarInventarioAsync(false);
            _autoRefreshTimer.Start();
        }

        public void DetenerTimer()
        {
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer = null;
            }
        }

        private async Task CargarInventarioAsync(bool mostrarSpinner)
        {
            try
            {
                if (mostrarSpinner) IsCargando = true;

                var nuevosProductos = await _inventarioApiService.ObtenerExistenciasAsync();

                if (nuevosProductos != null && nuevosProductos.Count > 0)
                {
                    // ⭐ COMPARACIÓN DE CAMBIOS
                    var nuevoHash = JsonSerializer.Serialize(nuevosProductos);

                    // Si es actualización automática y los datos son idénticos, NO HACEMOS NADA
                    if (!mostrarSpinner && nuevoHash == _ultimoHashInventario)
                    {
                        System.Diagnostics.Debug.WriteLine("Inventario sin cambios, omitiendo renderizado.");
                        return;
                    }

                    _ultimoHashInventario = nuevoHash;
                    _todosLosProductos = nuevosProductos;

                    CalcularEstadisticas();

                    // Actualizar UI en hilo principal
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ActualizarListaVisual();
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error inventario: {ex.Message}");
            }
            finally
            {
                IsCargando = false;
            }
        }

        private void ActualizarListaVisual()
        {
            if (_todosLosProductos == null) return;

            var filtrados = _todosLosProductos.AsEnumerable();

            // Filtro Texto
            if (!string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                var t = TextoBusqueda.ToLower();
                filtrados = filtrados.Where(p =>
                    p.NombreDelProducto.ToLower().Contains(t) ||
                    p.Producto.ToLower().Contains(t));
            }

            // Filtro Estado
            filtrados = FiltroSeleccionado switch
            {
                "Disponibles" => filtrados.Where(p => p.Existencia > 10),
                "Stock Bajo" => filtrados.Where(p => p.StockBajo && !p.Agotado),
                "Agotados" => filtrados.Where(p => p.Agotado),
                _ => filtrados
            };

            var listaFinal = filtrados.ToList(); // Materializar lista completa filtrada

            // Paginación
            var pagina = listaFinal.Take(PaginaActual * PRODUCTOS_POR_PAGINA).ToList();

            ProductosInventario.Clear();
            foreach (var p in pagina) ProductosInventario.Add(p);

            HayMasPaginas = listaFinal.Count > ProductosInventario.Count;
            MensajeEstado = $"{ProductosInventario.Count} productos mostrados";
        }

        private void CargarMas()
        {
            if (HayMasPaginas)
            {
                PaginaActual++;
                ActualizarListaVisual();
            }
        }

        private void CalcularEstadisticas()
        {
            TotalProductos = _todosLosProductos.Count;
            ProductosDisponibles = _todosLosProductos.Count(p => p.Existencia > 10);
            ProductosStockBajo = _todosLosProductos.Count(p => p.StockBajo && !p.Agotado);
            ProductosAgotados = _todosLosProductos.Count(p => p.Agotado);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            DetenerTimer();
        }
    }
}