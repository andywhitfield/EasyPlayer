using System.IO;
using System.Linq;
using System.Xml.Linq;
using EasyPlayer.Library;
using EasyPlayer.Library.Persistence;
using EasyPlayer.Persistence;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace EasyPlayer.Tests.Library.Persistence
{
    [TestClass]
    public class MediaItemPersisterTest
    {
        [TestMethod]
        public void When_saving_media_item_should_write_file_to_library_directory()
        {
            var persistence = new Mock<IPersistence>();
            var persister = new MediaItemPersister(persistence.Object);

            var mediaItem = new MediaItem { Id = "test1", Name = "test-1", IsAvailable = true, DataStream = () => new MemoryStream() };
            persister.Save(mediaItem);

            persistence.Verify(p => p.WriteTextFile("library", "test1", It.Is<string>(xml => MediaItemXml(mediaItem, xml))));
            persistence.Verify(p => p.WriteBinaryFile("library.data", "test1", It.IsAny<Stream>()));
        }

        [TestMethod]
        public void Given_media_binary_already_exists_when_saving_media_item_should_not_write_file_to_library_directory()
        {
            var persistence = new Mock<IPersistence>(MockBehavior.Strict);
            var persister = new MediaItemPersister(persistence.Object);

            persistence.Setup(p => p.Filenames("library.data")).Returns(new[] { "test1" });
            persistence.Setup(p => p.WriteTextFile(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));

            var mediaItem = new MediaItem { Id = "test1", Name = "test-1", IsAvailable = true, DataStream = () => new MemoryStream() };
            persister.Save(mediaItem);

            persistence.Verify(p => p.WriteTextFile("library", "test1", It.Is<string>(xml => MediaItemXml(mediaItem, xml))));
        }

        [TestMethod]
        public void When_loading_media_item_should_deserialize_from_xml()
        {
            var persistence = new Mock<IPersistence>();
            var persister = new MediaItemPersister(persistence.Object);

            persistence.Setup(p => p.Filenames("library")).Returns(new[] { "test1", "test2" });
            persistence.Setup(p => p.ReadTextFile("library", "test1")).Returns(@"<?xml version=""1.0"" encoding=""utf-16""?>
<MediaItem Name=""test-1"" Deleted=""false"" MediaPosition=""0"" UtcDateAddedToLibrary=""2011-09-08 13:30:55Z"">
  <ExtendedProperties />
</MediaItem>");
            persistence.Setup(p => p.ReadTextFile("library", "test2")).Returns(@"<?xml version=""1.0"" encoding=""utf-16""?>
<MediaItem Name=""test-2"" Deleted=""true"" MediaPosition=""0"" UtcDateAddedToLibrary=""2011-09-07 06:01:00Z"">
  <ExtendedProperties>
    <Prop1>Prop1Value</Prop1>
    <NoValueProp />
  </ExtendedProperties>
</MediaItem>");

            var allMedia = persister.LoadAll();
            Assert.AreEqual(2, allMedia.Count());

            var media = allMedia.ElementAt(0);
            Assert.AreEqual("test1", media.Id);
            Assert.AreEqual("test-1", media.Name);
            Assert.IsFalse(media.IsDeleted);
            Assert.AreEqual(0, media.MediaPosition, 0.01);
            Assert.AreEqual("2011-09-08 13:30:55Z", media.UtcDateAddedToLibrary.ToString("u"));

            media = allMedia.ElementAt(1);
            Assert.AreEqual("test2", media.Id);
            Assert.AreEqual("test-2", media.Name);
            Assert.IsTrue(media.IsDeleted);
            Assert.AreEqual(0, media.MediaPosition, 0.01);
            Assert.AreEqual("2011-09-07 06:01:00Z", media.UtcDateAddedToLibrary.ToString("u"));
            Assert.AreEqual("Prop1Value", media.ExtendedProperties["Prop1"]);
            Assert.AreEqual("", media.ExtendedProperties["NoValueProp"]);
        }

        private bool MediaItemXml(MediaItem item, string xml)
        {
            var xdoc = XDocument.Parse(xml);
            using (var writer = new StringWriter())
            {
                xdoc.Save(writer, SaveOptions.None);
                
                Assert.AreEqual(
                    string.Format(@"<?xml version=""1.0"" encoding=""utf-16""?>
<MediaItem Name=""{0}"" Deleted=""{1}"" MediaPosition=""{2}"" UtcDateAddedToLibrary=""{3}"">
  <ExtendedProperties />
</MediaItem>", item.Name, item.IsDeleted.ToString().ToLower(), item.MediaPosition, item.UtcDateAddedToLibrary.ToString("u")),
                    writer.ToString());

                return true;
            }
        }
    }
}
