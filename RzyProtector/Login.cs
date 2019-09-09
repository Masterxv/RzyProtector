using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TrinitySeal;

namespace RzyProtector
{
	public partial class Login : Form
	{


		public Login()
		{
			InitializeComponent();
			TrinitySeal.Seal.Secret = "UhS0Nbp7G0a2WVopuilnkY4tQMOgSwYd0swX4Q9sYjo2j";
			
		}

		private bool draggable;
		private int mouseX;
		private int mouseY;


		private void LoginOue()
		{
			if (File.Exists($"{Directory.GetCurrentDirectory()}\\infos.rzy"))
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
					string decodedpassword = Base64Decode(password);

					bool response = TrinitySeal.Seal.Login(decodedusername, decodedpassword, false);
					if (response)
					{
						MessageBox.Show($"Welcome Back {decodedusername} !", "Rzy Protector | Private Ver");
						this.Hide();
						var mainform = new Form1();
						mainform.Closed += (s, args) => this.Close();
						mainform.Show();
					}

				}

			}
			else
			{
				MessageBox.Show("Wrong login informations, remove the text from the infos.rzy files & login back.");
				Environment.Exit(1);
			}
		}
		private void panel1_MouseDown(object sender, MouseEventArgs e)
		{
			draggable = true;
			mouseX = Cursor.Position.X - this.Left;
			mouseY = Cursor.Position.Y - this.Top;
		}

		private void panel1_MouseMove(object sender, MouseEventArgs e)
		{
			if (draggable)
			{
				this.Top = Cursor.Position.Y - mouseY;
				this.Left = Cursor.Position.X - mouseX;
			}
		}

		private void panel1_MouseUp(object sender, MouseEventArgs e)
		{
			draggable = false;
		}

		private void FlatLabel2_Click(object sender, EventArgs e)
		{
			Environment.Exit(1);
		}

		private void Login_Load(object sender, EventArgs e)
		{

		}

		private void Registerbtn_Click(object sender, EventArgs e)
		{
			bool response = TrinitySeal.Seal.Register(usernametxt.Text, passwordtxt.Text, emailtxt.Text, tokentxt.Text, false);
			if (response)
			{
				if(!File.Exists(Directory.GetCurrentDirectory() + "\\infos.rzy"))
					{
					File.Create($"{Directory.GetCurrentDirectory()}\\infos.rzy");
				}
				MessageBox.Show("Success, you can now login.", "Rzy Protector | Private Ver");
			}
			else
			{
				MessageBox.Show("Error big nig", "Rzy Protector | Private Ver");
			}
		}

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

		private void Loginbtn_Click(object sender, EventArgs e)
		{
			bool response = TrinitySeal.Seal.Login(usernametxt.Text, passwordtxt.Text, false);
			if (response)
			{
				MessageBox.Show("Success.", "Rzy Protector | Private Ver");
				this.Hide();
				var mainform = new Form1();
				mainform.Closed += (s, args) => this.Close();
				mainform.Show();

				try
				{
					File.Create($"{Directory.GetCurrentDirectory()}\\infos.rzy");
					using (System.IO.StreamWriter file =
			   new System.IO.StreamWriter($"{Directory.GetCurrentDirectory()}\\infos.rzy"))
					{

						string base64username = Base64Encode(usernametxt.Text);
						string base64password = Base64Encode(passwordtxt.Text);

						file.WriteLine($"{base64username}:{base64password}");
					}

				}
				catch
				{ }
			}
			

			else
			{
				MessageBox.Show("Error big nig", "Rzy Protector | Private Ver");
			}
		}
	}
}
