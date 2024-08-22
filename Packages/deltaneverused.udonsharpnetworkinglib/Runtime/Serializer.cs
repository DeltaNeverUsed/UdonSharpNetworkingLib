using System;
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using Debug = UnityEngine.Debug;
using Type = System.Type;


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
        public static T[] SubArray<T>(T[] array, int startIndex, int length) {
            var subArray = new T[length];
            Array.Copy(array, startIndex, subArray, 0, length);
            return subArray;
        }

        public static T[] ResizeArray<T>(T[] array, int newSize, int copyLen) {
            var tempArray = new T[newSize];
            Array.Copy(array, tempArray, copyLen);
            return tempArray;
        }

        public static T[] ConvertAll<T, U>(Array array) {
            var newArray = new T[array.Length];
            for (var i = 0; i < array.Length; i++) {
                newArray[i] = (T)array.GetValue(i);
            }

            return newArray;
        }

        public static int GetSizeFromType(SerializedTypes type, object input) {
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

            if (type == SerializedTypes.String)
                return ((string)input).Length * 4 + (int)TypeSizes.Byte;
            return (int)map[(int)type] + (int)TypeSizes.Byte;
        }

        // ReSharper disable once PossibleNullReferenceException
        public static bool IsArray(object input) => Utilities.IsValid(input) && input.GetType().Name.EndsWith("[]");

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

        public static SerializedTypes GetSerializedType(object input, bool isArray) {
            if (!Utilities.IsValid(input))
                return SerializedTypes.Null;

            var type = input.GetType();
            var serializedType = SerializedTypes.Null;

            if (isArray) {
                if (type == typeof(object[]))
                    serializedType = SerializedTypes.Array;
                else if (type == typeof(bool[]))
                    serializedType = SerializedTypes.Boolean;
                else if (type == typeof(byte[]))
                    serializedType = SerializedTypes.Byte;
                else if (type == typeof(sbyte[]))
                    serializedType = SerializedTypes.SByte;
                else if (type == typeof(short[]))
                    serializedType = SerializedTypes.Int16;
                else if (type == typeof(ushort[]))
                    serializedType = SerializedTypes.UInt16;
                else if (type == typeof(int[]))
                    serializedType = SerializedTypes.Int32;
                else if (type == typeof(uint[]))
                    serializedType = SerializedTypes.UInt32;
                else if (type == typeof(long[]))
                    serializedType = SerializedTypes.Int64;
                else if (type == typeof(ulong[]))
                    serializedType = SerializedTypes.UInt64;
                else if (type == typeof(float[]))
                    serializedType = SerializedTypes.Single;
                else if (type == typeof(double[]))
                    serializedType = SerializedTypes.Double;
                else if (type == typeof(string[]))
                    serializedType = SerializedTypes.String;
                else if (type == typeof(VRCPlayerApi[]))
                    serializedType = SerializedTypes.VRCPlayerApi;
                else if (type == typeof(Color[]))
                    serializedType = SerializedTypes.Color;
                else if (type == typeof(Color32[]))
                    serializedType = SerializedTypes.Color32;
                else if (type == typeof(Vector2[]))
                    serializedType = SerializedTypes.Vector2;
                else if (type == typeof(Vector2Int[]))
                    serializedType = SerializedTypes.Vector2Int;
                else if (type == typeof(Vector3[]))
                    serializedType = SerializedTypes.Vector3;
                else if (type == typeof(Vector3Int[]))
                    serializedType = SerializedTypes.Vector3Int;
                else if (type == typeof(Vector4[]))
                    serializedType = SerializedTypes.Vector4;
                else if (type == typeof(Quaternion[]))
                    serializedType = SerializedTypes.Quaternion;
                else if (type == typeof(DateTime[]))
                    serializedType = SerializedTypes.DateTime;
                else
                    Debug.LogError($"unsupported type: {type.FullName}, falling back to null.");
            }
            else {
                if (type == typeof(object[]))
                serializedType = SerializedTypes.Array;
            else if (type == typeof(bool))
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
                Debug.LogError($"unsupported type: {type.FullName}, falling back to null.");
            }

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

        public static object DeserializeKnownType(byte[] bytes, SerializedTypes type, int startIndex) {
            switch (type) {
                case SerializedTypes.Boolean:
                    return DeserializeBool(bytes, startIndex);
                case SerializedTypes.SByte:
                    return DeserializeSByte(bytes, startIndex);
                case SerializedTypes.Byte:
                    return DeserializeByte(bytes, startIndex);
                case SerializedTypes.UInt16:
                    return DeserializeUInt16(bytes, startIndex);
                case SerializedTypes.Int16:
                    return DeserializeInt16(bytes, startIndex);
                case SerializedTypes.UInt32:
                    return DeserializeUInt32(bytes, startIndex);
                case SerializedTypes.Int32:
                    return DeserializeInt32(bytes, startIndex);
                case SerializedTypes.UInt64:
                    return DeserializeUInt64(bytes, startIndex);
                case SerializedTypes.Int64:
                    return DeserializeInt64(bytes, startIndex);
                case SerializedTypes.Single:
                    return DeserializeSingle(bytes, startIndex);
                case SerializedTypes.Double:
                    return DeserializeDouble(bytes, startIndex);
                case SerializedTypes.String:
                    return DeserializeString(bytes, startIndex);
                case SerializedTypes.VRCPlayerApi:
                    return DeserializeVRCPlayerApi(bytes, startIndex);
                case SerializedTypes.Color:
                    return DeserializeColor(bytes, startIndex);
                case SerializedTypes.Color32:
                    return DeserializeColor32(bytes, startIndex);
                case SerializedTypes.Vector2:
                    return DeserializeVector2(bytes, startIndex);
                case SerializedTypes.Vector2Int:
                    return DeserializeVector2Int(bytes, startIndex);
                case SerializedTypes.Vector3:
                    return DeserializeVector3(bytes, startIndex);
                case SerializedTypes.Vector3Int:
                    return DeserializeVector3Int(bytes, startIndex);
                case SerializedTypes.Vector4:
                    return DeserializeVector4(bytes, startIndex);
                case SerializedTypes.Quaternion:
                    return DeserializeQuaternion(bytes, startIndex);
                case SerializedTypes.DateTime:
                    return DeserializeDateTime(bytes, startIndex);
                case SerializedTypes.Null:
                case SerializedTypes.None:
                    return null;
                default:
                    return null;
            }
        }

        public static T[] Deserialize<T>(byte[] bytes) {
            return ConvertAll<T, object>(Deserialize(bytes));
        }

        [RecursiveMethod]
        public static object[] Deserialize(byte[] bytes) {
            var serlSize = DeserializeUInt16(bytes, 0);

            var outputObjects = new object[serlSize];
            var byteIndex = 2;

            var objectIndex = 0;

            while (objectIndex < serlSize) {
                var type = (SerializedTypes)DeserializeByte(bytes, byteIndex);

                if (type == SerializedTypes.Array) {
                    var arrayByteSize = DeserializeUInt16(bytes, byteIndex + 1);
                    var arrayBytes = SubArray(bytes, byteIndex + 3, arrayByteSize);
                    outputObjects[objectIndex] = Deserialize(arrayBytes);

                    byteIndex += 3 + arrayByteSize;
                    objectIndex++;
                    continue;
                }

                if (type == SerializedTypes.String) {
                    var stringByteSize = DeserializeUInt16(bytes, byteIndex + 1) + 2;
                    outputObjects[objectIndex] = DeserializeString(bytes, byteIndex + 1);
                    byteIndex += 1 + stringByteSize;
                    objectIndex++;
                    continue;
                }

                var byteSize = GetSizeFromType(type, null);
                outputObjects[objectIndex] = DeserializeKnownType(bytes, type, byteIndex + 1);
                byteIndex += byteSize;
                objectIndex++;
            }

            return outputObjects;
        }

        public static bool DeserializeBool(byte[] bytes, int startIndex) =>
            BitConverter.ToBoolean(bytes, startIndex);


        public static byte DeserializeByte(byte[] bytes, int startIndex) => bytes[startIndex];


        public static sbyte DeserializeSByte(byte[] bytes, int startIndex) =>
            Convert.ToSByte(bytes[startIndex] + sbyte.MinValue);


        public static short DeserializeInt16(byte[] bytes, int startIndex) =>
            BitConverter.ToInt16(bytes, startIndex);

        public static int DeserializeInt32(byte[] bytes, int startIndex) => BitConverter.ToInt32(bytes, startIndex);

        public static long DeserializeInt64(byte[] bytes, int startIndex) =>
            BitConverter.ToInt64(bytes, startIndex);

        public static ushort DeserializeUInt16(byte[] bytes, int startIndex) =>
            BitConverter.ToUInt16(bytes, startIndex);

        public static uint DeserializeUInt32(byte[] bytes, int startIndex) =>
            BitConverter.ToUInt32(bytes, startIndex);

        public static ulong DeserializeUInt64(byte[] bytes, int startIndex) =>
            BitConverter.ToUInt64(bytes, startIndex);

        public static float DeserializeSingle(byte[] bytes, int startIndex) =>
            BitConverter.ToSingle(bytes, startIndex);

        public static double DeserializeDouble(byte[] bytes, int startIndex) =>
            BitConverter.ToDouble(bytes, startIndex);

        public static string DeserializeString(byte[] bytes, int startIndex) {
            var stringByteSize = DeserializeUInt16(bytes, startIndex) + 2;
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

        public static VRCPlayerApi DeserializeVRCPlayerApi(byte[] bytes, int startIndex) {
            var playerBits = DeserializeUInt16(bytes, startIndex);

            if ((playerBits & 0x8000) != 0)
                return default;

            var playerId = (ushort)(playerBits & 0x7FFF);
            return VRCPlayerApi.GetPlayerById(playerId);
        }

        public static Color DeserializeColor(byte[] bytes, int startIndex) {
            var r = DeserializeSingle(bytes, startIndex);
            var g = DeserializeSingle(bytes, startIndex + 4);
            var b = DeserializeSingle(bytes, startIndex + 8);
            var a = DeserializeSingle(bytes, startIndex + 12);

            return new Color(r, g, b, a);
        }

        public static Color32 DeserializeColor32(byte[] bytes, int startIndex) =>
            new Color32(bytes[0], bytes[1], bytes[2], bytes[3]);

        public static Vector2 DeserializeVector2(byte[] bytes, int startIndex) {
            var x = DeserializeSingle(bytes, startIndex);
            var y = DeserializeSingle(bytes, startIndex + 4);

            return new Vector2(x, y);
        }

        public static Vector2Int DeserializeVector2Int(byte[] bytes, int startIndex) {
            var x = DeserializeInt32(bytes, startIndex);
            var y = DeserializeInt32(bytes, startIndex + 4);

            return new Vector2Int(x, y);
        }

        public static Vector3 DeserializeVector3(byte[] bytes, int startIndex) {
            var x = DeserializeSingle(bytes, startIndex);
            var y = DeserializeSingle(bytes, startIndex + 4);
            var z = DeserializeSingle(bytes, startIndex + 8);

            return new Vector3(x, y, z);
        }

        public static Vector3Int DeserializeVector3Int(byte[] bytes, int startIndex) {
            var x = DeserializeInt32(bytes, startIndex);
            var y = DeserializeInt32(bytes, startIndex + 4);
            var z = DeserializeInt32(bytes, startIndex + 8);

            return new Vector3Int(x, y, z);
        }

        public static Vector4 DeserializeVector4(byte[] bytes, int startIndex) {
            var x = DeserializeSingle(bytes, startIndex);
            var y = DeserializeSingle(bytes, startIndex + 4);
            var z = DeserializeSingle(bytes, startIndex + 8);
            var w = DeserializeSingle(bytes, startIndex + 12);

            return new Vector4(x, y, z, w);
        }


        public static Quaternion DeserializeQuaternion(byte[] bytes, int startIndex) {
            var deVec4 = DeserializeVector4(bytes, startIndex);
            return new Quaternion(
                deVec4.x,
                deVec4.y,
                deVec4.z,
                deVec4.w
            );
        }


        public static DateTime DeserializeDateTime(byte[] bytes, int startIndex) =>
            new DateTime(DeserializeInt64(bytes, startIndex));

        #endregion


        #region Serialize

        public static byte[] SerializeKnownType(object input, SerializedTypes type) {
            switch (type) {
                case SerializedTypes.Array:
                    return Serialize((Array)input);
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

        [RecursiveMethod]
        public static byte[] Serialize(Array input) {
            var inputSize = input.Length;
            
            var currentArrayLen = 512;
            var arrayIncSize = 512;
            var byteArray = new byte[currentArrayLen];
            var serlSize = SerializeUInt16((ushort)inputSize);
            byteArray[0] = serlSize[0];
            byteArray[1] = serlSize[1];

            var byteIndex = 2;

            for (var i = 0; i < inputSize; i++) {
                var arrayFreeSpace = currentArrayLen - byteIndex;
                
                var currentObject = input.GetValue(i);
                var isArray = IsArray(currentObject);
                
                if (isArray) {
                    var arrayBytes = Serialize((Array)currentObject);
                    var arraySize = arrayBytes.Length;

                    // Resize byte array to fit new array
                    var diff = arraySize - arrayFreeSpace;
                    if (diff > -5) {
                        currentArrayLen += arrayIncSize * (int)Mathf.Ceil(diff / (float)arrayIncSize);
                        byteArray = ResizeArray(byteArray, currentArrayLen, byteIndex);
                    }

                    // Serialized header info
                    byteArray[byteIndex] = (byte)SerializedTypes.Array;

                    var serializedArraySize = SerializeUInt16((ushort)arraySize);
                    byteArray[byteIndex + 1] = serializedArraySize[0];
                    byteArray[byteIndex + 2] = serializedArraySize[1];

                    Array.Copy(arrayBytes, 0, byteArray, byteIndex + 3, arraySize);
                    byteIndex += arraySize + 3;
                    continue;
                }
                
                var type = GetSerializedType(currentObject, false);

                if (type == SerializedTypes.String) {
                    byteArray[byteIndex] = (byte)SerializedTypes.String;
                    var stringBytes = SerializeString((string)currentObject);
                    var stringSize = stringBytes.Length;
                    
                    var diff = stringSize - arrayFreeSpace;
                    if (diff < -2) {
                        currentArrayLen += arrayIncSize * (int)Mathf.Ceil(diff / (float)arrayIncSize);
                        byteArray = ResizeArray(byteArray, currentArrayLen, byteIndex);
                    }
                    
                    Array.Copy(stringBytes, 0, byteArray, byteIndex + 1, stringSize);

                    byteIndex += stringSize + 1;
                    continue;
                }

                var serializedBytes = SerializeKnownType(currentObject, type);
                var byteSize = serializedBytes.Length + 1;
                
                if (arrayFreeSpace < byteSize) {
                    currentArrayLen += arrayIncSize;
                    byteArray = ResizeArray(byteArray, currentArrayLen, byteIndex);
                }
                
                byteArray[byteIndex] = (byte)((int)type & 0xFF); // god, why udon
                
                Array.Copy(serializedBytes, 0, byteArray, byteIndex + 1, byteSize - 1);
                byteIndex += byteSize;
            }

            return SubArray(byteArray, 0, byteIndex);
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
            var stringSize = SerializeUInt16((ushort)buffSize);
            newBuff[0] = stringSize[0];
            newBuff[1] = stringSize[1];
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

        #endregion
    }
}