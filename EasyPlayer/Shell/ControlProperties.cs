using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace EasyPlayer.Shell
{
    public static class ControlProperties
    {
        public static readonly DependencyProperty FocusedProperty =
            DependencyProperty.RegisterAttached(
                "Focused",
                typeof(bool),
                typeof(ControlProperties),
                new PropertyMetadata(OnFocusedChanged)
                );

        public static void SetFocused(DependencyObject d, bool focused)
        {
            d.SetValue(FocusedProperty, focused);
        }

        public static bool GetFocused(DependencyObject d)
        {
            return (bool)d.GetValue(FocusedProperty);
        }

        private static void OnFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue)
                return;

            var el = d as Control;
            if (el == null) return;
            if (!GetFocused(d)) return;
            el.Focus();
        }
    }
}
