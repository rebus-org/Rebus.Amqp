using System;
using System.Collections.Concurrent;
using Rebus.Tests.Contracts.Transports;
using Rebus.Transport;

namespace Rebus.Amqp.Tests
{
    public class AmqpLiteTransportFactory : ITransportFactory
    {
        readonly ConcurrentStack<IDisposable> _disposables = new ConcurrentStack<IDisposable>();

        public ITransport CreateOneWayClient()
        {
            var transport = new AmqpLiteTransport(null);

            _disposables.Push(transport);

            return transport;
        }

        public ITransport Create(string inputQueueAddress)
        {
            var transport = new AmqpLiteTransport(inputQueueAddress);

            _disposables.Push(transport);

            return transport;
        }

        public void CleanUp()
        {
            while (_disposables.TryPop(out var disposable))
            {
                disposable.Dispose();
            }
        }
    }
}