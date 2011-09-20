namespace EasyPlayer.Messages
{
    public class OutOfQuotaMessage
    {
        private readonly long currentQuotaSize;
        private readonly long quotaFree;
        private readonly long attemptingToSaveSize;

        public OutOfQuotaMessage(long currentQuotaSize, long quotaFree, long attemptingToSaveSize)
        {
            this.currentQuotaSize = currentQuotaSize;
            this.quotaFree = quotaFree;
            this.attemptingToSaveSize = attemptingToSaveSize;
        }

        public long CurrentQuotaSize { get { return currentQuotaSize; } }
        public long QuotaFree { get { return quotaFree; } }
        public long AttemptingToSaveSize { get { return attemptingToSaveSize; } }
    }
}
