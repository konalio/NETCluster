using System;
using System.Xml;

namespace ClusterMessages
{
    /// <summary>
    /// Helper class for retreiving cluster message type from XmlDocument.
    /// </summary>
    public static class MessageTypeResolver
    {
        public enum MessageType
        {
            Unknown,
            DivideProblem,
            NoOperation,
            PartialProblems,
            Register,
            RegisterResponse,
            Solution,
            SolutionRequest,
            SolveRequest,
            SolveRequestResponse,
            Status,
            Error
        }

        /// <summary>
        /// Returns appropriate type based on Xml message.
        /// </summary>
        /// <param name="message">Xml message.</param>
        /// <returns>Message type.</returns>
        public static MessageType GetMessageType(XmlDocument message)
        {
            if (message == null)
                throw new ArgumentNullException();

            if (message.DocumentElement == null)
                return MessageType.Unknown;

            var messageName = message.DocumentElement.Name;

            switch (messageName)
            {
                case "DivideProblem":
                    return MessageType.DivideProblem;
                case "NoOperation":
                    return MessageType.NoOperation;
                case "SolvePartialProblems":
                    return MessageType.PartialProblems;
                case "Register":
                    return MessageType.Register;
                case "RegisterResponse":
                    return MessageType.RegisterResponse;
                case "Solutions":
                    return MessageType.Solution;
                case "SolutionRequest":
                    return MessageType.SolutionRequest;
                case "SolveRequest":
                    return MessageType.SolveRequest;
                case "SolveRequestResponse":
                    return MessageType.SolveRequestResponse;
                case "Status":
                    return MessageType.Status;
                default:
                    return MessageType.Unknown;
            }
        }
    }
}
