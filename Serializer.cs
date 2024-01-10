using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;
using Random = System.Random;

namespace USPPNet {
    /*
     * Array:
     *      1 byte: SerializedType
     *      1 byte: array type
     *      4 bytes: array length
     *      ? bytes: array content
     * Other:
     *      1 byte: SerializedType
     *      1 - 16 bytes: content
     */

    public enum SerializedTypes {
        None,

        Boolean,
        Byte,
        SByte,

        Int16,
        UInt16,
        Int32,
        UInt32,
        Int64,
        UInt64,

        Single,
        Double,

        String,

        VRCPlayerApi,
        Color,
        Color32,
        Vector2,
        Vector2Int,
        Vector3,
        Vector3Int,
        Vector4,
        Quaternion,
        DateTime,

        Array,
        Null
    }

    public enum TypeSizes {
        Byte = 1,
        Int16 = 2,
        Int32 = 4,
        Int64 = 8,

        Single = 4,
        Double = 8,

        VRCPlayerApi = Int16,
        Color = Single * 4,
        Color32 = Byte * 4,

        Vector2 = Single * 2,
        Vector2Int = Int32 * 2,
        Vector3 = Single * 3,
        Vector3Int = Int32 * 3,
        Vector4 = Single * 4,

        Quaternion = Single * 4,

        Null = 0,
        Unknown = -1
    }

    public static class Serializer {
        [MenuItem("Test/test")]
        public static void Test() {
            var objects = new object[] {
                1, 2, 3,
                new[] { 1, 2, 3 },
                new[] { "a", "abc" }
            };

            //var bytes = Serialize(objects);

            //var original = 1.386593E+38f;
            var original = 524287.9f;

            //var bytes = SerializeSingle(original);
            //Debug.Log($"Original: {original}, deserialized: {DeserializeSingle(bytes)}");

            //var single = DeserializeSingle(bytes);

            var random = new Random();
            var stop = new Stopwatch();
            stop.Start();
            for (var i = 0; i < 10000000; i++) {
                var randomNum = new byte[4];
                random.NextBytes(randomNum);

                var single = DeserializeSingle(randomNum);

                if (double.IsNaN(single))
                    continue;

                var bytes = SerializeSingle(single);


                if (DeserializeUInt32(bytes) != DeserializeUInt32(randomNum)) {
                    Debug.Log(
                        $"FAILED! Original: {DeserializeUInt32(randomNum):X}, {DeserializeUInt32(bytes):X}, deserialized: {single}, ITERATION: {i}");
                    break;
                }
            }

            stop.Stop();


            Debug.Log($"Time: {stop.Elapsed.TotalSeconds}s {stop.Elapsed.TotalMilliseconds / 1000000}ms per");


            //Debug.Log($"Original: {original}, deserialized: {single}");
        }

        private static byte[] SubArray(byte[] array, int startIndex, int length) {
            var subArray = new byte[length];
            Array.Copy(array, startIndex, subArray, 0, length);
            return subArray;
        }

        public static int GetSizeFromType(SerializedTypes type, object input, bool isArray = false) {
            var map = new[] {
                TypeSizes.Null,

                TypeSizes.Byte,
                TypeSizes.Byte,
                TypeSizes.Byte,

                TypeSizes.Int16,
                TypeSizes.Int16,
                TypeSizes.Int32,
                TypeSizes.Int32,
                TypeSizes.Int64,
                TypeSizes.Int64,

                TypeSizes.Single,
                TypeSizes.Double,

                TypeSizes.Unknown,

                TypeSizes.VRCPlayerApi,

                TypeSizes.Color,
                TypeSizes.Color32,

                TypeSizes.Vector2,
                TypeSizes.Vector2Int,
                TypeSizes.Vector3,
                TypeSizes.Vector3Int,
                TypeSizes.Vector4,

                TypeSizes.Quaternion,

                TypeSizes.Int64,

                TypeSizes.Unknown,

                TypeSizes.Null
            };

            if (isArray) {
                var arrayBaseSize = (int)TypeSizes.Int32 + (int)TypeSizes.Int32 + (int)TypeSizes.Byte * 2;
                var array = (Array)input;
                if (type == SerializedTypes.String) {
                    var arraySize = array.Length + arrayBaseSize;
                    for (var i = 0; i < array.Length; i++)
                        arraySize += ((string)array.GetValue(i)).Length * 4 + 1;

                    return arraySize;
                }

                return (int)map[(int)type] * array.Length + arrayBaseSize;
            }

            if (type == SerializedTypes.String)
                return ((string)input).Length * 4 + (int)TypeSizes.Byte;

            return (int)map[(int)type] + (int)TypeSizes.Byte;
        }

        public static bool IsArray(object input) => Utilities.IsValid(input) && input.GetType().IsArray;

        public static SerializedTypes GetSerializedType(object input) {
            if (!Utilities.IsValid(input))
                return SerializedTypes.Null;

            var type = input.GetType();
            if (type.IsArray)
                type = type.GetElementType();
            var serializedType = SerializedTypes.None;

            if (type == typeof(bool))
                serializedType = SerializedTypes.Boolean;
            else if (type == typeof(byte))
                serializedType = SerializedTypes.Byte;
            else if (type == typeof(sbyte))
                serializedType = SerializedTypes.SByte;
            else if (type == typeof(short))
                serializedType = SerializedTypes.Int16;
            else if (type == typeof(ushort))
                serializedType = SerializedTypes.UInt16;
            else if (type == typeof(int))
                serializedType = SerializedTypes.Int32;
            else if (type == typeof(uint))
                serializedType = SerializedTypes.UInt32;
            else if (type == typeof(long))
                serializedType = SerializedTypes.Int64;
            else if (type == typeof(ulong))
                serializedType = SerializedTypes.UInt64;
            else if (type == typeof(float))
                serializedType = SerializedTypes.Single;
            else if (type == typeof(double))
                serializedType = SerializedTypes.Double;
            else if (type == typeof(string))
                serializedType = SerializedTypes.String;
            else if (type == typeof(VRCPlayerApi))
                serializedType = SerializedTypes.VRCPlayerApi;
            else if (type == typeof(Color))
                serializedType = SerializedTypes.Color;
            else if (type == typeof(Color32))
                serializedType = SerializedTypes.Color32;
            else if (type == typeof(Vector2))
                serializedType = SerializedTypes.Vector2;
            else if (type == typeof(Vector2Int))
                serializedType = SerializedTypes.Vector2Int;
            else if (type == typeof(Vector3))
                serializedType = SerializedTypes.Vector3;
            else if (type == typeof(Vector3Int))
                serializedType = SerializedTypes.Vector3Int;
            else if (type == typeof(Vector4))
                serializedType = SerializedTypes.Vector4;
            else if (type == typeof(Quaternion))
                serializedType = SerializedTypes.Quaternion;
            else if (type == typeof(DateTime))
                serializedType = SerializedTypes.DateTime;
            else
                Debug.Log($"actual type: {type.FullName}");

            return serializedType;
        }

        #region Deserialize

        public static object DeserializeKnownType(byte[] data, SerializedTypes type) {
            switch (type) {
                case SerializedTypes.Boolean:
                    return DeserializeBool(data);
                case SerializedTypes.SByte:
                case SerializedTypes.Byte:
                    return DeserializeByte(data);
                case SerializedTypes.UInt16:
                case SerializedTypes.Int16:
                    return DeserializeInt16(data);
                case SerializedTypes.UInt32:
                case SerializedTypes.Int32:
                    return DeserializeInt32(data);
                case SerializedTypes.UInt64:
                case SerializedTypes.Int64:
                    return DeserializeInt64(data);
                case SerializedTypes.Single:
                    return DeserializeSingle(data);
                case SerializedTypes.Double:
                    return DeserializeDouble(data);
                case SerializedTypes.String:
                    return DeserializeString(data);
                case SerializedTypes.VRCPlayerApi:
                    return DeserializeVRCPlayerApi(data);
                case SerializedTypes.Color:
                    return DeserializeColor(data);
                case SerializedTypes.Color32:
                    return DeserializeColor32(data);
                case SerializedTypes.Vector2:
                    return DeserializeVector2(data);
                case SerializedTypes.Vector2Int:
                    return DeserializeVector2Int(data);
                case SerializedTypes.Vector3:
                    return DeserializeVector3(data);
                case SerializedTypes.Vector3Int:
                    return DeserializeVector3Int(data);
                case SerializedTypes.Vector4:
                    return DeserializeVector4(data);
                case SerializedTypes.Quaternion:
                    return DeserializeQuaternion(data);
                case SerializedTypes.DateTime:
                    return DeserializeDateTime(data);
                case SerializedTypes.Null:
                case SerializedTypes.None:
                    return null;
                default:
                    return null;
            }
        }

        public static object[] Deserialize(byte[] bytes) => null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool DeserializeBool(byte[] bytes) => bytes[0] > 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte DeserializeByte(byte[] bytes) => bytes[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static sbyte DeserializeSByte(byte[] bytes) => (sbyte)bytes[0];

        public static short DeserializeInt16(byte[] bytes) => (short)(bytes[0] + (bytes[1] << 8));

        public static int DeserializeInt32(byte[] bytes) =>
            bytes[0] +
            (bytes[1] << 8) +
            (bytes[2] << 16) +
            (bytes[3] << 24);

        public static long DeserializeInt64(byte[] bytes) =>
            bytes[0] +
            ((long)bytes[1] << 8) +
            ((long)bytes[2] << 16) +
            ((long)bytes[3] << 24) +
            ((long)bytes[4] << 32) +
            ((long)bytes[5] << 40) +
            ((long)bytes[6] << 48) +
            ((long)bytes[7] << 56);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort DeserializeUInt16(byte[] bytes) => (ushort)DeserializeInt16(bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint DeserializeUInt32(byte[] bytes) => (uint)DeserializeInt32(bytes);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ulong DeserializeUInt64(byte[] bytes) => (ulong)DeserializeInt64(bytes);

        // Warning: this sometimes has an error of about 0.000,000,000,000,000,000,000,000,000,000,000,000,004,177,807,732 
        public static float DeserializeSingle(byte[] bytes) {
            var bits = DeserializeUInt32(bytes);

            if (bits == 0x00000000)
                return 0.0f;
            if (bits == 0x7F800000)
                return float.PositiveInfinity;
            if (bits == 0xFF800000)
                return float.NegativeInfinity;

            var sign = (bits & 0x80000000) == 0 ? 1 : -1;
            var mantissa = bits & 0x007FFFFF;
            var exponent = (int)((bits >> 23) & 0xFF);

            if (exponent == 0xFF && mantissa != 0)
                return float.NaN;

            //Debug.Log($"Deserl Sign: {sign}, mantissa: {mantissa}, exponent: {exponent}");

            var normalizedInverse = mantissa / 8388608f + 1;
            return normalizedInverse / Mathf.Pow(2, -exponent + 127) * sign;
        }

        // Warning: this has the same error as DeserializeSingle
        public static double DeserializeDouble(byte[] bytes) {
            var bits = DeserializeUInt64(bytes);

            if (bits == 0x0000000000000000)
                return 0.0;
            if (bits == 0x7FF0000000000000)
                return double.PositiveInfinity;
            if (bits == 0xFFF0000000000000)
                return double.NegativeInfinity;

            var sign = (bits & 0x8000000000000000) == 0 ? 1 : -1;
            var mantissa = bits & 0x000FFFFFFFFFFFFF;
            var exponent = (int)((bits >> 52) & 0x7FF);

            if (exponent == 0x7FF && mantissa != 0)
                return double.NaN;

            var normalizedInverse = mantissa / 4503599627370496.0 + 1;
            return normalizedInverse / Math.Pow(2, -exponent + 1023) * sign;
        }

        public static string DeserializeString(byte[] bytes) {
            var charBuffer = new char[bytes.Length - 4];

            var charIndex = 0;
            var index = 4;
            while (index < bytes.Length) {
                if ((bytes[index] & 0x80) == 0) // Single-byte character (ASCII)
                {
                    charBuffer[charIndex] = (char)bytes[index];
                    index++;
                }
                else if ((bytes[index] & 0xE0) == 0xC0) // Two-byte character
                {
                    charBuffer[charIndex] = (char)(((bytes[index] & 0x1F) << 6) | (bytes[index + 1] & 0x3F));
                    index += 2;
                }
                else if ((bytes[index] & 0xF0) == 0xE0) // Three-byte character
                {
                    charBuffer[charIndex] = (char)(((bytes[index] & 0x0F) << 12) | ((bytes[index + 1] & 0x3F) << 6) |
                                                   (bytes[index + 2] & 0x3F));
                    index += 3;
                }
                else if ((bytes[index] & 0xF8) == 0xF0) // Four-byte character
                {
                    charBuffer[charIndex] = (char)(((bytes[index] & 0x07) << 18) | ((bytes[index + 1] & 0x3F) << 12) |
                                                   ((bytes[index + 2] & 0x3F) << 6) | (bytes[index + 3] & 0x3F));
                    index += 4;
                }
                else {
                    index++; // eh
                    charIndex--;
                }

                charIndex++;
            }

            var newBuff = new char[charBuffer.Length];
            Array.Copy(charBuffer, newBuff, charBuffer.Length);

            return new string(newBuff);
        }

        public static VRCPlayerApi DeserializeVRCPlayerApi(byte[] data) {
            var playerBits = DeserializeUInt16(data);

            if ((playerBits & 0x8000) != 0)
                return new VRCPlayerApi();

            var playerId = (ushort)(playerBits & 0x7FFF);
            return VRCPlayerApi.GetPlayerById(playerId);
        }

        public static Color DeserializeColor(byte[] data) {
            var r = DeserializeSingle(SubArray(data, 0, 4));
            var g = DeserializeSingle(SubArray(data, 4, 4));
            var b = DeserializeSingle(SubArray(data, 8, 4));
            var a = DeserializeSingle(SubArray(data, 12, 4));

            return new Color(r, g, b, a);
        }

        public static Color32 DeserializeColor32(byte[] data) => new Color(data[0], data[1], data[2], data[3]);

        public static Vector2 DeserializeVector2(byte[] data) {
            var x = DeserializeSingle(SubArray(data, 0, 4));
            var y = DeserializeSingle(SubArray(data, 4, 4));

            return new Vector2(x, y);
        }

        public static Vector2Int DeserializeVector2Int(byte[] data) {
            var x = DeserializeInt32(SubArray(data, 0, 4));
            var y = DeserializeInt32(SubArray(data, 4, 4));

            return new Vector2Int(x, y);
        }

        public static Vector3 DeserializeVector3(byte[] data) {
            if (data.Length != (int)TypeSizes.Vector3)
                throw new ArgumentException("Invalid data length for Vector3 deserialization.");
            // Handle error or return default Vector3 as needed
            var x = DeserializeSingle(SubArray(data, 0, 4));
            var y = DeserializeSingle(SubArray(data, 4, 4));
            var z = DeserializeSingle(SubArray(data, 8, 4));

            return new Vector3(x, y, z);
        }

        public static Vector3Int DeserializeVector3Int(byte[] data) {
            var x = DeserializeInt32(SubArray(data, 0, 4));
            var y = DeserializeInt32(SubArray(data, 4, 4));
            var z = DeserializeInt32(SubArray(data, 8, 4));

            return new Vector3Int(x, y, z);
        }

        public static Vector4 DeserializeVector4(byte[] data) {
            var x = DeserializeSingle(SubArray(data, 0, 4));
            var y = DeserializeSingle(SubArray(data, 4, 4));
            var z = DeserializeSingle(SubArray(data, 8, 4));
            var w = DeserializeSingle(SubArray(data, 12, 4));

            return new Vector4(x, y, z, w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion DeserializeQuaternion(byte[] data) {
            var deVec4 = DeserializeVector4(data);
            return new Quaternion(
                deVec4.x,
                deVec4.y,
                deVec4.z,
                deVec4.w
            );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static DateTime DeserializeDateTime(byte[] data) => new DateTime(DeserializeInt64(data));

        #endregion


        #region Serialize

        public static byte[] SerializeKnownType(object input, SerializedTypes type) {
            switch (type) {
                case SerializedTypes.Boolean:
                    return SerializeBool((bool)input);
                case SerializedTypes.SByte:
                case SerializedTypes.Byte:
                    return SerializeByte((byte)input);
                case SerializedTypes.UInt16:
                case SerializedTypes.Int16:
                    return SerializeInt16((short)input);
                case SerializedTypes.UInt32:
                case SerializedTypes.Int32:
                    return SerializeInt32((int)input);
                case SerializedTypes.UInt64:
                case SerializedTypes.Int64:
                    return SerializeInt64((long)input);
                case SerializedTypes.Single:
                    return SerializeSingle((float)input);
                case SerializedTypes.Double:
                    return SerializeDouble((double)input);
                case SerializedTypes.String:
                    return SerializeString((string)input);
                case SerializedTypes.VRCPlayerApi:
                    return SerializeVRCPlayerApi((VRCPlayerApi)input);
                case SerializedTypes.Color:
                    return SerializeColor((Color)input);
                case SerializedTypes.Color32:
                    return SerializeColor32((Color32)input);
                case SerializedTypes.Vector2:
                    return SerializeVector2((Vector2)input);
                case SerializedTypes.Vector2Int:
                    return SerializeVector2Int((Vector2Int)input);
                case SerializedTypes.Vector3:
                    return SerializeVector3((Vector3)input);
                case SerializedTypes.Vector3Int:
                    return SerializeVector3Int((Vector3Int)input);
                case SerializedTypes.Vector4:
                    return SerializeVector4((Vector4)input);
                case SerializedTypes.Quaternion:
                    return SerializeQuaternion((Quaternion)input);
                case SerializedTypes.DateTime:
                    return SerializeDateTime((DateTime)input);
                case SerializedTypes.Null:
                case SerializedTypes.None:
                    return new byte[0];
                default:
                    return new byte[0];
            }
        }

        public static byte[] Serialize(object[] input) {
            var types = new SerializedTypes[input.Length];
            var isArray = new bool[input.Length];
            var sizes = new int[input.Length];

            var estimatedByteCount = 0;

            for (var i = 0; i < input.Length; i++) {
                var currentObject = input[i];
                var type = GetSerializedType(currentObject);
                var array = IsArray(currentObject);
                var size = GetSizeFromType(type, currentObject, array);

                types[i] = type;
                isArray[i] = array;
                sizes[i] = size;

                estimatedByteCount += size;

                Debug.Log($"type: {Enum.GetName(typeof(SerializedTypes), type)}, isArray: {array}, size = {size}");
            }

            var byteArray = new byte[estimatedByteCount];
            var byteIndex = 0;

            for (var i = 0; i < input.Length; i++) {
                byteArray[byteIndex] = (byte)types[i];
                var byteSize = sizes[i];

                switch (types[i]) {
                    case SerializedTypes.None:
                    case SerializedTypes.Null:
                        byteArray[byteIndex] = (byte)SerializedTypes.Null;
                        break;

                    case SerializedTypes.Boolean:
                        break;
                    case SerializedTypes.Byte:
                        break;
                    case SerializedTypes.SByte:
                        break;
                    case SerializedTypes.Int16:
                        break;
                    case SerializedTypes.UInt16:
                        break;
                    case SerializedTypes.Int32:
                        break;
                    case SerializedTypes.UInt32:
                        break;
                    case SerializedTypes.Int64:
                        break;
                    case SerializedTypes.UInt64:
                        break;
                    case SerializedTypes.Single:
                        break;
                    case SerializedTypes.Double:
                        break;
                    case SerializedTypes.String:
                        break;
                    case SerializedTypes.VRCPlayerApi:
                        break;
                    case SerializedTypes.Color:
                        break;
                    case SerializedTypes.Color32:
                        break;
                    case SerializedTypes.Vector2:
                        break;
                    case SerializedTypes.Vector2Int:
                        break;
                    case SerializedTypes.Vector3:
                        break;
                    case SerializedTypes.Vector3Int:
                        break;
                    case SerializedTypes.Vector4:
                        break;
                    case SerializedTypes.Quaternion:
                        break;
                    case SerializedTypes.DateTime:
                        break;
                    case SerializedTypes.Array:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                byteIndex += byteSize;
            }


            Debug.Log($"estimatedByteCount: {estimatedByteCount}");

            return new byte[] { };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeBool(bool input) => new[] { Convert.ToByte(input) };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeByte(byte input) => new[] { input };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeSByte(sbyte input) => new[] { (byte)input };

        public static byte[] SerializeInt16(short input) {
            return new[] {
                (byte)input,
                (byte)(input >> 8)
            };
        }

        public static byte[] SerializeInt32(int input) {
            return new[] {
                (byte)input,
                (byte)(input >> 8),
                (byte)(input >> 16),
                (byte)(input >> 24)
            };
        }

        public static byte[] SerializeInt64(long input) {
            return new[] {
                (byte)input,
                (byte)(input >> 8),
                (byte)(input >> 16),
                (byte)(input >> 24),
                (byte)(input >> 32),
                (byte)(input >> 40),
                (byte)(input >> 48),
                (byte)(input >> 56)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeUInt16(ushort input) => SerializeInt16((short)input);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeUInt32(uint input) => SerializeInt32((int)input);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeUInt64(ulong input) => SerializeInt64((long)input);

        // Need to optimize this more later
        // Warning: Sometimes the mantissa is off by 1. this shouldn't be that big of a deal, it is a Float after all.
        public static byte[] SerializeSingle(float input) {
            if (input == 0.0f)
                return SerializeUInt32(0x00000000);
            if (float.IsNaN(input))
                return SerializeUInt32(0x7FFFFFFF);
            if (float.IsPositiveInfinity(input))
                return SerializeUInt32(0x7F800000);
            if (float.IsNegativeInfinity(input))
                return SerializeUInt32(0xFF800000);

            var sign = input < 0;
            var absInput = Mathf.Abs(input);

            var exponent = Mathf.FloorToInt(Mathf.Log(absInput) / Mathf.Log(2)) + 127;
            // Get the exponent by repeatedly dividing the number by 2

            /* This works 100% but is a bit slow
           var exponent = 127;
           var absFloat = absInput;
           while (absFloat >= 2.0f)
           {
               absFloat /= 2.0f;
               exponent++;
           }

           while (absFloat < 1.0f)
           {
               absFloat *= 2.0f;
               exponent--;
           }
           */

            var negExpPow = Mathf.Pow(2, -exponent + 127);
            var normalized = absInput * negExpPow;

            var mantissa = (int)(normalized * 8388608) & 0x007FFFFF;


            var normalizedInverse = mantissa / 8388608f + 1;
            var reconstructedInput = normalizedInverse / negExpPow;

            if (reconstructedInput > absInput) {
                exponent--;
                normalized = absInput * Mathf.Pow(2, -exponent + 127);
                mantissa = (int)(normalized * 8388608) & 0x007FFFFF;
            }
            else if (reconstructedInput < absInput) {
                exponent++;
                normalized = absInput * Mathf.Pow(2, -exponent + 127);
                mantissa = (int)(normalized * 8388608) & 0x007FFFFF;
            }

            var test = (sign ? 0x80000000 : 0x0) | (uint)(((exponent & 0x000000FF) << 23) | mantissa);
            return SerializeUInt32(test);
        }

        // Warning: Sometimes the mantissa is off by 1. this shouldn't be that big of a deal, it is a Double after all.
        public static byte[] SerializeDouble(double input) {
            if (input == 0.0)
                return SerializeUInt64(0x0000000000000000);
            if (double.IsNaN(input))
                return SerializeUInt64(0x7FFFFFFFFFFFFFFF);
            if (double.IsPositiveInfinity(input))
                return SerializeUInt64(0x7FF0000000000000);
            if (double.IsNegativeInfinity(input))
                return SerializeUInt64(0xFFF0000000000000);

            var sign = input < 0;
            var absInput = Math.Abs(input);

            var exponent = (long)Math.Floor(Math.Log(absInput) / Math.Log(2)) + 1023;

            var negExpPow = Math.Pow(2, -exponent + 1023);
            var normalized = absInput * negExpPow;

            var mantissa = (long)(normalized * 4503599627370496) & 0x000FFFFFFFFFFFFF;

            var normalizedInverse = mantissa / 4503599627370496.0 + 1;
            var reconstructedInput = normalizedInverse / negExpPow;

            if (reconstructedInput > absInput) {
                exponent--;
                normalized = absInput * Math.Pow(2, -exponent + 1023);
                mantissa = (long)(normalized * 4503599627370496) & 0x000FFFFFFFFFFFFF;
            }
            else if (reconstructedInput < absInput) {
                exponent++;
                normalized = absInput * Math.Pow(2, -exponent + 1023);
                mantissa = (long)(normalized * 4503599627370496) & 0x000FFFFFFFFFFFFF;
            }

            var test = (sign ? 0x8000000000000000 : 0x0) |
                       (ulong)(((exponent & 0x00000000000007FF) << 52) | mantissa);
            return SerializeUInt64(test);
        }


        public static byte[] SerializeString(string inputString) {
            var buffer = new byte[inputString.Length * 4];
            var buffSize = 0;

            for (var index = 0; index < inputString.Length; index++) {
                var chr = inputString[index];
                // https://stackoverflow.com/questions/42012563/convert-unicode-code-points-to-utf-8-and-utf-32
                if (chr <= 0x7F) {
                    buffer[buffSize] = (byte)chr;
                    buffSize += 1;
                    continue;
                }

                if (chr <= 0x7FF) {
                    buffer[buffSize] = (byte)(0xC0 | (chr >> 6)); /* 110xxxxx */
                    buffer[buffSize + 1] = (byte)(0x80 | (chr & 0x3F)); /* 10xxxxxx */
                    buffSize += 2;
                    continue;
                }

                if (chr <= 0xFFFF) {
                    buffer[buffSize] = (byte)(0xE0 | (chr >> 12)); /* 1110xxxx */
                    buffer[buffSize + 1] = (byte)(0x80 | ((chr >> 6) & 0x3F)); /* 10xxxxxx */
                    buffer[buffSize + 2] = (byte)(0x80 | (chr & 0x3F)); /* 10xxxxxx */
                    buffSize += 3;
                    continue;
                }

                if (chr <= 0x10FFFF) {
                    buffer[buffSize] = (byte)(0xF0 | (chr >> 18)); /* 11110xxx */
                    buffer[buffSize + 1] = (byte)(0x80 | ((chr >> 12) & 0x3F)); /* 10xxxxxx */
                    buffer[buffSize + 2] = (byte)(0x80 | ((chr >> 6) & 0x3F)); /* 10xxxxxx */
                    buffer[buffSize + 3] = (byte)(0x80 | (chr & 0x3F)); /* 10xxxxxx */
                    buffSize += 4;
                }
            }

            var newBuff = new byte[buffSize + 4];
            Array.Copy(SerializeUInt32((uint)buffSize), newBuff, 4); // place array length as uint32 in first
            Array.Copy(buffer, 0, newBuff, 4, buffSize);
            return newBuff;
        }

        public static byte[] SerializeVRCPlayerApi(VRCPlayerApi input) {
            if (!input.IsValid())
                return SerializeUInt16(0x8000);

            var playerBits = (ushort)(input.playerId & 0x7FFF);
            return SerializeUInt16(playerBits);
        }

        public static byte[] SerializeColor(Color input) {
            var bytes = new byte[(int)TypeSizes.Color];

            Array.Copy(SerializeSingle(input.r), 0, bytes, 0, 4);
            Array.Copy(SerializeSingle(input.g), 0, bytes, 4, 4);
            Array.Copy(SerializeSingle(input.b), 0, bytes, 8, 4);
            Array.Copy(SerializeSingle(input.a), 0, bytes, 12, 4);

            return bytes;
        }

        public static byte[] SerializeColor32(Color32 input) {
            return new[] {
                input.r,
                input.g,
                input.b,
                input.a
            };
        }

        public static byte[] SerializeVector2(Vector2 input) {
            var bytes = new byte[(int)TypeSizes.Vector2];

            Array.Copy(SerializeSingle(input.x), 0, bytes, 0, 4);
            Array.Copy(SerializeSingle(input.y), 0, bytes, 4, 4);

            return bytes;
        }

        public static byte[] SerializeVector2Int(Vector2Int input) {
            var bytes = new byte[(int)TypeSizes.Vector2Int];

            Array.Copy(SerializeInt32(input.x), 0, bytes, 0, 4);
            Array.Copy(SerializeInt32(input.y), 0, bytes, 4, 4);

            return bytes;
        }

        public static byte[] SerializeVector3(Vector3 input) {
            var bytes = new byte[(int)TypeSizes.Vector3];

            Array.Copy(SerializeSingle(input.x), 0, bytes, 0, 4);
            Array.Copy(SerializeSingle(input.y), 0, bytes, 4, 4);
            Array.Copy(SerializeSingle(input.z), 0, bytes, 8, 4);

            return bytes;
        }

        public static byte[] SerializeVector3Int(Vector3Int input) {
            var bytes = new byte[(int)TypeSizes.Vector3Int];

            Array.Copy(SerializeInt32(input.x), 0, bytes, 0, 4);
            Array.Copy(SerializeInt32(input.y), 0, bytes, 4, 4);
            Array.Copy(SerializeInt32(input.z), 0, bytes, 8, 4);

            return bytes;
        }

        public static byte[] SerializeVector4(Vector4 input) {
            var bytes = new byte[(int)TypeSizes.Vector4];

            Array.Copy(SerializeSingle(input.x), 0, bytes, 0, 4);
            Array.Copy(SerializeSingle(input.y), 0, bytes, 4, 4);
            Array.Copy(SerializeSingle(input.z), 0, bytes, 8, 4);
            Array.Copy(SerializeSingle(input.w), 0, bytes, 8, 4);

            return bytes;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeQuaternion(Quaternion input) =>
            SerializeVector4(new Vector4(input.x, input.y, input.z, input.w));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte[] SerializeDateTime(DateTime input) => SerializeInt64(input.Ticks);

        public static byte[] SerializeArray(Array input, SerializedTypes arrayType) {
            if (arrayType == SerializedTypes.Array)
                return new[] { (byte)arrayType };

            var bytes = new byte[GetSizeFromType(arrayType, input, true) - 1];

            bytes[0] = (byte)arrayType;
            Array.Copy(SerializeInt32(input.Length), 0, bytes, 1, 4);

            var byteIndex = 5;
            if (arrayType == SerializedTypes.String) // handle strings separately
            {
                for (var i = 0; i < input.Length; i++) {
                    var currentString = (string)input.GetValue(i);
                    var serializedString = SerializeString(currentString);

                    var stringSize = serializedString.Length;
                    Array.Copy(serializedString, 0, bytes, byteIndex, stringSize);
                    byteIndex += stringSize;
                }

                var newByteArray = new byte[byteIndex];
                Array.Copy(bytes, 0, newByteArray, 0, byteIndex);
                return newByteArray;
            }

            var typeSize = GetSizeFromType(arrayType, input);
            for (var i = 0; i < input.Length; i++) {
                Array.Copy(SerializeKnownType(input.GetValue(i), arrayType), 0, bytes, byteIndex, typeSize);
                byteIndex += typeSize;
            }

            return bytes;
        }

        #endregion
    }
}