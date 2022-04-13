using System;

namespace Lab21_AaDS
{
    class Program
    {
        static void Main(string[] args)
        {
            var dic = new NotADictionary<string, int>();
            dic.Add("srenk", 0);
            dic.Add("srenk1", 1);
            dic.Add("srenk5", 5);
            dic.Add("pooq", 999);
            dic.Add("srenk2", 2);
            dic.Add("srenk4", 4);
            dic.Add("srenk3", 3);
            Console.WriteLine(dic);
        }
    }
}
