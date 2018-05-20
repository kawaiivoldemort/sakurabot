using System;
using System.Text;

namespace OpenDotaApi
{
    public static class AccessoryFunctions
    {
        public static string CenterText(string text, int length)
        {
            int spaces = length - text.Length;
            int padLeft = spaces/2 + text.Length;
            return text.PadLeft(padLeft).PadRight(length);
        }
    }
}