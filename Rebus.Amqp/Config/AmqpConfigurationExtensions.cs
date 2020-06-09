using System;
using Rebus.Amqp;
using Rebus.Transport;

namespace Rebus.Config
{
    /// <summary>
    /// Configuration extensions for the AMQP transport
    /// </summary>
    public static class AmqpConfigurationExtensions
    {
        /// <summary>
        /// Configures Rebus to use an AMQP-compliant message broker as the transport
        /// </summary>
        public static void UseAmqp(this StandardConfigurer<ITransport> configurer, string inputQueueName)
        {
            if (configurer == null) throw new ArgumentNullException(nameof(configurer));
            if (inputQueueName == null) throw new ArgumentNullException(nameof(inputQueueName));

            configurer.Register(c => new AmqpLiteTransport(inputQueueName));
        }
    }
}