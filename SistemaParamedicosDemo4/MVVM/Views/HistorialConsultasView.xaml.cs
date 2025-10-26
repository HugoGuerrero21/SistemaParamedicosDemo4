using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
	public partial class HistorialConsultasView : ContentPage
	{
		public HistorialConsultasView()
		{
			InitializeComponent();
			// ? ASIGNAR EL VIEWMODEL
			BindingContext = new HistorialConsultasViewModel();
		}
	}
}