# USPPNet
 UdonSharp RPC with arguments!

# How to use
1. Clone the git repo into your Project's assets folder.
2. Wait for it to patch the UdonSharp Compiler.
3. Include USPPNet and System in your script.
4. Add "// USPPNet Init" at the bottom of your UdonSharpBehaviour.
5. Add "// USPPNet OnDeserialization" inside your OnDeserialization function.
6. Add "// USPPNet OnPostSerialization" inside your OnPostSerialization function.
    - It is semi important that OnDeserialization comes before OnPostSerialization, this is mainly to prevent in accurate compile errors, for the same reason you should put all of your own code before and of the "// USPPNet" comments.

## Usage
### Boilerplate 
You must have all of this somewhere in your UdonSharpBehaviour class.
It's recommened to have the functions and comments in this order to prevent pain when debugging your code.
```csharp
// you must have these includes in any script that uses USPPNet
using USPPNet;
using System;

// Before class ↑

// Inside class ↓
// Comments that start with USPPNet are important, you need them for USPPNet to work

public override void OnDeserialization()
{
    // Your code here
    // USPPNet OnDeserialization
}
    
public override void OnPostSerialization(VRC.Udon.Common.SerializationResult result)
{
    // Your code here
    // USPPNet OnPostSerialization
}

// USPPNet Init
```
### Creating functions
```csharp
// To define a networked function
// You simply put USPPNET_ before the name of any function to make it a USPPNet function
void USPPNET_Ping() {
    // Do something
}
```
### Calling functions
```csharp
// Only the network owner of the object will be able to call USPPNet functions

USPPNET_Ping(); // The PreProcessor will parse this and turn it into a USPPNet remote function call
// So "USPPNET_Ping();" will become USPPNet_RPC("USPPNET_Ping");
// But that isn't really something you have to think about since it all happens in the background
```
### Calling functions with parameters!
```csharp
// You need to add defines for which parameter types your are going to use
// This is a bandwidth saving compromise. Please only specify the ones you need
// #defines must always be at the top of your script before any "using" keywords
// Might be automated in the future* 
#define USPPNet_int
#define USPPNet_string

// Create your function like normal
void USPPNET_Ping(string message, float time) {
    // Do something
}

private void Start() {
    // And call it like normal
    USPPNET_Ping("Hello there!", 420);
}
```

## Temp Example
```csharp
// you need to specify what parameter types you are going to use you do this by adding a define like so #define USPPNet_[TYPE] replacing [TYPE] with I.E string, float, or etc
#define USPPNet_int
#define USPPNet_string

// You must have these two
using USPPNet;
using System;

using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class Cube : UdonSharpBehaviour
{   
    private void USPPNET_Test(string msg, int test) // Demo method
    {
        Debug.Log($"Triggered! {msg}, num: {test}");
    }

    public override void Interact()
    {
        if (!Networking.IsOwner(gameObject))
            Networking.SetOwner(Networking.LocalPlayer, gameObject);

        USPPNET_Test("Hello there!", 69); // only the owner of the object can send RPC calls, this method gets called on everyone but the caller
        RequestSerialization(); // if you're using manual (i recommend you do) you need to call RequestSerialization to send the RPC
    }

    // Comments that start with USPPNet are important for it to work, don't remove these, or the PreProcessor won't be able to generate the code
    public override void OnDeserialization()
    {
        // Always put your own code above the USPPNet comments, otherwise debugging will get hard
        // USPPNet OnDeserialization
    }
    
    public override void OnPostSerialization(VRC.Udon.Common.SerializationResult result)
    {
        // You'd also want OnDeserialization to be before OnPostSerialization, for the same reason
        // USPPNet OnPostSerialization
    }

    // USPPNet Init
}
```

# Known Issues
1. Arrays are not supported.
2. Serialization will fail if you try to pass a null argument into function.
3. Not all value types are supported.
4. Network usage is relatively high ~170 Bytes(increases with more "USPPNet_[TYPE]" defines) for one call, it's not linear so two calls at once would take ~190 bytes. Theses are the results from the demo.
5. Trying to call USPPNET_[FUNC NAME] on an gameObject/Component other than itself will not be networked, and get called on the local client instead.