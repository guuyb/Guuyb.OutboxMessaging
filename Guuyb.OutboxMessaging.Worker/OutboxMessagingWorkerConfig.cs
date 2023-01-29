namespace Guuyb.OutboxMessaging.Worker
{
    public class OutboxMessagingWorkerConfig
    {
        // try to create queue/exchange if not exists
        public bool IsNeedToDeclare { get; set; }
        public string DefaultExchangeName { get; set; }
        public int? DelayBetweenExecutions { get; set; }
        public bool IsNeedToDeletePublishedMessages { get; set; }
    }
}
