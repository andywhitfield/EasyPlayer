using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace EasyPlayer.MediaControl
{
    public static class Media
    {
        public static readonly DependencyProperty MediaStreamProperty =
            DependencyProperty.RegisterAttached(
                "MediaStream",
                typeof(Stream),
                typeof(Media),
                new PropertyMetadata(OnMediaStreamChanged)
                );

        public static void SetMediaStream(DependencyObject d, Stream stream)
        {
            d.SetValue(MediaStreamProperty, stream);
        }

        public static Stream GetMediaStream(DependencyObject d)
        {
            return d.GetValue(MediaStreamProperty) as Stream;
        }

        private static void OnMediaStreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var mediaEl = d as MediaElement;
            if (mediaEl == null) return;
            mediaEl.SetSource(e.NewValue as Stream);
            mediaEl.Play();
        }
    }
}
