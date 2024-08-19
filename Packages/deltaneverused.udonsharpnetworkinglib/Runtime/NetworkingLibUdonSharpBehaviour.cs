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
        /// [1] Params: object[] serialized as a byte array
        /// [2*] PlayerTarget
        ///
        /// * Depends on current Network Target Mode
        /// </summary>
        private DataList _calls = new DataList();

        private int _callLen = 0;

        private bool _serializationRequired;

        /// <summary>
        /// Call a networked function.
        /// </summary>
        /// <param name="methodName">Target function, please use nameof(function)</param>
        /// <param name="target">The target player, only used for target type <see cref="UdonSharpNetworkingLib.NetworkingTargetType.Specific"/> set to <see langword="null" /> otherwise</param>
        /// <param name="args">Function arguments</param>
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

            switch (networkType) {
                case (byte)NetworkingTargetType.Local:
                case (byte)NetworkingTargetType.Master when Networking.LocalPlayer.isMaster:
                    NetworkingLib_FunctionCall((ushort)functionId, args);
                    return;
                case (byte)NetworkingTargetType.All:
                    NetworkingLib_FunctionCall((ushort)functionId, args);
                    break;
            }

            var dataArray = new object[isExtended ? 3 : 2];

            dataArray[0] = (ushort)functionId;
            dataArray[1] = args;

            if (isExtended) {
                dataArray[2] = target.playerId;
            }

            _callLen += dataArray.Length;
            _calls.Add(new DataToken(dataArray));

            _serializationRequired = true;
            RequestSerialization();
        }

        public override void OnDeserialization(DeserializationResult result) {
            if (Time.realtimeSinceStartup - result.sendTime < 8)
                HandleDeserialization();
            NewOnDeserialization();
            NewOnDeserialization(result);
        }

        private void HandleDeserialization() {
            if (_data.Length == 0)
                return;
            var calls = Serializer.Deserialize(_data);

            var functionIndex = 0;
            while (functionIndex < calls.Length) {
                var inc = 2;
                var targetType = ((byte[])GetProgramVariable(NetworkingTypeKey))[(ushort)calls[functionIndex]];
                // Check the networking types for the current function we're processing
                switch (targetType) {
                    case (byte)NetworkingTargetType.All:
                    case (byte)NetworkingTargetType.AllSelfExclusive:
                        break;
                    case (byte)NetworkingTargetType.Master:
                        if (!Networking.LocalPlayer.isMaster) {
                            functionIndex += inc;
                            continue;
                        }

                        break;
                    case (byte)NetworkingTargetType.Specific:
                        inc++;
                        if (Networking.LocalPlayer != calls[functionIndex + 2]) {
                            functionIndex += inc;
                            continue;
                        }

                        break;
                    default:
                        Debug.LogWarning("Invalid network type got");
                        functionIndex += inc;
                        continue;
                }

                // Call function if previews checks in switch don't fail
                var para = (object[])calls[functionIndex + 1];
                NetworkingLib_FunctionCall((ushort)calls[functionIndex], para);

                functionIndex += inc;
            }
        }

        public override void OnPreSerialization() {
            HandlePreSerialization();
            NewOnPreSerialization();
        }

        private void HandlePreSerialization() {
            if (!_serializationRequired)
                return;
            _serializationRequired = false;

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
            NewOnPostSerialization(result);
        }

        public virtual void NewOnDeserialization() { }
        public virtual void NewOnDeserialization(DeserializationResult result) { }
        public virtual void NewOnPreSerialization() { }
        public virtual void NewOnPostSerialization(SerializationResult result) { }

        /// <summary>
        /// Do not override!
        /// This function is used internally for calling the functions and is generated at compile time.
        /// </summary>
        protected virtual void NetworkingLib_FunctionCall(ushort functionId, object[] parameters) { }
    }
}
