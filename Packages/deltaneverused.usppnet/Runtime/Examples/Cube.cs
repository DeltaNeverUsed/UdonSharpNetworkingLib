using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon.Common;


namespace USPPNet.Examples {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class Cube : USPPNetUdonSharpBehaviour {
        private void USPPNET_Test(string msg, int test, VRCPlayerApi caller, int[] testArray) // Demo method
        {
            Debug.Log($"Triggered! {msg}, num: {test}, caller: ({caller.displayName}, {caller.playerId}), DebugArray: {testArray[0]}, {testArray[1]}, {testArray[2]}");
        }

        public override void Interact() {
            if (!Networking.IsOwner(gameObject))
                Networking.SetOwner(Networking.LocalPlayer, gameObject);

            USPPNET_Test("Hello there!",69, Networking.LocalPlayer, new []{ 1, 2 ,3 }); // only the owner of the object can send RPC calls, this method gets called on everyone but the caller
            RequestSerialization(); // if you're using manual (i recommend you do) you need to call RequestSerialization to send the RPC
        }

        // Comments that start with USPPNet are important for it to work, don't remove these, or the PreProcessor won't be able to generate the code
        public override void OnDeserialization() {
            // USPPNet OnDeserialization
            
            // your code here
        }

        public override void OnPostSerialization(SerializationResult result) {
            // USPPNet OnPostSerialization
            
            // your code here
            Debug.Log(bytesSent);
        }
        
        public override void OnPreSerialization() {
            // USPPNet OnPreSerialization
            
            // your code here
        }

        // USPPNet Init
    }
}