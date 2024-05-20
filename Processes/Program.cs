using System;
using System.Diagnostics;
using System.IO;

namespace Processes
{
	internal class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 1)
			{
				if (args[0] == "--version" || args[0] == "-v")
				{
					string winProfile = Environment.GetEnvironmentVariable("USERPROFILE");
					string version = File.ReadAllText($"{winProfile}\\source\\repos\\Processes\\Processes\\version.txt");
					Console.WriteLine(version);
				}
			}

			else if (args.Length >= 2)
			{
				if (args[0] == "--find")
				{
					Process[] processes = Process.GetProcesses();
					Process[] foundProcesses = Array.FindAll(processes,
						(Process elem) => elem.ProcessName.ToLower().Contains(args[1].ToLower()));

					if (foundProcesses.Length > 0)
					{
						Console.WriteLine("Found processes:");
						foreach (Process process in foundProcesses)
							Console.WriteLine($"Name: {process.ProcessName}, ID: {process.Id}");
					}
					else
						Console.WriteLine($"No processes found that contains {args[1]} in the name");
				}
				else if (args[0] == "--find-specific")
				{
					Process[] processes = Process.GetProcessesByName(args[1]);

					if (processes.Length > 0)
					{
						Console.WriteLine($"Found processes:");
						foreach (Process process in processes)
							Console.WriteLine($"Name: {process.ProcessName}, ID: {process.Id}");

					}
					else
					{
						Console.WriteLine($"No processes found with name {args[1]}");
						if (args.Length == 3 && args[2] == "--start")
						{
							Console.WriteLine("Starting process...");
							Process.Start(args[1]);
							Console.WriteLine("Process started");
						}
					}


				}
				else if (args[0] == "--kill")
				{
					Process[] processes = Process.GetProcessesByName(args[1]);

					int killedProcess = 0;
					foreach (Process process in processes)
					{
						process.Kill();
						killedProcess++;
					}
					Console.WriteLine($"Killed {killedProcess} processes");
				}
			}
			else
				Console.WriteLine($"Invalid syntax: {string.Join(" ", args)}");

			#if DEBUG
				Console.ReadLine();
			#endif
		}
	}
}
