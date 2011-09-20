using System.Diagnostics;
using Caliburn.Micro;
using EasyPlayer.Messages;

namespace EasyPlayer.Shell
{
    public class OutOfQuotaViewModel : Screen
    {
        private IEventAggregator eventAgg;
        private OutOfQuotaMessage outOfQuotaMessage;
        private long increaseTo;

        public OutOfQuotaViewModel(IEventAggregator eventAgg, OutOfQuotaMessage outOfQuotaMessage)
        {
            this.eventAgg = eventAgg;
            this.outOfQuotaMessage = outOfQuotaMessage;

            IncreaseTo = IncreaseToMin;
        }

        public override string DisplayName
        {
            get { return "Increase quota"; }
            set { }
        }

        public string QuotaSize { get { return outOfQuotaMessage.CurrentQuotaSize.ToString(); } }

        public string RequiredIncrease { get { return IncreaseToMin.ToString(); } }

        public long IncreaseToMin { get { return outOfQuotaMessage.AttemptingToSaveSize - outOfQuotaMessage.QuotaFree; } }
        
        public long IncreaseTo
        {
            get
            {
                return increaseTo;
            }
            set
            {
                increaseTo = value;
                NotifyOfPropertyChange(() => IncreaseTo);
            }
        }

        public void OK()
        {
            var increaseBy = IncreaseTo - outOfQuotaMessage.CurrentQuotaSize;
            Debug.WriteLine("Increasing quota by {0} bytes", increaseBy);
            eventAgg.Publish(new IncreaseQuotaMessage(increaseBy));
            TryClose();
        }
    }
}
