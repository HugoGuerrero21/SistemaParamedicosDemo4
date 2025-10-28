using System.Windows.Input;

namespace SistemaParamedicosDemo4.Behaviors
{
    /// <summary>
    /// Behavior que ejecuta un Command cuando la página aparece (OnAppearing)
    /// </summary>
    public class PageAppearingBehavior : Behavior<Page>
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

        protected override void OnAttachedTo(Page bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Appearing += OnPageAppearing;
        }

        protected override void OnDetachingFrom(Page bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.Appearing -= OnPageAppearing;
        }

        private void OnPageAppearing(object sender, EventArgs e)
        {
            if (Command?.CanExecute(null) == true)
            {
                Command.Execute(null);
            }
        }
    }
}