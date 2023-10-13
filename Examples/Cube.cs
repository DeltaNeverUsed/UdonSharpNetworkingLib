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

