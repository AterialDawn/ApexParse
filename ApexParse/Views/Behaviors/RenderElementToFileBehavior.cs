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
    internal class RenderElementToPathBehavior : Behavior<UIElement>
    {
        public static readonly DependencyProperty DestinationPathProperty =
            DependencyProperty.RegisterAttached(
                "DestinationPath",
                typeof(string),
                typeof(RenderElementToPathBehavior),
                new FrameworkPropertyMetadata(null) { BindsTwoWayByDefault = true });

        public static readonly DependencyProperty SaveNowProperty =
            DependencyProperty.RegisterAttached(
                "SaveNow",
                typeof(bool),
                typeof(RenderElementToPathBehavior),
                new FrameworkPropertyMetadata(false) { BindsTwoWayByDefault = true });

        public bool SaveNow
        {
            get { return (bool)GetValue(SaveNowProperty); }
            set { SetValue(SaveNowProperty, value); }
        }

        public string DestinationPath
        {
            get { return (string)GetValue(DestinationPathProperty); }
            set { SetValue(DestinationPathProperty, value); }
        }

        protected override void OnAttached()
        {
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(SaveNowProperty, typeof(RenderElementToPathBehavior)).AddValueChanged(this, onSaveNowChanged);
        }

        protected override void OnDetaching()
        {
            System.ComponentModel.DependencyPropertyDescriptor.FromProperty(SaveNowProperty, typeof(RenderElementToPathBehavior)).RemoveValueChanged(this, onSaveNowChanged);
        }

        private void onSaveNowChanged(object sender, EventArgs args)
        {
            if (SaveNow)
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
}
