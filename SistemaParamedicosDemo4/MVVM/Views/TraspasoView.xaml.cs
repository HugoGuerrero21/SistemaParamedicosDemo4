using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class TraspasoView : ContentPage
    {
        private TraspasoViewModel _viewModel;

        public TraspasoView()
        {
            InitializeComponent();
            _viewModel = new TraspasoViewModel();
            BindingContext = _viewModel;
            System.Diagnostics.Debug.WriteLine("✓ TraspasoView inicializada con ViewModel");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("🚀 TraspasoView.OnAppearing ejecutado");
            await _viewModel.InicializarVistaAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("👋 TraspasoView.OnDisappearing ejecutado");
            // Limpiar recursos del ViewModel
            _viewModel?.Dispose();
        }
    }
}