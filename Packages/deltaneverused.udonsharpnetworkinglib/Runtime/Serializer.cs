using System;
using System.Text;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;

namespace UdonSharpNetworkingLib {
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

        public static object DeserializeKnownType(ref byte[] bytes, SerializedTypes type, int startIndex) {
            switch (type) {
                case SerializedTypes.Boolean:
                    return DeserializeBool(ref bytes, startIndex);
                case SerializedTypes.SByte:
                    return DeserializeSByte(ref bytes, startIndex);
                case SerializedTypes.Byte:
                    return DeserializeByte(ref bytes, startIndex);
                case SerializedTypes.UInt16:
                    return DeserializeUInt16(ref bytes, startIndex);
                case SerializedTypes.Int16:
                    return DeserializeInt16(ref bytes, startIndex);
                case SerializedTypes.UInt32:
                    return DeserializeUInt32(ref bytes, startIndex);
                case SerializedTypes.Int32:
                    return DeserializeInt32(ref bytes, startIndex);
                case SerializedTypes.UInt64:
                    return DeserializeUInt64(ref bytes, startIndex);
                case SerializedTypes.Int64:
                    return DeserializeInt64(ref bytes, startIndex);
                case SerializedTypes.Single:
                    return DeserializeSingle(ref bytes, startIndex);
                case SerializedTypes.Double:
                    return DeserializeDouble(ref bytes, startIndex);
                case SerializedTypes.String:
                    return DeserializeString(ref bytes, startIndex);
                case SerializedTypes.VRCPlayerApi:
                    return DeserializeVRCPlayerApi(ref bytes, startIndex);
                case SerializedTypes.Color:
                    return DeserializeColor(ref bytes, startIndex);
                case SerializedTypes.Color32:
                    return DeserializeColor32(ref bytes, startIndex);
                case SerializedTypes.Vector2:
                    return DeserializeVector2(ref bytes, startIndex);
                case SerializedTypes.Vector2Int:
                    return DeserializeVector2Int(ref bytes, startIndex);
                case SerializedTypes.Vector3:
                    return DeserializeVector3(ref bytes, startIndex);
                case SerializedTypes.Vector3Int:
                    return DeserializeVector3Int(ref bytes, startIndex);
                case SerializedTypes.Vector4:
                    return DeserializeVector4(ref bytes, startIndex);
                case SerializedTypes.Quaternion:
                    return DeserializeQuaternion(ref bytes, startIndex);
                case SerializedTypes.DateTime:
                    return DeserializeDateTime(ref bytes, startIndex);
                case SerializedTypes.Null:
                case SerializedTypes.None:
                    return null;
                default:
                    return null;
            }
        }

        public static object[] Deserialize(ref byte[] bytes) {
            var serlSize = DeserializeUInt16(ref bytes, 0);

            var outputObjects = new object[serlSize];
            var byteIndex = 2;

            var objectIndex = 0;

            while (objectIndex < serlSize) {
                var type = (SerializedTypes)DeserializeByte(ref bytes, byteIndex);

                if (type == SerializedTypes.Array) {
                    var arrayLength = DeserializeUInt16(ref bytes, byteIndex + 4);

                    outputObjects[objectIndex] = DeserializeArray(ref bytes, byteIndex + 1);
                    byteIndex += 1 + arrayLength;
                    objectIndex++;
                    continue;
                }

                if (type == SerializedTypes.String) {
                    var stringByteSize = DeserializeUInt16(ref bytes, byteIndex + 1) + 2;
                    outputObjects[objectIndex] = DeserializeString(ref bytes, byteIndex + 1);
                    byteIndex += 1 + stringByteSize;
                    objectIndex++;
                    continue;
                }

                var byteSize = GetSizeFromType(type, null);
                outputObjects[objectIndex] = DeserializeKnownType(ref bytes, type, byteIndex + 1);
                byteIndex += byteSize;
                objectIndex++;
            }

            return outputObjects;
        }

        public static bool DeserializeBool(ref byte[] bytes, int startIndex) => BitConverter.ToBoolean(bytes, startIndex);


        public static byte DeserializeByte(ref byte[] bytes, int startIndex) => bytes[startIndex];


        public static sbyte DeserializeSByte(ref byte[] bytes, int startIndex) => Convert.ToSByte(bytes[startIndex] + sbyte.MinValue);


        public static short DeserializeInt16(ref byte[] bytes, int startIndex) => BitConverter.ToInt16(bytes, startIndex);
        
        public static int DeserializeInt32(ref byte[] bytes, int startIndex) => BitConverter.ToInt32(bytes, startIndex);

        public static long DeserializeInt64(ref byte[] bytes, int startIndex) => BitConverter.ToInt64(bytes, startIndex);
        
        public static ushort DeserializeUInt16(ref byte[] bytes, int startIndex) => BitConverter.ToUInt16(bytes, startIndex);
        
        public static uint DeserializeUInt32(ref byte[] bytes, int startIndex) => BitConverter.ToUInt32(bytes, startIndex);
        
        public static ulong DeserializeUInt64(ref byte[] bytes, int startIndex) => BitConverter.ToUInt64(bytes, startIndex);

        public static float DeserializeSingle(ref byte[] bytes, int startIndex) => BitConverter.ToSingle(bytes, startIndex);
        
        public static double DeserializeDouble(ref byte[] bytes, int startIndex) => BitConverter.ToDouble(bytes, startIndex);

        public static string DeserializeString(ref byte[] bytes, int startIndex) {
            var stringByteSize = DeserializeUInt16(ref bytes, startIndex) + 2;
            var stopIndex = startIndex + stringByteSize;
            var charBuffer = new char[stringByteSize];

            var charIndex = 0;
            var index = startIndex + 2;
            while (index < stopIndex) {
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

            var newBuff = new char[charIndex];
            Array.Copy(charBuffer, newBuff, charIndex);

            return new string(newBuff);
        }

        public static VRCPlayerApi DeserializeVRCPlayerApi(ref byte[] bytes, int startIndex) {
            var playerBits = DeserializeUInt16(ref bytes, startIndex);

            if ((playerBits & 0x8000) != 0)
                return default;

            var playerId = (ushort)(playerBits & 0x7FFF);
            return VRCPlayerApi.GetPlayerById(playerId);
        }

        public static Color DeserializeColor(ref byte[] bytes, int startIndex) {
            var r = DeserializeSingle(ref bytes, startIndex);
            var g = DeserializeSingle(ref bytes, startIndex + 4);
            var b = DeserializeSingle(ref bytes, startIndex + 8);
            var a = DeserializeSingle(ref bytes, startIndex + 12);

            return new Color(r, g, b, a);
        }

        public static Color32 DeserializeColor32(ref byte[] bytes, int startIndex) => new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);

        public static Vector2 DeserializeVector2(ref byte[] bytes, int startIndex) {
            var x = DeserializeSingle(ref bytes, startIndex);
            var y = DeserializeSingle(ref bytes, startIndex + 4);

            return new Vector2(x, y);
        }

        public static Vector2Int DeserializeVector2Int(ref byte[] bytes, int startIndex) {
            var x = DeserializeInt32(ref bytes, startIndex);
            var y = DeserializeInt32(ref bytes, startIndex + 4);

            return new Vector2Int(x, y);
        }

        public static Vector3 DeserializeVector3(ref byte[] bytes, int startIndex) {
            var x = DeserializeSingle(ref bytes, startIndex);
            var y = DeserializeSingle(ref bytes, startIndex + 4);
            var z = DeserializeSingle(ref bytes, startIndex + 8);

            return new Vector3(x, y, z);
        }

        public static Vector3Int DeserializeVector3Int(ref byte[] bytes, int startIndex) {
            var x = DeserializeInt32(ref bytes, startIndex);
            var y = DeserializeInt32(ref bytes, startIndex + 4);
            var z = DeserializeInt32(ref bytes, startIndex + 8);

            return new Vector3Int(x, y, z);
        }

        public static Vector4 DeserializeVector4(ref byte[] bytes, int startIndex) {
            var x = DeserializeSingle(ref bytes, startIndex);
            var y = DeserializeSingle(ref bytes, startIndex + 4);
            var z = DeserializeSingle(ref bytes, startIndex + 8);
            var w = DeserializeSingle(ref bytes, startIndex + 12);

            return new Vector4(x, y, z, w);
        }


        public static Quaternion DeserializeQuaternion(ref byte[] bytes, int startIndex) {
            var deVec4 = DeserializeVector4(ref bytes, startIndex);
            return new Quaternion(
                deVec4.x,
                deVec4.y,
                deVec4.z,
                deVec4.w
            );
        }


        public static DateTime DeserializeDateTime(ref byte[] bytes, int startIndex) => new DateTime(DeserializeInt64(ref bytes, startIndex));


        public static Array DeserializeArray(ref byte[] bytes, int startIndex) {
            var arrayType = (SerializedTypes)bytes[startIndex];

            if (arrayType == SerializedTypes.Array)
                return null;

            var arrayLength = DeserializeUInt16(ref bytes, startIndex + 1);
            var byteIndex = 5;

            var newArray = Array.CreateInstance(GetTypeFromSerializedType(arrayType), arrayLength);
            if (arrayType == SerializedTypes.String) {
                for (var i = 0; i < arrayLength; i++) {
                    var stringByteSize = DeserializeUInt16(ref bytes, startIndex + byteIndex) + 2;
                    newArray.SetValue(DeserializeString(ref bytes, startIndex + byteIndex), i);

                    byteIndex += stringByteSize;
                }
            }
            else {
                var typeSize = GetSizeFromType(arrayType, null) - 1;
                for (var i = 0; i < arrayLength; i++) {
                    newArray.SetValue(DeserializeKnownType(ref bytes, arrayType, startIndex + byteIndex), i);
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


        public static byte[] SerializeBool(bool input) => BitConverter.GetBytes(input);


        public static byte[] SerializeByte(byte input) => new[] { input };


        public static byte[] SerializeSByte(sbyte input) => new[] { (byte)(input - sbyte.MinValue) };


        public static byte[] SerializeInt16(short input) => BitConverter.GetBytes(input);
        
        public static byte[] SerializeInt32(int input) => BitConverter.GetBytes(input);


        public static byte[] SerializeInt64(long input) => BitConverter.GetBytes(input);
        
        public static byte[] SerializeUInt16(ushort input) => BitConverter.GetBytes(input);

        public static byte[] SerializeUInt32(uint input) => BitConverter.GetBytes(input);
        
        public static byte[] SerializeUInt64(ulong input) => BitConverter.GetBytes(input);

        public static byte[] SerializeSingle(float input) => BitConverter.GetBytes(input);

        public static byte[] SerializeDouble(double input) => BitConverter.GetBytes(input);
        
        public static byte[] SerializeString(string inputString) {
            var buffSize = 0;
            var buffer = new byte[inputString.Length * 4];

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