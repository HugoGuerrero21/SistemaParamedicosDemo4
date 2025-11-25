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

                // ⭐ VALIDAR QUE LOS REPOSITORIOS EXISTAN
                if (_tipoEnfermedadRepo == null)
                {
                    _tipoEnfermedadRepo = new TipoEnfermedadRepository();
                    System.Diagnostics.Debug.WriteLine("⚠️ TipoEnfermedadRepository era null, reinicializado");
                }

                if (_productoRepo == null)
                {
                    _productoRepo = new ProductoRepository();
                    System.Diagnostics.Debug.WriteLine("⚠️ ProductoRepository era null, reinicializado");
                }

                if (_inventarioApiService == null)
                {
                    _inventarioApiService = new InventarioApiService();
                    System.Diagnostics.Debug.WriteLine("⚠️ InventarioApiService era null, reinicializado");
                }

                // ⭐⭐⭐ 1. CARGAR TIPOS DE ENFERMEDAD DESDE LA BD (NO PRECARGADOS) ⭐⭐⭐
                System.Diagnostics.Debug.WriteLine("🏥 Cargando tipos de enfermedad desde BD local...");
                var tiposEnfermedad = await Task.Run(() => _tipoEnfermedadRepo.GetAllTypes(), cancellationToken);

                if (!cancellationToken.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        TiposEnfermedad.Clear();
                        foreach (var tipo in tiposEnfermedad)
                        {
                            TiposEnfermedad.Add(tipo);
                        }
                    });
                    System.Diagnostics.Debug.WriteLine($"✅ {TiposEnfermedad.Count} tipos de enfermedad cargados desde BD");
                }

                // 2. CARGAR PRODUCTOS DESDE LA API
                System.Diagnostics.Debug.WriteLine("📦 Cargando productos desde API...");
                var inventarioDto = await _inventarioApiService.ObtenerExistenciasAsync();

                if (inventarioDto == null || inventarioDto.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ API sin productos, cargando desde SQLite...");

                    // Fallback a SQLite
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

                // 3. CONVERTIR DTOs A MODELS
                var productos = await Task.Run(() =>
                    inventarioDto
                        .Where(inv => inv.Existencia > 0) // Solo productos con stock
                        .Select(dto => dto.ToProductoModel())
                        .ToList(),
                    cancellationToken
                );

                // 4. SINCRONIZAR CON SQLITE
                await Task.Run(() => _productoRepo.SincronizarProductosDesdeInventario(productos), cancellationToken);
                System.Diagnostics.Debug.WriteLine("✓ Productos sincronizados con SQLite");

                // 5. ACTUALIZAR UI
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

                // Fallback a SQLite en caso de error
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

                    System.Diagnostics.Debug.WriteLine($"✓ Fallback: {TiposEnfermedad.Count} tipos y {Medicamentos.Count} productos desde SQLite");
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

        private void SeleccionarEmpleado(EmpleadoModel empleado)
        {
            if (empleado != null)
            {
                EmpleadoSeleccionado = empleado;
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
                if (SeUtilizoMaterial && MedicamentosAgregados.Count == 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Advertencia",
                        "Ha indicado que se utilizó material pero no ha agregado ningún medicamento.",
                        "OK");
                    return;
                }

                var idEmpleadoGuardado = EmpleadoSeleccionado?.IdEmpleado;

                Consulta.IdUsuarioAcceso = Preferences.Get("IdUsuario", string.Empty);
                Consulta.IdEmpleado = EmpleadoSeleccionado.IdEmpleado;
                Consulta.FechaConsulta = DateTime.Now;
                Consulta.MotivoConsulta = MotivoConsulta;
                Consulta.Diagnostico = Diagnostico;
                Consulta.IdTipoEnfermedad = TipoEnfermedadSeleccionado.IdTipoEnfermedad;
                Consulta.FrecuenciaCardiaca = FrecuenciaCardiaca;
                Consulta.FrecuenciaRespiratoria = FrecuenciaRespiratoria;
                Consulta.Temperatura = Temperatura;
                Consulta.PresionArterial = TensionArterial;
                Consulta.Observaciones = ObservacionesSignos;
                Consulta.UltimaComida = UltimaComida;

                if (SeUtilizoMaterial && MedicamentosAgregados.Count > 0)
                {
                    Consulta.IdMovimiento = Guid.NewGuid().ToString();

                    foreach (var detalle in MedicamentosAgregados)
                    {
                        detalle.IdMovimiento = Consulta.IdMovimiento;
                    }
                }

                // ✅ 1. GUARDAR EN SQLITE (LOCAL)
                bool guardado = _consultaRepo.GuardarConsultaCompleta(
                    Consulta,
                    MedicamentosAgregados.ToList());

                if (!guardado)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Error al guardar localmente: {_consultaRepo.StatusMessage}",
                        "OK");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("✓ Consulta guardada en SQLite local");

                // ✅ 2. REGISTRAR SALIDA EN LA API (SI SE USARON MEDICAMENTOS)
                if (SeUtilizoMaterial && MedicamentosAgregados.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine("📤 Registrando salida en el servidor...");

                    // Convertir medicamentos a formato de la API
                    var productosSalida = MedicamentosAgregados.Select(m => new InventarioApiService.ProductoSalidaDTO
                    {
                        IdProducto = m.ClaveProducto,
                        Cantidad = m.Cantidad
                    }).ToList();

                    // Reinicializar servicio si fue disposed
                    if (_inventarioApiService == null)
                    {
                        _inventarioApiService = new InventarioApiService();
                    }

                    bool salidaRegistrada = await _inventarioApiService.RegistrarSalidaAsync(
                        EmpleadoSeleccionado.IdEmpleado,
                        Consulta.IdUsuarioAcceso,
                        productosSalida
                    );

                    if (!salidaRegistrada)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Advertencia",
                            "La consulta se guardó localmente, pero no se pudo registrar la salida en el servidor. " +
                            "Los productos se descontaron localmente pero no en el inventario real.",
                            "OK");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Salida registrada en el servidor");
                    }
                }

                // ✅ 3. MOSTRAR MENSAJE DE ÉXITO
                await Application.Current.MainPage.DisplayAlert(
                    "Éxito",
                    "Consulta guardada correctamente",
                    "OK");

                // ✅ 4. ENVIAR MENSAJE PARA ACTUALIZAR LISTA
                if (!string.IsNullOrEmpty(idEmpleadoGuardado))
                {
                    MessagingCenter.Send(this, "ConsultaGuardada", idEmpleadoGuardado);
                    System.Diagnostics.Debug.WriteLine($"✓ Mensaje enviado para actualizar empleado {idEmpleadoGuardado}");
                }

                // ✅ 5. LIMPIAR FORMULARIO
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
            _consultaRepo = null;  // ← Sets to null
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