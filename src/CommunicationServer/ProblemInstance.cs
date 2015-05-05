using System.Collections.Generic;

namespace CommunicationServer
{
    class ProblemInstance
    {
        public ulong Id;
        
        public List<SolutionsSolution> PartialSolutions = new List<SolutionsSolution>();
        public int SubproblemsCount = 0;

        public SolutionsSolution FinalSolution;
        public bool FinalSolutionFound = false;
        public byte[] CommonData;
    }
}
