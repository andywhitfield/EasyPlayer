using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using EasyPlayer.Library.Persistence;

namespace EasyPlayer.Library
{
    public class Library : ILibrary
    {
        private readonly IMediaItemPersister mediaItemPersister;
        private readonly ObservableCollection<MediaItem> mediaItems;

        public Library(IMediaItemPersister mediaItemPersister)
        {
            this.mediaItemPersister = mediaItemPersister;
            Debug.WriteLine("Creating library...");

            mediaItems = new ObservableCollection<MediaItem>(this.mediaItemPersister.LoadAll());

            /*
            mediaItems = new ObservableCollection<MediaItem> {
                new MediaItem { Name = "Maid with the Flaxen Hair", IsAvailable = true, DataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyPlayer.Library.Maid with the Flaxen Hair.mp3") },
                new MediaItem { Name = "Kalimba", IsAvailable = true, DataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyPlayer.Library.Kalimba.mp3") }
            };
             */

            Debug.WriteLine("Library populated successfully");
        }

        public ObservableCollection<MediaItem> MediaItems
        {
            get { return mediaItems; }
        }

        public MediaItem AddNewMediaItem(string name, Uri originalUri)
        {
            var newMediaItem = new MediaItem { Name = name, IsAvailable = false };
            MediaItems.Add(newMediaItem);

            Debug.WriteLine("Adding item {0} (url: {1}) to library", name, originalUri);

            var client = new WebClient();
            client.OpenReadCompleted += (s, e) =>
            {
                Debug.WriteLine("Item {0} (url: {1}) download complete.", name, originalUri);
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
