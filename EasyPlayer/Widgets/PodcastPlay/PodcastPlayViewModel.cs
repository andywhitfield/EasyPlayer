using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Library.DefaultView;
using EasyPlayer.Messages;

namespace EasyPlayer.Widgets.PodcastPlay
{
    public class PodcastPlayViewModel : Screen, IAppWidget, IHandle<PlayRequestMessage>, IHandle<NowPlayingMediaEndedMessage>, IHandle<MediaItemDeletedMessage>
    {
        private static readonly ILog log = Logger.Log<PodcastPlayViewModel>();
        private static readonly string PodcastPlay_Played = "PodcastPlay.Played";

        private readonly IEventAggregator eventAgg;
        private readonly ILibrary library;
        private readonly IPodcastPlayPersister persister;

        private List<MediaItem> playlistItems;
        private List<MediaItem> excludedLibraryItems;

        private MediaItem playing;

        public string Name { get { return "Podcast Play"; } }

        public PodcastPlayViewModel(ILibrary library, IEventAggregator eventAgg, IPodcastPlayPersister persister)
        {
            this.eventAgg = eventAgg;
            this.eventAgg.Subscribe(this);
            this.library = library;
            this.persister = persister;

            if (persister.IsEmpty)
            {
                playlistItems = library.MediaItems.Where(m => m.IsAvailable && !m.IsDeleted && !HasPlayed(m)).ToList();
                excludedLibraryItems = new List<MediaItem>();
            }
            else
            {
                playlistItems = persister.PlaylistItems.Join(library.MediaItems, p => p, m => m.Id, (p, m) => m).ToList();
                excludedLibraryItems = persister.ExcludedLibraryItems.Join(library.MediaItems, p => p, m => m.Id, (p, m) => m).ToList();
                SyncWithLibrary();
            }

            this.library.MediaItems.CollectionChanged += (s, e) => SyncWithLibrary();
        }

        private bool HasPlayed(MediaItem m)
        {
            string played;
            m.ExtendedProperties.TryGetValue(PodcastPlay_Played, out played);
            return played == "Y";
        }

        public IEnumerable<PlaylistItemViewModel> PlaylistItems
        {
            get
            {
                var index = 0;
                return playlistItems.Select(m =>
                {
                    var mediaItemView = new MediaItemViewModel(eventAgg, m);
                    mediaItemView.OnPlayRequested += (s, e) => OnPlayRequested(m);
                    return new PlaylistItemViewModel(this, mediaItemView, true, index++, playlistItems.Count);
                });
            }
        }

        public IEnumerable<PlaylistItemViewModel> ExcludedItems
        {
            get
            {
                var index = 0;
                return excludedLibraryItems.Select(m =>
                    new PlaylistItemViewModel(this, new MediaItemViewModel(eventAgg, m), false, index++, excludedLibraryItems.Count));
            }
        }

        private void SyncWithLibrary()
        {
            var validLibraryItems = library.MediaItems.Where(m => m.IsAvailable && !m.IsDeleted && !HasPlayed(m));
            foreach (var item in validLibraryItems)
            {
                if (playlistItems.Any(m => m.Id == item.Id)) continue;
                if (excludedLibraryItems.Any(m => m.Id == item.Id)) continue;
                // item is not in our set...add it:
                log.Info("Item {0} is not in our playlist...adding.", item.Name);
                playlistItems.Add(item);
            }

            var toRemove = new List<MediaItem>();
            foreach (var item in playlistItems)
            {
                if (validLibraryItems.Any(m => m.Id == item.Id)) continue;
                // item is no longer in the library, remove it from our playlist
                log.Info("Item {0} is no longer in the library, removing.", item.Name);
                toRemove.Add(item);
            }
            toRemove.ForEach(m => playlistItems.Remove(m));

            toRemove.Clear();
            foreach (var item in excludedLibraryItems)
            {
                if (validLibraryItems.Any(m => m.Id == item.Id)) continue;
                // item is no longer in the library, remove it from our playlist
                log.Info("Item {0} is no longer in the library, removing.", item.Name);
                toRemove.Add(item);
            }
            toRemove.ForEach(m => excludedLibraryItems.Remove(m));

            PlaylistsChanged();
        }

        public void PlaylistsChanged()
        {
            NotifyOfPropertyChange(() => PlaylistItems);
            NotifyOfPropertyChange(() => ExcludedItems);

            persister.Save(playlistItems.Select(m => m.Id), excludedLibraryItems.Select(m => m.Id));
        }

        public void Move(PlaylistItemViewModel item, bool moveUp)
        {
            Move(item.IsInPlaylist ? playlistItems : excludedLibraryItems, item, moveUp);
            PlaylistsChanged();
        }

        public void SwitchPlaylist(PlaylistItemViewModel item)
        {
            var removeFrom = item.IsInPlaylist ? playlistItems : excludedLibraryItems;
            var addTo = item.IsInPlaylist ? excludedLibraryItems : playlistItems;

            removeFrom.RemoveAt(item.DisplayIndex);
            addTo.Add(item.Model.Item);
            PlaylistsChanged();
        }

        private void Move(List<MediaItem> items, PlaylistItemViewModel item, bool moveUp)
        {
            items.RemoveAt(item.DisplayIndex);
            items.Insert(moveUp ? Math.Max(0, item.DisplayIndex - 1) : Math.Min(playlistItems.Count, item.DisplayIndex + 1), item.Model.Item);
        }

        private void OnPlayRequested(MediaItem mediaItem)
        {
            playing = mediaItem;
            log.Info("Media item '{0}' has been started from the podcast playlist. Auto-play has been enabled.", playing.Name);
        }

        public void Handle(PlayRequestMessage message)
        {
            // if the item that has been requested to play is not
            // the one playing, then the user must not have clicked
            // the Podcast Play widget library item
            if (playing == null) return;
            if (message.Media.Id != playing.Id)
            {
                log.Info("Media item '{0}' has been started, but not from the podcast play. Auto-play has been cancelled.", message.Media.Name);
                playing = null;
            }
        }

        public void Handle(MediaItemDeletedMessage message)
        {
            SyncWithLibrary();
        }

        public void Handle(NowPlayingMediaEndedMessage message)
        {
            if (playing == null) return;
            if (message.Media.Id != playing.Id)
            {
                log.Info("Media item '{0}' has ended, but not from the podcast play. Auto-play has been cancelled.", message.Media.Name);
                playing = null;
            }

            foreach (var item in playlistItems)
            {
                if (item.Id == message.Media.Id)
                {
                    playlistItems.Remove(item);
                    message.Media.ExtendedProperties.Remove(PodcastPlay_Played);
                    message.Media.ExtendedProperties.Add(PodcastPlay_Played, "Y");

                    PlaylistsChanged();

                    if (playlistItems.Any())
                    {
                        var nextItemToPlay = playlistItems.First();
                        OnPlayRequested(nextItemToPlay);
                        eventAgg.Publish(new PlayRequestMessage(nextItemToPlay));
                    }

                    return;
                }
            }

            log.Info("Media item '{0}' has ended, but this doesn't appear to be in the playlist. Auto-play has been cancelled.", message.Media.Name);
        }
    }
}
