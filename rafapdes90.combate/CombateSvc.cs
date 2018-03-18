using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.Text;
using System.Xml.Linq;

namespace rafapdes90.combate
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "CombateSvc" in both code and config file together.
    public class CombateSvc : ICombateSvc
    {
        public void DoWork()
        {
            Console.WriteLine(@"Teste Combate");
        }
    }
}
