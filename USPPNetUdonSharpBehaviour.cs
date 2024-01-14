using UdonSharp;
using UnityEngine;

namespace USPPNet {
    public abstract class USPPNetUdonSharpBehaviour : UdonSharpBehaviour {
        public int bytesSent;
        
        public virtual void USPPNet_RPC(string method, params object[] args) {
            
        }
    }
}
