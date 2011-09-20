namespace EasyPlayer.Messages
{
    public class IncreaseQuotaMessage
    {
        private readonly long increaseBy;

        public IncreaseQuotaMessage(long increaseBy)
        {
            this.increaseBy = increaseBy;
        }

        public long IncreaseBy { get { return increaseBy; } }
    }
}
