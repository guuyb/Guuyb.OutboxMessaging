# Проект Guuyb.OutboxMessaging.Data

Предоставляет типы для хранения сообщений на отсылку брокеру сообщений.

## Подключение

В проекте с реализацией DbContext'а следует завести сущностости, реализующие интерфейсы `IOutboxMessage` и `IOutboxMessageState`:
```
public class OutboxMessage : IOutboxMessage
{
    public DateTime CreateDate { get; set; }
    public int Id { get; set; }
    public byte[] Payload { get; set; }
    public string PayloadTypeName { get; set; }
    public int PublishAttemptCount { get; set; }
    public DateTime? PublishDate { get; set; }
    public OutboxMessageStateEnum StateId { get; set; }
    public string StringifiedPayload { get; set; }
    public string TargetQueueName { get; set; }
}

public class OutboxMessageState : IOutboxMessageState
{
    public OutboxMessageStateEnum Id { get; set; }
    public string Code { get; set; }
}
```

В целевом контексте следует завести DbSet'ы и применить конфигурацию:
```
public class SomeDbContext : DbContext
{
    ...

    #region OutboxMessaging
    public virtual DbSet<OutboxMessage> OutboxMessages { get; set; }
    public virtual DbSet<OutboxMessageState> OutboxMessageStates { get; set; }
    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ...
        modelBuilder.ApplyDbSetOutboxMessagingConfiguration<OutboxMessage, OutboxMessageState>(
                "OutboxMessage",
                "OutboxMessageState");
    }
}
```

## Использование
```
_context.OutboxMessages.Send(
    new DoSometingMqDto(),
    _config.AnotherAggregateQueueName,
    // use specific encoding if need
    o => o.Encoding = Encoding.Unicode); 

_context.OutboxMessages.Publish(new SomethingHappenedMqDto());
``` 

# Проект Guuyb.OutboxMessaging.Worker

Реализует `BackgroundService`, который периодически перекладывает сообщения из БД в шину сообщений.

## Подключение
```
services.AddDbSetOutboxMessagingWorker<OutboxMessage, SomeDbContext>()
```
