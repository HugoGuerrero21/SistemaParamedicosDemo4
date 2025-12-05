using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Models;
using SistemaParamedicosDemo4.MVVM.Views;
using SistemaParamedicosDemo4.Service;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class EmpleadosListViewModel : IQueryAttributable
    {
        #region Repositorios y Servicios
        private EmpleadoRepository _empleadoRepo;
        private ConsultaRepository _consultaRepo;
        private EmpleadoApiService _empleadoApiService; // ⭐ NUEVO
        #endregion

        #region Properties
        public ObservableCollection<EmpleadoModel> EmpleadosFiltrados { get; set; }
        private List<EmpleadoModel> _todosLosEmpleados;
        public string TextoBusqueda { get; set; }
        public int TotalEmpleados { get; set; }
        public bool IsBusy { get; set; }
        public string MensajeEstado { get; set; } // ⭐ NUEVO
        #endregion

        #region Commands
        public ICommand BuscarCommand { get; }
        public ICommand MostrarTodosCommand { get; }
        public ICommand FiltrarPorPuestoCommand { get; }
        public ICommand VerHistorialCommand { get; }
        public ICommand AppearingCommand { get; }
        #endregion

        public EmpleadosListViewModel()
        {
            _empleadoRepo = new EmpleadoRepository();
            _consultaRepo = new ConsultaRepository();
            _empleadoApiService = new EmpleadoApiService(); // ⭐ INICIALIZAR
            EmpleadosFiltrados = new ObservableCollection<EmpleadoModel>();

            // Inicializar Commands
            BuscarCommand = new Command(Buscar);
            MostrarTodosCommand = new Command(MostrarTodos);
            FiltrarPorPuestoCommand = new Command<string>(FiltrarPorPuesto);
            VerHistorialCommand = new Command<EmpleadoModel>(VerHistorial);
            
            // ⭐ COMANDO APPEARING SIN BLOQUEAR EL UI THREAD
            AppearingCommand = new Command(() => 
            {
                System.Diagnostics.Debug.WriteLine("👁️ AppearingCommand ejecutado");
                _ = OnAppearingAsync(); // Fire and forget
            });

            MessagingCenter.Subscribe<ConsultaViewModel, string>(this, "ConsultaGuardada", (sender, idEmpleado) =>
            {
                System.Diagnostics.Debug.WriteLine($"📩 Mensaje recibido en VM: actualizar empleado {idEmpleado}");
                RefrescarEmpleados();
            });

            // ⭐ NO CARGAR AQUÍ - esperar a OnAppearing
            System.Diagnostics.Debug.WriteLine("✓ EmpleadosListViewModel inicializado");

            // ⭐ PRUEBA INMEDIATA (TEMPORAL - SOLO PARA DEBUG)
            System.Diagnostics.Debug.WriteLine("🧪 Iniciando prueba inmediata de API...");
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(1000); // Esperar 1 segundo
                    System.Diagnostics.Debug.WriteLine("🧪 Ejecutando prueba de API...");
                    var test = await _empleadoApiService.ObtenerEmpleadosActivosAsync();
                    System.Diagnostics.Debug.WriteLine($"🧪 Resultado prueba: {test?.Count ?? 0} empleados");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"🧪 Error en prueba: {ex.Message}");
                }
            });
        }

        // ⭐ IMPLEMENTACIÓN DE IQueryAttributable
        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            System.Diagnostics.Debug.WriteLine("🔄 ApplyQueryAttributes - Refrescando empleados...");
            _ = RefrescarEmpleadosAsync(); // Ejecutar async sin bloquear
        }

        // ⭐ MÉTODO ASYNC PARA EL COMANDO APPEARING
        private async Task OnAppearingAsync()
        {
            System.Diagnostics.Debug.WriteLine("👁️ Vista apareció - Sincronizando con API...");
            await SincronizarEmpleadosAsync();
        }

        // ⭐ NUEVO MÉTODO: Sincronizar con API
        public async Task SincronizarEmpleadosAsync()
        {
            System.Diagnostics.Debug.WriteLine("========================================");
            System.Diagnostics.Debug.WriteLine("🚀 INICIANDO SINCRONIZACION DE EMPLEADOS");
            System.Diagnostics.Debug.WriteLine("========================================");

            if (IsBusy)
            {
                System.Diagnostics.Debug.WriteLine("⚠️ Ya hay una sincronización en progreso");
                return;
            }

            try
            {
                IsBusy = true;
                MensajeEstado = "Sincronizando empleados...";

                System.Diagnostics.Debug.WriteLine($"📡 EmpleadoApiService creado: {_empleadoApiService != null}");
               // System.Diagnostics.Debug.WriteLine($"📡 URL Base: {ApiConfiguration.BaseUrl}");
                System.Diagnostics.Debug.WriteLine("📡 Llamando a ObtenerEmpleadosActivosAsync()...");

                // 1️⃣ INTENTAR OBTENER DE LA API
                var empleadosDto = await _empleadoApiService.ObtenerEmpleadosActivosAsync();
                
                System.Diagnostics.Debug.WriteLine($"📡 Respuesta recibida. Empleados: {empleadosDto?.Count ?? 0}");

                if (empleadosDto != null && empleadosDto.Count > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"✅ {empleadosDto.Count} empleados obtenidos de la API");

                    // 2️⃣ CONVERTIR A MODELOS
                    var empleadosModels = empleadosDto.Select(dto => dto.ToEmpleadoModel()).ToList();

                    // 3️⃣ SINCRONIZAR EN SQLITE
                    bool sincronizado = _empleadoRepo.SincronizarEmpleados(empleadosModels);

                    if (sincronizado)
                    {
                        System.Diagnostics.Debug.WriteLine("✅ Empleados sincronizados en SQLite");
                        MensajeEstado = "Datos actualizados desde servidor";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ Error al sincronizar en SQLite");
                        MensajeEstado = "Error al guardar datos localmente";
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No se obtuvieron empleados de la API, usando datos locales");
                    MensajeEstado = "Usando datos locales (sin conexión)";
                }

                // 4️⃣ CARGAR EMPLEADOS (desde SQLite, ya actualizado)
                CargarEmpleados();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al sincronizar: {ex.Message}");
                MensajeEstado = "Error de conexión, usando datos locales";

                // FALLBACK: Cargar desde SQLite local
                CargarEmpleados();
            }
            finally
            {
                IsBusy = false;

                // Limpiar mensaje después de 3 segundos
                _ = Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    await MainThread.InvokeOnMainThreadAsync(() => MensajeEstado = string.Empty);
                });
            }
        }

        private void CargarEmpleados()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("📂 Cargando empleados desde SQLite...");

                var empleados = _empleadoRepo.GetAll();

                // Obtener total de consultas de cada empleado
                _todosLosEmpleados = empleados.Select(e =>
                {
                    var totalConsultas = _consultaRepo.GetConsultasByEmpleado(e.IdEmpleado).Count;
                    e.TotalConsultas = totalConsultas;
                    return e;
                }).ToList();

                MostrarTodos();
                TotalEmpleados = _todosLosEmpleados.Count;

                System.Diagnostics.Debug.WriteLine($"✅ {TotalEmpleados} empleados cargados desde SQLite");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar empleados: {ex.Message}");
            }
        }

        // ⭐ VERSIÓN ASYNC DEL MÉTODO DE REFRESCO
        public async Task RefrescarEmpleadosAsync()
        {
            System.Diagnostics.Debug.WriteLine("🔄 Refrescando lista de empleados con API...");
            await SincronizarEmpleadosAsync();
        }

        // Mantener versión sync para compatibilidad
        public void RefrescarEmpleados()
        {
            System.Diagnostics.Debug.WriteLine("🔄 Refrescando lista de empleados (sync)...");
            CargarEmpleados();
        }

        private void Buscar()
        {
            if (string.IsNullOrWhiteSpace(TextoBusqueda))
            {
                MostrarTodos();
                return;
            }

            var busqueda = TextoBusqueda.ToLower();

            var empleadosFiltrados = _todosLosEmpleados.Where(e =>
                e.Nombre.ToLower().Contains(busqueda) ||
                e.IdEmpleado.ToLower().Contains(busqueda) ||
                (e.NombrePuesto?.ToLower().Contains(busqueda) ?? false) ||
                (e.IdPuesto?.ToLower().Contains(busqueda) ?? false)
            ).ToList();

            ActualizarLista(empleadosFiltrados);
        }

        private const int EMPLEADOS_POR_PAGINA = 50; // Aumentado a 50

        private void MostrarTodos()
        {
            var empleadosAMostrar = _todosLosEmpleados.Take(EMPLEADOS_POR_PAGINA).ToList();
            ActualizarLista(empleadosAMostrar);
        }

        private void FiltrarPorPuesto(string puesto)
        {
            var empleadosFiltrados = _todosLosEmpleados
                .Where(e => e.IdPuesto.Equals(puesto, StringComparison.OrdinalIgnoreCase) ||
                            (e.NombrePuesto?.Equals(puesto, StringComparison.OrdinalIgnoreCase) ?? false))
                .ToList();

            ActualizarLista(empleadosFiltrados);
        }

        private void ActualizarLista(List<EmpleadoModel> empleados)
        {
            EmpleadosFiltrados.Clear();
            foreach (var empleado in empleados)
            {
                EmpleadosFiltrados.Add(empleado);
            }
        }

        private async void VerHistorial(EmpleadoModel empleado)
        {
            if (empleado == null) return;

            var parametros = new Dictionary<string, object>
            {
                { "Empleado", empleado }
            };

            await Shell.Current.GoToAsync("historial", parametros);
        }

        ~EmpleadosListViewModel()
        {
            MessagingCenter.Unsubscribe<ConsultaViewModel, string>(this, "ConsultaGuardada");
        }
    }
}