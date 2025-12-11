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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InicializarVistaAsync(); // Aquí inicia el timer
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel?.DetenerActualizacionAutomatica(); // ✅ DETENER TIMER
        }
    }
}