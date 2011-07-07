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
using Caliburn.Micro;

namespace EasyPlayer.Shell
{
    public class NavigationBarViewModel : Screen
    {
        private static readonly string DefaultText = "Type here to search, download audio (mp3) or video, and more...";

        private string searchValue;
        private SolidColorBrush searchValueColor;

        public NavigationBarViewModel()
        {
            SearchLostFocused();
        }

        public string SearchValue
        {
            get { return searchValue; }
            set
            {
                searchValue = value;
                NotifyOfPropertyChange(() => SearchValue);
                NotifyOfPropertyChange(() => CanGo);
            }
        }

        public void StartSearch()
        {
            // TODO: set SearchValue box as focus and select all text
        }

        public void SearchFocused()
        {
            if (SearchValue != DefaultText) return;
            SearchValue = "";
            SearchValueColor = new SolidColorBrush(Colors.Black);
        }

        public void SearchLostFocused()
        {
            if (!string.IsNullOrWhiteSpace(SearchValue)) return;
            SearchValue = DefaultText;
            SearchValueColor = new SolidColorBrush(Colors.Gray);
        }

        public void SearchKeyDown(KeyEventArgs e)
        {
            if (CanGo && e.Key == Key.Enter)
                Go();
        }

        public SolidColorBrush SearchValueColor
        {
            get { return searchValueColor; }
            set
            {
                searchValueColor = value;
                NotifyOfPropertyChange(() => SearchValueColor);
            }
        }

        public bool CanGo
        {
            get { return !string.IsNullOrWhiteSpace(SearchValue) && SearchValue != DefaultText; }
        }

        public void Go()
        {
            MessageBox.Show("Do something useful with the value: " + SearchValue);
        }
    }
}
