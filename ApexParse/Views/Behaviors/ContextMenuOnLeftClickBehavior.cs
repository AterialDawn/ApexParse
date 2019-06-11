using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace ApexParse.Views.Behaviors
{
    //Borrowed from https://github.com/thoemmi/Solutionizer/blob/master/Solutionizer/Infrastructure/ContextMenuOnLeftClickBehavior.cs
    public class ContextMenuOnLeftClickBehavior : Behavior<ButtonBase>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Click += OnClick;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Click -= OnClick;
            base.OnDetaching();
        }

        private void OnClick(object sender, RoutedEventArgs routedEventArgs)
        {
            var contextMenu = AssociatedObject.ContextMenu;
            if (contextMenu == null)
            {
                return;
            }

            contextMenu.PlacementTarget = AssociatedObject;
            contextMenu.Placement = PlacementMode.Bottom;
            contextMenu.IsOpen = true;
        }
    }
}
