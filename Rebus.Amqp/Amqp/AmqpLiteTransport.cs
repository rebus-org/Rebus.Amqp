using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rebus.Bus;
using Rebus.Messages;
using Rebus.Transport;
using AmqpLite = Amqp;
#pragma warning disable 1998

namespace Rebus.Amqp
{
    /// <summary>
    /// Implementation of <see cref="ITransport"/> that can run on any AMQP-compliant message broker
    /// </summary>
    public class AmqpLiteTransport : AbstractRebusTransport, IDisposable
    {

        AmqpLite.Address amqpAddress;
        AmqpLite.Connection amqpConnection;
        AmqpLite.Session amqpSession;
        AmqpLite.ReceiverLink amqpReceiver;

        /// <summary>
        /// Creates the transport
        /// </summary>
        public AmqpLiteTransport(string inputQueueName) : base(inputQueueName)
        {
            amqpAddress = new AmqpLite.Address("amqp://10.6.46.150:61616");
            amqpConnection = new AmqpLite.Connection(amqpAddress);
            amqpSession = new AmqpLite.Session(amqpConnection);

            if (inputQueueName != null)
            {
                amqpReceiver = new AmqpLite.ReceiverLink(amqpSession, "rebus-receiver", inputQueueName);
            }
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
            if (amqpReceiver == null) return null;

            AmqpLite.Message msg = await amqpReceiver.ReceiveAsync(TimeSpan.FromSeconds(0.5));

            if (msg == null) return null;

            if (msg.Properties?.AbsoluteExpiryTime != DateTime.MinValue && msg.Properties?.AbsoluteExpiryTime > DateTime.Now) return null;

            context.OnCompleted(async ctx =>
            {
                amqpReceiver.Accept(msg);
            });

            context.OnAborted(async ctx =>
            {
                amqpReceiver.Reject(msg);
            });

            context.OnDisposed(async ctx =>
            {
                amqpReceiver.Reject(msg);
            });


            var result = new TransportMessage(GetHeaders(msg), GetBytes(msg.Body.ToString()));

            return result;
        }

        /// <summary>
        /// Sends all of the outgoing messages buffered as part of the transaction context
        /// </summary>
        protected override async Task SendOutgoingMessages(IEnumerable<OutgoingMessage> outgoingMessages, ITransactionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            foreach (var message in outgoingMessages)
            {
                if (message.DestinationAddress == null) throw new ArgumentNullException(nameof(message.DestinationAddress));
                if (message == null) throw new ArgumentNullException(nameof(message));
                if(amqpSession == null) throw new ArgumentNullException(nameof(amqpSession));

                var amqpSender = new AmqpLite.SenderLink(amqpSession, "rebus-sender", message.DestinationAddress);

                var body = Encoding.UTF8.GetString(message.TransportMessage.Body);

                var msg = new AmqpLite.Message(body);
                msg.Properties = new AmqpLite.Framing.Properties { MessageId = message.TransportMessage.GetMessageId() };

                if (message.TransportMessage.Headers.TryGetValue(Headers.SentTime, out var sentTime))
                {
                    msg.Properties.CreationTime = DateTime.Parse(sentTime);


                    if (message.TransportMessage.Headers.TryGetValue(Headers.TimeToBeReceived, out var timeToBeReceived))
                    {
                        msg.Properties.AbsoluteExpiryTime = msg.Properties.CreationTime + TimeSpan.Parse(timeToBeReceived);
                    }
                }

                msg.ApplicationProperties = new AmqpLite.Framing.ApplicationProperties();

                foreach (var item in message.TransportMessage.Headers)
                {
                    msg.ApplicationProperties.Map.Add(item.Key, item.Value);
                } 


                await amqpSender.SendAsync(msg);
                await amqpSender.CloseAsync();
            } 
        }

        /// <summary>
        /// Disposes whatever resources that must be disposed
        /// </summary>
        public void Dispose()
        {
            if (amqpReceiver != null)
            {
                amqpReceiver.Close();
            }
            amqpSession.Close();
            amqpConnection.Close();
        }

        Dictionary<string, string> GetHeaders(AmqpLite.Message msg)
        {
            if (msg.ApplicationProperties == null)
                return new Dictionary<string, string>();

            var result = new Dictionary<string, string>();

            foreach (var item in msg.ApplicationProperties.Map)
            {
                result.Add(item.Key.ToString(), item.Value.ToString());
            }
            return result;
        }

        byte[] GetBytes(string body)
        {
            return Encoding.UTF8.GetBytes(body);
        }
    }
}