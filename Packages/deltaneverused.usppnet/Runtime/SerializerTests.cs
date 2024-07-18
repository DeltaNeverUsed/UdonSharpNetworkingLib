//#define LogSucc

using System;
using System.Diagnostics;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace USPPNet {
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SerializerTests : UdonSharpBehaviour {
        public int tests = 1;

        private void Start() {
            SendCustomEventDelayedSeconds(nameof(RunTests), 2);
        }

        public void RunTests() {
            var stop = new Stopwatch();

            Debug.Log("Starting Tests!");
            stop.Restart();
            for (var i = 0; i < tests; i++) {
                TestSerializerBool();
                TestSerializerByte();
                TestSerializerSByte();
                TestSerializerInt16();
                TestSerializerInt32();
                TestSerializerInt64();
                TestSerializerUInt16();
                TestSerializerUInt32();
                TestSerializerUInt64();
                TestSerializerSingle();
                TestSerializerDouble();
                TestSerializerString();
                TestSerializerVRCPlayerApi();
                TestSerializerColor();
                TestSerializerColor32();
                TestSerializerVector2();
                TestSerializerVector2Int();
                TestSerializerVector3();
                TestSerializerVector3Int();
                TestSerializerVector4();
                TestSerializerQuaternion();
                TestSerializerDateTime();
                TestSerializerArray();
            }

            stop.Stop();

            Debug.Log(
                $"Individual Tests Done! totalTime: {stop.Elapsed.TotalMilliseconds}ms, {stop.Elapsed.TotalMilliseconds / tests}ms per test");
            
            Debug.Log("Starting Tests!");
            var ser = new Stopwatch();
            var deser = new Stopwatch();
            for (var i = 0; i < tests; i++) {
                var things = new object[] {
                    "Test string!!",
                    Random.Range(0, 100),
                    (short)Random.Range(0, 255),
                    new Vector2(Random.value,Random.value),
                    new DateTime(Random.Range(0, int.MaxValue)),
                    123.3d,
                    Random.value,
                    new int[] {1, 2, 3, 4, 5, 6, 7},
                    new string[] {$"testing string {Random.value}", $"string test {Random.value}", $"more strings {Random.value}"},
                    90812739827649,
                    9132,
                    337,
                    8139,
                    true,
                    false,
                    true,
                    true,
                };
                
                ser.Start();
                var stuff = Serializer.Serialize(things);
                ser.Stop();
                deser.Start();
                var dethings = Serializer.Deserialize(stuff);
                deser.Stop();

                if (dethings.Length != things.Length) {
                    Debug.Log("things and dethings not same length");
                    return;
                }

                for (int j = 0; j < things.Length; j++) {
                    if (!things[j].Equals(dethings[j])) {
                        if (Serializer.IsArray(things[j])) {
                            //Debug.Log(
                            //    $"item at {j} not same, things: {((object[])things[j]).Length}, dethings: {((object[])dethings[j]).Length}");
                        }
                        else {
                            Debug.Log(
                                $"item at {j} not same, things: {things[j].ToString()}, dethings: {dethings[j].ToString()}");
                            return;
                        }
                        
                    }
                }
            }

            stop.Stop();

            Debug.Log(
                $"Object Array Tests Done! ser: {ser.Elapsed.TotalMilliseconds}ms, {ser.Elapsed.TotalMilliseconds / tests}ms per test");
            Debug.Log(
                $"Object Array Tests Done! deser: {deser.Elapsed.TotalMilliseconds}ms, {deser.Elapsed.TotalMilliseconds / tests}ms per test");
            Debug.Log(
                $"Object Array Tests Done! totalTime: {ser.Elapsed.TotalMilliseconds + deser.Elapsed.TotalMilliseconds}ms, {(ser.Elapsed.TotalMilliseconds + deser.Elapsed.TotalMilliseconds) / tests}ms per test");
        }

        private static void TestSerializerBool() {
            var input = Random.value > 0.5f;
            var serialized = Serializer.SerializeBool(input);
            var result = Serializer.DeserializeBool(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerBool: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Bool passed!");
#endif
            }
        }

        private static void TestSerializerByte() {
            var input = (byte)Random.Range(0, 256);
            var serialized = Serializer.SerializeByte(input);
            var result = Serializer.DeserializeByte(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerByte: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Byte passed!");
#endif
            }
        }

        private static void TestSerializerSByte() {
            var input = (sbyte)Random.Range(-128, 128);
            var serialized = Serializer.SerializeSByte(input);
            var result = Serializer.DeserializeSByte(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerSByte: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("SByte passed!");
#endif
            }
        }

        private static void TestSerializerInt16() {
            var input = (short)Random.Range(short.MinValue, short.MaxValue);
            var serialized = Serializer.SerializeInt16(input);
            var result = Serializer.DeserializeInt16(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerInt16: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Int16 passed!");
#endif
            }
        }

        private static void TestSerializerInt32() {
            var input = Random.Range(int.MinValue, int.MaxValue);
            var serialized = Serializer.SerializeInt32(input);
            var result = Serializer.DeserializeInt32(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerInt32: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Int32 passed!");
#endif
            }
        }

        private static void TestSerializerInt64() {
            long input = Random.Range(int.MinValue, int.MaxValue) * 24;
            var serialized = Serializer.SerializeInt64(input);
            var result = Serializer.DeserializeInt64(serialized, 0);
            if (input != result) {
                var debugString = "";
                for (var i = 0; i < serialized.Length; i++)
                    debugString += $"{serialized[i]:X}, ";
                Debug.Log($"Serializer.TestSerializerInt64: {debugString} Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Int64 passed!");
#endif
            }
        }

        private static void TestSerializerUInt16() {
            var input = (ushort)Random.Range(0, ushort.MaxValue + 1);
            var serialized = Serializer.SerializeUInt16(input);
            var result = Serializer.DeserializeUInt16(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerUInt16: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("UInt16 passed!");
#endif
            }
        }

        private static void TestSerializerUInt32() {
            var input = (uint)Random.Range(0, int.MaxValue) + (uint)Random.Range(0, int.MaxValue);
            var serialized = Serializer.SerializeUInt32(input);
            var result = Serializer.DeserializeUInt32(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerUInt32: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("UInt32 passed!");
#endif
            }
        }

        private static void TestSerializerUInt64() {
            var input = (ulong)Random.Range(0, int.MaxValue) + ((ulong)Random.Range(0, int.MaxValue) << 32);
            var serialized = Serializer.SerializeUInt64(input);
            var result = Serializer.DeserializeUInt64(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerUInt64: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("UInt64 passed!");
#endif
            }
        }

        private static void TestSerializerSingle() {
            var input = Random.Range(float.MinValue, float.MaxValue);
            var serialized = Serializer.SerializeSingle(input);
            var result = Serializer.DeserializeSingle(serialized, 0);
            if (!Mathf.Approximately(input, result)) {
                Debug.Log($"Serializer.TestSerializerSingle: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Single passed!");
#endif
            }
        }

        private static void TestSerializerDouble() {
            double input = Random.Range(float.MinValue, float.MaxValue);
            var serialized = Serializer.SerializeDouble(input);
            var result = Serializer.DeserializeDouble(serialized, 0);
            if (!Mathf.Approximately((float)input, (float)result)) {
                Debug.Log($"Serializer.TestSerializerDouble: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Double passed!");
#endif
            }
        }

        private static void TestSerializerString() {
            var input = "Test_String_" + Random.Range(0, 100);
            var serialized = Serializer.SerializeString(input);
            var result = Serializer.DeserializeString(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerString: Mismatch - Input: (\"{input}\", {input.Length}), Output: (\"{result}\", {result.Length})");
            }
            else {
#if LogSucc
                Debug.Log("String passed!");
#endif
            }
        }

        private static void TestSerializerVRCPlayerApi() {
            var serialized = Serializer.SerializeVRCPlayerApi(Networking.LocalPlayer);
            var result = Serializer.DeserializeVRCPlayerApi(serialized, 0);
            if (result == null || result.playerId != Networking.LocalPlayer.playerId) {
                Debug.Log(
                    $"Serializer.TestSerializerVRCPlayerApi: Mismatch - Input: {Networking.LocalPlayer.playerId}, Output: {(result == null ? "Was null" : result.playerId.ToString())}");
            }
            else {
#if LogSucc
                Debug.Log("VRCPlayerApi passed!");
#endif
            }
        }

        private static void TestSerializerColor() {
            var input = new Color(Random.value, Random.value, Random.value, Random.value);
            var serialized = Serializer.SerializeColor(input);
            var result = Serializer.DeserializeColor(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerColor: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Color passed!");
#endif
            }
        }

        private static void TestSerializerColor32() {
            var input = new Color32((byte)Random.Range(0, 256), (byte)Random.Range(0, 256), (byte)Random.Range(0, 256),
                (byte)Random.Range(0, 256));
            var serialized = Serializer.SerializeColor32(input);
            var result = Serializer.DeserializeColor32(serialized, 0);
            if (input.r != result.r || input.g != result.g || input.b != result.b || input.a != result.a) {
                Debug.Log($"Serializer.TestSerializerColor32: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Color32 passed!");
#endif
            }
        }

        private static void TestSerializerVector2() {
            var input = new Vector2(Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            var serialized = Serializer.SerializeVector2(input);
            var result = Serializer.DeserializeVector2(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerVector2: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Vec2 passed!");
#endif
            }
        }

        private static void TestSerializerVector2Int() {
            var input = new Vector2Int(Random.Range(-100, 100), Random.Range(-100, 100));
            var serialized = Serializer.SerializeVector2Int(input);
            var result = Serializer.DeserializeVector2Int(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerVector2Int: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Vec2Int passed!");
#endif
            }
        }

        private static void TestSerializerVector3() {
            var input = new Vector3(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f));
            var serialized = Serializer.SerializeVector3(input);
            var result = Serializer.DeserializeVector3(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerVector3: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Vec3 passed!");
#endif
            }
        }

        private static void TestSerializerVector3Int() {
            var input = new Vector3Int(Random.Range(-100, 100), Random.Range(-100, 100), Random.Range(-100, 100));
            var serialized = Serializer.SerializeVector3Int(input);
            var result = Serializer.DeserializeVector3Int(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerVector3Int: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Vec3Int passed!");
#endif
            }
        }

        private static void TestSerializerVector4() {
            var input = new Vector4(Random.Range(-100f, 100f), Random.Range(-100f, 100f), Random.Range(-100f, 100f),
                Random.Range(-100f, 100f));
            var serialized = Serializer.SerializeVector4(input);
            var result = Serializer.DeserializeVector4(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerVector4: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Vec4 passed");
#endif
            }
        }

        private static void TestSerializerQuaternion() {
            var input = new Quaternion(Random.value, Random.value, Random.value, Random.value).normalized;
            var serialized = Serializer.SerializeQuaternion(input);
            var result = Serializer.DeserializeQuaternion(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerQuaternion: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("Quat passed!");
#endif
            }
        }

        private static void TestSerializerDateTime() {
            var input = DateTime.Now.AddSeconds(Random.Range(-100000, 100000));
            var serialized = Serializer.SerializeDateTime(input);
            var result = Serializer.DeserializeDateTime(serialized, 0);
            if (input != result) {
                Debug.Log($"Serializer.TestSerializerDateTime: Mismatch - Input: {input}, Output: {result}");
            }
            else {
#if LogSucc
                Debug.Log("DateTime passed!");
#endif
            }
        }

        private static void TestSerializerArray() {
            int[] inputArray = { 1, 2, 3, 4, 5 };
            var serialized = Serializer.SerializeArray(inputArray, SerializedTypes.Int32);
            var resultArray = Serializer.DeserializeArray(serialized, 0);
            var result = (int[])resultArray;

            if (result == null || !Extensions.IsArraySame(inputArray, resultArray)) {
                Debug.Log(
                    $"Serializer.TestSerializerArray: Mismatch - Input: {Extensions.StringJoin(", ", inputArray)}, Output: {Extensions.StringJoin(", ", result)}");
            }
            else {
#if LogSucc
                Debug.Log("Array Passed");
#endif
            }
        }
    }
}