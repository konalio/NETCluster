using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationServer
{
    class ComponentStatus
    {
        public ulong id;
        public String type;
        public String[] solvableProblems;

        public ComponentStatus(ulong idVal, String typeVal, String[] problemsVal)
        {
            id = idVal;
            type = typeVal;
            solvableProblems = problemsVal;
        }
    }
}
