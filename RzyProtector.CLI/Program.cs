using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using Confuser.Core;
using Confuser.Core.Project;
using NDesk.Options;

namespace RzyProtector.CLI
{
	internal class Program
	{

		public static string Base64Encode(string plainText)
		{
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
			return System.Convert.ToBase64String(plainTextBytes);
		}

		public static string Base64Decode(string base64EncodedData)
		{
			var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
			return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
		}
		static int Main(string[] args)
		{

			TrinitySeal.Seal.Secret = "UhS0Nbp7G0a2WVopuilnkY4tQMOgSwYd0swX4Q9sYjo2j";
		
				
					bool response = TrinitySeal.Seal.Login("test", "test", false);
					if (response)
					{
						
					}
					else
					{
						Console.WriteLine("Server don't return the same response");
					}

				

			


			ConsoleColor original = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.White;
			string originalTitle = Console.Title;
			Console.Title = "";
			try
			{
				bool noPause = false;
				bool debug = false;
				string outDir = null;
				List<string> probePaths = new List<string>();
				List<string> plugins = new List<string>();
				var p = new OptionSet {
					{
						"n|nopause", "no pause after finishing protection.",
						value => { noPause = (value != null); }
					}, {
						"o|out=", "specifies output directory.",
						value => { outDir = value; }
					}, {
						"probe=", "specifies probe directory.",
						value => { probePaths.Add(value); }
					}, {
						"plugin=", "specifies plugin path.",
						value => { plugins.Add(value); }
					}, {
						"debug", "specifies debug symbol generation.",
						value => { debug = (value != null); }
					}
				};

				List<string> files;
				try
				{
					files = p.Parse(args);
					if (files.Count == 0)
						throw new ArgumentException("No input files specified.");
				}
				catch (Exception ex)
				{
					Console.Write("RzyProtector CLI: ");
					Console.WriteLine(ex.Message);
					PrintUsage();
					return -1;
				}

				var parameters = new ConfuserParameters();

				if (files.Count == 1 && Path.GetExtension(files[0]) == ".crproj")
				{
					var proj = new ConfuserProject();
					try
					{
						var xmlDoc = new XmlDocument();
						xmlDoc.Load(files[0]);
						proj.Load(xmlDoc);
						proj.BaseDirectory = Path.Combine(Path.GetDirectoryName(files[0]), proj.BaseDirectory);
					}
					catch (Exception ex)
					{
						WriteLineWithColor(ConsoleColor.Red, "Failed to load project:");
						WriteLineWithColor(ConsoleColor.Red, ex.ToString());
						return -1;
					}

					parameters.Project = proj;
				}
				else
				{
					if (string.IsNullOrEmpty(outDir))
					{
						Console.WriteLine("RzyProtector CLI: No output directory specified.");
						PrintUsage();
						return -1;
					}

					var proj = new ConfuserProject();

					if (Path.GetExtension(files[files.Count - 1]) == ".crproj")
					{
						var templateProj = new ConfuserProject();
						var xmlDoc = new XmlDocument();
						xmlDoc.Load(files[files.Count - 1]);
						templateProj.Load(xmlDoc);
						files.RemoveAt(files.Count - 1);

						foreach (var rule in templateProj.Rules)
							proj.Rules.Add(rule);
					}

					// Generate a ConfuserProject for input modules
					// Assuming first file = main module
					foreach (var input in files)
						proj.Add(new ProjectModule { Path = input });

					proj.BaseDirectory = Path.GetDirectoryName(files[0]);
					proj.OutputDirectory = outDir;
					foreach (var path in probePaths)
						proj.ProbePaths.Add(path);
					foreach (var path in plugins)
						proj.PluginPaths.Add(path);
					proj.Debug = debug;
					parameters.Project = proj;
				}

				int retVal = RunProject(parameters);

				if (NeedPause() && !noPause)
				{
					Console.WriteLine("Press any key to continue...");
					Console.ReadKey(true);
				}

				return retVal;
			}
			finally
			{
				Console.ForegroundColor = original;
			}
		}

		static int RunProject(ConfuserParameters parameters)
		{
			var logger = new ConsoleLogger();
			parameters.Logger = logger;

			ConfuserEngine.Run(parameters).Wait();

			return logger.ReturnValue;
		}

		static bool NeedPause()
		{
			return Debugger.IsAttached || string.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROMPT"));
		}

		static void PrintUsage()
		{
			WriteLine("Usage:");
			WriteLine("RzyProtector CLI -n|noPause <project configuration>");
			WriteLine("RzyProtector CLI -n|noPause -o|out=<output directory> <modules>");
			WriteLine("    -n|noPause : no pause after finishing protection.");
			WriteLine("    -o|out     : specifies output directory.");
			WriteLine("    -probe     : specifies probe directory.");
			WriteLine("    -plugin    : specifies plugin path.");
			WriteLine("    -debug     : specifies debug symbol generation.");
			Console.ReadLine();
		}

		static void WriteLineWithColor(ConsoleColor color, string txt)
		{
			ConsoleColor original = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine(txt);
			Console.ForegroundColor = original;
		}

		static void WriteLine(string txt)
		{
			Console.WriteLine(txt);
		}

		static void WriteLine()
		{
			Console.WriteLine();
		}

		class ConsoleLogger : ILogger
		{
			readonly DateTime begin;

			public ConsoleLogger()
			{
				begin = DateTime.Now;
			}

			public int ReturnValue { get; private set; }

			public void Debug(string msg)
			{
				//WriteLineWithColor(ConsoleColor.Gray, "[RZYPROTECTOR] Debug: Injecting...");
			}

			public void DebugFormat(string format, params object[] args)
			{
				//WriteLineWithColor(ConsoleColor.Gray, "[RZYPROTECTOR] Debug: Injecting...");
			}

			public void Info(string msg)
			{
				//WriteLineWithColor(ConsoleColor.White, "[RZYPROTECTOR] Info: " + msg);
			}

			public void InfoFormat(string format, params object[] args)
			{
				//WriteLineWithColor(ConsoleColor.White, "[RZYPROTECTOR] Info: " + string.Format(format, args));
			}

			public void Warn(string msg)
			{
				//WriteLineWithColor(ConsoleColor.Yellow, "[RZYPROTECTOR] Warn: " + msg);
			}

			public void WarnFormat(string format, params object[] args)
			{
				//WriteLineWithColor(ConsoleColor.Yellow, "[RZYPROTECTOR] Warn: " + string.Format(format, args));
			}

			public void WarnException(string msg, Exception ex)
			{
				//WriteLineWithColor(ConsoleColor.Yellow, "[RZYPROTECTOR] Warn: " + msg);
				//WriteLineWithColor(ConsoleColor.Yellow, "Exception: " + ex);
			}

			public void Error(string msg)
			{
				WriteLineWithColor(ConsoleColor.Red, "[RZYPROTECTOR] Error: " + msg);
				Console.ReadLine();
			}

			public void ErrorFormat(string format, params object[] args)
			{
				WriteLineWithColor(ConsoleColor.Red, "[RZYPROTECTOR] Error: " + string.Format(format, args));
				Console.ReadLine();

			}

			public void ErrorException(string msg, Exception ex)
			{
				WriteLineWithColor(ConsoleColor.Red, "[RZYPROTECTOR] Error: " + msg);
				WriteLineWithColor(ConsoleColor.Red, "Exception: " + ex);
				Console.ReadLine();

			}

			public void Progress(int progress, int overall) { }

			public void EndProgress() { }

			public void Finish(bool successful)
			{
				DateTime now = DateTime.Now;
				string timeString = string.Format(
					"at {0}, {1}:{2:d2} elapsed.",
					now.ToShortTimeString(),
					(int)now.Subtract(begin).TotalMinutes,
					now.Subtract(begin).Seconds);
				if (successful)
				{
					WriteLineWithColor(ConsoleColor.Green, "RzyProtector Protection has been finished in " + timeString + " ! \nCLI Will automaticaly close in 10 seconds.\nMade by RZY#2000");
					
					ReturnValue = 0;
					Thread.Sleep(10000);
					Environment.Exit(0);
				}
				else
				{
					WriteLineWithColor(ConsoleColor.Red, "Failed " + timeString);
					ReturnValue = 1;
					Console.WriteLine("CLI Will automaticaly close in 10 seconds.");
					Thread.Sleep(10000);
					Environment.Exit(0);
				}
			}
		}
	}
}