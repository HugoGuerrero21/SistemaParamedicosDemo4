using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views
{
	public partial class DetalleConsultaView : ContentPage
	{
		public DetalleConsultaView()
		{
			InitializeComponent();
			// ? ASIGNAR EL VIEWMODEL
			BindingContext = new DetalleConsultaViewModel();
		}
	}
}