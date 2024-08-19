# UdonSharp Networking Library
A compiler patch to have networked events with parameters.

# How to use
1. Download and install the project from my [VCC Listing](https://deltaneverused.github.io/VRChatPackages/)
2. Inherit from the ``NetworkingLibUdonSharpBehaviour`` class
3. Mark the functions you want to be networked with ``[NetworkingTarget()]``
4. Make sure that the person about to make a call is the network owner ``Networking.SetOwner(Networking.LocalPlayer, gameObject);``
5. Call the function with ``NetworkingLib_RPC(nameof(function), null, params);``

## Usage
### Boilerplate 
You need to inherit from the NetworkingLibUdonSharpBehaviour class
```csharp
using UdonSharp;


// I'd recommend using manual sync for your behaviours. 
[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class InteractExample : NetworkingLibUdonSharpBehaviour {}
```
### Creating functions
To create a networked function, add the ``NetworkingTarget`` attribute to the method.
``NetworkingTarget`` has 5 different modes.

1. All
2. AllSelfExclusive
3. Master
4. Specific
5. Local

Each mode describes who the function is called on.
```csharp
[NetworkingTarget(NetworkingTargetType.AllSelfExclusive)]
private void Ping() {
    // Do something
}
```
### Calling functions
Only the network owner of the object will be able to call networked functions
```csharp
NetworkingLib_RPC(nameof(Ping));
```

If your ``NetworkingTarget`` was set to ``Specific``, you'd need to specify the target player in the second parameter.
```csharp
VRCPlayerApi someplayer = ???;

NetworkingLib_RPC(nameof(Ping), someplayer);
```
### Calling functions with parameters!
```csharp
// Create your function like normal
[NetworkingTarget(NetworkingTargetType.AllSelfExclusive)]
private void Ping(string message, float time) {
    // Do something
}

private void Start() {
    NetworkingLib_RPC(nameof(Ping), null, "Hello there!", 420);
}
```

## Temp Example
```csharp
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace UdonSharpNetworkingLib.Samples {
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class InteractExample : NetworkingLibUdonSharpBehaviour {
        // Set the NetworkingTarget to who should receive the function call.
        [NetworkingTarget(NetworkingTargetType.AllSelfExclusive)]
        private void Test(string msg, int test, VRCPlayerApi caller, int[] testArray) { // Demo method
            Debug.Log($"Triggered! {msg}, num: {test}, caller: ({caller.displayName}, {caller.playerId}), DebugArray: {testArray[0]}, {testArray[1]}, {testArray[2]}");
        }

        public override void Interact() {
            // Make the local player the owner of the GameObject before calling.
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

            // Make the call
            NetworkingLib_RPC(nameof(Test), null, "Hello there!", 69, Networking.LocalPlayer, new[] { 1, 2, 3 });
        }
    }
}
```

# Known Issues
- None so far
