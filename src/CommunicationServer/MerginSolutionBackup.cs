using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommunicationServer
{
    class MerginSolutionBackup
    {
        public int ComponentID;
        public Solutions Solution;

        public MerginSolutionBackup() { }

        public static void AddBackup(List<MerginSolutionBackup> list, int compID, Solutions sol)
        {
            var element = new MerginSolutionBackup() { ComponentID = compID, Solution = sol };
            list.Add(element);
        }

        public static void RemoveBackup(List<MerginSolutionBackup> list, int problemInstanceID)
        {
            MerginSolutionBackup dpb = null;
            lock(list)
            {
                foreach(var element in list)
                {
                    if((int)element.Solution.Id==problemInstanceID)
                    {
                        dpb = element;
                        break;

                    }
                }
                list.Remove(dpb);
            }            
        }

        public static List<Solutions> GetAllElementsAndDelete(ref List<MerginSolutionBackup> problemsList, int compID)
        {
            List<MerginSolutionBackup> copy = new List<MerginSolutionBackup>(problemsList);
            List<Solutions> list = new List<Solutions>();

            foreach (var element in problemsList)
            {
                if ((int)element.ComponentID == compID)
                {
                    list.Add(element.Solution);
                }
            }
            problemsList = copy;
            return list;           

        }
    }
}
