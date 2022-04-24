using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaycastEngine
{
    public class EntryPoint
    {
        static void Main()
        {
            //Experimental.StartExperimental();
            RaycastRenderer raycast = new RaycastRenderer();
            raycast.Start();
        }
    }
}
