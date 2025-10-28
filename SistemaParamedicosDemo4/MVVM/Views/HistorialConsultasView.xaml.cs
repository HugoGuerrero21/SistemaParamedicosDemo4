using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class HistorialConsultasView : ContentPage
    {
        public HistorialConsultasView()
        {
            InitializeComponent();
            BindingContext = new HistorialConsultasViewModel();
        }
    }
}