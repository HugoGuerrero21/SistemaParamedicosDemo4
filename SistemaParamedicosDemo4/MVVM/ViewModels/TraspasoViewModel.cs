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
using System.Text.Json; // ✅ Necesario para comparar y evitar reinicios
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Dispatching;

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
        public ObservableCollection<TraspasoPendienteDto> TraspasosPendientes { get; set; }
        private List<TraspasoPendienteDto> _todosTraspasos;

        // ✅ Hash para controlar cambios y EVITAR REINICIOS
        private string _ultimoHashDatos = string.Empty;

        #region Properties para Tabs
        public string VistaActual { get; set; } = "Pendientes";

        public string ColorTabPendientes => VistaActual == "Pendientes" ? "#2E86AB" : "#E9ECEF";
        public string ColorTextoTabPendientes => VistaActual == "Pendientes" ? "#FFFFFF" : "#6C757D";
        public string ColorTabHistorial => VistaActual == "Historial" ? "#2E86AB" : "#E9ECEF";
        public string ColorTextoTabHistorial => VistaActual == "Historial" ? "#FFFFFF" : "#6C757D";

        public bool MostrandoPendientes => VistaActual == "Pendientes";
        #endregion

        public bool IsCargando { get; set; } = false;
        public string MensajeEstado { get; set; }
        public bool TieneTraspasos { get; set; }

        // Estadísticas
        public int TotalTraspasos { get; set; }
        public int TraspasosPendientesCount { get; set; }
        public int ProductosTotales { get; set; }
        public int ProductosCompletados { get; set; }
        public int ProductosPendientes { get; set; }

        private IDispatcherTimer _autoRefreshTimer;
        #endregion

        #region Commands
        public ICommand CargarTraspasosCommand { get; }
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
                _traspasoApiService = new TraspasoApiService();
                _traspasoRepo = new TraspasoRepository();

                TraspasosPendientes = new ObservableCollection<TraspasoPendienteDto>();
                _todosTraspasos = new List<TraspasoPendienteDto>();

                // Comandos
                CargarTraspasosCommand = new Command(async () => await CargarTraspasosAsync(true)); // True = manual con spinner
                ToggleDetalleCommand = new Command<TraspasoPendienteDto>(ToggleDetalle);

                CompletarDetalleCommand = new Command<TraspasoDetalleDto>(async (detalle) => await CompletarDetalleAsync(detalle));

                // ✅ Comandos de acción (Check y Cruz)
                ConfirmarRecepcionCommand = new Command<TraspasoDetalleDto>(async (detalle) => await ConfirmarRecepcionAsync(detalle));
                RechazarRecepcionCommand = new Command<TraspasoDetalleDto>(async (detalle) => await RechazarRecepcionAsync(detalle));

                CambiarVistaPendientesCommand = new Command(CambiarVistaPendientes);
                CambiarVistaHistorialCommand = new Command(CambiarVistaHistorial);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error VM: {ex.Message}");
            }
        }

        public async Task InicializarVistaAsync()
        {
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                _cancellationTokenSource = new CancellationTokenSource();

            // Primera carga con spinner
            await CargarTraspasosAsync(mostrarSpinner: true);

            // Iniciar Timer
            IniciarTimerAutomatico();
        }

        private void IniciarTimerAutomatico()
        {
            DetenerActualizacionAutomatica();
            _autoRefreshTimer = Application.Current.Dispatcher.CreateTimer();
            _autoRefreshTimer.Interval = TimeSpan.FromSeconds(15);
            _autoRefreshTimer.Tick += async (s, e) => await CargarTraspasosAsync(mostrarSpinner: false);
            _autoRefreshTimer.Start();
        }

        public void DetenerActualizacionAutomatica()
        {
            if (_autoRefreshTimer != null)
            {
                _autoRefreshTimer.Stop();
                _autoRefreshTimer = null;
            }
        }

        private async Task CargarTraspasosAsync(bool mostrarSpinner = false)
        {
            try
            {
                if (mostrarSpinner)
                {
                    IsCargando = true;
                    MensajeEstado = "Buscando actualizaciones...";
                }

                // 1. Obtener datos de la API
                var nuevosTraspasos = await _traspasoApiService.ObtenerTodosTraspasos();

                if (nuevosTraspasos == null) return;

                // 2. ⭐ COMPARACIÓN INTELIGENTE (LA CLAVE PARA NO REINICIAR)
                // Convertimos la lista nueva a texto para ver si cambió algo
                var nuevoHash = JsonSerializer.Serialize(nuevosTraspasos);

                // Si no es spinner (es automático) y los datos son IGUALES, nos salimos.
                if (!mostrarSpinner && nuevoHash == _ultimoHashDatos)
                {
                    System.Diagnostics.Debug.WriteLine("💤 Polling: Sin cambios, vista intacta.");
                    return; // 🛑 AQUÍ SE DETIENE SI NO HAY NADA NUEVO
                }

                // Si llegamos aquí, es que SÍ hubo cambios. Actualizamos.
                _ultimoHashDatos = nuevoHash;
                _todosTraspasos = nuevosTraspasos;

                // 3. ⭐ PRESERVAR ESTADO DE EXPANSIÓN (Si el paramédico tenía abierto uno, lo reabrimos)
                var idsExpandidos = TraspasosPendientes
                    .Where(t => t.Expandido)
                    .Select(t => t.IdTraspaso)
                    .ToHashSet();

                foreach (var t in _todosTraspasos)
                {
                    if (idsExpandidos.Contains(t.IdTraspaso))
                    {
                        t.Expandido = true;
                    }
                }

                CalcularEstadisticas();

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    ActualizarVista();
                });

                if (mostrarSpinner) MensajeEstado = "Actualizado";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error carga: {ex.Message}");
            }
            finally
            {
                IsCargando = false;
            }
        }

        private void ActualizarVista()
        {
            if (_todosTraspasos == null) return;

            var filtrados = VistaActual == "Pendientes"
                ? _todosTraspasos.Where(t => t.Status == 0).ToList()
                : _todosTraspasos.Where(t => t.Status == 1).ToList();

            TraspasosPendientes.Clear();
            foreach (var item in filtrados)
            {
                TraspasosPendientes.Add(item);
            }

            TieneTraspasos = TraspasosPendientes.Count > 0;

            if (TieneTraspasos && VistaActual == "Pendientes")
                MensajeEstado = $"{TraspasosPendientes.Count} pendientes";
            else if (!TieneTraspasos)
                MensajeEstado = VistaActual == "Pendientes" ? "Al día" : "Sin historial";
        }

        // ✅ Confirmar Recepción (Botón Verde) con Dialog
        private async Task ConfirmarRecepcionAsync(TraspasoDetalleDto detalle)
        {
            if (detalle == null) return;

            // ⭐ AQUÍ ESTÁ EL CUADRO DE CONFIRMACIÓN QUE PEDISTE
            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Confirmar Recepción",
                $"¿Confirma que recibió el producto:\n\n{detalle.NombreProducto}?\n\nSe marcará como completado.",
                "Sí, confirmar",
                "Cancelar");

            if (!confirmar) return;

            // Lógica para completar
            detalle.CantidadARecibir = detalle.CantidadFaltante;
            await CompletarDetalleAsync(detalle);
        }

        // ✅ Rechazar Recepción (Botón Rojo)
        private async Task RechazarRecepcionAsync(TraspasoDetalleDto detalle)
        {
            if (detalle == null) return;

            bool confirmar = await Application.Current.MainPage.DisplayAlert(
                "Rechazar",
                $"¿El producto {detalle.NombreProducto} NO llegó?\nSe marcará como no recibido.",
                "Sí, rechazar", "Cancelar");

            if (!confirmar) return;

            try
            {
                IsCargando = true;
                var idUsuario = Preferences.Get("IdUsuario", string.Empty);
                var dto = new RechazarDetalleDto
                {
                    IdTraspasoDetalle = detalle.IdTraspasoDetalle,
                    IdUsuarioReceptor = idUsuario
                };

                var resultado = await _traspasoApiService.RechazarDetalleAsync(dto);

                if (resultado?.Exito == true)
                {
                    // Forzar actualización inmediata tras acción manual
                    _ultimoHashDatos = "";
                    await CargarTraspasosAsync(false);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", resultado?.Mensaje ?? "Error", "OK");
                }
            }
            finally
            {
                IsCargando = false;
            }
        }

        private async Task CompletarDetalleAsync(TraspasoDetalleDto detalle)
        {
            try
            {
                IsCargando = true;
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
                    // Forzar actualización inmediata tras acción manual
                    _ultimoHashDatos = "";
                    await CargarTraspasosAsync(false);
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", resultado?.Mensaje ?? "Error", "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally
            {
                IsCargando = false;
            }
        }

        // Resto de métodos auxiliares...
        private void ToggleDetalle(TraspasoPendienteDto traspaso)
        {
            if (traspaso != null) traspaso.Expandido = !traspaso.Expandido;
        }

        private void CambiarVistaPendientes()
        {
            VistaActual = "Pendientes";
            OnPropertyChanged(nameof(VistaActual));
            OnPropertyChanged(nameof(ColorTabPendientes));
            OnPropertyChanged(nameof(ColorTextoTabPendientes));
            OnPropertyChanged(nameof(ColorTabHistorial));
            OnPropertyChanged(nameof(ColorTextoTabHistorial));
            OnPropertyChanged(nameof(MostrandoPendientes));
            ActualizarVista();
        }

        private void CambiarVistaHistorial()
        {
            VistaActual = "Historial";
            OnPropertyChanged(nameof(VistaActual));
            OnPropertyChanged(nameof(ColorTabPendientes));
            OnPropertyChanged(nameof(ColorTextoTabPendientes));
            OnPropertyChanged(nameof(ColorTabHistorial));
            OnPropertyChanged(nameof(ColorTextoTabHistorial));
            OnPropertyChanged(nameof(MostrandoPendientes));
            ActualizarVista();
        }

        private void CalcularEstadisticas()
        {
            if (_todosTraspasos == null) return;
            var pendientes = _todosTraspasos.Where(t => t.Status == 0).ToList();
            TotalTraspasos = pendientes.Count;
            ProductosTotales = pendientes.Sum(t => t.TotalProductos);
            ProductosCompletados = pendientes.Sum(t => t.ProductosCompletados);
            ProductosPendientes = ProductosTotales - ProductosCompletados;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            DetenerActualizacionAutomatica();
            _cancellationTokenSource?.Dispose();
        }
    }
}