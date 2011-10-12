using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Caliburn.Micro;
using EasyPlayer.Persistence;

namespace EasyPlayer.Widgets.PodcastPlay
{
    public class PodcastPlayPersister : IPodcastPlayPersister
    {
        private static readonly ILog log = Logger.Log<PodcastPlayPersister>();
        private static readonly string PersistenceFilename = "podcastplay.playlists";

        private readonly IPersistence persistence;

        public PodcastPlayPersister(IPersistence persistence)
        {
            this.persistence = persistence;
        }

        public bool IsEmpty
        {
            get { return !persistence.Filenames().Contains(PersistenceFilename); }
        }

        public IEnumerable<string> PlaylistItems
        {
            get { return Load("PlaylistItems"); }
        }

        public IEnumerable<string> ExcludedLibraryItems
        {
            get { return Load("ExcludedItems"); }
        }

        private IEnumerable<string> Load(string item)
        {
            log.Info("Loading {0}", item);

            var savedPlaylistsFile = this.persistence.ReadTextFile(PersistenceFilename);
            if (string.IsNullOrWhiteSpace(savedPlaylistsFile)) yield break;

            var serializedPlaylists = XDocument.Parse(savedPlaylistsFile);
            foreach (var playlistItem in serializedPlaylists.Root.Element(item).Descendants())
                yield return playlistItem.Value;
        }

        public void Save(IEnumerable<string> playlistItems, IEnumerable<string> excludedLibraryItems)
        {
            log.Info("Saving playlist items");
            var xml = new XDocument(
                new XElement("PodcastPlaylist",
                new XElement("PlaylistItems", playlistItems.Select(m => new XElement("Item", m))),
                new XElement("ExcludedItems", excludedLibraryItems.Select(m => new XElement("Item", m)))
                )
            );

            using (var writer = new StringWriter())
            {
                xml.Save(writer);
                persistence.WriteTextFile(PersistenceFilename, writer.ToString());
            }
        }
    }
}
