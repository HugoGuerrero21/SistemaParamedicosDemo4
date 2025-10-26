using PropertyChanged;
using SistemaParamedicosDemo4.Data.Repositories;
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

		#region Repositorio
		private UsuarioAccesoRepositories _usuariosRepository;
		#endregion

		public LoginViewModel()
		{
			_usuariosRepository = new UsuarioAccesoRepositories();
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
					// ⭐ CAMBIO: Navegar al AppShell en lugar de ConsultaView directamente
					Application.Current.MainPage = new AppShell();
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
			bool credencialesValidas = _usuariosRepository.ValidarCredenciales(usuario, password);
			return credencialesValidas;
		}

		private void OnPropertyChangedMethod()
		{
			((Command)LoginCommand).ChangeCanExecute();
		}
	}
}