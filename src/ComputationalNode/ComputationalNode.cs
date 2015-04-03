using System;
using System.Collections.Generic;
using System.Xml;
using ClusterMessages;
using ClusterUtils;

namespace ComputationalNode
{
    class ComputationalNode : RegisteredComponent
    {

        public ComputationalNode(ComponentConfig componentConfig) : base(componentConfig, "ComputationalNode") {}

        public void Start()
        {
            LogRuntimeInfo();
            Register();
            StartSendingStatus();
        }

        //process nooperation, partialproblems
        protected override void ProcessMessages(IEnumerable<XmlDocument> responses)
        {
            foreach (var xmlMessage in responses)
            {
                switch (MessageTypeResolver.GetMessageType(xmlMessage))
                {
                    case MessageTypeResolver.MessageType.NoOperation:
                        ProcessNoOperationMessage(xmlMessage);
                        break;
                    case MessageTypeResolver.MessageType.PartialProblems:
                        ProcessPartialProblemsMessage(xmlMessage);
                        break;
                }
            }
        }

        private void ProcessPartialProblemsMessage(XmlDocument xmlMessage)
        {
            var problemInstanceId = ulong.Parse(xmlMessage.GetElementsByTagName("Id")[0].InnerText);

            var taskId = ulong.Parse(xmlMessage.GetElementsByTagName("TaskId")[0].InnerText);

            Console.WriteLine("Received partial problem {0} from problem instance {1}.", taskId, problemInstanceId);
            
            CreateAndSendPartialSolution(taskId, problemInstanceId);
        }

        private void CreateAndSendPartialSolution(ulong taskId, ulong problemInstanceId)
        {
            var solution = new Solutions
            {
                Solutions1 = new[] {new SolutionsSolution {TaskId = taskId, Type = SolutionsSolutionType.Partial}},
                Id = problemInstanceId
            };

            SendMessageNoResponse(solution);
        }
    }
}
