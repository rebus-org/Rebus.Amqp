using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Rebus.Messages;
using Rebus.Transport;
#pragma warning disable 1998

namespace Rebus.Amqp
{
    /// <summary>
    /// Implementation of <see cref="ITransport"/> that can run on any AMQP-compliant message broker
    /// </summary>
    public class AmqpLiteTransport : AbstractRebusTransport, IDisposable
    {
        /// <summary>
        /// Creates the transport
        /// </summary>
        public AmqpLiteTransport(string inputQueueName) : base(inputQueueName)
        {
        }

        /// <summary>
        /// Doesn't do anything, as management is not part of AMQP
        /// </summary>
        public override void CreateQueue(string address)
        {
        }

        /// <summary>
        /// Receives the next transport message or null if none is available. Can block if it wants to, just respect the <paramref name="cancellationToken"/>
        /// </summary>
        public override async Task<TransportMessage> Receive(ITransactionContext context, CancellationToken cancellationToken)
        {
            return null;
        }

        /// <summary>
        /// Sends all of the outgoing messages buffered as part of the transaction context
        /// </summary>
        protected override async Task SendOutgoingMessages(IEnumerable<OutgoingMessage> outgoingMessages, ITransactionContext context)
        {
        }

        /// <summary>
        /// Disposes whatever resources that must be disposed
        /// </summary>
        public void Dispose()
        {
        }
    }
}