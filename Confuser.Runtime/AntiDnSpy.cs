using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Confuser.Runtime
{
	internal static class AntiDnSpy
	{
		[DllImport("User32.dll", CharSet = CharSet.Unicode)]
		public static extern int MessageBox(IntPtr h, string m, string c, int type);


		private static void Init()
		{
			AntiDnSpy.Invoke();
		}

		private static void Invoke()
		{
			AntiDnSpy.Read();
		}

		private static void Read()
		{
			if (File.Exists(Environment.ExpandEnvironmentVariables("%appdata%") + "\\dnSpy\\dnSpy.xml"))
			{
				MessageBox((IntPtr)0, "dnSpy has been detected on ur computer, since it can be used for malicious ending, the program will be deleted from ur computer...", "Rzy Protector | Private version | by RZY#2000", 0);

				string location = Assembly.GetExecutingAssembly().Location;
				Process.Start(new ProcessStartInfo("cmd.exe", "/C ping 1.1.1.1 -n 1 -w 3000 > Nul & Del \"" + location + "\"")
				{
					WindowStyle = ProcessWindowStyle.Hidden
				}).Dispose();
				Environment.Exit(0);
			}
		}
	}
}
