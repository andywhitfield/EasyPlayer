using Caliburn.Micro;
using EasyPlayer.Library;
using EasyPlayer.Messages;

namespace EasyPlayer.Widgets
{
    public class LibraryWidgetViewModel : IAppWidget
    {
        readonly IEventAggregator eventAgg;
        public LibraryWidgetViewModel(IEventAggregator eventAgg, ILibrary library)
        {
            this.eventAgg = eventAgg;
            LibraryItems = new BindableCollection<IMediaItem>(library.MediaItems);
        }

        public string Name { get { return "Library"; }  }
        public BindableCollection<IMediaItem> LibraryItems { get; private set; }

        public void PlayMediaItem(IMediaItem item)
        {
            eventAgg.Publish(new PlayRequestMessage(item));
        }
    }
}
