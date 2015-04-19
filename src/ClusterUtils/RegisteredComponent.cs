using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Xml;
using ClusterMessages;
using Timer = System.Timers.Timer;

namespace ClusterUtils
{
    /// <summary>
    /// General class for all components that register to main server:
    ///     - Node
    ///     - Manager
    ///     - Server as backup
    /// Adds support for component registration and sending status message.
    /// </summary>
    public abstract class RegisteredComponent : Component
    {
        /// <summary>
        /// Id received from server after registration.
        /// </summary>
        protected uint Id;

        /// <summary>
        /// Timeout received after registration. Status message is sent at least as frequent as ServerTimeout.
        /// </summary>
        protected int ServerTimeout;

        /// <summary>
        /// Container for thread statuses.
        /// </summary>
        protected readonly List<StatusThread> StatusThreads = new List<StatusThread>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config">Server address and port.</param>
        /// <param name="type">Component type.</param>
        protected RegisteredComponent(ComponentConfig config, string type) : base(config, type) {}

        /// <summary>
        /// Register to server and process register response message.
        /// </summary>
        /// <returns>True on registration success, false otherwise.</returns>
        protected bool Register()
        {
            var registerMessage = new Register
            {
                Type = Type,
                SolvableProblems = new []{new RegisterSolvableProblemsProblemName
                {
                    Value = "DVRP"
                }}
            };

            var response = SendMessageSingleResponse(registerMessage);

            return ProcessRegisterResponse(response);
        }

        /// <summary>
        /// Process registration response - retreive Id assigned by server and timeout.
        /// </summary>
        /// <param name="response"></param>
        /// <returns>True on registration success, false otherwise.</returns>
        private bool ProcessRegisterResponse(XmlDocument response)
        {
            Id = uint.Parse(response.GetElementsByTagName("Id")[0].InnerText);
            ServerTimeout = int.Parse(response.GetElementsByTagName("Timeout")[0].InnerText);
            ServerTimeout *= 1000;
            Console.WriteLine("Registered at server with Id: {0}.", Id);

            return Id > 0;
        }

        /// <summary>
        /// Event method sending status info.
        /// </summary>
        /// <param name="sender">ignored</param>
        /// <param name="e">ignored</param>
        /// <param name="message">Message to be sent.</param>
        private void SendStatusMessage(object sender, ElapsedEventArgs e,
                                    IClusterMessage message)
        {
            if (sender == null) throw new ArgumentNullException("sender");
            if (e == null) throw new ArgumentNullException("e");

            var responses = SendMessage(message);

            ProcessMessages(responses);
        }

        /// <summary>
        /// Method initializing status message timer.
        /// </summary>
        /// <param name="message">Message to be sent.</param>
        /// <param name="msCycleTime">Sending loop time in miliseconds.</param>
        public void KeepSendingStatus(Status message, int msCycleTime)
        {
            var sendStatus = new Timer(msCycleTime);
            sendStatus.Elapsed += (sender, e) => SendStatusMessage(sender, e, message);
            sendStatus.Start();
        }

        /// <summary>
        /// Creates status message and timer for status message sending.
        /// </summary>
        protected void StartSendingStatus()
        {
            var msStatusCycleTime = ServerTimeout / 2;

            var statusMessage = new Status {Id = Id, Threads = StatusThreads.ToArray()};

            var keepSendingStatusThread = new Thread(() =>
                    KeepSendingStatus(statusMessage, msStatusCycleTime));

            Console.WriteLine("Starting thread sending the Status messages.");
            keepSendingStatusThread.Start();
        }

        /// <summary>
        /// Method processing NoOperation message. Retreives backup info.
        /// </summary>
        /// <param name="xmlMessage">Received NoOperation message.</param>
        protected void ProcessNoOperationMessage(XmlDocument xmlMessage)
        {
            Console.WriteLine("Received NoOperation message.");
        }

        /// <summary>
        /// Helper method for sending message where's no response is expected.
        /// </summary>
        /// <param name="message">Message to be sent.</param>
        protected void SendMessageNoResponse(IClusterMessage message)
        {
            SendMessage(message);
        }

        /// <summary>
        /// Method for handling all messages that given components may handle.
        /// </summary>
        /// <param name="responses">Messages received from server.</param>
        protected abstract void ProcessMessages(IEnumerable<XmlDocument> responses);
    }
}
