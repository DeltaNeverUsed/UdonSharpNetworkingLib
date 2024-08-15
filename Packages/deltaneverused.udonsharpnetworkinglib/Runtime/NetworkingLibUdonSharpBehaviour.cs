using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon.Common;
using static UdonSharpNetworkingLib.UdonSharpNetworkingLibConsts;

namespace UdonSharpNetworkingLib {
    public abstract class NetworkingLibUdonSharpBehaviour : UdonSharpBehaviour {
        [UdonSynced] private byte[] _data = Array.Empty<byte>();

        /// <summary>
        /// [0] Function ID: Int
        /// [1] NetworkingTarget: NetworkingTarget as byte
        /// [2] Params: object[] serialized as a byte array
        /// [3*] PlayerTarget
        ///
        /// * Depends on current Network Target Mode
        /// </summary>
        private DataList _calls = new DataList();

        private int _callLen = 0;

        private bool _serializationRequired;

        public void NetworkingLib_RPC(string methodName, VRCPlayerApi target = null, params object[] args) {
            var functions = (string[])GetProgramVariable(FunctionListKey);

            var paramTypeNames = "_";
            foreach (var param in args) {
                paramTypeNames += param.GetType().Name;
            }

            var functionDefinition = methodName + paramTypeNames;

            var functionId = Array.IndexOf(functions, functionDefinition);
            if (functionId == -1) {
                Debug.LogError($"An invalid function of definition \"{functionDefinition}\" was called but not found.");
                return;
            }

            var networkType = ((byte[])GetProgramVariable(NetworkingTypeKey))[functionId];

            var isExtended = networkType == (byte)NetworkingTargetType.Specific;
            if (isExtended) {
                if (!Utilities.IsValid(target)) {
                    Debug.LogError($"An invalid player was passed into function definition \"{functionDefinition}\"");
                    return;
                }

                if (target.isLocal) {
                    networkType = (byte)NetworkingTargetType.Local;
                }
            }

            if (networkType == (byte)NetworkingTargetType.Local) {
                NetworkingLib_FunctionCall((ushort)functionId, args);
                return;
            }
            
            if (networkType == (byte)NetworkingTargetType.All)
                NetworkingLib_FunctionCall((ushort)functionId, args);

            var dataArray = new object[isExtended ? 4 : 3];

            dataArray[0] = (ushort)functionId;
            dataArray[1] = networkType;
            dataArray[2] = Serializer.Serialize(args);

            if (isExtended) {
                dataArray[3] = target.playerId;
            }

            _callLen += dataArray.Length;
            _calls.Add(new DataToken(dataArray));
            RequestSerialization();
        }

        public override void OnDeserialization(DeserializationResult result) {
            if (result.sendTime - Time.realtimeSinceStartup > 8)
                return;

            var calls = Serializer.Deserialize(ref _data);

            var functionIndex = 0;
            while (functionIndex < calls.Length) {
                var targetType = (NetworkingTargetType)calls[functionIndex + 1];
                // Check the networking types for the current function we're processing
                switch (targetType) {
                    case NetworkingTargetType.All:
                    case NetworkingTargetType.AllSelfExclusive:
                        break;
                    case NetworkingTargetType.Master:
                        if (!Networking.LocalPlayer.isMaster)
                            continue;
                        break;
                    case NetworkingTargetType.Specific:
                        if (Networking.LocalPlayer != calls[functionIndex + 3])
                            continue;
                        break;
                    case NetworkingTargetType.Local:
                    default:
                        Debug.LogWarning("Invalid network type got");
                        continue;
                }
                // Call function if previews checks in switch don't fail
                var para = (byte[])calls[functionIndex + 2];
                NetworkingLib_FunctionCall(_data[functionIndex], Serializer.Deserialize(ref para));
            }
            
        }

        public override void OnPreSerialization() {
            if (!_serializationRequired)
                return;

            // Copying all the stuff out of the individual object arrays into one big object array
            var objectCalls = new object[_callLen];
            var currentIndex = 0;
            var callsArray = _calls.ToArray();
            foreach (var call in callsArray) {
                var callArray = (object[])call.Reference;
                Array.Copy(callArray, 0, objectCalls, currentIndex, callArray.Length);
                currentIndex += callArray.Length;
            }

            _data = Serializer.Serialize(objectCalls);

            _callLen = 0;
            _calls = new DataList();
        }

        public override void OnPostSerialization(SerializationResult result) {
            _data = new byte[0];
        }

        protected virtual void NetworkingLib_FunctionCall(ushort functionId, params object[] parameters) { }
    }
}