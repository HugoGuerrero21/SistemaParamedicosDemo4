using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
    public partial class EmpleadosListView : ContentPage
    {
        public EmpleadosListView()
        {
            InitializeComponent();
            BindingContext = new EmpleadosListViewModel();
        }
    }
}