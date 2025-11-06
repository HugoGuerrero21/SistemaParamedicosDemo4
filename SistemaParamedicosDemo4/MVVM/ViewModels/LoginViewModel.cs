using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
using SistemaParamedicosDemo4.MVVM.Views;
using SistemaParamedicosDemo4.Services; // ← AGREGAR
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SistemaParamedicosDemo4.MVVM.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class LoginViewModel
    {
        #region Properties
        [OnChangedMethod(nameof(OnPropertyChangedMethod))]
        public string usuario { get; set; }

        [OnChangedMethod(nameof(OnPropertyChangedMethod))]
        public string password { get; set; }

        [OnChangedMethod(nameof(OnPropertyChangedMethod))]
        public bool isLoading { get; set; }
        #endregion

        #region Commands
        public ICommand LoginCommand { get; }
        #endregion

        #region Services
        private AuthApiService _authApiService;
        #endregion

        public LoginViewModel()
        {
            _authApiService = new AuthApiService();
            LoginCommand = new Command(Login, CanLogin);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(usuario) &&
                   !string.IsNullOrWhiteSpace(password) &&
                   !isLoading;
        }

        private async void Login()
        {
            try
            {
                isLoading = true;

                // Login con API
                var response = await _authApiService.LoginAsync(usuario, password);

                if (response.Success)
                {
                    // Guardar datos del usuario
                    Preferences.Set("NombreUsuario", response.Usuario.Nombre);
                    Preferences.Set("IdUsuario", response.Usuario.IdUsuarioAcc);
                    Preferences.Set("Area", response.Usuario.Area ?? "");
                    Preferences.Set("Puesto", response.Usuario.Puesto ?? "");

                    System.Diagnostics.Debug.WriteLine($"✓ Login exitoso: {response.Usuario.Nombre}");

                    // Navegar al AppShell
                    Application.Current.MainPage = new AppShell();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        response.Message,
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al iniciar sesión: {ex.Message}",
                    "OK");

                System.Diagnostics.Debug.WriteLine($"❌ Error login: {ex.Message}");
            }
            finally
            {
                isLoading = false;
            }
        }

        private void OnPropertyChangedMethod()
        {
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }
}