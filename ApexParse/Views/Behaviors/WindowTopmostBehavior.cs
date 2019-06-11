using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace ApexParse.Views.Behaviors
{
    class WindowTopmostBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty StayTopmostProperty =
            DependencyProperty.RegisterAttached(
                "StayTopmost",
                typeof(bool),
                typeof(WindowTopmostBehavior),
                new FrameworkPropertyMetadata(false) { BindsTwoWayByDefault = true });

        public bool StayTopmost
        {
            get { return (bool)GetValue(StayTopmostProperty); }
            set { SetValue(StayTopmostProperty, value); }
        }

        private bool _topmostAttached = false;

        protected override void OnAttached()
        {
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(StayTopmostProperty, typeof(WindowTopmostBehavior)).AddValueChanged(this, OnTopmostChanged);
            OnTopmostChanged(this, EventArgs.Empty);
        }

        protected override void OnDetaching()
        {
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(StayTopmostProperty, typeof(WindowTopmostBehavior)).RemoveValueChanged(this, OnTopmostChanged);
        }

        private void OnTopmostChanged(object sender, EventArgs args)
        {
            if (StayTopmost && !_topmostAttached)
            {
                AssociatedObject.Deactivated += AssociatedObject_Deactivated;
                AssociatedObject.Topmost = true;
                _topmostAttached = true;
            }
            else if (!StayTopmost && _topmostAttached)
            {
                AssociatedObject.Deactivated -= AssociatedObject_Deactivated;
                AssociatedObject.Topmost = false;
                _topmostAttached = false;
            }
        }

        private void AssociatedObject_Deactivated(object sender, EventArgs e)
        {
            if (!AssociatedObject.Topmost)
            {
                AssociatedObject.Topmost = true;
            }
        }
    }
}
