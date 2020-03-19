using CommandLine;

namespace PublisherAMQP
{
    class CommandLineOpts
    {
        // URI for message broker. Must be of the format amqp://<host>:<port> or amqps://<host>:<port>
        [Option("uri", Required = true, HelpText = "The URI for the AMQP Message Broker")]
        public string host { get; set; }
        // Connection Request Timeout
        [Option("ct", Default = 15000, HelpText = "the connection request timeout in milliseconds.")]
        public long connTimeout { get; set; }
        // UserName for authentication with the broker.
        [Option("cu", Default = null, HelpText = "The Username for authentication with the message broker")]
        public string username { get; set; }
        // Password for authentication with the broker
        [Option("cpwd", Default = null, HelpText = "The password for authentication with the message broker")]
        public string password { get; set; }
        [Option("cid", Default = null, HelpText = "The Client ID on the connection")]
        public string clientId { get; set; }
        // Logging Level
        [Option("log", Default = "warn", HelpText = "Sets the log level for the application and NMS Library. The levels are (from highest verbosity): debug,info,warn,error,fatal.")]
        public string logLevel { get; set; }
        //
        [Option("topic", Default = null, HelpText = "Topic to publish messages to. Can not be used with --queue.")]
        public string topic { get; set; }
        //
        [Option("queue", Default = null, HelpText = "Queue to publish messages to. Can not be used with --topic.")]
        public string queue { get; set; }
        //
        [Option('n', "messages", Default = 5, HelpText = "Number of messages to send.")]
        public int NUM_MSG { get; set; }
        //
        [Option("deliveryMode", Default = 0, HelpText = "Message Delivery Mode, Persistnent(0) and Non Persistent(1). The default is Persistent(0).")]
        public int mode { get; set; }
    }
}
