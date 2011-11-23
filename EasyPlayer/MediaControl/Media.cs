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
        public static readonly DependencyProperty PlayerStateProperty =
            DependencyProperty.RegisterAttached(
                "PlayerState",
                typeof(PlayerState),
                typeof(Media),
                new PropertyMetadata(OnPlayerStateChanged)
                );

        public static void SetMediaStream(DependencyObject d, Stream stream)
        {
            d.SetValue(MediaStreamProperty, stream);
        }

        public static Stream GetMediaStream(DependencyObject d)
        {
            return d.GetValue(MediaStreamProperty) as Stream;
        }

        public static void SetPlayerState(DependencyObject d, PlayerState playerState)
        {
            d.SetValue(PlayerStateProperty, playerState);
        }

        public static PlayerState GetPlayerState(DependencyObject d)
        {
            return (PlayerState) d.GetValue(PlayerStateProperty);
        }

        private static void OnMediaStreamChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var mediaEl = d as MediaElement;
            if (mediaEl == null) return;

            var stream = e.OldValue as Stream;
            if (stream != null) stream.Dispose();

            mediaEl.Source = null;

            stream = e.NewValue as Stream;
            if (stream == null) return;
            mediaEl.SetSource(stream);
            mediaEl.Play();
        }

        private static void OnPlayerStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var mediaEl = d as MediaElement;
            if (mediaEl == null) return;

            var playerState = (PlayerState)e.NewValue;
            switch (playerState)
            {
                case PlayerState.Paused:
                    mediaEl.Pause();
                    break;
                case PlayerState.Playing:
                    mediaEl.Play();
                    break;
                case PlayerState.Stopped:
                    mediaEl.Stop();
                    break;
            }
        }
    }
}
