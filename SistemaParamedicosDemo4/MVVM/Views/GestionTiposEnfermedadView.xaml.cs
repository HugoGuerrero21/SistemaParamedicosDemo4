using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class GestionTiposEnfermedadView : ContentPage
    {
        private GestionTiposEnfermedadViewModel _viewModel;

        public GestionTiposEnfermedadView()
        {
            InitializeComponent();
            _viewModel = new GestionTiposEnfermedadViewModel();
            BindingContext = _viewModel;
        }

        // ⭐ AGREGAR ESTE MÉTODO
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.InicializarAsync();
        }
    }
}