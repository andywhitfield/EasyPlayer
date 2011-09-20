using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using EasyPlayer.Persistence;

namespace EasyPlayer.Library.Persistence
{
    public class MediaItemPersister : IMediaItemPersister
    {
        private readonly IPersistence persistence;

        public MediaItemPersister(IPersistence persistence)
        {
            this.persistence = persistence;
        }

        public void Save(MediaItem mediaItem)
        {
            if (!mediaItem.IsAvailable)
            {
                Debug.WriteLine("Item {0} isn't available, cannot save!", mediaItem.Name);
                return;
            }

            this.persistence.WriteTextFile("library", mediaItem.Name, Serialize(mediaItem));
            if (mediaItem.IsDeleted)
                this.persistence.DeleteFile("library.data", mediaItem.Name);
            else if (!this.persistence.Filenames("library.data").Contains(mediaItem.Name))
                this.persistence.WriteBinaryFile("library.data", mediaItem.Name, mediaItem.DataStream());

            Debug.WriteLine("Item {0} saved", mediaItem.Name);
        }

        public IEnumerable<MediaItem> LoadAll()
        {
            foreach (var mediaName in this.persistence.Filenames("library"))
            {
                Debug.WriteLine("Loading {0}", mediaName);

                var serializedMediaItem = XDocument.Parse(this.persistence.ReadTextFile("library", mediaName));
                var item = new MediaItem
                {
                    Name = mediaName,
                    IsAvailable = true,
                    IsDeleted = bool.Parse(serializedMediaItem.Element("MediaItem").Attribute("Deleted").Value),
                    MediaPosition = double.Parse(serializedMediaItem.Element("MediaItem").Attribute("MediaPosition").Value),
                    UtcDateAddedToLibrary = DateTime.ParseExact(serializedMediaItem.Element("MediaItem").Attribute("UtcDateAddedToLibrary").Value, "u", null, DateTimeStyles.AssumeUniversal).ToUniversalTime(),
                    DataStream = () => this.persistence.ReadBinaryFile("library.data", mediaName),
                };

                foreach (var extendedProps in serializedMediaItem.Element("MediaItem").Element("ExtendedProperties").Descendants())
                    item.ExtendedProperties.Add(extendedProps.Name.LocalName, extendedProps.Value);

                yield return item;
            }
        }

        private string Serialize(MediaItem item)
        {
            var xml = new XDocument(
              new XElement("MediaItem",
                new XAttribute("Name", item.Name),
                new XAttribute("Deleted", item.IsDeleted),
                new XAttribute("MediaPosition", item.MediaPosition),
                new XAttribute("UtcDateAddedToLibrary", item.UtcDateAddedToLibrary.ToString("u")),
                new XElement("ExtendedProperties",
                    from ep
                    in item.ExtendedProperties
                    select new XElement(ep.Key, ep.Value)
                )
              )
            );
            using (var writer = new StringWriter())
            {
                xml.Save(writer);
                return writer.ToString();
            }
        }
    }
}
