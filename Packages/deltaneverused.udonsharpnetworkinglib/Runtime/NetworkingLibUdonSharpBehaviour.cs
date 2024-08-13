using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using static UdonSharpNetworkingLib.UdonSharpNetworkingLibConsts;

namespace UdonSharpNetworkingLib {
    public abstract class NetworkingLibUdonSharpBehaviour : UdonSharpBehaviour {
        [UdonSynced] private byte[] _data = Array.Empty<byte>();

        /// <summary>
        /// [0] Function ID: Int
        /// [1] NetworkingTarget: NetworkingTarget as byte
        /// [2] Params: object[]
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

            var dataArray = new object[isExtended ? 4 : 3];

            dataArray[0] = functionId;
            dataArray[1] = networkType;
            dataArray[2] = args;
            
            if (isExtended) {
                if (!Utilities.IsValid(target)) {
                    Debug.LogError($"An invalid player was passed into function definition \"{functionDefinition}\"");
                    return;
                }
                dataArray[3] = target.playerId;
            }

            _callLen += dataArray.Length;
            _calls.Add(new DataToken(dataArray));
        }

        public override void OnDeserialization() {
            
        }
        
        public override void OnPreSerialization() {
            if (!_serializationRequired)
                return;

            var objectCalls = new object[_callLen];
            var currentIndex = 0;
            foreach (var call in _calls) {
                var callArray = (object[])call.Reference;
                Array.Copy(callArray, 0, objectCalls, currentIndex, callArray.Length);
                currentIndex += callArray.Length;
            }
            
            _data = Serializer.Serialize(objectCalls);

            _callLen = 0;
            _calls = new DataList();
        }

        protected virtual void NetworkingLib_FunctionCall(ushort functionId, params object[] parameters) {
            
        }
    }
}
