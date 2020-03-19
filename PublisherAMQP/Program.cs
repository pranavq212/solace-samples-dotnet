using Apache.NMS;
using Apache.NMS.AMQP;
using CommandLine;
using System;

namespace PublisherAMQP
{
    class Program
    {
        private static void RunWithOptions(CommandLineOpts opts)
        {
            ITrace logger = new Logger(Logger.ToLogLevel(opts.logLevel));
            Tracer.Trace = logger;

            string ip = opts.host;
            Uri providerUri = new Uri(ip);
            Console.WriteLine("scheme: {0}", providerUri.Scheme);

            IConnection conn = null;
            if (opts.topic == null && opts.queue == null)
            {
                Console.WriteLine("ERROR: Must specify a topic or queue destination");
                return;
            }
            try
            {
                NmsConnectionFactory factory = new NmsConnectionFactory(ip);
                if (opts.username != null)
                {
                    factory.UserName = opts.username;
                }
                if (opts.password != null)
                {
                    factory.Password = opts.password;
                }
                if (opts.clientId != null)
                {
                    factory.ClientId = opts.clientId;
                }

                if (opts.connTimeout != default)
                {
                    factory.SendTimeout = opts.connTimeout;
                }

                Console.WriteLine("Creating Connection...");
                conn = factory.CreateConnection();
                conn.ExceptionListener += (logger as Logger).LogException;
                Console.WriteLine("Created Connection.");
                Console.WriteLine("Version: {0}", conn.MetaData);
                Console.WriteLine("Creating Session...");
                ISession ses = conn.CreateSession();
                Console.WriteLine("Session Created.");

                conn.Start();
                IDestination dest = (opts.topic == null) ? (IDestination)ses.GetQueue(opts.queue) : (IDestination)ses.GetTopic(opts.topic);
                Console.WriteLine("Creating Message Producer for : {0}...", dest);
                IMessageProducer prod = ses.CreateProducer(dest);
                IMessageConsumer consumer = ses.CreateConsumer(dest);
                Console.WriteLine("Created Message Producer.");
                prod.DeliveryMode = opts.mode == 0 ? MsgDeliveryMode.NonPersistent : MsgDeliveryMode.Persistent;
                prod.TimeToLive = TimeSpan.FromSeconds(20);
                ITextMessage msg = prod.CreateTextMessage("Hello World!");

                Console.WriteLine("Sending Msg: {0}", msg.ToString());
                Console.WriteLine("Starting Connection...");
                conn.Start();
                Console.WriteLine("Connection Started: {0} Resquest Timeout: {1}", conn.IsStarted, conn.RequestTimeout);
                Console.WriteLine("Sending {0} Messages...", opts.NUM_MSG);
                for (int i = 0; i < opts.NUM_MSG; i++)
                {
                    Tracer.InfoFormat("Sending Msg {0}", i + 1);
                    // Text Msg Body 
                    msg.Text = "Hello World! n: " + i;
                    prod.Send(msg);
                    msg.ClearBody();
                }

                IMessage rmsg = null;
                for (int i = 0; i < opts.NUM_MSG; i++)
                {
                    Tracer.InfoFormat("Waiting to receive message {0} from consumer.", i);
                    rmsg = consumer.Receive(TimeSpan.FromMilliseconds(opts.connTimeout));
                    if (rmsg == null)
                    {
                        Console.WriteLine("Failed to receive Message in {0}ms.", opts.connTimeout);
                    }
                    else
                    {
                        Console.WriteLine("Received Message with id {0} and contents {1}.", rmsg.NMSMessageId, rmsg.ToString());
                    }
                }
                if (conn.IsStarted)
                {
                    Console.WriteLine("Closing Connection...");
                    conn.Close();
                    Console.WriteLine("Connection Closed.");
                }
            }
            catch (NMSException ne)
            {
                Console.WriteLine("Caught NMSException : {0} \nStack: {1}", ne.Message, ne);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught unexpected exception : {0}", e);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }
        private static void RunWithOptionsStructured(CommandLineOpts opts)
        {
            ITrace logger = new Logger(Logger.ToLogLevel(opts.logLevel));
            Tracer.Trace = logger;

            string ip = opts.host;
            Uri providerUri = new Uri(ip);
            Console.WriteLine("scheme: {0}", providerUri.Scheme);

            IConnection conn = null;
            if (opts.topic == null && opts.queue == null)
            {
                Console.WriteLine("ERROR: Must specify a topic or queue destination");
                return;
            }
            try
            {
                NmsConnectionFactory factory = new NmsConnectionFactory(ip);
                if (opts.username != null)
                {
                    factory.UserName = opts.username;
                }
                if (opts.password != null)
                {
                    factory.Password = opts.password;
                }
                if (opts.clientId != null)
                {
                    factory.ClientId = opts.clientId;
                }

                if (opts.connTimeout != default)
                {
                    factory.SendTimeout = opts.connTimeout;
                }

                Console.WriteLine("Creating Connection...");
                conn = factory.CreateConnection();
                conn.ExceptionListener += (logger as Logger).LogException;
                Console.WriteLine("Created Connection.");
                Console.WriteLine("Version: {0}", conn.MetaData);
                Console.WriteLine("Creating Session...");
                ISession ses = conn.CreateSession();
                Console.WriteLine("Session Created.");

                conn.Start();
                IDestination dest = (opts.topic == null) ? (IDestination)ses.GetQueue(opts.queue) : (IDestination)ses.GetTopic(opts.topic);
                Console.WriteLine("Creating Message Producer for : {0}...", dest);
                IMessageProducer prod = ses.CreateProducer(dest);
                IMessageConsumer consumer = ses.CreateConsumer(dest);
                Console.WriteLine("Created Message Producer.");
                prod.DeliveryMode = opts.mode == 0 ? MsgDeliveryMode.NonPersistent : MsgDeliveryMode.Persistent;
                prod.TimeToLive = TimeSpan.FromSeconds(20);
                IMapMessage mapMsg = prod.CreateMapMessage();
                IStreamMessage streamMsg = prod.CreateStreamMessage();

                Console.WriteLine("Starting Connection...");
                conn.Start();
                Console.WriteLine("Connection Started: {0} Resquest Timeout: {1}", conn.IsStarted, conn.RequestTimeout);
                Tracer.InfoFormat("Sending MapMsg");
                // Map Msg Body 
                mapMsg.Body.SetString("mykey", "Hello World!");
                mapMsg.Body.SetBytes("myBytesKey", new byte[] { 0x6d, 0x61, 0x70 });
                Console.WriteLine("Sending Msg: {0}", mapMsg.ToString());
                prod.Send(mapMsg);
                mapMsg.ClearBody();

                // Stream  Msg Body
                streamMsg.WriteBytes(new byte[] { 0x53, 0x74, 0x72 });
                streamMsg.WriteInt64(1354684651565648484L);
                streamMsg.WriteObject("bar");
                streamMsg.Properties["foobar"] = 42 + "";
                Console.WriteLine("Sending Msg: {0}", streamMsg.ToString());
                prod.Send(streamMsg);
                streamMsg.ClearBody();

                IMessage rmsg = null;
                for (int i = 0; i < 2; i++)
                {
                    Tracer.InfoFormat("Waiting to receive message {0} from consumer.", i);
                    rmsg = consumer.Receive(TimeSpan.FromMilliseconds(opts.connTimeout));
                    if (rmsg == null)
                    {
                        Console.WriteLine("Failed to receive Message in {0}ms.", opts.connTimeout);
                    }
                    else
                    {
                        Console.WriteLine("Received Message with id {0} and contents {1}.", rmsg.NMSMessageId, rmsg.ToString());
                        foreach (string key in rmsg.Properties.Keys)
                        {
                            Console.WriteLine("Message contains Property[{0}] = {1}", key, rmsg.Properties[key].ToString());
                        }
                    }
                }
                if (conn.IsStarted)
                {
                    Console.WriteLine("Closing Connection...");
                    conn.Close();
                    Console.WriteLine("Connection Closed.");
                }
            }
            catch (NMSException ne)
            {
                Console.WriteLine("Caught NMSException : {0} \nStack: {1}", ne.Message, ne);
            }
            catch (Exception e)
            {
                Console.WriteLine("Caught unexpected exception : {0}", e);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Dispose();
                }
            }
        }
        static void Main(string[] args)
        {
            //CommandLineOpts opts = new CommandLineOpts();
            //opts.host = "amqp://admin:admin@127.0.0.1:5672/";//"amqp://127.0.0.1:5672"; 
            //opts.username = "admin";
            //opts.password = "admin";
            //opts.topic = "solace/topic";

            //ParserResult<CommandLineOpts> result = CommandLine.Parser.Default.ParseArguments<CommandLineOpts>(args)
            //    .WithParsed<CommandLineOpts>(opts => RunWithOptions(opts));

            //result = CommandLine.Parser.Default.ParseArguments<CommandLineOpts>(args)
            //    .WithParsed<CommandLineOpts>(options => RunWithOptionsStructured(options));

            #region Transaction

            //var connectionFactory = new NmsConnectionFactory("admin", "admin", "amqp://127.0.0.1:5672");

            var connectionFactory = new NmsConnectionFactory("solace-cloud-client", "ovkkdgqsfmefmogoa3k19h3cd", "amqp://mr22gx8ufrq6kv.messaging.solace.cloud:20876");

            var connection = connectionFactory.CreateConnection();
            connection.ClientId = "TransactionsExampleSender";

            var session = connection.CreateSession();
            var queue = session.GetQueue("TransactionQueue");
            var producer = session.CreateProducer(queue);

            for (int i = 1; i <= 5; i++)
            {
                ITextMessage message = producer.CreateTextMessage($"Message  {i}");
                producer.Send(message);
                Console.WriteLine("Sent message " + i);
            }

            //session.Rollback();
            //Console.WriteLine("Rollback");

            for (int i = 6; i <= 10; i++)
            {
                ITextMessage message = producer.CreateTextMessage($"Message  {i}");
                producer.Send(message);
                Console.WriteLine("Sent message " + i);
            }

            //session.Commit();

            var consumer = session.CreateConsumer(queue);

            connection.Start();

            for (int i = 0; i < 10; i++)
            {
                var message = consumer.Receive() as ITextMessage;
                Console.WriteLine("Message " + message.Text + " received");
            }

            Console.ReadKey();

            producer.Close();
            session.Close();
            connection.Close();

            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            #endregion  
        }
    }
}
