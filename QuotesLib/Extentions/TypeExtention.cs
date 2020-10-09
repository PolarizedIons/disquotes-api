using System;
using System.Collections.Generic;
using System.Linq;

namespace QuotesLib.Extentions
{
    public static class TypeExtention
    {
        public static IEnumerable<Type> FindAllInAssembly(this Type type)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => type.IsAssignableFrom(x) &&
                            !x.IsInterface &&
                            !x.IsAbstract &&
                            x.GetConstructors().Any(c => c.IsPublic)
                            );
        }
    }
}
