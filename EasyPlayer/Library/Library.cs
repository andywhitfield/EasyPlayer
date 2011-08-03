﻿using System;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Net;
using System.Threading;
using System.Diagnostics;

namespace EasyPlayer.Library
{
    public class Library : ILibrary
    {
        private readonly ObservableCollection<MediaItem> dummyItems;

        public Library()
        {
            Debug.WriteLine("Creating library...");

            dummyItems = new ObservableCollection<MediaItem> {
                new MediaItem { Name = "Maid with the Flaxen Hair", IsAvailable = true, DataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyPlayer.Library.Maid with the Flaxen Hair.mp3") },
                new MediaItem { Name = "Kalimba", IsAvailable = true, DataStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("EasyPlayer.Library.Kalimba.mp3") }
            };

            Debug.WriteLine("Library populated successfully");
        }

        public ObservableCollection<MediaItem> MediaItems
        {
            get { return dummyItems; }
        }

        public MediaItem AddNewMediaItem(string name, Uri originalUri)
        {
            var request = WebRequest.Create(originalUri);

            var newMediaItem = new MediaItem { Name = name, IsAvailable = false };
            MediaItems.Add(newMediaItem);

            Debug.WriteLine("Adding item {0} (url: {1}) to library", name, originalUri);

            var client = new WebClient();
            client.OpenReadCompleted += (s, e) =>
            {
                Debug.WriteLine("Item {0} (url: {1}) download complete.", name, originalUri);
                newMediaItem.DataStream = e.Result;
                newMediaItem.IsAvailable = true;
            };
            client.DownloadProgressChanged += (s, e) =>
            {
                newMediaItem.DownloadProgress = e.ProgressPercentage;
            };
            client.OpenReadAsync(originalUri);

            return newMediaItem;
        }
    }
}
