using System.Windows.Input;

namespace SistemaParamedicosDemo4.Behaviors
{
    public class PageAppearingBehavior : Behavior<ContentPage>
    {
        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(
                nameof(Command),
                typeof(ICommand),
                typeof(PageAppearingBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttachedTo(ContentPage bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Appearing += OnPageAppearing;
            System.Diagnostics.Debug.WriteLine("✓ PageAppearingBehavior attached");
        }

        protected override void OnDetachingFrom(ContentPage bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Appearing -= OnPageAppearing;
            System.Diagnostics.Debug.WriteLine("✓ PageAppearingBehavior detached");
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("👁️ OnPageAppearing - Ejecutando comando...");

            if (Command != null && Command.CanExecute(null))
            {
                System.Diagnostics.Debug.WriteLine("✓ Comando puede ejecutarse");
                Command.Execute(null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"❌ Comando no puede ejecutarse. Command null: {Command == null}");
            }
        }
    }
}