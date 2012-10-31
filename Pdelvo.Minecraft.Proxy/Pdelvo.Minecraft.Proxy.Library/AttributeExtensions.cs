using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pdelvo.Minecraft.Proxy.Library
{
    /// <summary>
    ///   Provide functionality to work with custom attributes. This is needed because the extension methods in the framework are not implemented in mono.
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        ///   Get a collection of attributes of a given type in a given assembly
        /// </summary>
        /// <typeparam name="T"> The type of the attribute </typeparam>
        /// <param name="assembly"> The assembly </param>
        /// <param name="inherit"> True if this method should also return derived types, otherwise false </param>
        /// <returns> A collection of attributes </returns>
        public static IEnumerable<T> GetAttributes<T>(this Assembly assembly, bool inherit = true) where T : Attribute
        {
            object[] attributes = assembly.GetCustomAttributes(typeof (T), inherit);

            return attributes.Select(m => (T) m);
        }
    }
}