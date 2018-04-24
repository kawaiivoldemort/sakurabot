using System;

namespace Sakura.Uwu.Common
{
    public static class Accessories
    {
        public static void PrintArray(object[] arr)
        {
            Console.Write("{ ");
            foreach (var obj in arr)
            {
                if (obj != null)
                {
                    Console.Write("{0} ", obj.ToString());
                }
            }
            Console.WriteLine(" }");
        }
    }
}