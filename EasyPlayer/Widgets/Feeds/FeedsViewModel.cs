using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using Caliburn.Micro;
using EasyPlayer.Library;

namespace EasyPlayer.Widgets.Feeds
{
    public class FeedsViewModel : Screen, IAppWidget, ICanNavigate
    {
        private static readonly ILog log = Logger.Log<FeedsViewModel>();
        private static readonly TimeSpan timeBetweenRefresh = TimeSpan.FromMinutes(30);
        private static readonly TimeSpan timeBetweenRefreshWhenNoConnection = TimeSpan.FromMinutes(5);
        public static Func<bool> IsNetworkAvailable = () => NetworkInterface.GetIsNetworkAvailable();

        private readonly ILibrary library;
        private readonly IFeedRepository feedRepository;
        private Timer refreshTimer;
        private bool refreshInProgress;

        public FeedsViewModel(ILibrary library, IFeedRepository feedRepository)
        {
            this.library = library;
            this.feedRepository = feedRepository;

            refreshTimer = new Timer(RefreshFeeds, null, (int)TimeSpan.FromMinutes(1).TotalMilliseconds, Timeout.Infinite);
        }

        public string Name { get { return "Feeds"; } }
        public IEnumerable<Feed> FeedItems { get { return feedRepository.Feeds; } }
        public bool CanRefreshAll
        {
            get { return !refreshInProgress; }
            set
            {
                refreshInProgress = !value;
                NotifyOfPropertyChange(() => CanRefreshAll);
            }
        }

        public void RefreshAll()
        {
            refreshTimer.Change(0, -1);
        }

        public void DeleteFeedItem(Feed item)
        {
            feedRepository.Delete(item);
        }

        public bool CanNavigateTo(string searchValue)
        {
            return IsActive && searchValue.TrimStart().StartsWith("http://", StringComparison.CurrentCultureIgnoreCase);
        }

        public void NavigateTo(string searchValue)
        {
            feedRepository.Add(new Uri(searchValue));
        }

        public void RefreshFeeds(object state)
        {
            CanRefreshAll = false;
            try
            {
                log.Info("Starting refresh of feeds to check for new items to download");

                if (!IsNetworkAvailable())
                {
                    log.Info("Cannot check feeds - no connection is available. Will check again in {0} seconds.", timeBetweenRefreshWhenNoConnection.TotalSeconds);
                    refreshTimer.Change((int)timeBetweenRefreshWhenNoConnection.TotalMilliseconds, Timeout.Infinite);
                    return;
                }

                foreach (var feed in FeedItems)
                {
                    var newFeedItems = feed.GetNewItemsInFeed();
                    foreach (var newFeedItem in newFeedItems)
                        library.AddNewMediaItem(feed.Name + ": " + newFeedItem.Title, newFeedItem.EnclosureUrl);
                    feed.DownloadedGuids = feed.DownloadedGuids.Concat(newFeedItems.Select(fi => fi.Guid)).ToArray();
                    feed.LastCheckInfo = string.Format("Last checked: {0} {1}{2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), newFeedItems.Any() ? (" - " + newFeedItems.Count() + " new items found.") : "");
                    feedRepository.Update(feed);
                    NotifyOfPropertyChange(() => FeedItems);
                }

                log.Info("Completed refresh of feeds. Will check again in {0} seconds.", timeBetweenRefresh.TotalSeconds);

                refreshTimer.Change((int)timeBetweenRefresh.TotalMilliseconds, Timeout.Infinite);
            }
            catch (Exception ex)
            {
                log.Info("Error refreshing feeds!");
                log.Error(ex);
            }
            finally
            {
                CanRefreshAll = true;
            }
        }
    }
}
