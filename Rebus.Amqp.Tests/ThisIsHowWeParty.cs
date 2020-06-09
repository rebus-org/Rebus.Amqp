using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Rebus.Activation;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Tests.Contracts;
using Rebus.Tests.Contracts.Extensions;

#pragma warning disable 1998

namespace Rebus.Amqp.Tests
{
    [TestFixture]
    public class ThisIsHowWeParty : FixtureBase
    {
        [Test]
        public async Task BasicSendReceiveTest()
        {
            var gotTheString = Using(new ManualResetEvent(initialState: false));
            var receiver = Using(new BuiltinHandlerActivator());

            receiver.Handle<string>(async msg =>
            {
                Console.WriteLine($"OK we got this: {msg}");
                gotTheString.Set();
            });

            Configure.With(receiver)
                .Transport(t => t.UseAmqp("receiver"))
                .Start();

            var sender = Configure.With(Using(new BuiltinHandlerActivator()))
                .Transport(t => t.UseAmqp("sender"))
                .Routing(t => t.TypeBased().Map<string>("receiver"))
                .Start();

            await sender.Send("HEJ MED DIG!!!! 😎");

            gotTheString.WaitOrDie(
                timeout: TimeSpan.FromSeconds(5),
                errorMessage: "Did not receive the expected string within 5 s timeout"
            );
        }
    }
}