using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Caliburn.Micro;
using EasyPlayer.Persistence;

namespace EasyPlayer.Library.Persistence
{
    public class MediaItemPersister : IMediaItemPersister
    {
        private static readonly ILog log = Logger.Log<MediaItemPersister>();

        private readonly IPersistence persistence;

        public MediaItemPersister(IPersistence persistence)
        {
            this.persistence = persistence;
        }

        public void Save(MediaItem mediaItem)
        {
            if (!mediaItem.IsAvailable)
            {
                log.Warn("Item {0} isn't available, cannot save!", mediaItem.Name);
                return;
            }

            this.persistence.WriteTextFile("library", mediaItem.Id, Serialize(mediaItem));
            if (mediaItem.IsDeleted)
                this.persistence.DeleteFile("library.data", mediaItem.Id);
            else if (!this.persistence.Filenames("library.data").Contains(mediaItem.Id))
                this.persistence.WriteBinaryFile("library.data", mediaItem.Id, mediaItem.DataStream());

            log.Info("Item {0} saved", mediaItem.Name);
        }

        public MediaItem Load(string mediaId)
        {
            log.Info("Loading {0}", mediaId);

            var serializedMediaItem = XDocument.Parse(this.persistence.ReadTextFile("library", mediaId));
            var item = new MediaItem
            {
                Id = mediaId,
                Name = serializedMediaItem.Element("MediaItem").Attribute("Name").Value,
                IsAvailable = true,
                IsDeleted = bool.Parse(serializedMediaItem.Element("MediaItem").Attribute("Deleted").Value),
                MediaPosition = double.Parse(serializedMediaItem.Element("MediaItem").Attribute("MediaPosition").Value),
                UtcDateAddedToLibrary = DateTime.ParseExact(serializedMediaItem.Element("MediaItem").Attribute("UtcDateAddedToLibrary").Value, "u", null, DateTimeStyles.AssumeUniversal).ToUniversalTime(),
                DataStream = () => this.persistence.ReadBinaryFile("library.data", mediaId),
            };

            foreach (var extendedProps in serializedMediaItem.Element("MediaItem").Element("ExtendedProperties").Descendants())
                item.ExtendedProperties.Add(extendedProps.Name.LocalName, extendedProps.Value);

            return item;
        }

        public IEnumerable<MediaItem> LoadAll()
        {
            return from mediaId in this.persistence.Filenames("library") select Load(mediaId);
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
