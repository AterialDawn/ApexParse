using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ApexParse.Views.Behaviors
{
    internal class BehaviorRelayer
    {
        public Action Receiver { get; set; } = null;

        public void Execute()
        {
            Receiver?.Invoke();
        }
    }
    internal class RenderElementToPathBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty DestinationPathProperty =
            DependencyProperty.RegisterAttached(
                "DestinationPath",
                typeof(string),
                typeof(RenderElementToPathBehavior),
                new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty SaveNowRelayProperty =
            DependencyProperty.RegisterAttached(
                "SaveNowRelay",
                typeof(BehaviorRelayer),
                typeof(RenderElementToPathBehavior),
                new FrameworkPropertyMetadata() { BindsTwoWayByDefault = true });

        public BehaviorRelayer SaveNowRelay
        {
            get { return (BehaviorRelayer)GetValue(SaveNowRelayProperty); }
            set { SetValue(SaveNowRelayProperty, value); }
        }

        public string DestinationPath
        {
            get { return (string)GetValue(DestinationPathProperty); }
            set { SetValue(DestinationPathProperty, value); }
        }

        protected override void OnAttached()
        {
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(SaveNowRelayProperty, typeof(RenderElementToPathBehavior)).AddValueChanged(this, onRelayChanged);
        }

        protected override void OnDetaching()
        {
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(SaveNowRelayProperty, typeof(RenderElementToPathBehavior)).RemoveValueChanged(this, onRelayChanged);
        }

        private void onRelayChanged(object sender, EventArgs args)
        {
            SaveNowRelay.Receiver = renderUiElementNow;
        }

        private void renderUiElementNow()
        {
            UIElement element = AssociatedObject as UIElement;
            RenderTargetBitmap bmp = new RenderTargetBitmap((int)element.RenderSize.Width, (int)element.RenderSize.Height, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(element);

            var encoder = new JpegBitmapEncoder();
            encoder.QualityLevel = 95;

            encoder.Frames.Add(BitmapFrame.Create(bmp));
            using (Stream stm = File.Create(DestinationPath))
                encoder.Save(stm);
        }
    }
}
