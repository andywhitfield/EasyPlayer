using System;
using System.Linq;
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
using System.Collections.Generic;

namespace EasyPlayer.Shell
{
    public class NavigationBarViewModel : Screen
    {
        private static readonly string DefaultText = "Type here to search, download audio (mp3) or video, and more...";

        private readonly IEnumerable<ICanNavigate> navigationAddins;
        private string searchValue;
        private bool searchFocused;
        private SolidColorBrush searchValueColor;

        public NavigationBarViewModel(IEnumerable<ICanNavigate> navigationAddins)
        {
            this.navigationAddins = navigationAddins;
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

        public bool SearchFocused
        {
            get { return searchFocused; }
            set
            {
                if (searchFocused == value) return;
                searchFocused = value;
                NotifyOfPropertyChange(() => SearchFocused);
            }
        }

        public void StartSearch()
        {
            SearchFocused = true;
        }

        public void SearchGotFocused()
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
            SearchFocused = false;
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
            get { return !string.IsNullOrWhiteSpace(SearchValue) && SearchValue != DefaultText && CanNavigate; }
        }

        public bool CanNavigate
        {
            get { return navigationAddins.Any(c => c.CanNavigateTo(SearchValue)); }
        }

        public void Go()
        {
            var navigator = navigationAddins.FirstOrDefault(c => c.CanNavigateTo(SearchValue));
            if (navigator == null) return;
            navigator.NavigateTo(SearchValue);
        }
    }
}
