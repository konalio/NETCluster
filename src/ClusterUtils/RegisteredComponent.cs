using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;
using System.Xml;
using ClusterMessages;
using Timer = System.Timers.Timer;

namespace ClusterUtils
{
    public abstract class RegisteredComponent : Component
    {
        protected uint Id;
        protected int ServerTimeout;
        protected readonly List<StatusThread> StatusThreads = new List<StatusThread>();

        protected RegisteredComponent(ComponentConfig config, string type) : base(config, type) {}

        protected bool Register()
        {
            var registerMessage = new Register
            {
                Type = Type
            };

            var response = SendMessageSingleResponse(registerMessage);

            return ProcessRegisterResponse(response);
        }

        private bool ProcessRegisterResponse(XmlDocument response)
        {
            Id = uint.Parse(response.GetElementsByTagName("Id")[0].InnerText);
            ServerTimeout = int.Parse(response.GetElementsByTagName("Timeout")[0].InnerText);
            ServerTimeout *= 1000;
            Console.WriteLine("Registered at server with Id: {0}.", Id);

            return Id > 0;
        }

        private void SendStatusMessage(object sender, ElapsedEventArgs e,
                                    IClusterMessage message)
        {
            if (sender == null) throw new ArgumentNullException("sender");
            if (e == null) throw new ArgumentNullException("e");

            var responses = SendMessage(message);

            ProcessMessages(responses);
        }

        public void KeepSendingStatus(Status message, int msCycleTime)
        {
            var sendStatus = new Timer(msCycleTime);
            sendStatus.Elapsed += (sender, e) => SendStatusMessage(sender, e, message);
            sendStatus.Start();
        }

        protected void StartSendingStatus()
        {
            var msStatusCycleTime = ServerTimeout / 2;

            var statusMessage = new Status {Id = Id, Threads = StatusThreads.ToArray()};

            var keepSendingStatusThread = new Thread(() =>
                    KeepSendingStatus(statusMessage, msStatusCycleTime));

            Console.WriteLine("Starting thread sending the Status messages.");
            keepSendingStatusThread.Start();
        }

        protected void ProcessNoOperationMessage(XmlDocument xmlMessage)
        {
            Console.WriteLine("Received NoOperation message.");
        }

        protected void SendMessageNoResponse(IClusterMessage message)
        {
            SendMessage(message);
        }

        protected abstract void ProcessMessages(IEnumerable<XmlDocument> responses);
    }
}
