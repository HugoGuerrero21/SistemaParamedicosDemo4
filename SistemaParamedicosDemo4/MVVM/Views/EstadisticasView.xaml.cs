using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class EstadisticasView : ContentPage
    {
        private EstadisticasViewModel _viewModel;

        public EstadisticasView()
        {
            InitializeComponent();
            _viewModel = new EstadisticasViewModel();
            BindingContext = _viewModel;

            System.Diagnostics.Debug.WriteLine("✓ EstadisticasView inicializada");
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("👁️ EstadisticasView.OnAppearing");

            await _viewModel.InicializarAsync();
        }
    }
}