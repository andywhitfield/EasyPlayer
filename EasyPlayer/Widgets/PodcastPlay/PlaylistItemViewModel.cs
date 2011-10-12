using Caliburn.Micro;
using EasyPlayer.Library.DefaultView;

namespace EasyPlayer.Widgets.PodcastPlay
{
    public class PlaylistItemViewModel : Screen
    {
        private static ILog log = Logger.Log<PlaylistItemViewModel>();

        private readonly PodcastPlayViewModel playViewModel;
        internal readonly MediaItemViewModel Model;
        internal readonly int DisplayIndex;
        internal readonly bool IsInPlaylist;
        private readonly int totalPlaylistItems;

        public PlaylistItemViewModel(PodcastPlayViewModel playViewModel, MediaItemViewModel model, bool isInPlaylist, int displayIndex, int totalPlaylistItems)
        {
            this.playViewModel = playViewModel;
            this.Model = model;
            this.IsInPlaylist = isInPlaylist;
            this.DisplayIndex = displayIndex;
            this.totalPlaylistItems = totalPlaylistItems;
        }

        public MediaItemViewModel MediaItemView { get { return Model; } }

        public bool CanIncludeInPlaylist { get { return !IsInPlaylist; } }
        public bool CanExcludeFromPlaylist { get { return IsInPlaylist; } }

        public bool CanMoveUpPlaylist { get { return DisplayIndex > 0; } }
        public bool CanMoveDownPlaylist { get { return DisplayIndex < (totalPlaylistItems - 1); } }

        public void IncludeInPlaylist() { playViewModel.SwitchPlaylist(this); }
        public void ExcludeFromPlaylist() { playViewModel.SwitchPlaylist(this); }
        public void MoveUpPlaylist() { playViewModel.Move(this, true); }
        public void MoveDownPlaylist() { playViewModel.Move(this, false); }
    }
}
