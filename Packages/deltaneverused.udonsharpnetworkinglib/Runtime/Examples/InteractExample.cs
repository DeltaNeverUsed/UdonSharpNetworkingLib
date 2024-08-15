using UdonSharp;
using UnityEngine;
using VRC.SDKBase;


namespace UdonSharpNetworkingLib.Examples {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InteractExample : NetworkingLibUdonSharpBehaviour {
        [NetworkingTarget(NetworkingTargetType.All)]
        private void Test(string msg, int test, VRCPlayerApi caller, int[] testArray) { // Demo method
            Debug.Log($"Triggered! {msg}, num: {test}, caller: ({caller.displayName}, {caller.playerId}), DebugArray: {testArray[0]}, {testArray[1]}, {testArray[2]}");
        }

        public override void Interact() {
            // Make the local player the owner of the GameObject before calling.
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
            
            Debug.Log($"{((string[])GetProgramVariable(UdonSharpNetworkingLibConsts.FunctionListKey))[0]}: {((byte[])GetProgramVariable(UdonSharpNetworkingLibConsts.NetworkingTypeKey))[0]}");
            
            //NetworkingLib_RPC(nameof(Test), null, "Hello there!", 69, Networking.LocalPlayer, new[] { 1, 2, 3 });
        }
    }
}