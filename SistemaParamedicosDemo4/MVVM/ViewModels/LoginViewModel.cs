using PropertyChanged;
using SistemaParamedicosDemo4.MVVM.Views;
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

        public LoginViewModel()
        {
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

                bool loginExitoso = await ValidarCredenciales(usuario, password);

                if (loginExitoso)
                {
                    // Navegamos a la vista de consulta
                    Application.Current.MainPage = new NavigationPage(new ConsultaView());
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "Usuario o contraseña incorrectos",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    $"Error al iniciar sesión: {ex.Message}",
                    "OK");
            }
            finally
            {
                isLoading = false;
            }
        }

        private async Task<bool> ValidarCredenciales(string usuario, string password)
        {
            await Task.Delay(100);

            return (usuario == "admin" && password == "admin123")
                || (usuario == "paramedico" && password == "para123");
        }

        private void OnPropertyChangedMethod()
        {
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }
}
