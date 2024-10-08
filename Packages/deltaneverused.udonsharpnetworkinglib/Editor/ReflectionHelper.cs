using System;
using System.Linq;
using System.Reflection;

namespace UdonSharpNetworkingLib {
    public static class ReflectionHelper {
        public static MethodInfo GetMethod(Type type, string methodName, Type[] parameterTypes, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance) {
            return type.GetMethod(methodName, flags, null, parameterTypes,
                null);
        }

        public static PropertyInfo GetProperty(Type type, string propertyName, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance) {
            return type.GetProperty(propertyName, flags);
        }

        public static ConstructorInfo GetConstructor(Type type, Type[] parameterTypes, BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance) {
            return type.GetConstructor(flags, null, parameterTypes, null);
        }

        public static Type ByName(string name) {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Reverse()) {
                var tt = assembly.GetType(name);
                if (tt != null) {
                    return tt;
                }
            }

            return null;
        }
    }
}