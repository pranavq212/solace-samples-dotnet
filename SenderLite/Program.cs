using Amqp;
using System;

namespace SenderLite
{
    class Program
    {
        static void Main(string[] args)
        {
            //Address address = new Address("amqp://admin:admin@localhost:5672");
            Address address = new Address("amqp://solace-cloud-client:ovkkdgqsfmefmogoa3k19h3cd@mr22gx8ufrq6kv.messaging.solace.cloud:20876");
            Connection connection = new Connection(address);
            Session session = new Session(connection);

            Message message = new Message("Hello AMQP!");
            SenderLink sender = new SenderLink(session, "sender-link", "TransactionQueue");
            sender.Send(message);
            Console.WriteLine("Sent Hello AMQP!");

            sender.Close();
            session.Close();
            connection.Close();
        }
    }
}
