using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
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
    public class TraspasoViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Servicios y Repositorios
        private TraspasoApiService _traspasoApiService;
        private TraspasoRepository _traspasoRepo;
        private CancellationTokenSource _cancellationTokenSource;
        #endregion

        #region Properties
        // Colecciones
        public ObservableCollection<TraspasoPendienteDto> TraspasosPendientes { get; set; }
        private List<TraspasoPendienteDto> _todosTraspasos;

        #region Properties para Tabs
        public string VistaActual { get; set; } = "Pendientes"; // "Pendientes" o "Historial"

        public string ColorTabPendientes => VistaActual == "Pendientes" ? "#2E86AB" : "#E9ECEF";
        public string ColorTextoTabPendientes => VistaActual == "Pendientes" ? "#FFFFFF" : "#6C757D";
        public string ColorTabHistorial => VistaActual == "Historial" ? "#2E86AB" : "#E9ECEF";
        public string ColorTextoTabHistorial => VistaActual == "Historial" ? "#FFFFFF" : "#6C757D";

        public bool MostrandoPendientes => VistaActual == "Pendientes";
        public bool MostrandoHistorial => VistaActual == "Historial";
        #endregion

        // Estados
        public bool IsCargando { get; set; } = false;
        public string MensajeEstado { get; set; }
        public bool TieneTraspasos { get; set; }

        // Estadísticas
        public int TotalTraspasos { get; set; }
        public int TraspasosPendientesCount { get; set; }
        public int ProductosTotales { get; set; }
        public int ProductosCompletados { get; set; }
        public int ProductosPendientes { get; set; }

        // Control de carga
        private bool _datosCargados = false;
        #endregion

        #region Commands
        public ICommand CargarTraspasosCommand { get; }
        public ICommand RefrescarCommand { get; }
        public ICommand ToggleDetalleCommand { get; }
        public ICommand CompletarDetalleCommand { get; }
        public ICommand CompletarTodoCommand { get; }
        public ICommand AutocompletarCantidadesCommand { get; }

        public ICommand ConfirmarRecepcionCommand { get; }
        public ICommand RechazarRecepcionCommand { get; }
        public ICommand CambiarVistaPendientesCommand { get; }
        public ICommand CambiarVistaHistorialCommand { get; }
        #endregion

        public TraspasoViewModel()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                // Inicializar servicios
                _traspasoApiService = new TraspasoApiService();
                _traspasoRepo = new TraspasoRepository();

                // Inicializar colecciones
                TraspasosPendientes = new ObservableCollection<TraspasoPendienteDto>();
                _todosTraspasos = new List<TraspasoPendienteDto>();

                VistaActual = "Pendientes";

                // Inicializar comandos
                CargarTraspasosCommand = new Command(async () => await CargarTraspasosAsync());
                RefrescarCommand = new Command(async () => await RefrescarTraspasosAsync());
                ToggleDetalleCommand = new Command<TraspasoPendienteDto>(ToggleDetalle);
                CompletarDetalleCommand = new Command<TraspasoDetalleDto>(async (detalle) => await CompletarDetalleAsync(detalle), (detalle) => CanCompletarDetalle(detalle));
                CompletarTodoCommand = new Command<TraspasoPendienteDto>(async (traspaso) => await CompletarTodoTraspasoAsync(traspaso), (traspaso) => CanCompletarTodo(traspaso));
                AutocompletarCantidadesCommand = new Command<TraspasoPendienteDto>(AutocompletarCantidades);
                ConfirmarRecepcionCommand = new Command<TraspasoDetalleDto>(async (detalle) => await ConfirmarRecepcionAsync(detalle), (detalle) => detalle?.Completada == 0);
                RechazarRecepcionCommand = new Command<TraspasoDetalleDto>(async (detalle) => await RechazarRecepcionAsync(detalle), (detalle) => detalle?.Completada == 0);
                CambiarVistaPendientesCommand = new Command(CambiarVistaPendientes);
                CambiarVistaHistorialCommand = new Command(CambiarVistaHistorial);

                System.Diagnostics.Debug.WriteLine("✓ TraspasoViewModel inicializado");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en constructor: {ex.Message}");
            }
        }

        /// <summary>
        /// Inicializa la vista cuando aparece
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
                    await CargarTraspasosAsync(_cancellationTokenSource.Token);
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

        private void CambiarVistaPendientes()
        {
            System.Diagnostics.Debug.WriteLine("🔄 Cambiando a vista PENDIENTES");

            VistaActual = "Pendientes";

            // Actualizar propiedades de UI
            OnPropertyChanged(nameof(VistaActual));
            OnPropertyChanged(nameof(ColorTabPendientes));
            OnPropertyChanged(nameof(ColorTextoTabPendientes));
            OnPropertyChanged(nameof(ColorTabHistorial));
            OnPropertyChanged(nameof(ColorTextoTabHistorial));
            OnPropertyChanged(nameof(MostrandoPendientes));
            OnPropertyChanged(nameof(MostrandoHistorial));

            // Actualizar vista
            ActualizarVista();
        }

        private void CambiarVistaHistorial()
        {
            System.Diagnostics.Debug.WriteLine("🔄 Cambiando a vista HISTORIAL");

            VistaActual = "Historial";

            // Actualizar propiedades de UI
            OnPropertyChanged(nameof(VistaActual));
            OnPropertyChanged(nameof(ColorTabPendientes));
            OnPropertyChanged(nameof(ColorTextoTabPendientes));
            OnPropertyChanged(nameof(ColorTabHistorial));
            OnPropertyChanged(nameof(ColorTextoTabHistorial));
            OnPropertyChanged(nameof(MostrandoPendientes));
            OnPropertyChanged(nameof(MostrandoHistorial));

            // Actualizar vista
            ActualizarVista();
        }

        private void ActualizarVista()
        {
            if (_todosTraspasos == null || _todosTraspasos.Count == 0)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ No hay traspasos para mostrar");
                return;
            }

            var traspasosFiltrados = VistaActual == "Pendientes"
                ? _todosTraspasos.Where(t => t.Status == 0).ToList()  // Pendientes
                : _todosTraspasos.Where(t => t.Status == 1).ToList(); // Completados

            System.Diagnostics.Debug.WriteLine($"📊 Vista: {VistaActual}");
            System.Diagnostics.Debug.WriteLine($"📊 Total traspasos: {_todosTraspasos.Count}");
            System.Diagnostics.Debug.WriteLine($"📊 Traspasos filtrados: {traspasosFiltrados.Count}");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                TraspasosPendientes.Clear();

                foreach (var traspaso in traspasosFiltrados)
                {
                    System.Diagnostics.Debug.WriteLine($"  • Traspaso {traspaso.IdTraspaso} - Status: {traspaso.Status} ({traspaso.StatusTexto})");
                    TraspasosPendientes.Add(traspaso);
                }

                OnPropertyChanged(nameof(TraspasosPendientes));
            });

            // Actualizar mensaje
            if (VistaActual == "Pendientes")
            {
                MensajeEstado = traspasosFiltrados.Count > 0
                    ? $"{traspasosFiltrados.Count} traspasos pendientes"
                    : "No hay traspasos pendientes";
            }
            else
            {
                MensajeEstado = traspasosFiltrados.Count > 0
                    ? $"{traspasosFiltrados.Count} traspasos completados"
                    : "No hay historial de traspasos";
            }
        }

        /// <summary>
        /// Carga los traspasos pendientes desde la API
        /// </summary>
        private async Task CargarTraspasosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsCargando = true;
                MensajeEstado = "Cargando traspasos...";

                // Cargar TODOS los traspasos (pendientes Y completados)
                var traspasos = await _traspasoApiService.ObtenerTodosTraspasos();

                if (traspasos == null || traspasos.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("ℹ️ No hay traspasos");
                    MensajeEstado = "No hay traspasos";
                    TieneTraspasos = false;
                    _datosCargados = true;
                    return;
                }

                // Guardar en memoria
                _todosTraspasos = traspasos;
                _datosCargados = true;

                System.Diagnostics.Debug.WriteLine($"📦 Total traspasos cargados: {_todosTraspasos.Count}");

                // Contar por status
                var pendientes = _todosTraspasos.Count(t => t.Status == 0);
                var completados = _todosTraspasos.Count(t => t.Status == 1);

                System.Diagnostics.Debug.WriteLine($"  • Pendientes (Status=0): {pendientes}");
                System.Diagnostics.Debug.WriteLine($"  • Completados (Status=1): {completados}");

                // Calcular estadísticas
                CalcularEstadisticas();

                // Actualizar UI
                if (!cancellationToken.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        ActualizarVista(); // Filtrar según vista actual
                    });

                    TieneTraspasos = true;
                }

                System.Diagnostics.Debug.WriteLine($"✅ Traspasos cargados correctamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar traspasos: {ex.Message}");
                MensajeEstado = "Error al cargar traspasos";
                TieneTraspasos = false;
            }
            finally
            {
                IsCargando = false;
            }
        }

        /// <summary>
        /// Refresca los traspasos desde la API
        /// </summary>
        private async Task RefrescarTraspasosAsync()
        {
            _datosCargados = false;
            await CargarTraspasosAsync();
        }

        /// <summary>
        /// Expande/colapsa los detalles de un traspaso
        /// </summary>
        private void ToggleDetalle(TraspasoPendienteDto traspaso)
        {
            if (traspaso != null)
            {
                traspaso.Expandido = !traspaso.Expandido;
            }
        }

        /// <summary>
        /// Autocompleta las cantidades a recibir con las cantidades faltantes
        /// </summary>
        private void AutocompletarCantidades(TraspasoPendienteDto traspaso)
        {
            if (traspaso?.Detalles == null) return;

            foreach (var detalle in traspaso.Detalles)
            {
                if (detalle.Completada == 0)
                {
                    detalle.CantidadARecibir = detalle.CantidadFaltante;
                }
            }

            System.Diagnostics.Debug.WriteLine($"✓ Cantidades autocompletadas para traspaso {traspaso.IdTraspaso}");
        }

        /// <summary>
        /// Valida si se puede completar un detalle
        /// </summary>
        private bool CanCompletarDetalle(TraspasoDetalleDto detalle)
        {
            return detalle != null &&
                   detalle.Completada == 0 &&
                   detalle.CantidadARecibir > 0;
        }

        /// <summary>
        /// Completa un detalle individual
        /// </summary>
        private async Task CompletarDetalleAsync(TraspasoDetalleDto detalle)
        {
            if (detalle == null || detalle.CantidadARecibir <= 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Debe ingresar una cantidad válida mayor a 0",
                    "OK");
                return;
            }

            // Validar si excede la cantidad
            if (detalle.ExcedeCantidad)
            {
                bool continuar = await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Cantidad Excedida",
                    $"La cantidad a recibir ({detalle.CantidadARecibir}) más lo ya recibido ({detalle.CantidadRecibida}) " +
                    $"excede la cantidad esperada ({detalle.Cantidad}).\n\n" +
                    $"Exceso: +{(detalle.CantidadRecibida + detalle.CantidadARecibir) - detalle.Cantidad} unidades\n\n" +
                    $"¿Desea continuar?",
                    "Sí", "No");

                if (!continuar) return;
            }

            try
            {
                IsCargando = true;
                MensajeEstado = $"Completando {detalle.IdProducto}...";

                var idUsuario = Preferences.Get("IdUsuario", string.Empty);

                var dto = new CompletarDetalleDto
                {
                    IdTraspasoDetalle = detalle.IdTraspasoDetalle,
                    CantidadRecibida = detalle.CantidadARecibir,
                    IdUsuarioReceptor = idUsuario
                };

                var resultado = await _traspasoApiService.CompletarDetalleAsync(dto);

                if (resultado?.Exito == true)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        resultado.Mensaje,
                        "OK");

                    // Recargar traspasos
                    await RefrescarTraspasosAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        resultado?.Mensaje ?? "No se pudo completar el detalle",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al completar detalle: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al completar el detalle",
                    "OK");
            }
            finally
            {
                IsCargando = false;
                MensajeEstado = string.Empty;
            }
        }

        /// <summary>
        /// Valida si se puede completar todo el traspaso
        /// </summary>
        private bool CanCompletarTodo(TraspasoPendienteDto traspaso)
        {
            return traspaso?.Detalles?.Any(d => d.Completada == 0 && d.CantidadARecibir > 0) == true;
        }

        /// <summary>
        /// Completa todos los detalles pendientes de un traspaso
        /// </summary>
        private async Task CompletarTodoTraspasoAsync(TraspasoPendienteDto traspaso)
        {
            if (traspaso?.Detalles == null) return;

            var detallesPorCompletar = traspaso.Detalles
                .Where(d => d.Completada == 0 && d.CantidadARecibir > 0)
                .ToList();

            if (detallesPorCompletar.Count == 0)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Advertencia",
                    "No hay productos pendientes con cantidad a recibir",
                    "OK");
                return;
            }

            // Confirmar
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Recepción",
                $"¿Desea recibir {detallesPorCompletar.Count} productos del traspaso {traspaso.IdTraspaso}?",
                "Sí", "No");

            if (!confirmar) return;

            try
            {
                IsCargando = true;
                MensajeEstado = $"Completando traspaso {traspaso.IdTraspaso}...";

                var idUsuario = Preferences.Get("IdUsuario", string.Empty);

                var dto = new CompletarTraspasoDto
                {
                    IdTraspaso = traspaso.IdTraspaso,
                    IdUsuarioReceptor = idUsuario,
                    Detalles = detallesPorCompletar.Select(d => new DetalleRecepcionDto
                    {
                        IdTraspasoDetalle = d.IdTraspasoDetalle,
                        CantidadRecibida = d.CantidadARecibir
                    }).ToList()
                };

                var resultado = await _traspasoApiService.CompletarTraspasoAsync(dto);

                if (resultado?.Exito == true)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Éxito",
                        resultado.Mensaje,
                        "OK");

                    // Recargar traspasos
                    await RefrescarTraspasosAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        resultado?.Mensaje ?? "No se pudo completar el traspaso",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al completar traspaso: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al completar el traspaso",
                    "OK");
            }
            finally
            {
                IsCargando = false;
                MensajeEstado = string.Empty;
            }
        }

        /// <summary>
        /// Calcula las estadísticas del inventario
        /// </summary>
        private void CalcularEstadisticas()
        {
            if (_todosTraspasos == null || _todosTraspasos.Count == 0)
            {
                TotalTraspasos = 0;
                TraspasosPendientesCount = 0;
                ProductosTotales = 0;
                ProductosCompletados = 0;
                ProductosPendientes = 0;
                return;
            }

            // Contar solo traspasos PENDIENTES (Status == 0)
            var traspasosPendientes = _todosTraspasos.Where(t => t.Status == 0).ToList();

            TotalTraspasos = traspasosPendientes.Count;
            TraspasosPendientesCount = traspasosPendientes.Count;
            ProductosTotales = traspasosPendientes.Sum(t => t.TotalProductos);
            ProductosCompletados = traspasosPendientes.Sum(t => t.ProductosCompletados);
            ProductosPendientes = ProductosTotales - ProductosCompletados;

            System.Diagnostics.Debug.WriteLine($"📊 Estadísticas actualizadas:");
            System.Diagnostics.Debug.WriteLine($"  • Traspasos Pendientes: {TraspasosPendientesCount}");
            System.Diagnostics.Debug.WriteLine($"  • Productos Totales: {ProductosTotales}");
            System.Diagnostics.Debug.WriteLine($"  • Completados: {ProductosCompletados}");
            System.Diagnostics.Debug.WriteLine($"  • Pendientes: {ProductosPendientes}");
        }

        /// <summary>
        /// Confirma la recepción de un producto (botón ✓)
        /// </summary>
        /// <summary>
        /// Confirma la recepción COMPLETA de un producto (botón ✓)
        /// Marca que SÍ llegó TODO lo que faltaba
        /// </summary>
        private async Task ConfirmarRecepcionAsync(TraspasoDetalleDto detalle)
        {
            if (detalle == null) return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "✅ Confirmar Recepción Completa",
                $"¿Confirma que SÍ recibió TODO el producto?\n\n" +
                $"📦 {detalle.NombreProducto}\n" +
                $"📊 Cantidad a recibir: {detalle.CantidadFaltante}\n\n" +
                $"Esto marcará el producto como completado.",
                "Sí, Recibido",
                "Cancelar");

            if (!confirmar) return;

            try
            {
                // Autocompletar con la cantidad faltante
                detalle.CantidadARecibir = detalle.CantidadFaltante;

                // Ejecutar el comando de completar
                await CompletarDetalleAsync(detalle);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al confirmar recepción: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al confirmar la recepción",
                    "OK");
            }
        }

        /// <summary>
        /// Rechaza la recepción de un producto (botón ✕)
        /// Marca que NO llegó NADA (deja el producto como pendiente)
        /// </summary>
        private async Task RechazarRecepcionAsync(TraspasoDetalleDto detalle)
        {
            if (detalle == null) return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "❌ Producto No Recibido",
                $"¿Confirma que este producto NO llegó?\n\n" +
                $"📦 {detalle.NombreProducto}\n" +
                $"📊 Cantidad esperada: {detalle.CantidadFaltante}\n\n" +
                $"⚠️ IMPORTANTE:\n" +
                $"• El producto se marcará como completado\n" +
                $"• NO se generará entrada en inventario\n" +
                $"• NO podrá recibirlo después",
                "Sí, No llegó",
                "Cancelar");

            if (!confirmar) return;

            try
            {
                IsCargando = true;
                MensajeEstado = $"Rechazando {detalle.IdProducto}...";

                var idUsuario = Preferences.Get("IdUsuario", string.Empty);

                var dto = new RechazarDetalleDto
                {
                    IdTraspasoDetalle = detalle.IdTraspasoDetalle,
                    IdUsuarioReceptor = idUsuario
                };

                var resultado = await _traspasoApiService.RechazarDetalleAsync(dto);

                if (resultado?.Exito == true)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "✅ Producto Rechazado",
                        $"{resultado.Mensaje}\n\n" +
                        $"El producto quedó registrado como NO recibido.",
                        "OK");

                    // Recargar traspasos
                    await RefrescarTraspasosAsync();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "❌ Error",
                        resultado?.Mensaje ?? "No se pudo rechazar el producto",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al rechazar producto: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Ocurrió un error al rechazar el producto",
                    "OK");
            }
            finally
            {
                IsCargando = false;
                MensajeEstado = string.Empty;
            }
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

            _traspasoApiService = null;
            _traspasoRepo = null;
        }
        #endregion
    }
}