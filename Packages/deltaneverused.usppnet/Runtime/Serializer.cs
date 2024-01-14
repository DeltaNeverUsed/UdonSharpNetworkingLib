using System;
using System.Text;
using UnityEngine;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;

namespace USPPNet {
    /*
     * Array:
     *      1 byte: (SerializedTypes) SerializedType
     *      1 byte: (SerializedTypes) array type
     *      2 bytes: (UInt16) array length
     *      2 bytes: (UInt16) array size in bytes
     *      ? bytes: array content
     * String:
     *      1 byte: (SerializedTypes) SerializedType
     *      2 byte: (UInt16) StringSize in bytes
     * Other:
     *      1 byte: (SerializedTypes) SerializedType
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
                var arrayBaseSize = (int)TypeSizes.Int32 + (int)TypeSizes.Byte * 2;
                var array = (Array)input;
                if (type == SerializedTypes.String) {
                    var arraySize = arrayBaseSize;
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

        // ReSharper disable once PossibleNullReferenceException
        public static bool IsArray(object input) => Utilities.IsValid(input) && input.GetType().FullName.EndsWith("[]");

        public static Type GetTypeFromSerializedType(SerializedTypes type) {
            switch (type) {
                case SerializedTypes.Boolean:
                    return typeof(bool);
                case SerializedTypes.Byte:
                    return typeof(byte);
                case SerializedTypes.SByte:
                    return typeof(sbyte);
                case SerializedTypes.Int16:
                    return typeof(short);
                case SerializedTypes.UInt16:
                    return typeof(ushort);
                case SerializedTypes.Int32:
                    return typeof(int);
                case SerializedTypes.UInt32:
                    return typeof(uint);
                case SerializedTypes.Int64:
                    return typeof(long);
                case SerializedTypes.UInt64:
                    return typeof(ulong);
                case SerializedTypes.Single:
                    return typeof(float);
                case SerializedTypes.Double:
                    return typeof(double);
                case SerializedTypes.String:
                    return typeof(string);
                case SerializedTypes.VRCPlayerApi:
                    return typeof(VRCPlayerApi);
                case SerializedTypes.Color:
                    return typeof(Color);
                case SerializedTypes.Color32:
                    return typeof(Color32);
                case SerializedTypes.Vector2:
                    return typeof(Vector2);
                case SerializedTypes.Vector2Int:
                    return typeof(Vector2Int);
                case SerializedTypes.Vector3:
                    return typeof(Vector3);
                case SerializedTypes.Vector3Int:
                    return typeof(Vector3Int);
                case SerializedTypes.Vector4:
                    return typeof(Vector4);
                case SerializedTypes.Quaternion:
                    return typeof(Quaternion);
                case SerializedTypes.DateTime:
                    return typeof(DateTime);
                case SerializedTypes.Null:
                case SerializedTypes.None:
                default:
                    return null;
            }
        }

        public static SerializedTypes GetSerializedType(object input) {
            if (!Utilities.IsValid(input))
                return SerializedTypes.Null;

            var type = input.GetType();
            var serializedType = SerializedTypes.Null;

            if (type == typeof(bool) || type == typeof(bool[]))
                serializedType = SerializedTypes.Boolean;
            else if (type == typeof(byte) || type == typeof(byte[]))
                serializedType = SerializedTypes.Byte;
            else if (type == typeof(sbyte) || type == typeof(sbyte[]))
                serializedType = SerializedTypes.SByte;
            else if (type == typeof(short) || type == typeof(short[]))
                serializedType = SerializedTypes.Int16;
            else if (type == typeof(ushort) || type == typeof(ushort[]))
                serializedType = SerializedTypes.UInt16;
            else if (type == typeof(int) || type == typeof(int[]))
                serializedType = SerializedTypes.Int32;
            else if (type == typeof(uint) || type == typeof(uint[]))
                serializedType = SerializedTypes.UInt32;
            else if (type == typeof(long) || type == typeof(long[]))
                serializedType = SerializedTypes.Int64;
            else if (type == typeof(ulong) || type == typeof(ulong[]))
                serializedType = SerializedTypes.UInt64;
            else if (type == typeof(float) || type == typeof(float[]))
                serializedType = SerializedTypes.Single;
            else if (type == typeof(double) || type == typeof(double[]))
                serializedType = SerializedTypes.Double;
            else if (type == typeof(string) || type == typeof(string[]))
                serializedType = SerializedTypes.String;
            else if (type == typeof(VRCPlayerApi) || type == typeof(VRCPlayerApi[]))
                serializedType = SerializedTypes.VRCPlayerApi;
            else if (type == typeof(Color) || type == typeof(Color[]))
                serializedType = SerializedTypes.Color;
            else if (type == typeof(Color32) || type == typeof(Color32[]))
                serializedType = SerializedTypes.Color32;
            else if (type == typeof(Vector2) || type == typeof(Vector2[]))
                serializedType = SerializedTypes.Vector2;
            else if (type == typeof(Vector2Int) || type == typeof(Vector2Int[]))
                serializedType = SerializedTypes.Vector2Int;
            else if (type == typeof(Vector3) || type == typeof(Vector3[]))
                serializedType = SerializedTypes.Vector3;
            else if (type == typeof(Vector3Int) || type == typeof(Vector3Int[]))
                serializedType = SerializedTypes.Vector3Int;
            else if (type == typeof(Vector4) || type == typeof(Vector4[]))
                serializedType = SerializedTypes.Vector4;
            else if (type == typeof(Quaternion) || type == typeof(Quaternion[]))
                serializedType = SerializedTypes.Quaternion;
            else if (type == typeof(DateTime) || type == typeof(DateTime[]))
                serializedType = SerializedTypes.DateTime;
            else
                Debug.LogError($"unsupported type: {type.FullName}");

            return serializedType;
        }
#if !COMPILER_UDONSHARP && UNITY_EDITOR
        public static void PrintWholeArray(Array obj) {
            var sb = new StringBuilder();

            void DPrint(Array obj, int currentIndent = 0) {
                for (var i = 0; i < obj.Length; i++) {
                    var current = obj.GetValue(i);

                    var typeName = (" (" + (current == null ? "null" : current.GetType().Name) + ")").PadRight(16, ' ');

                    sb.AppendLine("".PadRight(currentIndent, ' ') + typeName + ": " + current);

                    if (current != null && current.GetType().IsArray)
                        DPrint((Array)current, currentIndent + 8);
                }
            }

            DPrint(obj);
            Debug.Log(sb.ToString());
        }
#endif

        #region Deserialize

        public static object DeserializeKnownType(byte[] bytes, SerializedTypes type) {
            switch (type) {
                case SerializedTypes.Boolean:
                    return DeserializeBool(bytes);
                case SerializedTypes.SByte:
                    return DeserializeSByte(bytes);
                case SerializedTypes.Byte:
                    return DeserializeByte(bytes);
                case SerializedTypes.UInt16:
                    return DeserializeUInt16(bytes);
                case SerializedTypes.Int16:
                    return DeserializeInt16(bytes);
                case SerializedTypes.UInt32:
                    return DeserializeUInt32(bytes);
                case SerializedTypes.Int32:
                    return DeserializeInt32(bytes);
                case SerializedTypes.UInt64:
                    return DeserializeUInt64(bytes);
                case SerializedTypes.Int64:
                    return DeserializeInt64(bytes);
                case SerializedTypes.Single:
                    return DeserializeSingle(bytes);
                case SerializedTypes.Double:
                    return DeserializeDouble(bytes);
                case SerializedTypes.String:
                    return DeserializeString(bytes);
                case SerializedTypes.VRCPlayerApi:
                    return DeserializeVRCPlayerApi(bytes);
                case SerializedTypes.Color:
                    return DeserializeColor(bytes);
                case SerializedTypes.Color32:
                    return DeserializeColor32(bytes);
                case SerializedTypes.Vector2:
                    return DeserializeVector2(bytes);
                case SerializedTypes.Vector2Int:
                    return DeserializeVector2Int(bytes);
                case SerializedTypes.Vector3:
                    return DeserializeVector3(bytes);
                case SerializedTypes.Vector3Int:
                    return DeserializeVector3Int(bytes);
                case SerializedTypes.Vector4:
                    return DeserializeVector4(bytes);
                case SerializedTypes.Quaternion:
                    return DeserializeQuaternion(bytes);
                case SerializedTypes.DateTime:
                    return DeserializeDateTime(bytes);
                case SerializedTypes.Null:
                case SerializedTypes.None:
                    return null;
                default:
                    return null;
            }
        }

        public static object[] Deserialize(byte[] bytes) {
            var serlSize = DeserializeUInt16(SubArray(bytes, 0, 2));

            var outputObjects = new object[serlSize];
            var byteIndex = 2;

            var objectIndex = 0;

            while (objectIndex < serlSize) {
                var type = (SerializedTypes)DeserializeByte(SubArray(bytes, byteIndex, 1));

                if (type == SerializedTypes.Array) {
                    var arrayLength = DeserializeUInt16(SubArray(bytes, byteIndex + 4, 2));

                    outputObjects[objectIndex] = DeserializeArray(SubArray(bytes, byteIndex + 1, arrayLength));
                    byteIndex += 1 + arrayLength;
                    objectIndex++;
                    continue;
                }

                if (type == SerializedTypes.String) {
                    var stringByteSize = DeserializeUInt16(SubArray(bytes, byteIndex + 1, 2)) + 2;
                    outputObjects[objectIndex] = DeserializeString(SubArray(bytes, byteIndex + 1, stringByteSize));
                    byteIndex += 1 + stringByteSize;
                    objectIndex++;
                    continue;
                }

                var byteSize = GetSizeFromType(type, null);
                outputObjects[objectIndex] = DeserializeKnownType(SubArray(bytes, byteIndex + 1, byteSize - 1), type);
                byteIndex += byteSize;
                objectIndex++;
            }

            return outputObjects;
        }

        public static bool DeserializeBool(byte[] bytes) => bytes[0] > 0;


        public static byte DeserializeByte(byte[] bytes) => bytes[0];


        public static sbyte DeserializeSByte(byte[] bytes) => Convert.ToSByte(bytes[0] + sbyte.MinValue);


        public static short DeserializeInt16(byte[] bytes) {
            var result = bytes[0] + ((bytes[1] & 0x7F) << 8);

            if ((bytes[1] & 0x80) != 0)
                return (short)(short.MinValue + result);
            return (short)result;
        }


        public static int DeserializeInt32(byte[] bytes) {
            var result = bytes[0] +
                         (bytes[1] << 8) +
                         (bytes[2] << 16) +
                         ((bytes[3] & 0x7F) << 24);

            if ((bytes[3] & 0x80) != 0)
                return int.MinValue + result;
            return result;
        }

        public static long DeserializeInt64(byte[] bytes) {
            var result = bytes[0] +
                         (Convert.ToUInt64(bytes[1]) << 8) +
                         (Convert.ToUInt64(bytes[2]) << 16) +
                         (Convert.ToUInt64(bytes[3]) << 24) +
                         (Convert.ToUInt64(bytes[4]) << 32) +
                         (Convert.ToUInt64(bytes[5]) << 40) +
                         (Convert.ToUInt64(bytes[6]) << 48) +
                         (Convert.ToUInt64(bytes[7] & 0x7F) << 56);

            if ((bytes[7] & 0x80) != 0)
                return long.MinValue + (long)result;
            return (long)result;
        }


        public static ushort DeserializeUInt16(byte[] bytes) => (ushort)(bytes[0] + (bytes[1] << 8));


        public static uint DeserializeUInt32(byte[] bytes) =>
            bytes[0] +
            (Convert.ToUInt32(bytes[1]) << 8) +
            (Convert.ToUInt32(bytes[2]) << 16) +
            (Convert.ToUInt32(bytes[3]) << 24);


        public static ulong DeserializeUInt64(byte[] bytes) =>
            bytes[0] +
            (Convert.ToUInt64(bytes[1]) << 8) +
            (Convert.ToUInt64(bytes[2]) << 16) +
            (Convert.ToUInt64(bytes[3]) << 24) +
            (Convert.ToUInt64(bytes[4]) << 32) +
            (Convert.ToUInt64(bytes[5]) << 40) +
            (Convert.ToUInt64(bytes[6]) << 48) +
            (Convert.ToUInt64(bytes[7]) << 56);

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
            var charBuffer = new char[bytes.Length - 2];

            var charIndex = 0;
            var index = 2;
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

        public static VRCPlayerApi DeserializeVRCPlayerApi(byte[] bytes) {
            var playerBits = DeserializeUInt16(bytes);

            if ((playerBits & 0x8000) != 0)
                return default;

            var playerId = (ushort)(playerBits & 0x7FFF);
            return VRCPlayerApi.GetPlayerById(playerId);
        }

        public static Color DeserializeColor(byte[] bytes) {
            var r = DeserializeSingle(SubArray(bytes, 0, 4));
            var g = DeserializeSingle(SubArray(bytes, 4, 4));
            var b = DeserializeSingle(SubArray(bytes, 8, 4));
            var a = DeserializeSingle(SubArray(bytes, 12, 4));

            return new Color(r, g, b, a);
        }

        public static Color32 DeserializeColor32(byte[] bytes) => new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);

        public static Vector2 DeserializeVector2(byte[] bytes) {
            var x = DeserializeSingle(SubArray(bytes, 0, 4));
            var y = DeserializeSingle(SubArray(bytes, 4, 4));

            return new Vector2(x, y);
        }

        public static Vector2Int DeserializeVector2Int(byte[] bytes) {
            var x = DeserializeInt32(SubArray(bytes, 0, 4));
            var y = DeserializeInt32(SubArray(bytes, 4, 4));

            return new Vector2Int(x, y);
        }

        public static Vector3 DeserializeVector3(byte[] bytes) {
            var x = DeserializeSingle(SubArray(bytes, 0, 4));
            var y = DeserializeSingle(SubArray(bytes, 4, 4));
            var z = DeserializeSingle(SubArray(bytes, 8, 4));

            return new Vector3(x, y, z);
        }

        public static Vector3Int DeserializeVector3Int(byte[] bytes) {
            var x = DeserializeInt32(SubArray(bytes, 0, 4));
            var y = DeserializeInt32(SubArray(bytes, 4, 4));
            var z = DeserializeInt32(SubArray(bytes, 8, 4));

            return new Vector3Int(x, y, z);
        }

        public static Vector4 DeserializeVector4(byte[] bytes) {
            var x = DeserializeSingle(SubArray(bytes, 0, 4));
            var y = DeserializeSingle(SubArray(bytes, 4, 4));
            var z = DeserializeSingle(SubArray(bytes, 8, 4));
            var w = DeserializeSingle(SubArray(bytes, 12, 4));

            return new Vector4(x, y, z, w);
        }


        public static Quaternion DeserializeQuaternion(byte[] bytes) {
            var deVec4 = DeserializeVector4(bytes);
            return new Quaternion(
                deVec4.x,
                deVec4.y,
                deVec4.z,
                deVec4.w
            );
        }


        public static DateTime DeserializeDateTime(byte[] bytes) => new DateTime(DeserializeInt64(bytes));


        public static Array DeserializeArray(byte[] bytes) {
            var arrayType = (SerializedTypes)bytes[0];

            if (arrayType == SerializedTypes.Array)
                return null;

            var arrayLength = DeserializeUInt16(SubArray(bytes, 1, 2));
            var byteIndex = 5;


            var newArray = Array.CreateInstance(GetTypeFromSerializedType(arrayType), arrayLength);
            if (arrayType == SerializedTypes.String) {
                for (var i = 0; i < arrayLength; i++) {
                    var stringByteSize = DeserializeUInt16(SubArray(bytes, byteIndex, 2)) + 2;
                    newArray.SetValue(DeserializeString(SubArray(bytes, byteIndex, stringByteSize)), i);

                    byteIndex += stringByteSize;
                }
            }
            else {
                var typeSize = GetSizeFromType(arrayType, null) - 1;
                for (var i = 0; i < arrayLength; i++) {
                    newArray.SetValue(DeserializeKnownType(SubArray(bytes, byteIndex, typeSize), arrayType), i);
                    byteIndex += typeSize;
                }
            }


            return newArray;
        }

        #endregion


        #region Serialize

        public static byte[] SerializeKnownType(object input, SerializedTypes type) {
            switch (type) {
                case SerializedTypes.Boolean:
                    return SerializeBool((bool)input);
                case SerializedTypes.SByte:
                    return SerializeSByte((sbyte)input);
                case SerializedTypes.Byte:
                    return SerializeByte((byte)input);
                case SerializedTypes.UInt16:
                    return SerializeUInt16((ushort)input);
                case SerializedTypes.Int16:
                    return SerializeInt16((short)input);
                case SerializedTypes.UInt32:
                    return SerializeUInt32((uint)input);
                case SerializedTypes.Int32:
                    return SerializeInt32((int)input);
                case SerializedTypes.UInt64:
                    return SerializeUInt64((ulong)input);
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
            }

            var byteArray = new byte[estimatedByteCount + (int)TypeSizes.Int16];
            var serlSize = SerializeUInt16((ushort)input.Length);
            byteArray[0] = serlSize[0];
            byteArray[1] = serlSize[1];

            var byteIndex = 2;

            for (var i = 0; i < input.Length; i++) {
                if (isArray[i]) {
                    byteArray[byteIndex] = (byte)SerializedTypes.Array;
                    var arrayBytes = SerializeArray((Array)input[i], types[i]);
                    var arraySize = arrayBytes.Length;

                    Array.Copy(arrayBytes, 0, byteArray, byteIndex + 1, arraySize);
                    byteIndex += arraySize + 1;
                    continue;
                }

                if (types[i] == SerializedTypes.String) {
                    byteArray[byteIndex] = (byte)SerializedTypes.String;
                    var stringBytes = SerializeString((string)input[i]);
                    var stringSize = stringBytes.Length;
                    Array.Copy(stringBytes, 0, byteArray, byteIndex + 1, stringSize);

                    byteIndex += stringSize + 1;
                    continue;
                }

                byteArray[byteIndex] = (byte)((int)types[i] & 0xFF); // god, why udon
                var byteSize = sizes[i];

                var serializedBytes = SerializeKnownType(input[i], types[i]);
                Array.Copy(serializedBytes, 0, byteArray, byteIndex + 1, byteSize - 1);

                byteIndex += byteSize;
            }

            //Debug.Log($"estimatedByteCount: {estimatedByteCount + 2}, actualMinifiedSize: {byteIndex}");

            var minifiedByteArray = SubArray(byteArray, 0, byteIndex);
            return minifiedByteArray;
        }


        public static byte[] SerializeBool(bool input) => new[] { Convert.ToByte(input) };


        public static byte[] SerializeByte(byte input) => new[] { input };


        public static byte[] SerializeSByte(sbyte input) => new[] { (byte)(input - sbyte.MinValue) };


        public static byte[] SerializeInt16(short input) {
            return new[] {
                Convert.ToByte(input & 0xFF),
                Convert.ToByte((input >> 8) & 0xFF) // | Convert.ToByte(input >= 0 ? 0x0 : 0x80))
            };
        }


        public static byte[] SerializeInt32(int input) {
            return new[] {
                Convert.ToByte(input & 0xFF),
                Convert.ToByte((input >> 8) & 0xFF),
                Convert.ToByte((input >> 16) & 0xFF),
                Convert.ToByte((input >> 24) & 0xFF) //| Convert.ToByte(input >= 0 ? 0x0 : 0x80))
            };
        }


        public static byte[] SerializeInt64(long input) {
            return new[] {
                Convert.ToByte(input & 0xFF),
                Convert.ToByte((input >> 8) & 0xFF),
                Convert.ToByte((input >> 16) & 0xFF),
                Convert.ToByte((input >> 24) & 0xFF),
                Convert.ToByte((input >> 32) & 0xFF),
                Convert.ToByte((input >> 40) & 0xFF),
                Convert.ToByte((input >> 48) & 0xFF),
                Convert.ToByte((input >> 56) & 0xFF)
            };
        }


        public static byte[] SerializeUInt16(ushort input) {
            return new[] {
                (byte)(input & 0xFF),
                (byte)((input >> 8) & 0xFF)
            };
        }


        public static byte[] SerializeUInt32(uint input) {
            return new[] {
                (byte)(input & 0xFF),
                (byte)((input >> 8) & 0xFF),
                (byte)((input >> 16) & 0xFF),
                (byte)((input >> 24) & 0xFF)
            };
        }


        public static byte[] SerializeUInt64(ulong input) {
            return new[] {
                (byte)(input & 0xFF),
                (byte)((input >> 8) & 0xFF),
                (byte)((input >> 16) & 0xFF),
                (byte)((input >> 24) & 0xFF),
                (byte)((input >> 32) & 0xFF),
                (byte)((input >> 40) & 0xFF),
                (byte)((input >> 48) & 0xFF),
                (byte)((input >> 56) & 0xFF)
            };
        }

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

            var newBuff = new byte[buffSize + 2];
            Array.Copy(SerializeUInt16((ushort)buffSize), newBuff, 2); // place array length as uint16 in first
            Array.Copy(buffer, 0, newBuff, 2, buffSize);
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
            Array.Copy(SerializeSingle(input.w), 0, bytes, 12, 4);

            return bytes;
        }


        public static byte[] SerializeQuaternion(Quaternion input) =>
            SerializeVector4(new Vector4(input.x, input.y, input.z, input.w));


        public static byte[] SerializeDateTime(DateTime input) => SerializeInt64(input.Ticks);

        public static byte[] SerializeArray(Array input, SerializedTypes arrayType) {
            if (arrayType == SerializedTypes.Array)
                return new[] { (byte)arrayType };

            var bytes = new byte[GetSizeFromType(arrayType, input, true) - 1];

            bytes[0] = (byte)((int)arrayType & 0xFF);
            Array.Copy(SerializeUInt16((ushort)input.Length), 0, bytes, 1, 2);

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
                Array.Copy(SerializeUInt16((ushort)byteIndex), 0, newByteArray, 3, 2);
                return newByteArray;
            }

            var typeSize = GetSizeFromType(arrayType, input) - 1;
            for (var i = 0; i < input.Length; i++) {
                Array.Copy(SerializeKnownType(input.GetValue(i), arrayType), 0, bytes, byteIndex, typeSize);
                byteIndex += typeSize;
            }

            Array.Copy(SerializeUInt16((ushort)byteIndex), 0, bytes, 3, 2);
            return bytes;
        }

        #endregion
    }
}