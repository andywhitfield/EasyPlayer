using System;
using System.Collections.ObjectModel;
using System.Net;
using Caliburn.Micro;
using EasyPlayer.Library.Persistence;

namespace EasyPlayer.Library
{
    public class Library : ILibrary
    {
        private static readonly ILog log = Logger.Log<Library>();

        private readonly IMediaItemPersister mediaItemPersister;
        private readonly ObservableCollection<MediaItem> mediaItems;

        public Library(IMediaItemPersister mediaItemPersister)
        {
            this.mediaItemPersister = mediaItemPersister;
            log.Info("Creating library...");

            mediaItems = new ObservableCollection<MediaItem>(this.mediaItemPersister.LoadAll());

            log.Info("Library populated successfully");
        }

        public ObservableCollection<MediaItem> MediaItems
        {
            get { return mediaItems; }
        }

        public MediaItem AddNewMediaItem(string name, Uri originalUri)
        {
            var newMediaItem = new MediaItem { Name = name, IsAvailable = false };
            MediaItems.Add(newMediaItem);

            log.Info("Adding item {0} (url: {1}) to library", name, originalUri);

            var client = new WebClient();
            client.OpenReadCompleted += (s, e) =>
            {
                log.Info("Item {0} (url: {1}) download complete.", name, originalUri);
                newMediaItem.DataStream = () => e.Result;
                newMediaItem.IsAvailable = true;
                mediaItemPersister.Save(newMediaItem);
            };
            client.DownloadProgressChanged += (s, e) =>
            {
                newMediaItem.DownloadProgress = e.ProgressPercentage;
            };
            client.OpenReadAsync(originalUri);

            return newMediaItem;
        }

        public void Update(MediaItem item)
        {
            mediaItemPersister.Save(item);
        }
    }
}
