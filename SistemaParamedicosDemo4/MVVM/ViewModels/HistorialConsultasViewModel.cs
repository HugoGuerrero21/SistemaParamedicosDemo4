using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.DTOS;
using SistemaParamedicosDemo4.MVVM.Models;
using SistemaParamedicosDemo4.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    [QueryProperty(nameof(Empleado), "Empleado")]
    public class HistorialConsultasViewModel : INotifyPropertyChanged
    {
        #region Repositorios
        private ConsultaRepository _consultaRepo;
        private MovimientoDetalleRepository _movimientoDetalleRepo;
        private MovimientoDetalleApiService _movimientoDetalleApiService;
        private ProductoRepository _productoRepo;
        private TipoEnfermedadRepository _tipoEnfermedadRepo;
        private UsuarioAccesoRepositories _usuarioRepo;
        #endregion

        #region Properties
        private EmpleadoModel _empleado;
        public EmpleadoModel Empleado
        {
            get => _empleado;
            set
            {
                _empleado = value;
                OnPropertyChanged();
                if (value != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔍 Empleado recibido: {value.IdEmpleado} - {value.Nombre}");
                    CalcularEdadEmpleado();
                     _ = RecargarConsultasAsync();
                }
            }
        }

        public ObservableCollection<ConsultaModelExtendido> Consultas { get; set; }
        public int TotalConsultas { get; set; }
        public bool IsBusy { get; set; }
        public int EdadEmpleado { get; set; }
        #endregion

        #region Commands
        public ICommand VerDetalleCommand { get; }
        public ICommand ToggleExpandidoCommand { get; }
        #endregion

        public HistorialConsultasViewModel()
        {
            _consultaRepo = new ConsultaRepository();
            _movimientoDetalleRepo = new MovimientoDetalleRepository();
            _productoRepo = new ProductoRepository();
            _tipoEnfermedadRepo = new TipoEnfermedadRepository();
            _usuarioRepo = new UsuarioAccesoRepositories();
            _movimientoDetalleApiService = new MovimientoDetalleApiService();

            Consultas = new ObservableCollection<ConsultaModelExtendido>();
            VerDetalleCommand = new Command<ConsultaModelExtendido>(VerDetalle);
            ToggleExpandidoCommand = new Command<ConsultaModelExtendido>(OnToggleExpandido);

            System.Diagnostics.Debug.WriteLine("✓ HistorialConsultasViewModel inicializado");
        }

        private async Task InicializarConsultasAsync()
        {
            try
            {
                IsBusy = true;
                await SincronizarConsultasAsync();
                await MainThread.InvokeOnMainThreadAsync(() => CargarConsultas());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inicializando consultas: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void CalcularEdadEmpleado()
        {
            if (Empleado != null)
            {
                var hoy = DateTime.Today;
                var edad = hoy.Year - Empleado.FechaNacimiento.Year;
                if (Empleado.FechaNacimiento.Date > hoy.AddYears(-edad))
                    edad--;

                EdadEmpleado = edad;
                System.Diagnostics.Debug.WriteLine($"✓ Edad calculada: {EdadEmpleado} años");
            }
        }

        public async Task RecargarConsultasAsync()
        {
            try
            {
                IsBusy = true;

                // 1️⃣ Sincronizar tipos de enfermedad PRIMERO
                System.Diagnostics.Debug.WriteLine("🔄 Sincronizando tipos antes de cargar consultas...");
                await SincronizarTiposEnfermedadAsync();

                // 2️⃣ Sincronizar consultas desde API
                await SincronizarConsultasAsync();

                // 3️⃣ Cargar en UI
                await MainThread.InvokeOnMainThreadAsync(() => CargarConsultas());

                System.Diagnostics.Debug.WriteLine("✅ Recarga completa finalizada");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error recargando: {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task SincronizarTiposEnfermedadAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Sincronizando tipos de enfermedad desde API...");
                
                var tipoEnfermedadApiService = new TipoEnfermedadApiService();
                var tiposDto = await tipoEnfermedadApiService.ObtenerTiposEnfermedadAsync();

                if (tiposDto != null && tiposDto.Count > 0)
                {
                    var tiposModels = tiposDto.Select(dto => dto.ToModel()).ToList();
                    _tipoEnfermedadRepo.SincronizarTiposEnfermedad(tiposModels);
                    System.Diagnostics.Debug.WriteLine($"✅ {tiposModels.Count} tipos sincronizados");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No se obtuvieron tipos de enfermedad de la API");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"⚠️ Error sincronizando tipos: {ex.Message}");
                // No lanzar excepción, continuar con los tipos locales
            }
        }

        private async Task SincronizarConsultasAsync()
        {
            try
            {
                var consultaApiService = new ConsultaApiService();

                System.Diagnostics.Debug.WriteLine($"🔄 Sincronizando consultas de {Empleado.IdEmpleado}...");

                var consultasApi = await consultaApiService.ObtenerConsultasPorEmpleadoAsync(Empleado.IdEmpleado);

                if (consultasApi != null && consultasApi.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✓ {consultasApi.Count} consultas obtenidas de la API");

                    var consultasExistentes = _consultaRepo.GetConsultasByEmpleado(Empleado.IdEmpleado);
                    System.Diagnostics.Debug.WriteLine($"📋 {consultasExistentes.Count} consultas ya en SQLite");

                    foreach (var consultaDto in consultasApi)
                    {
                        try
                        {
                            // Verificar si ya existe
                            var existe = consultasExistentes.Any(c =>
                                c.IdEmpleado == consultaDto.IdEmpleado &&
                                c.FechaConsulta == consultaDto.FechaConsulta &&
                                c.MotivoConsulta == consultaDto.MotivoConsulta);

                            if (existe)
                            {
                                System.Diagnostics.Debug.WriteLine($"  ℹ️ Consulta del {consultaDto.FechaConsulta:dd/MM/yyyy HH:mm} ya existe");
                                continue;
                            }

                            // Convertir DTO a MODEL
                            var consultaModel = new ConsultaModel
                            {
                                IdEmpleado = consultaDto.IdEmpleado,
                                IdUsuarioAcc = consultaDto.IdUsuarioAcc,
                                IdTipoEnfermedad = consultaDto.IdTipoEnfermedad,
                                IdMovimiento = consultaDto.IdMovimiento,
                                MotivoConsulta = consultaDto.MotivoConsulta,
                                FechaConsulta = consultaDto.FechaConsulta,
                                FrecuenciaRespiratoria = consultaDto.FrecuenciaRespiratoria ?? 0,
                                FrecuenciaCardiaca = consultaDto.FrecuenciaCardiaca ?? 0,
                                Temperatura = consultaDto.Temperatura.HasValue
                                    ? consultaDto.Temperatura.Value.ToString("F1")
                                    : string.Empty,
                                PresionArterial = consultaDto.PresionArterial ?? string.Empty,
                                Observaciones = consultaDto.Observaciones ?? string.Empty,
                                UltimaComida = consultaDto.UltimaComida ?? string.Empty,
                                Diagnostico = consultaDto.Diagnostico
                            };

                            // Insertar consulta
                            int resultado = _consultaRepo.InsertarConsulta(consultaModel);

                            if (resultado > 0)
                            {
                                System.Diagnostics.Debug.WriteLine($"  ✓ Nueva consulta guardada (ID local: {resultado})");

                                // ⭐ GUARDAR MEDICAMENTOS SI HAY
                                if (consultaDto.Medicamentos != null && consultaDto.Medicamentos.Count > 0 &&
                                    !string.IsNullOrEmpty(consultaDto.IdMovimiento))
                                {
                                    System.Diagnostics.Debug.WriteLine($"  📦 Guardando {consultaDto.Medicamentos.Count} medicamentos...");

                                    foreach (var medDto in consultaDto.Medicamentos)
                                    {
                                        var detalleModel = new MovimientoDetalleModel
                                        {
                                            IdMovimientoDetalle = Guid.NewGuid().ToString("N").Substring(0, 25),
                                            IdMovimiento = consultaDto.IdMovimiento,
                                            ClaveProducto = medDto.IdProducto,
                                            Cantidad = medDto.Cantidad,
                                            CantidadUtilizada = medDto.Cantidad,
                                            Observaciones = medDto.Observaciones,
                                            Status = 1
                                        };

                                        _movimientoDetalleRepo.InsertarDetalle(detalleModel);
                                        System.Diagnostics.Debug.WriteLine($"    ✓ Medicamento guardado: {medDto.IdProducto}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"  ❌ Error guardando consulta: {ex.Message}");
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"✅ Sincronización completada");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No se obtuvieron consultas de la API");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error sincronizando: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }
        // ⭐ REEMPLAZAR SOLO CargarMedicamentosDelayado en HistorialConsultasViewModel.cs

        private async void CargarMedicamentosDelayado(ConsultaModelExtendido consulta)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📦 Cargando medicamentos para movimiento: {consulta.IdMovimiento}");

                // Los medicamentos ya deben estar en SQLite después de sincronizar
                var medicamentos = _movimientoDetalleRepo.GetDetallesByMovimiento(consulta.IdMovimiento);
                System.Diagnostics.Debug.WriteLine($"📦 {medicamentos.Count} medicamentos encontrados en BD");

                // ⭐ NUEVO: Crear el objeto Producto directamente desde Observaciones
                foreach (var medicamento in medicamentos)
                {
                    System.Diagnostics.Debug.WriteLine($"  - Detalle: {medicamento.IdMovimientoDetalle} | Producto: {medicamento.ClaveProducto} | Cantidad: {medicamento.Cantidad}");
                    System.Diagnostics.Debug.WriteLine($"    Observaciones: '{medicamento.Observaciones}'");

                    // ⭐ USAR EL NOMBRE DE OBSERVACIONES DIRECTAMENTE
                    medicamento.Producto = new ProductoModel
                    {
                        ProductoId = medicamento.ClaveProducto,
                        Nombre = string.IsNullOrWhiteSpace(medicamento.Observaciones)
                            ? "Sin nombre"
                            : medicamento.Observaciones,
                        Descripcion = "Producto de consulta"
                    };

                    System.Diagnostics.Debug.WriteLine($"    ✓ Nombre asignado: {medicamento.Producto.Nombre}");
                }

                // Actualizar UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    consulta.Medicamentos = new ObservableCollection<MovimientoDetalleModel>(medicamentos);
                    consulta.TieneMedicamentos = medicamentos.Count > 0;
                    System.Diagnostics.Debug.WriteLine($"✅ {medicamentos.Count} medicamentos en UI con nombres");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        private void CargarConsultas()
        {
            try
            {
                Consultas.Clear();

                System.Diagnostics.Debug.WriteLine($"📋 Cargando consultas de: {Empleado.IdEmpleado}");

                var consultas = _consultaRepo.GetConsultasByEmpleado(Empleado.IdEmpleado);

                System.Diagnostics.Debug.WriteLine($"✓ Consultas encontradas en BD: {consultas.Count}");

                var tiposDisponibles = _tipoEnfermedadRepo.GetAllTypes();
                System.Diagnostics.Debug.WriteLine($"📦 Tipos disponibles: {tiposDisponibles.Count}");

                foreach (var c in consultas)
                {
                    System.Diagnostics.Debug.WriteLine($"\n  📄 Procesando consulta ID: {c.IdConsulta}");
                    System.Diagnostics.Debug.WriteLine($"     Fecha: {c.FechaConsulta:dd/MM/yyyy HH:mm}");
                    System.Diagnostics.Debug.WriteLine($"     IdTipoEnfermedad: {c.IdTipoEnfermedad}");
                    System.Diagnostics.Debug.WriteLine($"     IdUsuarioAcc: '{c.IdUsuarioAcc}'");

                    // ⭐ 1. CARGAR TIPO DE ENFERMEDAD
                    var tipoModelo = tiposDisponibles.FirstOrDefault(t => t.IdTipoEnfermedad == c.IdTipoEnfermedad);

                    if (tipoModelo != null)
                    {
                        c.TipoEnfermedad = tipoModelo;
                        System.Diagnostics.Debug.WriteLine($"     ✅ Tipo: {tipoModelo.NombreEnfermedad}");
                    }
                    else
                    {
                        c.TipoEnfermedad = new TipoEnfermedadModel
                        {
                            IdTipoEnfermedad = c.IdTipoEnfermedad,
                            NombreEnfermedad = $"Tipo {c.IdTipoEnfermedad}"
                        };
                        System.Diagnostics.Debug.WriteLine($"     ⚠️ Tipo {c.IdTipoEnfermedad} no encontrado");
                    }

                    // ⭐ 2. CARGAR USUARIO/PARAMÉDICO
                    if (!string.IsNullOrEmpty(c.IdUsuarioAcc))
                    {
                        c.UsuariosAcceso = _usuarioRepo.GetById(c.IdUsuarioAcc);

                        if (c.UsuariosAcceso != null)
                        {
                            System.Diagnostics.Debug.WriteLine($"     ✅ Paramédico: {c.UsuariosAcceso.Nombre}");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"     ❌ Paramédico NO encontrado para ID: '{c.IdUsuarioAcc}'");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"     ⚠️ IdUsuarioAcc está vacío o nulo");
                    }

                    // ⭐ 3. CREAR CONSULTA EXTENDIDA
                    var consultaExtendida = new ConsultaModelExtendido
                    {
                        IdConsulta = c.IdConsulta,
                        IdEmpleado = c.IdEmpleado,
                        Empleado = c.Empleado,
                        IdUsuarioAcc = c.IdUsuarioAcc,
                        UsuariosAcceso = c.UsuariosAcceso, // ⭐ CRÍTICO: Asignar el usuario
                        IdTipoEnfermedad = c.IdTipoEnfermedad,
                        TipoEnfermedad = c.TipoEnfermedad,
                        IdMovimiento = c.IdMovimiento,
                        FrecuenciaRespiratoria = c.FrecuenciaRespiratoria,
                        FrecuenciaCardiaca = c.FrecuenciaCardiaca,
                        Temperatura = string.IsNullOrWhiteSpace(c.Temperatura) ? "N/A" : $"{c.Temperatura}°C",
                        PresionArterial = string.IsNullOrWhiteSpace(c.PresionArterial) ? "N/A" : c.PresionArterial,
                        Observaciones = c.Observaciones,
                        UltimaComida = c.UltimaComida,
                        MotivoConsulta = c.MotivoConsulta,
                        FechaConsulta = c.FechaConsulta,
                        Diagnostico = c.Diagnostico,
                        TieneMovimiento = !string.IsNullOrEmpty(c.IdMovimiento),
                        EstaExpandido = false,
                        Medicamentos = new ObservableCollection<MovimientoDetalleModel>(),
                        TieneMedicamentos = false,
                        TieneObservaciones = !string.IsNullOrWhiteSpace(c.Observaciones)
                    };

                    Consultas.Add(consultaExtendida);
                }

                TotalConsultas = Consultas.Count;
                System.Diagnostics.Debug.WriteLine($"\n✅ {TotalConsultas} consultas cargadas en la UI");

                OnPropertyChanged(nameof(Consultas));
                OnPropertyChanged(nameof(TotalConsultas));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"\n❌ ERROR al cargar consultas: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
        }

        private async void VerDetalle(ConsultaModelExtendido consulta)
        {
            if (consulta == null) return;

            var parametros = new Dictionary<string, object>
            {
                { "IdConsulta", consulta.IdConsulta }
            };

            await Shell.Current.GoToAsync("detalleConsulta", parametros);
        }

        private void OnToggleExpandido(ConsultaModelExtendido consulta)
        {
            if (consulta == null) return;

            System.Diagnostics.Debug.WriteLine($"🔄 Toggle expandido - Consulta {consulta.IdConsulta}");

            if (!consulta.EstaExpandido && !consulta.TieneMedicamentos && !string.IsNullOrEmpty(consulta.IdMovimiento))
            {
                System.Diagnostics.Debug.WriteLine("📦 Cargando medicamentos...");
                CargarMedicamentosDelayado(consulta);
            }

            consulta.EstaExpandido = !consulta.EstaExpandido;
        }



        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ConsultaModelExtendido : ConsultaModel, INotifyPropertyChanged
    {
        private bool _estaExpandido;
        public bool EstaExpandido
        {
            get => _estaExpandido;
            set
            {
                _estaExpandido = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IconoExpandido));
            }
        }

        public string IconoExpandido => EstaExpandido ? "▼" : "▶";

        public bool TieneMovimiento { get; set; }
        public bool TieneObservaciones { get; set; }
        public bool TieneMedicamentos { get; set; }

        public ObservableCollection<MovimientoDetalleModel> Medicamentos { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}