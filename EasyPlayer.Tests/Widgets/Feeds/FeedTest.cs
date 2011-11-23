using System;
using System.Linq;
using EasyPlayer.Widgets.Feeds;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EasyPlayer.Tests.Widgets.Feeds
{
    [TestClass]
    public class FeedTest
    {
        [TestMethod]
        public void Given_an_rss_document_should_create_feed_with_title()
        {
            var rssUri = new Uri("protocol://host/uri.rss");
            Feed.Downloader = (u, cb) =>
            {
                if (u == rssUri)
                    cb(false, null, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<rss xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:atom=""http://www.w3.org/2005/Atom"" version=""2.0"">
	<channel>
		<title>My New Feed</title>
		<item>
			<title>Whistle</title>
			<guid isPermaLink=""true"">item1</guid>
			<enclosure url=""http://blockedcontent/whistle.mp3"" type=""audio/mp3"" />
		</item>
		<item>
			<title>Shout</title>
			<guid isPermaLink=""true"">item2</guid>
			<enclosure url=""http://blockedcontent/shout.mp3"" type=""audio/mp3"" />
		</item>
	</channel>
</rss>");
            };

            Feed.CreateFrom(rssUri, f =>
            {
                Assert.AreEqual("My New Feed", f.Name);
                Assert.AreEqual(rssUri, f.Url);
                Assert.AreEqual(2, f.DownloadedGuids.Length);
                Assert.AreEqual("item1", f.DownloadedGuids[0]);
                Assert.AreEqual("item2", f.DownloadedGuids[1]);
            });
        }

        [TestMethod]
        public void Given_a_feed_with_two_new_items_should_download()
        {
            var rssUri = new Uri("protocol://host/uri.rss");
            Feed.Downloader = (u, cb) =>
            {
                if (u == rssUri)
                    cb(false, null, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<rss xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:atom=""http://www.w3.org/2005/Atom"" version=""2.0"">
	<channel>
		<title>My New Feed</title>
		<item>
			<title>Whistle</title>
			<guid isPermaLink=""true"">item1</guid>
			<enclosure url=""http://blockedcontent/whistle.mp3"" type=""audio/mp3"" />
		</item>
		<item>
			<title>Shout</title>
			<guid isPermaLink=""true"">item2</guid>
			<enclosure url=""http://blockedcontent/shout.mp3"" type=""audio/mp3"" />
		</item>
		<item>
			<title>Hello</title>
			<guid isPermaLink=""true"">item3</guid>
			<enclosure url=""http://blockedcontent/hello.mp3"" type=""audio/mp3"" />
		</item>
	</channel>
</rss>");
            };

            var feed = new Feed { Name = "test", DownloadedGuids = new[] { "item1" }, Url = rssUri };
            var newItemsToDownload = feed.GetNewItemsInFeed();
            Assert.AreEqual(2, newItemsToDownload.Count());

            var feedItem = newItemsToDownload.FirstOrDefault(fi => fi.Guid == "item2");
            Assert.IsNotNull(feedItem);
            Assert.AreEqual("Shout", feedItem.Title);
            Assert.AreEqual("http://blockedcontent/shout.mp3", feedItem.EnclosureUrl.ToString());

            feedItem = newItemsToDownload.FirstOrDefault(fi => fi.Guid == "item3");
            Assert.IsNotNull(feedItem);
            Assert.AreEqual("Hello", feedItem.Title);
            Assert.AreEqual("http://blockedcontent/hello.mp3", feedItem.EnclosureUrl.ToString());
        }


        [TestMethod]
        public void Given_a_feed_with_no_new_items_should_not_download()
        {
            var rssUri = new Uri("protocol://host/uri.rss");
            Feed.Downloader = (u, cb) =>
            {
                if (u == rssUri)
                    cb(false, null, @"<?xml version=""1.0"" encoding=""UTF-8""?>
<rss xmlns:itunes=""http://www.itunes.com/dtds/podcast-1.0.dtd"" xmlns:atom=""http://www.w3.org/2005/Atom"" version=""2.0"">
	<channel>
		<title>My New Feed</title>
		<item>
			<title>Whistle</title>
			<guid isPermaLink=""true"">item1</guid>
			<enclosure url=""http://blockedcontent/whistle.mp3"" type=""audio/mp3"" />
		</item>
    </channel>
</rss>");
            };

            var feed = new Feed { Name = "test", DownloadedGuids = new[] { "item1" }, Url = rssUri };
            var newItemsToDownload = feed.GetNewItemsInFeed();
            Assert.AreEqual(0, newItemsToDownload.Count());
        }
    }
}
