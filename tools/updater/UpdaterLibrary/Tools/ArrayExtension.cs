using System.Collections.Generic;

namespace UpdaterLibrary.Tools;
public static class ArrayExtension
{
    public static string ToStringRecurs(this object obj)
    {
        if (obj is object[] array)
            return string.Join(", ", array.Select(item => item?.ToStringRecurs() ?? ""));
        else if (obj is List<object> list)
            return string.Join(", ", list.Select(item => item?.ToStringRecurs() ?? ""));
        else return obj.ToString() ?? "";
    }
}
