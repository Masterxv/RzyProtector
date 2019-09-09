using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RzyProtector
{
	static class Program
	{
		/// <summary>
		/// Point d'entrée principal de l'application.
		/// </summary>
		/// 

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

		[STAThread]
		static void Main()
		{
			/*	if (File.Exists($"{Directory.GetCurrentDirectory()}\\infos.rzy"))
				{
					string[] oue = File.ReadAllLines($"{Directory.GetCurrentDirectory()}\\infos.rzy");
					if (oue.Length > 0.01)
					{
						string text = File.ReadAllText("infos.rzy");
						string username = text.Split(new char[]
							{
								':'
							})[0].Replace(" ", "");

						string password = text.Split(new char[]
							{
								':'
							})[1].Replace(" ", "");
						string decodedusername = Base64Decode(username);
						string decodedpassword = Base64Decode(password);*/

					TrinitySeal.Seal.Secret = "UhS0Nbp7G0a2WVopuilnkY4tQMOgSwYd0swX4Q9sYjo2j";

					bool response = TrinitySeal.Seal.Login("test", "test", false);
					if (response)
					{
						MessageBox.Show($"Welcome to Rzy Protector !", "Rzy Protector | Private Ver");
						/*var loginform = new Login();

						loginform.Hide();
						var mainform = new Form1();
						mainform.Closed += (s, args) => loginform.Close();*/
						Application.EnableVisualStyles();
						Application.Run(new Form1());


					}
					
		}
	}
}
