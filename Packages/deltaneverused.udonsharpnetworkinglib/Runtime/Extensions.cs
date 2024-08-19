using System;

namespace UdonSharpNetworkingLib {
    public static class Extensions {
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