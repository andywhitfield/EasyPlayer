using EasyPlayer.Widgets;

namespace EasyPlayer.Messages
{
    public class ActivateWidgetMessage
    {
        public readonly IAppWidget Widget;

        public ActivateWidgetMessage(IAppWidget widget)
        {
            this.Widget = widget;
        }
    }
}
