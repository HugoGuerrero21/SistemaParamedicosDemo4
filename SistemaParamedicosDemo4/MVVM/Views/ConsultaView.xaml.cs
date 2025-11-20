using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class ConsultaView : ContentPage
    {
        private ConsultaViewModel _viewModel;

        public ConsultaView()
        {
            InitializeComponent();
            _viewModel = new ConsultaViewModel();
            BindingContext = _viewModel;

            System.Diagnostics.Debug.WriteLine("? ConsultaView inicializada con ViewModel");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("??? ConsultaView.OnAppearing ejecutado");

            await _viewModel.InicializarVistaAsync();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("?? ConsultaView.OnDisappearing ejecutado");

            // Limpiar recursos del ViewModel
            _viewModel?.Dispose();
        }
    }
}