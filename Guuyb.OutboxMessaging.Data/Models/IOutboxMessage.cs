using System;

namespace Guuyb.OutboxMessaging.Data.Models
{
    public interface IOutboxMessage
    {
        DateTime CreatedAt { get; set; }
        int Id { get; set; }
        byte[] Payload { get; set; }
        string PayloadTypeName { get; set; }
        int PublishAttemptCount { get; set; }
        DateTime? PublishedAt { get; set; }
        OutboxMessageStateEnum StateId { get; set; }
        string TargetQueueName { get; set; }
        string RoutingKey { get; set; }
        string ParentActivityId { get; set; }
        DateTime? DelayUntil { get; set; }
    }
}
