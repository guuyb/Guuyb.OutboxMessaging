namespace Guuyb.OutboxMessaging.Data.Models
{
    public interface IOutboxMessageState
    {
        OutboxMessageStateEnum Id { get; set; }
        string Code { get; set; }
    }
}
