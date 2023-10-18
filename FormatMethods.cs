using System.Runtime.InteropServices;
using static HungerGames.Helpers.Extensions;

namespace HungerGames.Formatters;

public static class StringFormatter
{
    public unsafe static string Format(string input, params object[] __f)
    {
        char* p = (char*)Marshal.AllocHGlobal((input.Length + 1) * sizeof(char));

        fixed (char* i = input)
        {
            foreach (var j in 1..__f.Length)
            {
                int index = input.IndexOf($"{{{j - 1}}}");
                if (index == -1)
                    continue;
                int length = input.Substring(index).IndexOf('}') - 1;
                if (length == -1)
                    continue;
                var s = __f[j - 1].ToString();
                for (int k = 0; k < s.Length; k++)
                {
                    p[index + k] = s[k];
                }
            }
        }

        return default!;
    }
}