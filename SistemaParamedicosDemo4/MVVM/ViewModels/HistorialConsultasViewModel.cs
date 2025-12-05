using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
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
        private ProductoRepository _productoRepo;
        private TipoEnfermedadRepository _tipoEnfermedadRepo; // ⭐ AGREGAR
        private UsuarioAccesoRepositories _usuarioRepo; // ⭐ AGREGAR
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

                    // ⭐ PRIMERO SINCRONIZAR, LUEGO CARGAR
                    Task.Run(async () =>
                    {
                        await SincronizarConsultasAsync();
                        await MainThread.InvokeOnMainThreadAsync(() => CargarConsultas());
                    });
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
            _tipoEnfermedadRepo = new TipoEnfermedadRepository(); // ⭐ INICIALIZAR
            _usuarioRepo = new UsuarioAccesoRepositories(); // ⭐ INICIALIZAR

            Consultas = new ObservableCollection<ConsultaModelExtendido>();
            VerDetalleCommand = new Command<ConsultaModelExtendido>(VerDetalle);
            ToggleExpandidoCommand = new Command<ConsultaModelExtendido>(OnToggleExpandido);

            System.Diagnostics.Debug.WriteLine("✓ HistorialConsultasViewModel inicializado");
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

        private void CargarConsultas()
        {
            try
            {
                IsBusy = true;
                Consultas.Clear();

                System.Diagnostics.Debug.WriteLine($"📋 Cargando consultas de: {Empleado.IdEmpleado}");

                // ✅ USAR MÉTODO CORRECTO
                var consultas = _consultaRepo.GetConsultasByEmpleado(Empleado.IdEmpleado);

                System.Diagnostics.Debug.WriteLine($"✓ Consultas encontradas en BD: {consultas.Count}");

                // ⭐ MOSTRAR TODAS LAS CONSULTAS
                foreach (var c in consultas)
                {
                    System.Diagnostics.Debug.WriteLine($"  - ID: {c.IdConsulta}, Fecha: {c.FechaConsulta}, Motivo: {c.MotivoConsulta}");
                }

                // ✅ CARGAR RELACIONES MANUALMENTE
                foreach (var consulta in consultas)
                {
                    System.Diagnostics.Debug.WriteLine($"  Procesando consulta ID: {consulta.IdConsulta}");

                    // Cargar TipoEnfermedad
                    if (consulta.TipoEnfermedad == null)
                    {
                        consulta.TipoEnfermedad = _tipoEnfermedadRepo.GetById(consulta.IdTipoEnfermedad);
                        System.Diagnostics.Debug.WriteLine($"    TipoEnfermedad: {consulta.TipoEnfermedad?.NombreEnfermedad ?? "NULL"}");
                    }

                    // Cargar Usuario
                    if (consulta.UsuariosAcceso == null && !string.IsNullOrEmpty(consulta.IdUsuarioAcc))
                    {
                        var usuarios = _usuarioRepo.GetAllUsuarios();
                        consulta.UsuariosAcceso = usuarios.FirstOrDefault(u => u.IdUsuario == consulta.IdUsuarioAcc);
                        System.Diagnostics.Debug.WriteLine($"    Usuario: {consulta.UsuariosAcceso?.Nombre ?? "NULL"}");
                    }

                    var consultaExtendida = new ConsultaModelExtendido
                    {
                        IdConsulta = consulta.IdConsulta,
                        IdEmpleado = consulta.IdEmpleado,
                        Empleado = consulta.Empleado,
                        IdUsuarioAcc = consulta.IdUsuarioAcc,
                        UsuariosAcceso = consulta.UsuariosAcceso,
                        IdTipoEnfermedad = consulta.IdTipoEnfermedad,
                        TipoEnfermedad = consulta.TipoEnfermedad,
                        IdMovimiento = consulta.IdMovimiento,
                        FrecuenciaRespiratoria = consulta.FrecuenciaRespiratoria,
                        FrecuenciaCardiaca = consulta.FrecuenciaCardiaca,
                        Temperatura = string.IsNullOrWhiteSpace(consulta.Temperatura) ? "N/A" : consulta.Temperatura,
                        PresionArterial = string.IsNullOrWhiteSpace(consulta.PresionArterial) ? "N/A" : consulta.PresionArterial,
                        Observaciones = consulta.Observaciones,
                        UltimaComida = consulta.UltimaComida,
                        MotivoConsulta = consulta.MotivoConsulta,
                        FechaConsulta = consulta.FechaConsulta,
                        Diagnostico = consulta.Diagnostico,
                        TieneMovimiento = !string.IsNullOrEmpty(consulta.IdMovimiento),
                        EstaExpandido = false,
                        Medicamentos = new ObservableCollection<MovimientoDetalleModel>(),
                        TieneMedicamentos = false
                    };

                    consultaExtendida.TieneObservaciones = !string.IsNullOrWhiteSpace(consulta.Observaciones);

                    Consultas.Add(consultaExtendida);
                }

                TotalConsultas = Consultas.Count;
                System.Diagnostics.Debug.WriteLine($"✅ {TotalConsultas} consultas cargadas exitosamente");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar consultas: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsBusy = false;
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

                    // Aquí deberías guardarlas en SQLite si no existen
                    // Por ahora solo las mostramos
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error sincronizando: {ex.Message}");
            }
        }

        private void OnToggleExpandido(ConsultaModelExtendido consulta)
        {
            if (consulta == null) return;

            System.Diagnostics.Debug.WriteLine($"🔄 Toggle expandido - Consulta {consulta.IdConsulta}");
            System.Diagnostics.Debug.WriteLine($"   EstaExpandido: {consulta.EstaExpandido} -> {!consulta.EstaExpandido}");
            System.Diagnostics.Debug.WriteLine($"   TieneMedicamentos: {consulta.TieneMedicamentos}");
            System.Diagnostics.Debug.WriteLine($"   IdMovimiento: {consulta.IdMovimiento ?? "NULL"}");

            // ✅ Solo cargar medicamentos cuando se expande por primera vez
            if (!consulta.EstaExpandido && !consulta.TieneMedicamentos && !string.IsNullOrEmpty(consulta.IdMovimiento))
            {
                System.Diagnostics.Debug.WriteLine("📦 Cargando medicamentos...");
                CargarMedicamentosDelayado(consulta);
            }

            consulta.EstaExpandido = !consulta.EstaExpandido;
        }

        private async void CargarMedicamentosDelayado(ConsultaModelExtendido consulta)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"📦 Buscando medicamentos para movimiento: {consulta.IdMovimiento}");

                var medicamentos = _movimientoDetalleRepo.GetDetallesByMovimiento(consulta.IdMovimiento);
                System.Diagnostics.Debug.WriteLine($"✓ Medicamentos encontrados: {medicamentos.Count}");

                // ✅ Cargar el nombre del producto
                foreach (var medicamento in medicamentos)
                {
                    System.Diagnostics.Debug.WriteLine($"  Cargando producto: {medicamento.ClaveProducto}");

                    var producto = _productoRepo.GetProductosById(medicamento.ClaveProducto);
                    if (producto != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✓ Producto encontrado: {producto.Nombre}");
                        medicamento.Producto = producto;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"  ⚠️ Producto NO encontrado para: {medicamento.ClaveProducto}");
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    consulta.Medicamentos = new ObservableCollection<MovimientoDetalleModel>(medicamentos);
                    consulta.TieneMedicamentos = medicamentos.Count > 0;
                    System.Diagnostics.Debug.WriteLine($"✅ {medicamentos.Count} medicamentos agregados a la consulta");
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar medicamentos: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ Stack: {ex.StackTrace}");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ⭐ CLASE EXTENDIDA CON ACORDEÓN Y MEDICAMENTOS
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

        // ⭐ LISTA DE MEDICAMENTOS
        public ObservableCollection<MovimientoDetalleModel> Medicamentos { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}