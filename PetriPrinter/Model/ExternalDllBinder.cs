using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace PetriPrinter.Model
{
    public class ExternalDllBinder : SerializationBinder
    {
        List<Assembly> asmblies;

        public ExternalDllBinder(List<Assembly> asmblies)
        {
            this.asmblies = asmblies;
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            Type ttd = null;
            try
            {
                string toassname = assemblyName.Split(',')[0];
                // = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly ass in asmblies)
                {
                    if (ass.FullName.Split(',')[0] == toassname)
                    {
                        ttd = ass.GetType(typeName);
                        break;
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (ttd == null)
            {
                ttd = Type.GetType(String.Format("{0}, {1}", typeName, assemblyName));
            }

            return ttd;
        }
    }
}
