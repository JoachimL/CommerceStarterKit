using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OxxCommerceStarterKit.Core.Services
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> items)
        {
            return items ?? Enumerable.Empty<T>();
        }
    }
}
