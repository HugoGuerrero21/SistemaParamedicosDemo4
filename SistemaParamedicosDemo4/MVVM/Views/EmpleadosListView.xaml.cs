using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
	public partial class EmpleadosListView : ContentPage
	{
		private EmpleadosListViewModel _viewModel;

		public EmpleadosListView()
		{
			InitializeComponent();
			_viewModel = (EmpleadosListViewModel)BindingContext;

			// ⭐ SUSCRIBIRSE AL MENSAJE
			MessagingCenter.Subscribe<ConsultaViewModel, string>(this, "ConsultaGuardada", (sender, idEmpleado) =>
			{
				System.Diagnostics.Debug.WriteLine($"📩 Mensaje recibido: actualizar empleado {idEmpleado}");
				_viewModel.RefrescarEmpleados();
			});
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			// ⭐ DESUSCRIBIRSE AL SALIR DE LA VISTA
			MessagingCenter.Unsubscribe<ConsultaViewModel, string>(this, "ConsultaGuardada");
		}

		private void OnEmpleadoSelected(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.Count > 0)
			{
				((CollectionView)sender).SelectedItem = null;
			}
		}
	}
}