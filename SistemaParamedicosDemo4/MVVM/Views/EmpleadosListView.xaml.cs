using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class EmpleadosListView : ContentPage
    {
        private EmpleadosListViewModel _viewModel;

        public EmpleadosListView()
        {
            InitializeComponent();

            System.Diagnostics.Debug.WriteLine("🏗️ EmpleadosListView - Constructor");

            // ⭐ CREAR E INICIALIZAR EL VIEWMODEL AQUÍ
            _viewModel = new EmpleadosListViewModel();
            BindingContext = _viewModel;

            System.Diagnostics.Debug.WriteLine("✓ ViewModel asignado al BindingContext");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            System.Diagnostics.Debug.WriteLine("👁️ EmpleadosListView.OnAppearing");

            // ⭐ LLAMAR DIRECTAMENTE AL MÉTODO ASYNC DEL VIEWMODEL
            if (_viewModel != null)
            {
                await _viewModel.SincronizarEmpleadosAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("👋 EmpleadosListView.OnDisappearing");
        }
    }
}