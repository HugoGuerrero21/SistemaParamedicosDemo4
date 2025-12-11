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
    public class GestionTiposEnfermedadViewModel : INotifyPropertyChanged
    {
        #region Repositorios y Servicios
        private TipoEnfermedadRepository _tipoEnfermedadRepo;
        private TipoEnfermedadApiService _tipoEnfermedadApiService;
        #endregion

        #region Properties
        public ObservableCollection<TipoEnfermedadModel> TiposEnfermedad { get; set; }
        public string NuevoTipoNombre { get; set; }
        public bool IsCargando { get; set; }
        public bool MostrarFormulario { get; set; }
        public string MensajeEstado { get; set; }
        public int TotalTipos { get; set; }
        #endregion

        #region Commands
        public ICommand CargarTiposCommand { get; }
        public ICommand MostrarFormularioCommand { get; }
        public ICommand GuardarTipoCommand { get; }
        public ICommand CancelarCommand { get; }
        #endregion

        public GestionTiposEnfermedadViewModel()
        {
            _tipoEnfermedadRepo = new TipoEnfermedadRepository();
            _tipoEnfermedadApiService = new TipoEnfermedadApiService();

            TiposEnfermedad = new ObservableCollection<TipoEnfermedadModel>();

            // Inicializar comandos
            CargarTiposCommand = new Command(async () => await CargarTiposAsync());
            MostrarFormularioCommand = new Command(MostrarFormularioNuevoTipo);
            GuardarTipoCommand = new Command(async () => await GuardarTipoAsync(), CanGuardar);
            CancelarCommand = new Command(CancelarFormulario);

            // Configurar PropertyChanged
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(NuevoTipoNombre))
                {
                    ((Command)GuardarTipoCommand).ChangeCanExecute();
                }
            };
        }

        public async Task InicializarAsync()
        {
            await CargarTiposAsync();
        }

        private async Task CargarTiposAsync()
        {
            try
            {
                IsCargando = true;
                MensajeEstado = "Cargando clasificaciones...";

                // 1. INTENTAR CARGAR DESDE LA API
                var tiposDto = await _tipoEnfermedadApiService.ObtenerTiposEnfermedadAsync();

                if (tiposDto != null && tiposDto.Count > 0)
                {
                    // 2. CONVERTIR Y ORDENAR ALFABÉTICAMENTE
                    var tipos = tiposDto
                        .Select(dto => dto.ToModel())
                        .OrderBy(t => t.NombreEnfermedad) // ⭐ ORDEN ALFABÉTICO
                        .ToList();

                    // 3. SINCRONIZAR CON SQLITE
                    _tipoEnfermedadRepo.SincronizarTiposEnfermedad(tipos);

                    // 4. ACTUALIZAR UI
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        TiposEnfermedad.Clear();
                        foreach (var tipo in tipos)
                        {
                            TiposEnfermedad.Add(tipo);
                        }
                        TotalTipos = TiposEnfermedad.Count;
                        MensajeEstado = $"{TotalTipos} clasificaciones disponibles";
                    });

                    System.Diagnostics.Debug.WriteLine($"✅ {TotalTipos} tipos cargados desde API");
                }
                else
                {
                    // FALLBACK A SQLITE
                    System.Diagnostics.Debug.WriteLine("⚠️ API sin datos, cargando desde SQLite...");

                    var tiposLocal = _tipoEnfermedadRepo.GetAllTypes()
                        .OrderBy(t => t.NombreEnfermedad) // ⭐ ORDEN ALFABÉTICO
                        .ToList();

                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        TiposEnfermedad.Clear();
                        foreach (var tipo in tiposLocal)
                        {
                            TiposEnfermedad.Add(tipo);
                        }
                        TotalTipos = TiposEnfermedad.Count;
                        MensajeEstado = $"{TotalTipos} clasificaciones (caché local)";
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al cargar tipos: {ex.Message}");
                MensajeEstado = "Error al cargar clasificaciones";
            }
            finally
            {
                IsCargando = false;
            }
        }

        private void MostrarFormularioNuevoTipo()
        {
            NuevoTipoNombre = string.Empty;
            MostrarFormulario = true;
        }

        private bool CanGuardar()
        {
            return !string.IsNullOrWhiteSpace(NuevoTipoNombre) &&
                   NuevoTipoNombre.Length >= 3;
        }

        private async Task GuardarTipoAsync()
        {
            try
            {
                // PRIMERA CONFIRMACIÓN
                bool confirmar1 = await Application.Current.MainPage.DisplayAlert(
                    "Confirmar",
                    $"¿Está seguro que desea crear el tipo de enfermedad '{NuevoTipoNombre}'?\n\n" +
                    "⚠️ Una vez creado, NO podrá eliminarlo.",
                    "Continuar",
                    "Cancelar");

                if (!confirmar1) return;

                // SEGUNDA CONFIRMACIÓN
                bool confirmar2 = await Application.Current.MainPage.DisplayAlert(
                    "⚠️ Confirmación Final",
                    $"Esta es su última oportunidad para cancelar.\n\n" +
                    $"Tipo a crear: '{NuevoTipoNombre}'\n\n" +
                    "¿Desea continuar?",
                    "Sí, crear",
                    "No, cancelar");

                if (!confirmar2) return;

                IsCargando = true;
                MensajeEstado = "Guardando tipo de enfermedad...";

                // OBTENER ID DE USUARIO
                var idUsuario = Preferences.Get("IdUsuario", string.Empty);

                var dto = new CrearTipoEnfermedadDto
                {
                    NombreEnfermedad = NuevoTipoNombre.Trim(),
                    IdUsuarioAcc = idUsuario
                };

                // INTENTAR GUARDAR EN LA API
                var (exito, mensaje, tipoCreado) = await _tipoEnfermedadApiService.CrearTipoEnfermedadAsync(dto);

                if (exito)
                {
                    // GUARDAR EN SQLITE
                    var nuevoTipo = tipoCreado.ToModel();
                    _tipoEnfermedadRepo.InsertarTipo(nuevoTipo);

                    // ACTUALIZAR UI
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        var todosLosTipos = TiposEnfermedad.ToList();
                        todosLosTipos.Add(nuevoTipo);

                        // Reordenar alfabéticamente
                        todosLosTipos = todosLosTipos
                            .OrderBy(t => t.NombreEnfermedad)
                            .ToList();

                        TiposEnfermedad.Clear();
                        foreach (var tipo in todosLosTipos)
                        {
                            TiposEnfermedad.Add(tipo);
                        }

                        TotalTipos = TiposEnfermedad.Count;
                    });

                    await Application.Current.MainPage.DisplayAlert(
                        "Éxito",
                        "Tipo de enfermedad creado correctamente",
                        "OK");

                    CancelarFormulario();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        mensaje,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al guardar: {ex.Message}",
                    "OK");
            }
            finally
            {
                IsCargando = false;
                MensajeEstado = $"{TotalTipos} tipos de enfermedad disponibles";
            }
        }

        private void CancelarFormulario()
        {
            NuevoTipoNombre = string.Empty;
            MostrarFormulario = false;
        }

        public async Task SincronizarTodosTiposAsync()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("🔄 Sincronizando tipos de enfermedad desde API...");

                var tiposDto = await _tipoEnfermedadApiService.ObtenerTiposEnfermedadAsync();

                if (tiposDto == null || tiposDto.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("⚠️ No se obtuvieron tipos de la API");
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"✅ {tiposDto.Count} tipos obtenidos de la API");

                // CONVERTIR Y SINCRONIZAR
                var tiposModels = tiposDto.Select(dto => dto.ToModel()).ToList();
                _tipoEnfermedadRepo.SincronizarTiposEnfermedad(tiposModels);

                // ACTUALIZAR UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    TiposEnfermedad.Clear();
                    foreach (var tipo in tiposModels)
                    {
                        TiposEnfermedad.Add(tipo);
                        System.Diagnostics.Debug.WriteLine($"  ✅ Tipo {tipo.IdTipoEnfermedad}: {tipo.NombreEnfermedad}");
                    }
                    TotalTipos = TiposEnfermedad.Count;
                });

                System.Diagnostics.Debug.WriteLine($"✅ Sincronización completa: {TotalTipos} tipos ahora disponibles");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error sincronizando tipos: {ex.Message}");
            }
        }


        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}