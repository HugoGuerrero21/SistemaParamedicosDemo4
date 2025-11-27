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


        #region Repositorios y Servicios
        private TipoEnfermedadRepository _tipoEnfermedadRepo;
        private ProductoRepository _productoRepo;
        private ConsultaRepository _consultaRepo;
        private EmpleadoRepository _empleadoRepo;
        private EmpleadoApiService _empleadoApiService;
        private InventarioApiService _inventarioApiService;
        private ConsultaApiService _consultaApiService;
        private TipoEnfermedadApiService _tipoEnfermedadApiService;
        #endregion

        #region Properties



        // PROPIEDADES PARA BÚSQUEDA
        public bool MostrarBusquedaEmpleado { get; set; } = true;
        public bool MostrarFormularioConsulta { get; set; } = false;
        public string TextoBusqueda { get; set; }
        public ObservableCollection<EmpleadoModel> EmpleadosFiltrados { get; set; }
        public bool IsCargandoEmpleados { get; set; } = false;
        public string MensajeEstado { get; set; }
        public int TotalEmpleados { get; set; }

        private List<EmpleadoModel> _todosLosEmpleados;
        private bool _empleadosCargados = false;
        private CancellationTokenSource _cancellationTokenSource;

        // Modelo principal de la consulta
        public ConsultaModel Consulta { get; set; }
        public EmpleadoModel EmpleadoSeleccionado { get; set; }
        public int Edad { get; set; }

        // Signos vitales
        public string TensionArterial { get; set; }
        public string Temperatura { get; set; }
        public short FrecuenciaCardiaca { get; set; }
        public byte FrecuenciaRespiratoria { get; set; }
        public string ObservacionesSignos { get; set; }

        // Consulta
        public string MotivoConsulta { get; set; }
        public string Diagnostico { get; set; }
        public TipoEnfermedadModel TipoEnfermedadSeleccionado { get; set; }
        public string Tratamiento { get; set; }
        public string UltimaComida { get; set; }

        // PROPIEDADES PARA HISTORIAL
        public ObservableCollection<ConsultaResumenModel> UltimasConsultas { get; set; }
        public bool TieneConsultasPrevias => UltimasConsultas?.Count > 0;
        public bool HistorialExpandido { get; set; }

        // Material/Medicamentos
        public bool SeUtilizoMaterial { get; set; }
        public ProductoModel MedicamentoSeleccionado { get; set; }
        public double CantidadDisponible { get; set; }
        public double CantidadMedicamento { get; set; }
        public string ObservacionesMedicamento { get; set; }

        public bool TieneMedicamentosAgregados => MedicamentosAgregados?.Count > 0;

        // Colecciones
        public ObservableCollection<TipoEnfermedadModel> TiposEnfermedad { get; set; }
        public ObservableCollection<ProductoModel> Medicamentos { get; set; }
        public ObservableCollection<MovimientoDetalleModel> MedicamentosAgregados { get; set; }

        // COMANDOS
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

                // Inicializar repositorios y servicios
                _tipoEnfermedadRepo = new TipoEnfermedadRepository();
                _productoRepo = new ProductoRepository();
                _consultaRepo = new ConsultaRepository();
                _empleadoRepo = new EmpleadoRepository();
                _empleadoApiService = new EmpleadoApiService();
                _inventarioApiService = new InventarioApiService();
                _consultaApiService = new ConsultaApiService();
                _tipoEnfermedadApiService = new TipoEnfermedadApiService();

                Consulta = new ConsultaModel { FechaConsulta = DateTime.Now };

                TiposEnfermedad = new ObservableCollection<TipoEnfermedadModel>();
                Medicamentos = new ObservableCollection<ProductoModel>();
                MedicamentosAgregados = new ObservableCollection<MovimientoDetalleModel>();
                UltimasConsultas = new ObservableCollection<ConsultaResumenModel>();
                EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>();
                _todosLosEmpleados = new List<EmpleadoModel>();

                // Cuando cambie la colección, notificar que la propiedad calculada cambió
                MedicamentosAgregados.CollectionChanged += (s, e) =>
                {
                    this.OnPropertyChanged(nameof(TieneMedicamentosAgregados));
                };

                // Inicializar Commands
                BuscarEmpleadoCommand = new Command(Buscar);
                MostrarTodosEmpleadosCommand = new Command(async () => await MostrarTodosAsync());
                SeleccionarEmpleadoCommand = new Command<EmpleadoModel>(SeleccionarEmpleado);
                CambiarEmpleadoCommand = new Command(CambiarEmpleado);

                AgregarMedicamentoCommand = new Command(AgregarMedicamento, CanAgregarMedicamento);
                EliminarMedicamentoCommand = new Command<MovimientoDetalleModel>(EliminarMedicamento);
                GuardarCommand = new Command(Guardar, CanGuardar);
                CancelarCommand = new Command(Cancelar);
                ToggleHistorialCommand = new Command(() => HistorialExpandido = !HistorialExpandido);

                // Configurar PropertyChanged
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

                // Cargar datos desde SQLite (SOLO tipos de enfermedad y productos)
                //CargarDatosDesdeBaseDatos();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en constructor: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        public async Task InicializarVistaAsync()
        {
            System.Diagnostics.Debug.WriteLine("🚀 InicializarVistaAsync EJECUTÁNDOSE");

            // ⭐ REINICIALIZAR CANCELLATION TOKEN SI FUE DISPOSED
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            // ⭐ REINICIALIZAR SERVICIOS SI FUERON DISPOSED
            if (_empleadoApiService == null)
            {
                _empleadoApiService = new EmpleadoApiService();
                System.Diagnostics.Debug.WriteLine("✓ EmpleadoApiService reinicializado");
            }

            if (_inventarioApiService == null)
            {
                _inventarioApiService = new InventarioApiService();
                System.Diagnostics.Debug.WriteLine("✓ InventarioApiService reinicializado");
            }

            // ⭐ REINICIALIZAR REPOSITORIOS SI FUERON DISPOSED
            if (_tipoEnfermedadRepo == null)
            {
                _tipoEnfermedadRepo = new TipoEnfermedadRepository();
                System.Diagnostics.Debug.WriteLine("✓ TipoEnfermedadRepository reinicializado");
            }

            if (_productoRepo == null)
            {
                _productoRepo = new ProductoRepository();
                System.Diagnostics.Debug.WriteLine("✓ ProductoRepository reinicializado");
            }

            if (_consultaRepo == null)
            {
                _consultaRepo = new ConsultaRepository();
                System.Diagnostics.Debug.WriteLine("✓ ConsultaRepository reinicializado");
            }

            if (_empleadoRepo == null)
            {
                _empleadoRepo = new EmpleadoRepository();
                System.Diagnostics.Debug.WriteLine("✓ EmpleadoRepository reinicializado");
            }

            if (_consultaApiService == null)
            {
                _consultaApiService = new ConsultaApiService();
            }


                try
                {
                if (!_empleadosCargados)
                {
                    System.Diagnostics.Debug.WriteLine("🚀 Empleados no cargados, procediendo...");

                    // Probar conexión primero
                    var conexionExitosa = await _empleadoApiService.ProbarConexionAsync();
                    if (!conexionExitosa)
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ No hay conexión con la API, intentando cargar desde SQLite...");
                        MensajeEstado = "Sin conexión - usando caché local";
                    }

                    await CargarEmpleadosAsync(_cancellationTokenSource.Token);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ Empleados ya cargados, saltando carga...");
                }

                // ⭐ CARGAR INVENTARIO REAL
                await CargarInventarioAsync(_cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Inicialización cancelada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en InicializarVistaAsync: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        private async Task CargarInventarioAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📦 Cargando datos desde BD y API...");

                // ⭐ VALIDAR REPOSITORIOS
                if (_tipoEnfermedadRepo == null)
                {
                    _tipoEnfermedadRepo = new TipoEnfermedadRepository();
                }

                if (_productoRepo == null)
                {
                    _productoRepo = new ProductoRepository();
                }

                if (_inventarioApiService == null)
                {
                    _inventarioApiService = new InventarioApiService();
                }

                // ⭐ AGREGAR SERVICIO DE TIPOS DE ENFERMEDAD SI NO EXISTE
                if (_tipoEnfermedadApiService == null)
                {
                    _tipoEnfermedadApiService = new TipoEnfermedadApiService();
                    System.Diagnostics.Debug.WriteLine("✓ TipoEnfermedadApiService inicializado");
                }

                // ⭐⭐⭐ 1. CARGAR TIPOS DE ENFERMEDAD DESDE LA API (NO DESDE BD) ⭐⭐⭐
                System.Diagnostics.Debug.WriteLine("🏥 Sincronizando tipos de enfermedad desde API...");

                var tiposDto = await _tipoEnfermedadApiService.ObtenerTiposEnfermedadAsync();

                if (tiposDto != null && tiposDto.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✓ {tiposDto.Count} tipos obtenidos desde API");

                    // Convertir DTOs a Models
                    var tiposModels = tiposDto.Select(dto => dto.ToModel()).ToList();

                    // Sincronizar con SQLite
                    await Task.Run(() => _tipoEnfermedadRepo.SincronizarTiposEnfermedad(tiposModels), cancellationToken);
                    System.Diagnostics.Debug.WriteLine("✓ Tipos sincronizados con SQLite");

                    // Actualizar UI
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            TiposEnfermedad.Clear();
                            foreach (var tipo in tiposModels)
                            {
                                TiposEnfermedad.Add(tipo);
                                System.Diagnostics.Debug.WriteLine($"   + {tipo.IdTipoEnfermedad}: {tipo.NombreEnfermedad}");
                            }
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ {TiposEnfermedad.Count} tipos de enfermedad cargados en UI desde API");
                }
                else
                {
                    // FALLBACK: Cargar desde SQLite si la API falla
                    System.Diagnostics.Debug.WriteLine("⚠️ API sin tipos, cargando desde SQLite...");

                    var tiposLocal = await Task.Run(() => _tipoEnfermedadRepo.GetAllTypes(), cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            TiposEnfermedad.Clear();
                            foreach (var tipo in tiposLocal)
                            {
                                TiposEnfermedad.Add(tipo);
                            }
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"✓ {TiposEnfermedad.Count} tipos cargados desde SQLite (fallback)");
                }

                // ⭐ 2. VERIFICAR SI HAY TIPOS
                if (TiposEnfermedad.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No hay tipos de enfermedad disponibles");
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Advertencia",
                            "No se encontraron tipos de enfermedad. Por favor, sincronice los datos primero.",
                            "OK");
                    });
                }

                // 3. CARGAR PRODUCTOS DESDE LA API (igual que antes)
                System.Diagnostics.Debug.WriteLine("📦 Cargando productos desde API...");
                var inventarioDto = await _inventarioApiService.ObtenerExistenciasAsync();

                if (inventarioDto == null || inventarioDto.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ API sin productos, cargando desde SQLite...");

                    var productosLocal = await Task.Run(() => _productoRepo.GetProductoConsStock(), cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            Medicamentos.Clear();
                            foreach (var producto in productosLocal)
                            {
                                Medicamentos.Add(producto);
                            }
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"✓ {Medicamentos.Count} productos cargados desde SQLite");
                    return;
                }

                // 4. CONVERTIR DTOs A MODELS
                var productos = await Task.Run(() =>
                    inventarioDto
                        .Where(inv => inv.Existencia > 0)
                        .Select(dto => dto.ToProductoModel())
                        .ToList(),
                    cancellationToken
                );

                // 5. SINCRONIZAR CON SQLITE
                await Task.Run(() => _productoRepo.SincronizarProductosDesdeInventario(productos), cancellationToken);
                System.Diagnostics.Debug.WriteLine("✓ Productos sincronizados con SQLite");

                // 6. ACTUALIZAR UI
                if (!cancellationToken.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Medicamentos.Clear();
                        foreach (var producto in productos)
                        {
                            Medicamentos.Add(producto);
                        }
                        System.Diagnostics.Debug.WriteLine($"✓ {Medicamentos.Count} productos cargados en UI");
                    });
                }

                System.Diagnostics.Debug.WriteLine("✅ TODOS LOS DATOS CARGADOS EXITOSAMENTE");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Carga de datos cancelada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar datos: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                // Fallback completo
                try
                {
                    var tiposLocal = await Task.Run(() => _tipoEnfermedadRepo.GetAllTypes(), cancellationToken);
                    var productosLocal = await Task.Run(() => _productoRepo.GetProductoConsStock(), cancellationToken);

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            TiposEnfermedad.Clear();
                            foreach (var tipo in tiposLocal)
                            {
                                TiposEnfermedad.Add(tipo);
                            }

                            Medicamentos.Clear();
                            foreach (var producto in productosLocal)
                            {
                                Medicamentos.Add(producto);
                            }
                        });
                    }

                    System.Diagnostics.Debug.WriteLine($"✓ Fallback completo: {TiposEnfermedad.Count} tipos y {Medicamentos.Count} productos");
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error en fallback: {fallbackEx.Message}");
                }
            }
        }



        #region Métodos de Búsqueda

        /// <summary>
        /// Carga empleados desde SQLite (IGUAL que EmpleadosListViewModel)
        /// </summary>
        private async Task CargarEmpleadosAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                IsCargandoEmpleados = true;
                MensajeEstado = "Cargando empleados desde API...";

                // 1. CARGAR DESDE LA API
                var empleadosDto = await _empleadoApiService.ObtenerEmpleadosActivosAsync();

                if (empleadosDto == null || empleadosDto.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ API sin datos, cargando desde SQLite...");
                    MensajeEstado = "Cargando desde caché local...";

                    var empleadosLocal = await Task.Run(() => _empleadoRepo.GetAll(), cancellationToken);

                    if (empleadosLocal.Count == 0)
                    {
                        MensajeEstado = "No se encontraron empleados";
                        return;
                    }

                    _todosLosEmpleados = empleadosLocal;
                    TotalEmpleados = _todosLosEmpleados.Count;
                    MensajeEstado = $"{TotalEmpleados} empleados (caché local)";
                    _empleadosCargados = true;

                    await MostrarTodosAsync();
                    return;
                }

                // 2. CONVERTIR DTOs A MODELS
                var empleados = await Task.Run(() =>
                    empleadosDto.Select(dto => dto.ToEmpleadoModel()).ToList(), cancellationToken
                );

                // 3. SINCRONIZAR CON SQLITE
                await Task.Run(() => _empleadoRepo.SincronizarEmpleados(empleados), cancellationToken);
                System.Diagnostics.Debug.WriteLine("✓ Empleados sincronizados con SQLite");

                // 4. SINCRONIZAR PUESTOS (extraer de los empleados)
                var puestosUnicos = empleadosDto
                    .Where(e => e.Puesto != null)
                    .Select(e => e.Puesto.ToPuestoModel())
                    .GroupBy(p => p.IdPuesto)
                    .Select(g => g.First())
                    .ToList();

                if (puestosUnicos.Count > 0)
                {
                    var puestoRepo = new PuestoRepository();
                    await Task.Run(() => puestoRepo.SincronizarPuestos(puestosUnicos), cancellationToken);
                    System.Diagnostics.Debug.WriteLine($"✓ {puestosUnicos.Count} puestos sincronizados con SQLite");
                }

                // 5. ACTUALIZAR VARIABLES (NO UI AÚN)
                _todosLosEmpleados = empleados;
                TotalEmpleados = _todosLosEmpleados.Count;
                MensajeEstado = $"{TotalEmpleados} empleados disponibles";
                _empleadosCargados = true;

                System.Diagnostics.Debug.WriteLine($"✓ {TotalEmpleados} empleados cargados");

                // 6. ACTUALIZAR UI AL FINAL (Con verificación de cancelación)
                if (!cancellationToken.IsCancellationRequested)
                {
                    await MostrarTodosAsync();
                }

                System.Diagnostics.Debug.WriteLine("✓ Proceso completado sin errores");
            }
            catch (OperationCanceledException)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Carga de empleados cancelada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");

                // Fallback a SQLite
                try
                {
                    var empleadosLocal = await Task.Run(() => _empleadoRepo.GetAll(), cancellationToken);

                    _todosLosEmpleados = empleadosLocal;
                    TotalEmpleados = _todosLosEmpleados.Count;
                    MensajeEstado = $"Error de conexión. Usando caché: {TotalEmpleados} empleados";
                    _empleadosCargados = true;

                    await MostrarTodosAsync();
                }
                catch (Exception fallbackEx)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error en fallback: {fallbackEx.Message}");
                    MensajeEstado = "Error al cargar empleados";
                }
            }
            finally
            {
                IsCargandoEmpleados = false;
                System.Diagnostics.Debug.WriteLine("🏁 CargarEmpleadosAsync finalizado");
            }
        }


        private async void Buscar()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                await MostrarTodosAsync();
                return;
            }

            // ⭐ VALIDAR QUE LOS EMPLEADOS ESTÉN CARGADOS
            if (_todosLosEmpleados == null || _todosLosEmpleados.Count == 0)
            {
                MensajeEstado = "Cargando empleados...";
                return;
            }

            IsCargandoEmpleados = true;

            try
            {
                // ⭐ OPCIÓN 1: Buscar localmente (más rápido)
                var busqueda = TextoBusqueda.ToLower();
                var empleadosFiltrados = _todosLosEmpleados.Where(e =>
                    e.Nombre.ToLower().Contains(busqueda) ||
                    e.IdEmpleado.ToLower().Contains(busqueda) ||
                    (e.IdPuesto?.ToLower().Contains(busqueda) ?? false)
                ).ToList();

                await ActualizarListaAsync(empleadosFiltrados);

                MensajeEstado = empleadosFiltrados.Count == 0
                    ? "No se encontraron empleados"
                    : $"{empleadosFiltrados.Count} empleados encontrados";
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error en búsqueda: {ex.Message}");
                MensajeEstado = "Error al buscar";
            }
            finally
            {
                IsCargandoEmpleados = false;
            }
        }

        private async Task MostrarTodosAsync()
        {
            if (_todosLosEmpleados == null || _todosLosEmpleados.Count == 0)
            {
                try
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>();
                        OnPropertyChanged(nameof(EmpleadosFiltrados));
                    });
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Error al limpiar EmpleadosFiltrados: {ex.Message}");
                }

                MensajeEstado = "No hay empleados disponibles";
                return;
            }

            var empleadosAMostrar = _todosLosEmpleados.Take(50).ToList();

            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>(empleadosAMostrar);
                    OnPropertyChanged(nameof(EmpleadosFiltrados));
                    System.Diagnostics.Debug.WriteLine($"✓ UI actualizada con {empleadosAMostrar.Count} empleados");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar UI: {ex.Message}");
            }

            MensajeEstado = _todosLosEmpleados.Count > 50
                ? $"Mostrando 50 de {_todosLosEmpleados.Count} empleados. Usa la búsqueda."
                : $"{_todosLosEmpleados.Count} empleados disponibles";
        }

        private async Task ActualizarListaAsync(List<EmpleadoModel> empleados)
        {
            var empleadosAMostrar = empleados.Take(50).ToList();

            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>(empleadosAMostrar);
                    OnPropertyChanged(nameof(EmpleadosFiltrados));
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al actualizar lista: {ex.Message}");
            }
        }

        private async void SeleccionarEmpleado(EmpleadoModel empleado)
        {
            if (empleado != null)
            {
                EmpleadoSeleccionado = empleado;

                // ⭐ SINCRONIZAR CONSULTAS DESDE LA API
                await SincronizarConsultasDesdeApiAsync(empleado.IdEmpleado);

                CalcularEdad();

                // Cambiar a la vista del formulario
                MostrarBusquedaEmpleado = false;
                MostrarFormularioConsulta = true;

                System.Diagnostics.Debug.WriteLine($"✓ Empleado seleccionado: {empleado.Nombre}");
            }
        }

        private async void CambiarEmpleado()
        {
            LimpiarFormulario();

            MostrarFormularioConsulta = false;
            MostrarBusquedaEmpleado = true;
            EmpleadoSeleccionado = null;
            TextoBusqueda = string.Empty;

            if (_empleadosCargados)
            {
                await MostrarTodosAsync();
            }
        }

        #endregion

        //private void CargarDatosDesdeBaseDatos()
        //{
        //    try
        //    {
        //        System.Diagnostics.Debug.WriteLine("Cargando datos desde SQLite...");

        //        var tiposEnfermedad = _tipoEnfermedadRepo.GetAllTypes();
        //        foreach (var tipo in tiposEnfermedad)
        //        {
        //            TiposEnfermedad.Add(tipo);
        //        }
        //        System.Diagnostics.Debug.WriteLine($"✓ {TiposEnfermedad.Count} tipos de enfermedad cargados");

        //        var productos = _productoRepo.GetProductoConsStock();
        //        foreach (var producto in productos)
        //        {
        //            Medicamentos.Add(producto);
        //        }
        //        System.Diagnostics.Debug.WriteLine($"✓ {Medicamentos.Count} productos cargados");
        //    }
        //    catch (Exception ex)
        //    {
        //        System.Diagnostics.Debug.WriteLine($"❌ Error al cargar datos: {ex.Message}");
        //    }
        //}

        private void CalcularEdad()
        {
            if (EmpleadoSeleccionado != null)
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - EmpleadoSeleccionado.FechaNacimiento.Year;
                if (EmpleadoSeleccionado.FechaNacimiento.Date > hoy.AddYears(-edad)) edad--;
                Edad = edad;

                CargarUltimasConsultas();
            }
        }

        private void CargarUltimasConsultas()
        {
            try
            {
                // ⭐ VALIDAR QUE LOS REPOSITORIOS EXISTAN
                if (EmpleadoSeleccionado == null)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No hay empleado seleccionado");
                    return;
                }

                if (_consultaRepo == null)
                {
                    _consultaRepo = new ConsultaRepository();
                    System.Diagnostics.Debug.WriteLine("⚠️ ConsultaRepository era null, reinicializado");
                }

                if (_tipoEnfermedadRepo == null)
                {
                    _tipoEnfermedadRepo = new TipoEnfermedadRepository();
                    System.Diagnostics.Debug.WriteLine("⚠️ TipoEnfermedadRepository era null, reinicializado");
                }

                UltimasConsultas.Clear();

                var consultas = _consultaRepo.GetConsultasByEmpleado(EmpleadoSeleccionado.IdEmpleado)
                    .OrderByDescending(c => c.FechaConsulta)
                    .Take(5)
                    .ToList();

                foreach (var consulta in consultas)
                {
                    consulta.TipoEnfermedad = _tipoEnfermedadRepo.GetById(consulta.IdTipoEnfermedad);

                    var resumen = new ConsultaResumenModel
                    {
                        FechaConsulta = consulta.FechaConsulta,
                        MotivoConsulta = consulta.MotivoConsulta,
                        Diagnostico = consulta.Diagnostico,
                        TipoEnfermedad = consulta.TipoEnfermedad?.NombreEnfermedad ?? "Sin clasificar"
                    };

                    UltimasConsultas.Add(resumen);
                }

                System.Diagnostics.Debug.WriteLine($"✓ {UltimasConsultas.Count} consultas previas cargadas");
                OnPropertyChanged(nameof(TieneConsultasPrevias));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar consultas: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        private void ActualizarCantidadDisponible()
        {
            if (MedicamentoSeleccionado != null)
            {
                CantidadDisponible = MedicamentoSeleccionado.CantidadDisponible;
            }
            else
            {
                CantidadDisponible = 0;
            }
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
            if (MedicamentoSeleccionado != null && CantidadMedicamento > 0)
            {
                if (CantidadMedicamento > CantidadDisponible)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"La cantidad solicitada ({CantidadMedicamento}) excede la cantidad disponible ({CantidadDisponible})",
                        "OK");
                    return;
                }

                var medicamentoExistente = MedicamentosAgregados
                    .FirstOrDefault(m => m.ClaveProducto == MedicamentoSeleccionado.ProductoId);

                if (medicamentoExistente != null)
                {
                    bool actualizar = await Application.Current.MainPage.DisplayAlert(
                        "Medicamento existente",
                        "Este medicamento ya ha sido agregado. ¿Desea aumentar la cantidad?",
                        "Sí",
                        "No");

                    if (actualizar)
                    {
                        double nuevaCantidad = medicamentoExistente.Cantidad + CantidadMedicamento;
                        double cantidadActualDisponible = MedicamentoSeleccionado.CantidadDisponible + medicamentoExistente.Cantidad;

                        if (nuevaCantidad > cantidadActualDisponible)
                        {
                            await Application.Current.MainPage.DisplayAlert(
                                "Error",
                                $"La cantidad total ({nuevaCantidad}) excedería la cantidad disponible ({cantidadActualDisponible})",
                                "OK");
                            return;
                        }

                        medicamentoExistente.Cantidad = nuevaCantidad;
                        medicamentoExistente.Observaciones = ObservacionesMedicamento ?? medicamentoExistente.Observaciones;
                        MedicamentoSeleccionado.CantidadDisponible -= CantidadMedicamento;
                    }

                    LimpiarCamposMedicamento();
                    return;
                }

                var movimientoDetalle = new MovimientoDetalleModel
                {
                    IdMovimientoDetalle = Guid.NewGuid().ToString(),
                    ClaveProducto = MedicamentoSeleccionado.ProductoId,
                    // ✅ NO guardar la referencia completa del producto
                    Producto = null, // ← Evitar referencia circular
                    Cantidad = CantidadMedicamento,
                    Observaciones = string.IsNullOrWhiteSpace(ObservacionesMedicamento)
                        ? "Sin observaciones"
                        : ObservacionesMedicamento,
                    Status = 1
                };

                MedicamentosAgregados.Add(movimientoDetalle);
        
                // ✅ IMPORTANTE: Crear una COPIA de la cantidad disponible
                var cantidadOriginal = MedicamentoSeleccionado.CantidadDisponible;
                MedicamentoSeleccionado.CantidadDisponible = Math.Max(0, cantidadOriginal - CantidadMedicamento);
        
                LimpiarCamposMedicamento();

                await Application.Current.MainPage.DisplayAlert(
                    "Éxito",
                    "Medicamento agregado correctamente",
                    "OK");
            }
        }

        private async void EliminarMedicamento(MovimientoDetalleModel medicamento)
        {
            if (medicamento != null)
            {
                bool respuesta = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar",
                    $"¿Desea eliminar {medicamento.NombreMedicamento}?",
                    "Sí",
                    "No");

                if (respuesta)
                {
                    var productoOriginal = Medicamentos
                        .FirstOrDefault(p => p.ProductoId == medicamento.ClaveProducto);

                    if (productoOriginal != null)
                    {
                        productoOriginal.CantidadDisponible += medicamento.Cantidad;
                    }

                    MedicamentosAgregados.Remove(medicamento);
                }
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
                // ✅ 1. VALIDACIONES
                if (SeUtilizoMaterial && MedicamentosAgregados.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Advertencia",
                        "Ha indicado que se utilizó material pero no ha agregado ningún medicamento.",
                        "OK");
                    return;
                }

                var idEmpleadoGuardado = EmpleadoSeleccionado?.IdEmpleado;

                // ✅ 2. CREAR DTO PARA LA API
                var consultaDto = new CrearConsultaDto
                {
                    IdEmpleado = EmpleadoSeleccionado.IdEmpleado,
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
                    Medicamentos = new List<MedicamentoConsultaDto>()
                };

                // ✅ 3. AGREGAR MEDICAMENTOS
                if (SeUtilizoMaterial && MedicamentosAgregados.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"📦 Preparando {MedicamentosAgregados.Count} medicamentos...");

                    foreach (var medicamento in MedicamentosAgregados)
                    {
                        consultaDto.Medicamentos.Add(new MedicamentoConsultaDto
                        {
                            IdProducto = medicamento.ClaveProducto,
                            Cantidad = (float)medicamento.Cantidad,
                            Observaciones = medicamento.Observaciones
                        });
                    }
                }

                // ✅ 4. ENVIAR A LA API
                System.Diagnostics.Debug.WriteLine("📤 Enviando consulta al servidor...");

                if (_consultaApiService == null)
                {
                    _consultaApiService = new ConsultaApiService();
                }

                var respuesta = await _consultaApiService.CrearConsultaAsync(consultaDto);

                if (respuesta == null)
                {
                    // ⚠️ FALLBACK: Guardar localmente
                    System.Diagnostics.Debug.WriteLine("⚠️ Error en API, guardando localmente...");

                    var consultaLocal = new ConsultaModel
                    {
                        IdUsuarioAcc = consultaDto.IdUsuarioAcc, // ⭐ Propiedad correcta
                        IdEmpleado = consultaDto.IdEmpleado,
                        FechaConsulta = consultaDto.FechaConsulta,
                        MotivoConsulta = consultaDto.MotivoConsulta,
                        Diagnostico = consultaDto.Diagnostico,
                        IdTipoEnfermedad = consultaDto.IdTipoEnfermedad,
                        FrecuenciaCardiaca = (short)(consultaDto.FrecuenciaCardiaca ?? 0),
                        FrecuenciaRespiratoria = (byte)(consultaDto.FrecuenciaRespiratoria ?? 0),
                        Temperatura = consultaDto.Temperatura?.ToString() ?? string.Empty,
                        PresionArterial = consultaDto.PresionArterial ?? string.Empty,
                        Observaciones = consultaDto.Observaciones ?? string.Empty,
                        UltimaComida = consultaDto.UltimaComida ?? string.Empty
                    };

                    if (SeUtilizoMaterial && MedicamentosAgregados.Count > 0)
                    {
                        consultaLocal.IdMovimiento = Guid.NewGuid().ToString();
                        foreach (var detalle in MedicamentosAgregados)
                        {
                            detalle.IdMovimiento = consultaLocal.IdMovimiento;
                        }
                    }

                    bool guardadoLocal = _consultaRepo.GuardarConsultaCompleta(
                        consultaLocal,
                        MedicamentosAgregados.ToList());

                    if (!guardadoLocal)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            "No se pudo guardar la consulta.",
                            "OK");
                        return;
                    }

                    await Application.Current.MainPage.DisplayAlert(
                        "Guardado localmente",
                        "No se pudo conectar con el servidor. La consulta se guardó localmente.",
                        "OK");
                }
                else
                {
                    // ✅ ÉXITO
                    System.Diagnostics.Debug.WriteLine($"✅ Consulta guardada con ID: {respuesta.IdConsulta}");

                    // ⭐ Usar el método ToModel() de ConsultaExtensions
                    var consultaLocal = respuesta.ToModel();

                    // Guardar en SQLite como caché
                    _consultaRepo.GuardarConsultaCompleta(consultaLocal, MedicamentosAgregados.ToList());
                    System.Diagnostics.Debug.WriteLine("✓ Consulta guardada en SQLite como caché");

                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Consulta guardada correctamente.",
                        "OK");
                }

                // ✅ 5. NOTIFICAR ACTUALIZACIÓN
                if (!string.IsNullOrEmpty(idEmpleadoGuardado))
                {
                    MessagingCenter.Send(this, "ConsultaGuardada", idEmpleadoGuardado);
                }

                // ✅ 6. LIMPIAR
                CambiarEmpleado();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al guardar: {ex.Message}",
                    "OK");
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        private async Task SincronizarConsultasDesdeApiAsync(string idEmpleado)
        {
            try
            {
                if (_consultaApiService == null)
                {
                    _consultaApiService = new ConsultaApiService();
                }

                System.Diagnostics.Debug.WriteLine($"🔄 Sincronizando consultas de {idEmpleado} desde API...");

                var consultasApi = await _consultaApiService.ObtenerConsultasPorEmpleadoAsync(idEmpleado);

                if (consultasApi == null || consultasApi.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No hay consultas en la API");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✓ {consultasApi.Count} consultas obtenidas de la API");

                // Guardar cada consulta en SQLite si no existe
                foreach (var consultaDto in consultasApi)
                {
                    // Verificar si ya existe
                    var existe = _consultaRepo.GetConsultaById(consultaDto.IdConsulta, false);

                    if (existe == null)
                    {
                        var consultaLocal = new ConsultaModel
                        {
                            IdConsulta = consultaDto.IdConsulta,
                            IdEmpleado = consultaDto.IdEmpleado,
                            IdUsuarioAcc = "SYNC", // Temporal
                            IdTipoEnfermedad = 1, // Deberías obtener el ID correcto
                            MotivoConsulta = consultaDto.MotivoConsulta,
                            Diagnostico = consultaDto.Diagnostico,
                            FechaConsulta = consultaDto.FechaConsulta,
                            FrecuenciaCardiaca = 0,
                            FrecuenciaRespiratoria = 0,
                            Temperatura = "",
                            PresionArterial = "",
                            Observaciones = "",
                            UltimaComida = ""
                        };

                        _consultaRepo.InsertarConsulta(consultaLocal);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Consulta {consultaDto.IdConsulta} sincronizada");
                    }
                }

                System.Diagnostics.Debug.WriteLine("✅ Sincronización completada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al sincronizar: {ex.Message}");
            }
        }

        private async void Cancelar()
        {
            bool respuesta = await Application.Current.MainPage.DisplayAlert(
                "Cancelar",
                "¿Está seguro que desea cancelar? Se perderán todos los cambios.",
                "Sí",
                "No");

            if (respuesta)
            {
                foreach (var medicamento in MedicamentosAgregados)
                {
                    var productoOriginal = Medicamentos
                        .FirstOrDefault(p => p.ProductoId == medicamento.ClaveProducto);

                    if (productoOriginal != null)
                    {
                        productoOriginal.CantidadDisponible += medicamento.Cantidad;
                    }
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

            Consulta = new ConsultaModel
            {
                FechaConsulta = DateTime.Now
            };

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

            _empleadoApiService = null;
            _inventarioApiService = null;
            _consultaRepo = null;
            _consultaApiService = null;
            _productoRepo = null;
            _tipoEnfermedadRepo = null;
            _empleadoRepo = null;
        }

        #endregion
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