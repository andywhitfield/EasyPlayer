using System;
using Caliburn.Micro;
using EasyPlayer.Messages;

namespace EasyPlayer.Library.DefaultView
{
    public class MediaItemViewModel : Screen
    {
        private static readonly ILog log = Logger.Log<MediaItemViewModel>();

        public event EventHandler OnPlayRequested;

        private readonly IEventAggregator eventAgg;
        private readonly MediaItem item;

        public MediaItemViewModel(IEventAggregator eventAgg, MediaItem item)
        {
            this.eventAgg = eventAgg;
            this.item = item;
            this.item.DownloadProgressChanged += (s, e) => NotifyOfPropertyChange(() => Name);
            this.item.IsAvailableChanged += (s, e) =>
            {
                NotifyOfPropertyChange(() => Name);
                NotifyOfPropertyChange(() => CanPlayMediaItem);
                NotifyOfPropertyChange(() => CanDeleteMediaItem);
            };
            this.item.IsDeletedChanged += (s, e) =>
            {
                NotifyOfPropertyChange(() => CanPlayMediaItem);
                NotifyOfPropertyChange(() => CanDeleteMediaItem);
            };
        }

        public MediaItem Item { get { return item; } }

        public string Name
        {
            get
            {
                var name = item.Name;
                if (!CanPlayMediaItem)
                    name = string.Format("{0}{1}Downloading...{2}%", name, Environment.NewLine, item.DownloadProgress);
                return name;
            }
        }

        public bool CanPlayMediaItem { get { return item.IsAvailable && !item.IsDeleted; } }
        public void PlayMediaItem()
        {
            if (OnPlayRequested != null) OnPlayRequested(this, EventArgs.Empty);
            eventAgg.Publish(new PlayRequestMessage(item));
        }

        public bool CanDeleteMediaItem { get { return CanPlayMediaItem; } }
        public void DeleteMediaItem()
        {
            log.Info("Deleting item {0}", item.Name);
            item.IsDeleted = true;
            eventAgg.Publish(new MediaItemDeletedMessage(item));
        }
    }
}
