using Amqp;
using System;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            //Address address = new Address("amqp://admin:admin@localhost:5672");
            Address address = new Address("amqp://solace-cloud-client:ovkkdgqsfmefmogoa3k19h3cd@mr22gx8ufrq6kv.messaging.solace.cloud:20876");
            Connection connection = new Connection(address);
            Session session = new Session(connection);
            ReceiverLink receiver = new ReceiverLink(session, "receiver-link", "TransactionQueue");

            Console.WriteLine("Receiver connected to broker.");
            Message message = receiver.Receive();
            Console.WriteLine("Received " + message.Body);
            receiver.Accept(message);

            receiver.Close();
            session.Close();
            connection.Close();
        }
    }
}
