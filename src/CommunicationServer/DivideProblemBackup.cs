using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationServer
{
    class DivideProblemBackup
    {
        public int ComponentID;
        public DivideProblem Problem;

        public DivideProblemBackup() { }

        public static void AddBackup(List<DivideProblemBackup> list, int compID, DivideProblem prob)
        {
            var element = new DivideProblemBackup() { ComponentID = compID, Problem = prob };
            list.Add(element);
        }

        public static void RemoveBackup(List<DivideProblemBackup> list, int problemInstanceID)
        {
            DivideProblemBackup dpb = null;
            lock(list)
            {
                foreach(var element in list)
                {
                    if((int)element.Problem.Id==problemInstanceID)
                    {
                        dpb = element;
                        break;

                    }
                }
                list.Remove(dpb);
            }            
        }

        public static List<DivideProblem> GetAllElementsAndDelete(ref List<DivideProblemBackup> problemsList, int compID)
        {            
            List<DivideProblemBackup> copy = new List<DivideProblemBackup>(problemsList);
            List<DivideProblem> list = new List<DivideProblem>();

            foreach (var element in problemsList)
            {
                if ((int)element.ComponentID == compID)
                {
                    list.Add(element.Problem);
                }
            }
            problemsList = copy;
            return list;           

        }

    }
}
