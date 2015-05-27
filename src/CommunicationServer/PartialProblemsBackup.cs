using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationServer
{
    /// <summary>
    /// Backup class for PartialProblems 
    /// </summary>
    class PartialProblemsBackup
    {       

        public int ComponentID;
        public SolvePartialProblems Problem;

        public PartialProblemsBackup() { }       

        public static void AddPartialProblemBackup(List<PartialProblemsBackup> list, int compID, SolvePartialProblems spp)
        {
            PartialProblemsBackup element = new PartialProblemsBackup() { ComponentID = compID, Problem = spp };
            list.Add(element);
        }

        public static void RemovePartialProblemBackup(List<PartialProblemsBackup> list, int TID, int PID)
        {
            PartialProblemsBackup element = null;

            lock (list)
            {
                foreach (PartialProblemsBackup ppb in list)
                {
                    if ((int)ppb.Problem.PartialProblems[0].TaskId == TID && (int)ppb.Problem.Id == PID)
                    {
                        element = ppb;
                        break;
                    }
                }
                list.Remove(element);
            }            
        }

        public static List<SolvePartialProblems> GetComponentsPartialProblemsAndDelete(ref List<PartialProblemsBackup> problemsList, int componentID)
        {
            List<SolvePartialProblems> list = new List<SolvePartialProblems>();
            List<PartialProblemsBackup> copy = new List<PartialProblemsBackup>(problemsList);
            
            foreach (PartialProblemsBackup ppb in problemsList)
            {
                if ((int)ppb.ComponentID == componentID)
                {
                    list.Add(ppb.Problem);
                    copy.Remove(ppb);
                }
            }

            problemsList = copy;            
            return list;
        }


    }
}
