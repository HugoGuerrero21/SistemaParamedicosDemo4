using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Models;
using SistemaParamedicosDemo4.Service;
using SistemaParamedicosDemo4.DTOS;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ConsultaViewModel : INotifyPropertyChanged, IDisposable
    {
        #region Repositorios y Servicios (INSTANCIAS NORMALES - NO ESTÁTICAS)
        private TipoEnfermedadRepository _tipoEnfermedadRepo;
        private ProductoRepository _productoRepo;
        private ConsultaRepository _consultaRepo;
        private EmpleadoRepository _empleadoRepo;
        private EmpleadoApiService _empleadoApiService;
        private InventarioApiService _inventarioApiService;
        private ConsultaApiService _consultaApiService;
        private TipoEnfermedadApiService _tipoEnfermedadApiService;
        #endregion

        #region Caché y Control (CON LÍMITE DE TAMAÑO)
        private CancellationTokenSource _cancellationTokenSource;
        private List<EmpleadoModel> _todosLosEmpleados;
        private bool _empleadosCargados = false;

        // ⭐ CACHÉ CON LÍMITE
        private readonly LRUCache<string, List<ConsultaResumenModel>> _cacheConsultas =
            new LRUCache<string, List<ConsultaResumenModel>>(maxSize: 50);

        private DateTime _ultimaSincronizacionInventario = DateTime.MinValue;
        private bool _tiposEnfermedadCargados = false;
        private bool _medicamentosCargados = false;
        private const int CACHE_MINUTOS = 30;
        #endregion

        #region Properties

        public bool MostrarBusquedaEmpleado { get; set; } = true;
        public bool MostrarFormularioConsulta { get; set; } = false;
        public string TextoBusqueda { get; set; }
        public ObservableCollection<EmpleadoModel> EmpleadosFiltrados { get; set; }
        public bool IsCargandoEmpleados { get; set; } = false;
        public string MensajeEstado { get; set; }
        public int TotalEmpleados { get; set; }

        public ConsultaModel Consulta { get; set; }
        public EmpleadoModel EmpleadoSeleccionado { get; set; }
        public int Edad { get; set; }

        public string TensionArterial { get; set; }
        public string Temperatura { get; set; }
        public short FrecuenciaCardiaca { get; set; }
        public byte FrecuenciaRespiratoria { get; set; }
        public string ObservacionesSignos { get; set; }

        public string MotivoConsulta { get; set; }
        public string Diagnostico { get; set; }
        public TipoEnfermedadModel TipoEnfermedadSeleccionado { get; set; }
        public string Tratamiento { get; set; }
        public string UltimaComida { get; set; }

        public ObservableCollection<ConsultaResumenModel> UltimasConsultas { get; set; }
        public bool TieneConsultasPrevias => UltimasConsultas?.Count > 0;
        public bool HistorialExpandido { get; set; }

        public bool SeUtilizoMaterial { get; set; }
        public ProductoModel MedicamentoSeleccionado { get; set; }
        public double CantidadDisponible { get; set; }
        public double CantidadMedicamento { get; set; }
        public string ObservacionesMedicamento { get; set; }

        public bool TieneMedicamentosAgregados => MedicamentosAgregados?.Count > 0;

        public ObservableCollection<TipoEnfermedadModel> TiposEnfermedad { get; set; }
        public ObservableCollection<ProductoModel> Medicamentos { get; set; }
        public ObservableCollection<MovimientoDetalleModel> MedicamentosAgregados { get; set; }

        public ICommand BuscarEmpleadoCommand { get; }
        public ICommand MostrarTodosEmpleadosCommand { get; }
        public ICommand SeleccionarEmpleadoCommand { get; }
        public ICommand CambiarEmpleadoCommand { get; }
        public ICommand AgregarMedicamentoCommand { get; }
        public ICommand EliminarMedicamentoCommand { get; }
        public ICommand GuardarCommand { get; }
        public ICommand CancelarCommand { get; }
        public ICommand ToggleHistorialCommand { get; }
        #endregion

        public ConsultaViewModel()
        {
            try
            {
                _cancellationTokenSource = new CancellationTokenSource();

                // ⭐ INICIALIZAR SOLO CUANDO SE NECESITE (NO EN CONSTRUCTOR)
                TiposEnfermedad = new ObservableCollection<TipoEnfermedadModel>();
                Medicamentos = new ObservableCollection<ProductoModel>();
                MedicamentosAgregados = new ObservableCollection<MovimientoDetalleModel>();
                UltimasConsultas = new ObservableCollection<ConsultaResumenModel>();
                EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>();
                _todosLosEmpleados = new List<EmpleadoModel>();

                Consulta = new ConsultaModel { FechaConsulta = DateTime.Now };

                MedicamentosAgregados.CollectionChanged += (s, e) =>
                {
                    this.OnPropertyChanged(nameof(TieneMedicamentosAgregados));
                };

                BuscarEmpleadoCommand = new Command(Buscar);
                MostrarTodosEmpleadosCommand = new Command(async () => await MostrarTodosAsync());
                SeleccionarEmpleadoCommand = new Command<EmpleadoModel>(SeleccionarEmpleado);
                CambiarEmpleadoCommand = new Command(CambiarEmpleado);

                AgregarMedicamentoCommand = new Command(AgregarMedicamento, CanAgregarMedicamento);
                EliminarMedicamentoCommand = new Command<MovimientoDetalleModel>(EliminarMedicamento);
                GuardarCommand = new Command(Guardar, CanGuardar);
                CancelarCommand = new Command(Cancelar);
                ToggleHistorialCommand = new Command(() => HistorialExpandido = !HistorialExpandido);

                PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(MedicamentoSeleccionado))
                    {
                        ActualizarCantidadDisponible();
                        ((Command)AgregarMedicamentoCommand).ChangeCanExecute();
                    }
                    else if (e.PropertyName == nameof(CantidadMedicamento) ||
                             e.PropertyName == nameof(SeUtilizoMaterial) ||
                             e.PropertyName == nameof(CantidadDisponible))
                    {
                        ((Command)AgregarMedicamentoCommand).ChangeCanExecute();
                    }
                    else if (e.PropertyName == nameof(MotivoConsulta) ||
                             e.PropertyName == nameof(Diagnostico) ||
                             e.PropertyName == nameof(TipoEnfermedadSeleccionado))
                    {
                        ((Command)GuardarCommand).ChangeCanExecute();
                    }
                };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en constructor: {ex.Message}");
            }
        }

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
                // ⭐ CREAR REPOSITORIOS Y SERVICIOS AQUÍ (NO EN PROPIEDADES)
                if (_empleadoApiService == null)
                    _empleadoApiService = new EmpleadoApiService();
                if (_inventarioApiService == null)
                    _inventarioApiService = new InventarioApiService();
                if (_tipoEnfermedadRepo == null)
                    _tipoEnfermedadRepo = new TipoEnfermedadRepository();
                if (_productoRepo == null)
                    _productoRepo = new ProductoRepository();
                if (_consultaRepo == null)
                    _consultaRepo = new ConsultaRepository();
                if (_empleadoRepo == null)
                    _empleadoRepo = new EmpleadoRepository();
                if (_consultaApiService == null)
                    _consultaApiService = new ConsultaApiService();
                if (_tipoEnfermedadApiService == null)
                    _tipoEnfermedadApiService = new TipoEnfermedadApiService();

                // ⭐ CARGAR TODO SECUENCIALMENTE (NO EN PARALELO)
                if (!_empleadosCargados)
                {
                    await CargarEmpleadosAsync(_cancellationTokenSource.Token);
                }

                if (!_tiposEnfermedadCargados || !_medicamentosCargados)
                {
                    await CargarInventarioAsync(_cancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Inicialización cancelada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private async Task CargarInventarioAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                if (_tiposEnfermedadCargados && _medicamentosCargados &&
                    (DateTime.Now - _ultimaSincronizacionInventario).TotalMinutes < CACHE_MINUTOS)
                {
                    return;
                }

                System.Diagnostics.Debug.WriteLine("📦 Cargando datos...");

                // ⭐ TIPOS DE ENFERMEDAD
                if (!_tiposEnfermedadCargados)
                {
                    try
                    {
                        var tiposDto = await _tipoEnfermedadApiService.ObtenerTiposEnfermedadAsync();

                        if (tiposDto?.Count > 0)
                        {
                            var tiposModels = tiposDto.Select(dto => dto.ToModel()).ToList();

                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                TiposEnfermedad.Clear();
                                foreach (var tipo in tiposModels)
                                {
                                    TiposEnfermedad.Add(tipo);
                                }
                            });

                            _tiposEnfermedadCargados = true;
                        }
                    }
                    catch
                    {
                        var tiposLocal = _tipoEnfermedadRepo.GetAllTypes();
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            TiposEnfermedad.Clear();
                            foreach (var tipo in tiposLocal)
                                TiposEnfermedad.Add(tipo);
                        });
                        _tiposEnfermedadCargados = true;
                    }
                }

                // ⭐ MEDICAMENTOS
                if (!_medicamentosCargados)
                {
                    try
                    {
                        var inventarioDto = await _inventarioApiService.ObtenerExistenciasAsync();

                        if (inventarioDto?.Count > 0)
                        {
                            var productos = inventarioDto
                                .Where(inv => inv.Existencia > 0)
                                .Select(dto => dto.ToProductoModel())
                                .ToList();

                            await MainThread.InvokeOnMainThreadAsync(() =>
                            {
                                Medicamentos.Clear();
                                foreach (var p in productos)
                                    Medicamentos.Add(p);
                            });

                            _medicamentosCargados = true;
                        }
                    }
                    catch
                    {
                        var productosLocal = _productoRepo.GetProductoConsStock();
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Medicamentos.Clear();
                            foreach (var p in productosLocal)
                                Medicamentos.Add(p);
                        });
                        _medicamentosCargados = true;
                    }
                }

                _ultimaSincronizacionInventario = DateTime.Now;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        #region Métodos de Búsqueda

        private async Task CargarEmpleadosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsCargandoEmpleados = true;
                MensajeEstado = "Cargando empleados...";

                var empleadosDto = await _empleadoApiService.ObtenerEmpleadosActivosAsync();

                if (empleadosDto?.Count > 0)
                {
                    var empleados = empleadosDto.Select(dto => dto.ToEmpleadoModel()).ToList();
                    _todosLosEmpleados = empleados;
                    TotalEmpleados = empleados.Count;
                    _empleadosCargados = true;
                }
                else
                {
                    var empleadosLocal = _empleadoRepo.GetAll();
                    _todosLosEmpleados = empleadosLocal;
                    TotalEmpleados = empleadosLocal.Count;
                    _empleadosCargados = true;
                }

                await MostrarTodosAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                try
                {
                    var empleadosLocal = _empleadoRepo.GetAll();
                    _todosLosEmpleados = empleadosLocal;
                    TotalEmpleados = empleadosLocal.Count;
                    _empleadosCargados = true;
                    await MostrarTodosAsync();
                }
                catch
                {
                    MensajeEstado = "Error al cargar empleados";
                }
            }
            finally
            {
                IsCargandoEmpleados = false;
            }
        }

        private async void Buscar()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                await MostrarTodosAsync();
                return;
            }

            if (_todosLosEmpleados?.Count == 0)
                return;

            IsCargandoEmpleados = true;

            try
            {
                var busqueda = TextoBusqueda.ToLower();
                var empleadosFiltrados = _todosLosEmpleados
                    .Where(e =>
                        e.Nombre.ToLower().Contains(busqueda) ||
                        e.IdEmpleado.ToLower().Contains(busqueda) ||
                        (e.IdPuesto?.ToLower().Contains(busqueda) ?? false)
                    )
                    .ToList();

                await ActualizarListaAsync(empleadosFiltrados);
                MensajeEstado = $"{empleadosFiltrados.Count} encontrados";
            }
            finally
            {
                IsCargandoEmpleados = false;
            }
        }

        private async Task MostrarTodosAsync()
        {
            var empleadosAMostrar = _todosLosEmpleados?.Take(50).ToList() ?? new List<EmpleadoModel>();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>(empleadosAMostrar);
                OnPropertyChanged(nameof(EmpleadosFiltrados));
            });
        }

        private async Task ActualizarListaAsync(List<EmpleadoModel> empleados)
        {
            var empleadosAMostrar = empleados.Take(50).ToList();

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>(empleadosAMostrar);
                OnPropertyChanged(nameof(EmpleadosFiltrados));
            });
        }

        private async void SeleccionarEmpleado(EmpleadoModel empleado)
        {
            if (empleado == null) return;

            EmpleadoSeleccionado = empleado;
            CalcularEdad();

            MostrarBusquedaEmpleado = false;
            MostrarFormularioConsulta = true;

            // ⭐ CARGAR EN BACKGROUND SIN BLOQUEAR
            await CargarUltimasConsultasAsync(empleado.IdEmpleado);
        }

        private async void CambiarEmpleado()
        {
            LimpiarFormulario();
            MostrarFormularioConsulta = false;
            MostrarBusquedaEmpleado = true;
            EmpleadoSeleccionado = null;
            TextoBusqueda = string.Empty;

            if (_empleadosCargados)
                await MostrarTodosAsync();
        }

        #endregion

        private void CalcularEdad()
        {
            if (EmpleadoSeleccionado != null)
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - EmpleadoSeleccionado.FechaNacimiento.Year;
                if (EmpleadoSeleccionado.FechaNacimiento.Date > hoy.AddYears(-edad))
                    edad--;
                Edad = edad;
            }
        }

        private async Task CargarUltimasConsultasAsync(string idEmpleado)
        {
            try
            {
                // ⭐ VERIFICAR CACHÉ
                if (_cacheConsultas.TryGetValue(idEmpleado, out var consultasEnCache))
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        UltimasConsultas.Clear();
                        foreach (var c in consultasEnCache)
                            UltimasConsultas.Add(c);
                        OnPropertyChanged(nameof(TieneConsultasPrevias));
                    });
                    return;
                }

                var consultas = _consultaRepo.GetConsultasByEmpleado(idEmpleado)
                    .OrderByDescending(c => c.FechaConsulta)
                    .Take(5)
                    .ToList();

                if (consultas.Count == 0)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        UltimasConsultas.Clear();
                        OnPropertyChanged(nameof(TieneConsultasPrevias));
                    });
                    return;
                }

                var tiposEnfermedadMap = new Dictionary<int, string>();
                var tiposUnicos = consultas.Select(c => c.IdTipoEnfermedad).Distinct().ToList();

                foreach (var idTipo in tiposUnicos)
                {
                    var tipo = _tipoEnfermedadRepo.GetById(idTipo);
                    if (tipo != null)
                        tiposEnfermedadMap[idTipo] = tipo.NombreEnfermedad;
                }

                var resumenes = consultas
                    .Select(c => new ConsultaResumenModel
                    {
                        FechaConsulta = c.FechaConsulta,
                        MotivoConsulta = c.MotivoConsulta,
                        Diagnostico = c.Diagnostico,
                        TipoEnfermedad = tiposEnfermedadMap.TryGetValue(c.IdTipoEnfermedad, out var nombre)
                            ? nombre
                            : "Sin clasificar"
                    })
                    .ToList();

                _cacheConsultas.Add(idEmpleado, resumenes);

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    UltimasConsultas.Clear();
                    foreach (var r in resumenes)
                        UltimasConsultas.Add(r);
                    OnPropertyChanged(nameof(TieneConsultasPrevias));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
            }
        }

        private void ActualizarCantidadDisponible()
        {
            CantidadDisponible = MedicamentoSeleccionado?.CantidadDisponible ?? 0;
        }

        private bool CanAgregarMedicamento()
        {
            return SeUtilizoMaterial &&
                   MedicamentoSeleccionado != null &&
                   CantidadMedicamento > 0 &&
                   CantidadMedicamento <= CantidadDisponible;
        }

        private async void AgregarMedicamento()
        {
            if (MedicamentoSeleccionado?.CantidadDisponible < CantidadMedicamento)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Cantidad insuficiente", "OK");
                return;
            }

            var existente = MedicamentosAgregados
                .FirstOrDefault(m => m.ClaveProducto == MedicamentoSeleccionado.ProductoId);

            if (existente != null)
            {
                existente.Cantidad += CantidadMedicamento;
                MedicamentoSeleccionado.CantidadDisponible -= CantidadMedicamento;
                LimpiarCamposMedicamento();
                return;
            }

            MedicamentosAgregados.Add(new MovimientoDetalleModel
            {
                IdMovimientoDetalle = Guid.NewGuid().ToString(),
                ClaveProducto = MedicamentoSeleccionado.ProductoId,
                Cantidad = CantidadMedicamento,
                Observaciones = ObservacionesMedicamento ?? "Sin observaciones",
                Status = 1
            });

            MedicamentoSeleccionado.CantidadDisponible -= CantidadMedicamento;
            LimpiarCamposMedicamento();
        }

        private async void EliminarMedicamento(MovimientoDetalleModel medicamento)
        {
            if (medicamento == null) return;

            bool respuesta = await Application.Current.MainPage.DisplayAlert(
                "Confirmar", $"¿Eliminar {medicamento.NombreMedicamento}?", "Sí", "No");

            if (respuesta)
            {
                var producto = Medicamentos.FirstOrDefault(p => p.ProductoId == medicamento.ClaveProducto);
                if (producto != null)
                    producto.CantidadDisponible += medicamento.Cantidad;

                MedicamentosAgregados.Remove(medicamento);
            }
        }

        private void LimpiarCamposMedicamento()
        {
            MedicamentoSeleccionado = null;
            CantidadMedicamento = 0;
            ObservacionesMedicamento = string.Empty;
            CantidadDisponible = 0;
        }

        private bool CanGuardar()
        {
            return EmpleadoSeleccionado != null &&
                   !string.IsNullOrWhiteSpace(MotivoConsulta) &&
                   !string.IsNullOrWhiteSpace(Diagnostico) &&
                   TipoEnfermedadSeleccionado != null;
        }

        private async void Guardar()
        {
            try
            {
                if (SeUtilizoMaterial && MedicamentosAgregados.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error", "Debe agregar medicamentos", "OK");
                    return;
                }

                var idEmpleado = EmpleadoSeleccionado.IdEmpleado;

                var consultaDto = new CrearConsultaDto
                {
                    IdEmpleado = idEmpleado,
                    IdUsuarioAcc = Preferences.Get("IdUsuario", string.Empty),
                    IdTipoEnfermedad = TipoEnfermedadSeleccionado.IdTipoEnfermedad,
                    MotivoConsulta = MotivoConsulta,
                    FechaConsulta = DateTime.Now,
                    Diagnostico = Diagnostico,
                    FrecuenciaCardiaca = FrecuenciaCardiaca == 0 ? null : (short?)FrecuenciaCardiaca,
                    FrecuenciaRespiratoria = FrecuenciaRespiratoria == 0 ? null : (byte?)FrecuenciaRespiratoria,
                    Temperatura = string.IsNullOrWhiteSpace(Temperatura) ? null : decimal.Parse(Temperatura),
                    PresionArterial = TensionArterial,
                    Observaciones = ObservacionesSignos,
                    UltimaComida = UltimaComida,
                    Medicamentos = MedicamentosAgregados
                        .Select(m => new MedicamentoConsultaDto
                        {
                            IdProducto = m.ClaveProducto,
                            Cantidad = (float)m.Cantidad,
                            Observaciones = m.Observaciones
                        })
                        .ToList()
                };

                var respuesta = await _consultaApiService.CrearConsultaAsync(consultaDto);

                if (respuesta != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Éxito", "Consulta guardada", "OK");
                    _cacheConsultas.Remove(idEmpleado); // ⭐ Limpiar caché
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "No se pudo guardar", "OK");
                }

                MessagingCenter.Send(this, "ConsultaGuardada", idEmpleado);
                CambiarEmpleado();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void Cancelar()
        {
            bool respuesta = await Application.Current.MainPage.DisplayAlert(
                "Cancelar", "¿Descartar cambios?", "Sí", "No");

            if (respuesta)
            {
                foreach (var med in MedicamentosAgregados)
                {
                    var prod = Medicamentos.FirstOrDefault(p => p.ProductoId == med.ClaveProducto);
                    if (prod != null)
                        prod.CantidadDisponible += med.Cantidad;
                }

                CambiarEmpleado();
            }
        }

        private void LimpiarFormulario()
        {
            TensionArterial = string.Empty;
            Temperatura = string.Empty;
            FrecuenciaCardiaca = 0;
            FrecuenciaRespiratoria = 0;
            ObservacionesSignos = string.Empty;
            MotivoConsulta = string.Empty;
            Diagnostico = string.Empty;
            TipoEnfermedadSeleccionado = null;
            Tratamiento = string.Empty;
            UltimaComida = string.Empty;
            SeUtilizoMaterial = false;
            MedicamentosAgregados.Clear();
            LimpiarCamposMedicamento();

            Consulta = new ConsultaModel { FechaConsulta = DateTime.Now };
            UltimasConsultas.Clear();
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
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cacheConsultas?.Clear();
        }
        #endregion
    }
}

// ⭐ CACHE LRU CON LÍMITE DE TAMAÑO
public class LRUCache<TKey, TValue> where TKey : notnull
{
    private readonly int _maxSize;
    private readonly Dictionary<TKey, TValue> _cache;
    private readonly LinkedList<TKey> _order;

    public LRUCache(int maxSize = 100)
    {
        _maxSize = maxSize;
        _cache = new Dictionary<TKey, TValue>(maxSize);
        _order = new LinkedList<TKey>();
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (_cache.TryGetValue(key, out value))
        {
            _order.Remove(key);
            _order.AddLast(key);
            return true;
        }
        return false;
    }

    public void Add(TKey key, TValue value)
    {
        if (_cache.ContainsKey(key))
        {
            _order.Remove(key);
            _cache[key] = value;
            _order.AddLast(key);
            return;
        }

        if (_cache.Count >= _maxSize)
        {
            var oldest = _order.First.Value;
            _order.RemoveFirst();
            _cache.Remove(oldest);
        }

        _cache[key] = value;
        _order.AddLast(key);
    }

    public void Remove(TKey key)
    {
        if (_cache.Remove(key))
            _order.Remove(key);
    }

    public void Clear()
    {
        _cache.Clear();
        _order.Clear();
    }
}

public class ConsultaResumenModel
{
    public DateTime FechaConsulta { get; set; }
    public string MotivoConsulta { get; set; }
    public string Diagnostico { get; set; }
    public string TipoEnfermedad { get; set; }
    public string FechaFormateada => FechaConsulta.ToString("dd/MM/yyyy HH:mm");
}