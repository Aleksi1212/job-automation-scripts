using System;
using System.IO;

namespace SetDotEnv
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = "C:\\Users\\A505471\\source\\repos\\MFClientAppDeploymentAutomation\\MFClientAppDeploymentAutomation\\.env";

            if (args.Length > 0)
            {
                string varName = args[0];
                string varData = args[1];

                if (args.Length == 3)
                {
                    path = args[2];
                }

                try
                {
                    File.WriteAllText(path, $"{varName}={varData}");
                    Console.WriteLine($"Dotenv updated at: {path}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            else
            {
                Console.WriteLine("No arguments provided");
            }
        }
    }
}
