using System;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace USPPNet {
    public static class USFuncs {
        public static object[] USPPNet_ParseParams<T>(params T[] args) {
            var tempArr = new object[args.Length * 2];


            return tempArr;
        }
    }

    public static class Extensions {
        public static T[] USPPNet_ConcatArray<T>(this T[] array, T[] items) {
            var arr1_len = array.Length;
            var arr2_len = items.Length;
            var temp_arr = new T[arr1_len + arr2_len];

            Array.Copy(array, temp_arr, arr1_len);
            Array.Copy(items, 0, temp_arr, arr1_len, arr2_len);

            return temp_arr;
        }

        [ItemCanBeNull]
        public static T[] USPPNet_AppendArray<T>(this T[] array, T item) {
            return array.USPPNet_ConcatArray(new[] { item });
        }

        public static int USPPNet_IndexOf<T>(this T[] array, T needle) {
            for (var i = 0; i < array.Length; i++)
                if (array[i].Equals(needle))
                    return i;

            return -1;
        }

        public static bool IsArraySame(Array arr1, Array arr2) {
            if (arr1.Length != arr2.Length)
                return false;
            for (var i = 0; i < arr1.Length; i++)
                if (!arr1.GetValue(i).Equals(arr2.GetValue(i)))
                    return false;
            return true;
        }

        public static string StringJoin(string joiner, Array arr) {
            if (arr.Length < 1)
                return "";
            var tempString = arr.GetValue(arr.Length - 1).ToString();
            for (var i = arr.Length - 2; i >= 0; i--)
                tempString = arr.GetValue(i) + ", " + tempString;
            return tempString;
        }
    }
}