using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Caliburn.Micro;
using EasyPlayer.Persistence;

namespace EasyPlayer.Widgets.Feeds
{
    public class FeedRepository : IFeedRepository
    {
        private static readonly ILog log = Logger.Log<FeedRepository>();
        private static readonly string PersistenceFilename = "feeds.persist";

        private CustomObservableCollection<Feed> feeds = new CustomObservableCollection<Feed>();
        private IPersistence persistence;

        public FeedRepository(IPersistence persistence)
        {
            this.persistence = persistence;
            feeds = new CustomObservableCollection<Feed>(Load());
        }

        public ObservableCollection<Feed> Feeds
        {
            get { return feeds; }
        }

        public void Add(Uri feedUrl)
        {
            Feed.CreateFrom(feedUrl, f =>
            {
                feeds.Add(f);
                Save();
            });
        }

        public void Delete(Feed item)
        {
            feeds.Remove(item);
            Save();
        }

        public void Update(Feed feed)
        {
            Save();
            feeds.RaiseCollectionChanged();
        }

        private IEnumerable<Feed> Load()
        {
            log.Info("Loading feed repository");

            var feedFile = this.persistence.ReadTextFile(PersistenceFilename);
            if (string.IsNullOrWhiteSpace(feedFile)) yield break;

            var serializedFeeds = XDocument.Parse(feedFile);
            foreach (var feed in serializedFeeds.Root.Elements("Feed"))
                yield return Load(feed);
        }

        private Feed Load(XElement feed)
        {
            var item = new Feed
            {
                Name = feed.Attribute("Name").Value,
                Url = new Uri(feed.Attribute("Url").Value),
                LastCheckInfo = feed.Attribute("LastCheckInfo").Value,
                DownloadedGuids = feed.Elements("DownloadedGuid").Select(e => e.Value).ToArray()
            };

            return item;
        }

        private void Save()
        {
            log.Info("Saving feed repository");
            var xml = new XDocument(new XElement("Feeds", feeds.Select(f => FeedXml(f))));

            using (var writer = new StringWriter())
            {
                xml.Save(writer);
                persistence.WriteTextFile(PersistenceFilename, writer.ToString());
            }
        }

        private XElement FeedXml(Feed feed)
        {
            return new XElement("Feed",
                new XAttribute("Name", feed.Name),
                new XAttribute("Url", feed.Url),
                new XAttribute("LastCheckInfo", feed.LastCheckInfo),
                feed.DownloadedGuids.Where(guid => !string.IsNullOrWhiteSpace(guid)).Select(guid => new XElement("DownloadedGuid", guid))
                );
        }
    }
}
