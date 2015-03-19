using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterMessages
{
    class RegisterResponseMessage : ClusterMessage
    {
        public RegisterResponse Message { public get; private set; }

        public RegisterResponseMessage(int id, int timeout)
        {
            Message = new RegisterResponse
            {
                Id = id.ToString(),
                Timeout = timeout.ToString()
            };
        }
    }
}
