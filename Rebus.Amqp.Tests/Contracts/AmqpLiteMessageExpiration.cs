using NUnit.Framework;
using Rebus.Tests.Contracts.Transports;

namespace Rebus.Amqp.Tests.Contracts
{
    [TestFixture]
    public class AmqpLiteMessageExpiration : MessageExpiration<AmqpLiteTransportFactory>
    {
    }
}