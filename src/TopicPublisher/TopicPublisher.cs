﻿using System;
using System.Text;
using System.Threading;
using SolaceSystems.Solclient.Messaging;

/// <summary>
/// Solace Systems Messaging API tutorial: TopicPublisher
/// </summary>

namespace Tutorial
{
    /// <summary>
    /// Demonstrates how to use Solace Systems Messaging API for publishing a message
    /// </summary>
    class TopicPublisher
    {
        string VPNName { get; set; }
        string UserName { get; set; }
        string Password { get; set; }

        const int DefaultReconnectRetries = 3;
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
                using (ISession session = context.CreateSession(sessionProps, null, null))
                {
                    ReturnCode returnCode = session.Connect();
                    if (returnCode == ReturnCode.SOLCLIENT_OK)
                    {
                        Console.WriteLine("Session successfully connected.");
                        PublishMessage(session);
                        WaitEventWaitHandle.Set();
                    }
                    else
                    {
                        Console.WriteLine("Error connecting, return code: {0}", returnCode);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: {0}", ex.Message);
            }
        }

        private void PublishMessage(ISession session)
        {
            try
            {
                // Create the message
                using (IMessage message = ContextFactory.Instance.CreateMessage())
                {
                    message.Destination = ContextFactory.Instance.CreateTopic("tutorial/topic");
                    // Create the message content as a binary attachment
                    message.BinaryAttachment = Encoding.ASCII.GetBytes("Sample Message");

                    // Publish the message to the topic on the Solace messaging router
                    Console.WriteLine("Publishing message...");
                    ReturnCode returnCode = session.Send(message);
                    if (returnCode == ReturnCode.SOLCLIENT_OK)
                    {
                        Console.WriteLine("Done.");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("Publishing failed, return code: {0}", returnCode);
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: {0}", ex.Message);
                Console.ReadKey();
            }
        }

        #region Main
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: TopicPublisher <host> <username>@<vpnname> <password>");
                Environment.Exit(1);
            }

            string[] split = args[1].Split('@');
            if (split.Length != 2)
            {
                Console.WriteLine("Usage: TopicPublisher <host> <username>@<vpnname> <password>");
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
                    TopicPublisher topicPublisher = new TopicPublisher()
                    {
                        VPNName = vpnname,
                        UserName = username,
                        Password = password
                    };

                    // Run the application within the context and against the host
                    topicPublisher.Run(context, host);
                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception thrown: {0}", ex.Message);
                Console.ReadKey();
            }
            finally
            {
                // Dispose Solace Systems Messaging API
                ContextFactory.Instance.Cleanup();
            }
            Console.WriteLine("Finished.");
            Console.ReadKey();
        }

        #endregion
    }

}
