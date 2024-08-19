using System;
using JetBrains.Annotations;

namespace UdonSharpNetworkingLib {
    public enum NetworkingTargetType : byte {
        All,
        AllSelfExclusive,
        
        Master,
        
        Specific,
        
        Local,
    }
    
    [PublicAPI]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class NetworkingTargetAttribute : Attribute {
        public NetworkingTargetType NetworkTargetType { get; }

        public NetworkingTargetAttribute(NetworkingTargetType networkSyncTypeIn)
        {
            NetworkTargetType = networkSyncTypeIn;
        }
    }
}