using SistemaParamedicosDemo4.MVVM.ViewModels;

namespace SistemaParamedicosDemo4.MVVM.Views;

public partial class ConsultaView : ContentPage
{
	public ConsultaView()
	{
		InitializeComponent();
		BindingContext = new ConsultaViewModel();
	}
}