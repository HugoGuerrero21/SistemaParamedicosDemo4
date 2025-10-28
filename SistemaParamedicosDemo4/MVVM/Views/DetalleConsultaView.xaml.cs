using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class DetalleConsultaView : ContentPage
    {
        public DetalleConsultaView()
        {
            InitializeComponent();
            BindingContext = new DetalleConsultaViewModel();
        }
    }
}