﻿using System;
using System.Text;
using System.Diagnostics;
using SolaceSystems.Solclient.Messaging;
using System.Threading;

/// <summary>
/// Solace Systems Messaging API tutorial: TopicSubscriber
/// </summary>

namespace Tutorial
{
    /// <summary>
    /// Demonstrates how to use Solace Systems Messaging API for subscribing to a topic and receiving a message
    /// </summary>
    class TopicSubscriber : IDisposable
    {
        string VPNName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }

        const int DefaultReconnectRetries = 3;

        private ISession Session = null;
        private EventWaitHandle WaitEventWaitHandle = new AutoResetEvent(false);

        void Run(IContext context, string host)
        {
            try
            {

                // Validate parameters
                if (context == null)
                {
                    throw new ArgumentException("Solace Systems API context Router must be not null.", "context");
                }
                if (string.IsNullOrWhiteSpace(host))
                {
                    throw new ArgumentException("Solace Messaging Router host name must be non-empty.", "host");
                }
                if (string.IsNullOrWhiteSpace(VPNName))
                {
                    throw new InvalidOperationException("VPN name must be non-empty.");
                }
                if (string.IsNullOrWhiteSpace(UserName))
                {
                    throw new InvalidOperationException("Client username must be non-empty.");
                }

                // Create session properties
                SessionProperties sessionProps = new SessionProperties()
                {
                    Host = host,
                    VPNName = VPNName,
                    UserName = UserName,
                    Password = Password,
                    ReconnectRetries = DefaultReconnectRetries
                };

                // Connect to the Solace messaging router
                Console.WriteLine("Connecting as {0}@{1} on {2}...", UserName, VPNName, host);
                Trace.WriteLine("Connecting as " + UserName + "@"+ VPNName + " on "+ host + "...");
            
                // NOTICE HandleMessage as the message event handler
                Session = context.CreateSession(sessionProps, HandleMessage, null);
                ReturnCode returnCode = Session.Connect();
                if (returnCode == ReturnCode.SOLCLIENT_OK)
                {
                    Console.WriteLine("Session successfully connected.");
                    Trace.WriteLine("Session successfully connected.");

                    // This is the topic on Solace messaging router where a message is published
                    // Must subscribe to it to receive messages
                    Session.Subscribe(ContextFactory.Instance.CreateTopic("tutorial/topic"), true);

                    Console.WriteLine("Waiting for a message to be published...");
                    Trace.WriteLine("Waiting for a message to be published...");
                    WaitEventWaitHandle.WaitOne();
                }
                else
                {
                    Console.WriteLine("Error connecting, return code: {0}", returnCode);
                    Trace.WriteLine("Error connecting, return code: "+ returnCode);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: {0}", ex.Message);
                Trace.WriteLine("Exception thrown: " + ex.Message);
            }
        }

        /// <summary>
        /// This event handler is invoked by Solace Systems Messaging API when a message arrives
        /// </summary>
        /// <param name="source"></param>
        /// <param name="args"></param>
        private void HandleMessage(object source, MessageEventArgs args)
        {
            try
            {
                Console.WriteLine("Received published message.");
                Trace.WriteLine("Received published message.");
                // Received a message
                using (IMessage message = args.Message)
                {
                    // Expecting the message content as a binary attachment
                    Console.WriteLine("Message content: {0}", Encoding.ASCII.GetString(message.BinaryAttachment));
                    Trace.WriteLine("Message content: " + Encoding.ASCII.GetString(message.BinaryAttachment));
                    // finish the program
                    //WaitEventWaitHandle.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: {0}", ex.Message);
                Trace.WriteLine("Exception thrown: "+ex.Message);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Session != null)
                    {
                        Session.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        #region Main
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TopicPublisher <host> <username>@<vpnname> <password>");
                Trace.WriteLine("Usage: TopicPublisher <host> <username>@<vpnname> <password>");
                Environment.Exit(1);
            }

            string[] split = args[1].Split('@');
            if (split.Length != 2)
            {
                Console.WriteLine("Usage: TopicPublisher <host> <username>@<vpnname> <password>");
                Trace.WriteLine("Usage: TopicPublisher <host> <username>@<vpnname> <password>");
                Environment.Exit(1);
            }

            string host = args[0]; // Solace messaging router host name or IP address
            string username = split[0];
            string vpnname = split[1];
            string password = args[2];

            // Initialize Solace Systems Messaging API with logging to console at Warning level
            ContextFactoryProperties cfp = new ContextFactoryProperties()
            {
                SolClientLogLevel = SolLogLevel.Warning
            };
            cfp.LogToConsoleError();
            ContextFactory.Instance.Init(cfp);

            try
            {
                // Context must be created first
                using (IContext context = ContextFactory.Instance.CreateContext(new ContextProperties(), null))
                {
                    // Create the application
                    using (TopicSubscriber topicSubscriber = new TopicSubscriber()
                    {
                        VPNName = vpnname,
                        UserName = username,
                        Password = password
                    })
                    {
                        // Run the application within the context and against the host
                        topicSubscriber.Run(context, host);
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: {0}", ex.Message);
                Trace.WriteLine("Exception thrown: "+ex.Message);
            }
            finally
            {
                // Dispose Solace Systems Messaging API
                ContextFactory.Instance.Cleanup();
            }
            Console.WriteLine("Finished.");
            Trace.WriteLine("Finished.");
        }
        #endregion
    }

}
