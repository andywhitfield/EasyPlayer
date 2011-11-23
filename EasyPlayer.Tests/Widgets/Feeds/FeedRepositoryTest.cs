using System;
using System.Linq;
using EasyPlayer.Persistence;
using EasyPlayer.Widgets.Feeds;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Widgets.Feeds
{
    [TestClass]
    public class FeedRepositoryTest
    {
        [TestMethod]
        public void Given_a_new_rss_uri_should_add_feed_to_list_and_persist()
        {
            var persistence = new Mock<IPersistence>();
            var feedRepo = new FeedRepository(persistence.Object);

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

            feedRepo.Add(rssUri);
            Assert.AreEqual(1, feedRepo.Feeds.Count);
            Assert.AreEqual("My New Feed", feedRepo.Feeds[0].Name);
            persistence.Verify(p => p.WriteTextFile(It.IsAny<string>(), It.Is<string>(contents => ContainsFeedDetails(contents, "My New Feed", "item1"))));
        }

        [TestMethod]
        public void Given_persisted_feeds_should_rehydrate()
        {
            var persistence = new Mock<IPersistence>();
            persistence.Setup(p => p.ReadTextFile(It.IsAny<string>())).Returns(@"<Root>
<Feed Name=""First feed"" Url=""http://blockedcontent/first.rss"" LastCheckInfo="""">
 <DownloadedGuid>item1</DownloadedGuid>
 <DownloadedGuid>item2</DownloadedGuid>
</Feed>
<Feed Name=""Second feed"" Url=""http://blockedcontent/second.rss"" LastCheckInfo="""" />
</Root>");

            var feedRepo = new FeedRepository(persistence.Object);
            Assert.AreEqual(2, feedRepo.Feeds.Count);

            Assert.AreEqual("First feed", feedRepo.Feeds[0].Name);
            Assert.AreEqual(2, feedRepo.Feeds[0].DownloadedGuids.Count());
            Assert.AreEqual("item1", feedRepo.Feeds[0].DownloadedGuids.ElementAt(0));
            Assert.AreEqual("item2", feedRepo.Feeds[0].DownloadedGuids.ElementAt(1));

            Assert.AreEqual("Second feed", feedRepo.Feeds[1].Name);
            Assert.AreEqual(0, feedRepo.Feeds[1].DownloadedGuids.Count());
        }

        private bool ContainsFeedDetails(string xml, params string[] shouldContain)
        {
            foreach (var contains in shouldContain)
                Assert.IsTrue(xml.Contains(contains), "xml content didn't contain expected value '" + contains + "': " + xml);
            return true;
        }
    }
}
