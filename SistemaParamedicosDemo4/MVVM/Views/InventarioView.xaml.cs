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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InicializarVistaAsync(); // Aquí inicia el timer
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }
    }
}