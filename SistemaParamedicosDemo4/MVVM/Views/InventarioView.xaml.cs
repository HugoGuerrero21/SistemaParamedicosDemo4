using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class InventarioView : ContentPage
    {
        private InventarioViewModel _viewModel;

        public InventarioView()
        {
            InitializeComponent();
            _viewModel = new InventarioViewModel();
            BindingContext = _viewModel;
            System.Diagnostics.Debug.WriteLine("✓ InventarioView inicializada con ViewModel");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("🚀 InventarioView.OnAppearing ejecutado");
            await _viewModel.InicializarVistaAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("👋 InventarioView.OnDisappearing ejecutado");
            // Limpiar recursos del ViewModel
            _viewModel?.Dispose();
        }
    }
}