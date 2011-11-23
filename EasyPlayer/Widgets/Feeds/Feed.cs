using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Xml.Linq;
using System.Xml.XPath;
using Caliburn.Micro;

namespace EasyPlayer.Widgets.Feeds
{
    public class Feed
    {
        private static readonly ILog log = Logger.Log<Feed>();

        public static Action<Uri, Action<bool, Exception, string>> Downloader = (uri, onDownloadCallback) =>
        {
            var webClient = new WebClient();
            webClient.DownloadStringCompleted += (s, e) => onDownloadCallback(e.Cancelled, e.Error, e.Result);
            webClient.DownloadStringAsync(uri);
        };

        public static void CreateFrom(Uri rssOrAtomUri, Action<Feed> onCreated)
        {
            DownloadFeed(rssOrAtomUri, feedXml =>
            {
                if (feedXml == null)
                {
                    log.Warn("Could not create feed from {0}!", rssOrAtomUri);
                    return;
                }

                var title = feedXml.XPathSelectElement("//channel/title");
                if (title == null)
                {
                    log.Warn("Could not find feed title, cannot create new feed from {0}", rssOrAtomUri);
                    return;
                }

                onCreated(new Feed
                {
                    Name = title.Value,
                    Url = rssOrAtomUri,
                    DownloadedGuids = feedXml.XPathSelectElements("//item/guid").Select(x => x.Value).ToArray()
                });
            });
        }

        public Feed()
        {
            Name = "";
            DownloadedGuids = new string[0];
            LastCheckInfo = "";
        }

        public virtual string Name { get; set; }
        public virtual Uri Url { get; set; }
        public virtual string[] DownloadedGuids { get; set; }
        public virtual string LastCheckInfo { get; set; }

        public virtual IEnumerable<FeedItem> GetNewItemsInFeed()
        {
            var waitToComplete = new ManualResetEvent(false);
            var items = new List<FeedItem>();

            DownloadFeed(Url, xml =>
            {
                if (xml == null) xml = new XDocument();
                try
                {
                    foreach (var feedItem in xml.XPathSelectElements("//item"))
                    {
                        var guid = feedItem.Element("guid").Value;
                        var title = feedItem.Element("title").Value;
                        var mp3Url = feedItem.Element("enclosure").Attribute("url").Value;

                        log.Info("Checking feed item {0} ({1})", title, guid);

                        if (DownloadedGuids.Contains(guid))
                        {
                            log.Info("Feed item {0} ({1}) already downloaded.", title, guid);
                            continue;
                        }

                        items.Add(new FeedItem(guid, title, new Uri(mp3Url)));
                    }
                }
                finally
                {
                    waitToComplete.Set();
                }
            });

            waitToComplete.WaitOne();
            return items;
        }

        private static void DownloadFeed(Uri downloadUri, Action<XDocument> downloadedFeed)
        {
            Downloader(downloadUri, (cancelled, error, result) =>
            {
                if (cancelled)
                {
                    log.Warn("Download of feed from {0} was cancelled", downloadUri);
                    downloadedFeed(null);
                    return;
                }
                if (error != null)
                {
                    log.Warn("An error occurred downloading feed from {0}", downloadUri);
                    downloadedFeed(null);
                    log.Error(error);
                }

                XDocument feedXml = null;
                try
                {
                    log.Info("Downloaded from {0}: {1}", downloadUri, result);
                    feedXml = XDocument.Parse(result);
                }
                catch (Exception ex)
                {
                    log.Warn("Error reading feed {0}", downloadUri);
                    log.Error(ex);
                }
                downloadedFeed(feedXml);
            });
        }
    }
}
