using Microsoft.EntityFrameworkCore;
using Guuyb.OutboxMessaging.Data.Models;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Guuyb.OutboxMessaging.Data.Core.Test
{
    public class OutboxMessageDbSetTests
    {
        private OutboxMessageContext _context;

        public OutboxMessageDbSetTests()
        {
            var options = new DbContextOptionsBuilder<OutboxMessageContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new OutboxMessageContext(options);
        }

        [Fact]
        public async Task Sending_command_should_be_successful()
        {
            var expectedTargetQueueName = "some-service-queue";

            _context.AnotherOutboxMessages.Send(new SomeCommand(), expectedTargetQueueName);
            await _context.SaveChangesAsync();

            var message = await _context.AnotherOutboxMessages.SingleOrDefaultAsync();
            Assert.NotNull(message);
            Assert.Equal(expectedTargetQueueName, message.TargetQueueName);
            Assert.Equal(typeof(SomeCommand).Name, message.PayloadTypeName);
            Assert.Equal(OutboxMessageStateEnum.New, message.StateId);
        }

        [Fact]
        public async Task Publishing_event_should_be_successful()
        {
            _context.AnotherOutboxMessages.Publish(new SomeEvent());
            await _context.SaveChangesAsync();

            var message = await _context.AnotherOutboxMessages.SingleOrDefaultAsync();
            Assert.NotNull(message);
            Assert.Null(message.TargetQueueName);
            Assert.Equal(typeof(SomeEvent).Name, message.PayloadTypeName);
            Assert.Equal(OutboxMessageStateEnum.New, message.StateId);
        }

        private class SomeCommand { }
        private class SomeEvent { }
    }
}
