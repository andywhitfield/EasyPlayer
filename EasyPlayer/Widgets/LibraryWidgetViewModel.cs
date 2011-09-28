using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Library.DefaultView;
using EasyPlayer.Messages;

namespace EasyPlayer.Widgets
{
    public class LibraryWidgetViewModel : Screen, IAppWidget, ICanNavigate, IHandle<MediaItemDeletedMessage>
    {
        private static readonly ILog log = Logger.Log<LibraryWidgetViewModel>();

        readonly IEventAggregator eventAgg;
        readonly ILibrary library;
        readonly System.Action onCollectionChanged;

        public LibraryWidgetViewModel(IEventAggregator eventAgg, ILibrary library)
        {
            this.eventAgg = eventAgg;
            eventAgg.Subscribe(this);
            this.library = library;

            onCollectionChanged = () =>
            {
                LibraryItems = library.MediaItems.Where(m => !m.IsDeleted).Select(m => new MediaItemViewModel(eventAgg, m));
                log.Info("Found {0} media items to show", LibraryItems.Count());
                NotifyOfPropertyChange(() => LibraryItems);
            };

            library.MediaItems.CollectionChanged += (s, e) => onCollectionChanged();
            onCollectionChanged();
        }

        public string Name { get { return "Library"; }  }
        public IEnumerable<MediaItemViewModel> LibraryItems { get; private set; }

        public void PlayMediaItem(MediaItem item)
        {
            eventAgg.Publish(new PlayRequestMessage(item));
        }

        public bool CanNavigateTo(string searchValue)
        {
            return searchValue.EndsWith(".mp3", StringComparison.CurrentCultureIgnoreCase);
        }

        public void NavigateTo(string searchValue)
        {
            eventAgg.Publish(new ActivateWidgetMessage(this));
            Uri mediaUri;
            if (searchValue.StartsWith("http", StringComparison.CurrentCultureIgnoreCase))
                mediaUri = new Uri(searchValue, UriKind.Absolute);
            else
                mediaUri = new Uri("file://" + searchValue);

            library.AddNewMediaItem(ItemName(mediaUri), mediaUri);
        }

        private string ItemName(Uri uri)
        {
            var url = uri.ToString();
            var lastIndex = url.LastIndexOfAny(new [] { '/', '\\' });
            var filename = url.Substring(lastIndex + 1);
            lastIndex = filename.LastIndexOf('.');
            if (lastIndex >= 0)
                filename = filename.Substring(0, lastIndex);
            return filename;
        }

        public void Handle(MediaItemDeletedMessage message)
        {
            onCollectionChanged();
            library.Update(message.Media);
        }
    }
}
