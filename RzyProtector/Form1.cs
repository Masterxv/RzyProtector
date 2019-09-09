using Confuser.Core;
using Confuser.Core.Project;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using Rule = Confuser.Core.Project.Rule;

namespace RzyProtector
{
	public partial class Form1 : Form
	{

		public Form1()
		{
			InitializeComponent();

		}

		private bool draggable;
		private int mouseX;
		private int mouseY;

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


		private void thirteenTextBox1_DragEnter(object sender, DragEventArgs e)
		{
			if (e.Data.GetDataPresent(DataFormats.FileDrop))
				e.Effect = DragDropEffects.Copy;
			else
				e.Effect = DragDropEffects.None;
		}
		private void thirteenTextBox1_DragDrop(object sender, DragEventArgs e)
		{
			try
			{
				Array array = (Array)e.Data.GetData(DataFormats.FileDrop);
				if (array != null)
				{
					string path = array.GetValue(0).ToString();
					int num = path.LastIndexOf(".");
					if (num != -1)
					{
						string extension = path.Substring(num).ToLower();
						if (extension == ".exe" || extension == ".dll")
						{
							Activate();
							thirteenTextBox1.Text = path;
						}
					}
				}
			}
			catch { }
		}
		private void FlatLabel2_Click(object sender, EventArgs e)
		{
			Environment.Exit(0);

		}

		private void Form1_Load(object sender, EventArgs e)
		{
			string temp = Path.GetTempPath();


			try
			{


				File.Delete($"{Directory.GetCurrentDirectory()}\\Configs\\CustomModule.rzy");
				File.Delete($"{Directory.GetCurrentDirectory()}\\Configs\\CustomRenamer.rzy");


			}
			catch
			{

			}
			// Auto login system
			/*string text = File.ReadAllText("infos.rzy");
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
			TrinitySeal.Seal.GrabVariables("Gsbu6csR12dthY04WZ66hy29y", "UhS0Nbp7G0a2WVopuilnkY4tQMOgSwYd0swX4Q9sYjo2j", "test", "test");

			flatGroupBox4.Hide();
			customgroupbox.Hide();
			warnlbl.Hide();
			flatGroupBox3.Hide();


			DirectoryInfo d = new DirectoryInfo($"{Directory.GetCurrentDirectory()}\\Projects");
			FileInfo[] Files = d.GetFiles("*.rzyproj"); 
			string str = "";
			foreach (FileInfo file in Files)
			{
				//str = str + ", " + file.Name;
				listBox1.Items.Remove("");
				listBox1.Items.Add(file.Name);
			}

		}

		private void ThirteenButton1_Click(object sender, EventArgs e)
		{
			OpenFileDialog k = new OpenFileDialog();
			DialogResult result = k.ShowDialog(); // Show the dialog.
			if (result == DialogResult.OK) // Test result.
			{
				string file = k.FileName;
				thirteenTextBox1.Text = file;
			}
		}

		private void ThirteenButton2_Click(object sender, EventArgs e)
		{
			MessageBox.Show("You just need to click browse and choose your file \nOr you can just drag/drop your file , then select the protections you want and hit obfuscate. Obfuscated file will be in the RzyProtector folder of the input file");

		}

		private void Customtxt_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void ThirteenTextBox2_TextChanged(object sender, EventArgs e)
		{

		}

		private static Random random = new Random();
		public static string RandomLetters(int length)
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static string RandomChinese(int length)
		{
			const string chars = "池	驰弛地他她拖水马馬人也豆贝尔维艾克斯艾尺艾弗";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static string RandomArabic(int length)
		{
			const string chars = "بَاءجِيمحَاءخَاءذَالسِين";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

		public static string RandomNumbers(int length)
		{
			const string chars = "123456789";
			return new string(Enumerable.Repeat(chars, length)
			  .Select(s => s[random.Next(s.Length)]).ToArray());
		}

	

		private void BunifuFlatButton2_Click(object sender, EventArgs e)
		{
			customgroupbox.Hide();
			flatGroupBox4.Hide();

			flatGroupBox2.Hide();

			flatGroupBox3.Show();
		}

		private void BunifuFlatButton1_Click(object sender, EventArgs e)
		{
			customgroupbox.Hide();
			flatGroupBox3.Hide();
			flatGroupBox4.Hide();

			flatGroupBox2.Show();
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
		private void ThirteenButton4_Click(object sender, EventArgs e)
		{
			if (thirteenTextBox1.Text == "")
				MessageBox.Show("You need to select a file to protect !", "RzyObfuscator");
			else
			{

			


				string path = Directory.GetCurrentDirectory() + "\\Configs\\CustomRenamer.rzy";
				string dir = Path.GetDirectoryName(path);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}

				string path2 = Directory.GetCurrentDirectory() + "\\Configs\\CustomModule.rzy";
				string dir2 = Path.GetDirectoryName(path2);
				if (!Directory.Exists(dir2))
				{
					Directory.CreateDirectory(dir2);
				}

				ConfuserProject proj = new ConfuserProject();
				proj.BaseDirectory = Path.GetDirectoryName(thirteenTextBox1.Text);
				proj.OutputDirectory = Path.Combine(Path.GetDirectoryName(thirteenTextBox1.Text) + @"\RzyProtector");
				ProjectModule module = new ProjectModule();
				module.Path = Path.GetFileName(thirteenTextBox1.Text);
				proj.Add(module);

				Rule rule = new Rule("true", ProtectionPreset.None, false);



					if (antivmcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("avm"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antiildasmcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("ildasm"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antide4dot.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("de4dot"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antidnspy.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("dnspy"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antitampercheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("tamper"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antidumpcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventdump"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antidebugcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventdebug"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antimemoryeditcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("memory"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antiwatermarkcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("watermark"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (callicheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("calliprot"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (constantscheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("const"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (ctrlflowcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("control flow"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (junkcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("addjunk"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (refproxycheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("proxy"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (resourcescheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("resources"), SettingItemAction.Add);
						rule.Add(protection);
					}
					
					if (erasecheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("erase"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (fakenativecheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("native"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (lcltofieldscheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("local to field"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (modulerenamercheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("modulerenamer"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (stackunderflowcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("stack"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (admincheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("forceadmin"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (hidemethodscheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("methodshide"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (invalidmetadatacheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("metadata"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (md5check.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("checksum"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (modulefloodcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("floodmodule"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (fakeobfuscatorcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("fakeobf"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (renamercheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("renamer"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antifiddlercheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventfiddler"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (antihttpcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventhttp"), SettingItemAction.Add);
						rule.Add(protection);
					}
					if (processmonitorcheck.Checked)
					{
						SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("processmonitor"), SettingItemAction.Add);
						rule.Add(protection);
					}
				if (mutateconstantscheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>("Mutate Constants", SettingItemAction.Add);
					rule.Add(protection);
				}
				if (weakcallicheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>("Weak Calli", SettingItemAction.Add);
					rule.Add(protection);
				}
				//SettingItem<Protection> protectionR = new SettingItem<Protection>("Lite Calli", SettingItemAction.Add);
				//rule.Add(protectionR);


				proj.PluginPaths.Clear();
			



			proj.Rules.Add(rule);

			XmlDocument doc = proj.Save();
			doc.Save("temp.crproj");


			Process.Start("RzyProtector.CLI.exe", "-n temp.crproj").WaitForExit();
			File.Delete("temp.crproj");

		}
	
		}

		private void Arabic_CheckedChanged(object sender, EventArgs e)
		{

		}

	

		

		private void Fakeobfuscatorcheck_CheckedChanged(object sender, EventArgs e)
		{

		}

		private void FlatGroupBox3_Click(object sender, EventArgs e)
		{

		}

		private void Antide4dotcheck_CheckedChanged(object sender, EventArgs e)
		{
			if(antide4dotcheck.Checked == true)
			{
				warnlbl.Show();
				warnlbl.Text = "WARN: Anti De4dot may does not work, this feature is still in BETA.";
			}
			else
			{
				warnlbl.Hide();
			}
		}

		private void Constantscheck_CheckedChanged(object sender, EventArgs e)
		{
			if(constantscheck.Checked == true)
			{
				warnlbl.Show();
				warnlbl.Text = "WARN: Constants protection may cause an error.";
			}
			else
			{
				warnlbl.Hide();
			}
		}

		private void Renamercheck_CheckedChanged(object sender, EventArgs e)
		{
		

		}

		private void LinkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://github.com/BedTheGod");
		}

		private void LinkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://github.com/HoLLy-HaCKeR/Confuser.Protections.HoLLy");
		}

		private void LinkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			Process.Start("https://discord.gg/uCxuWwN");
		}

		private void BunifuSeparator1_Load(object sender, EventArgs e)
		{

		}

		private void BunifuFlatButton3_Click(object sender, EventArgs e)
		{
			MessageBox.Show("Credits to:\n\nBed: For the renamer\n\nCentos: Helping me a bit for anti fiddler & for giving me process monitor protection", "Credits", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

		}

		private void BunifuFlatButton4_Click(object sender, EventArgs e)
		{
			flatGroupBox4.Hide();

			customgroupbox.Show();
			flatGroupBox2.Hide();
			flatGroupBox3.Hide();

		}

		private void ThirteenButton7_Click(object sender, EventArgs e)
		{
			string path = Directory.GetCurrentDirectory() + "\\Configs\\CustomModule.rzy";
			string dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}



			string customtxt = "жфRZY# ф2000жжж ф";
			string customassemblytxt = "жфRZY# ф2000жжж ф";


			
				customtxt = thirteenTextBox7.Text;
			

			
			customassemblytxt = thirteenTextBox6.Text;


			using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
			{
				writer.WriteLine($"{customassemblytxt}:{customtxt}");
			}
		}

		private void ThirteenButton6_Click(object sender, EventArgs e)
		{
			string path = Directory.GetCurrentDirectory() + "\\Configs\\CustomRenamer.rzy";
			string dir = Path.GetDirectoryName(path);
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}




			string customtxt = "";

			
				customtxt = thirteenTextBox5.Text;
			

			using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
			{
				writer.WriteLine($"{customtxt}");
			}
		}

		private void ThirteenButton3_Click(object sender, EventArgs e)
		{
			string customtxt = "жфRZY# ф2000жжж ф";
			string customassemblytxt = "жфRZY# ф2000жжж ф";
			string path = Directory.GetCurrentDirectory() + "\\Configs\\CustomModule.rzy";

			using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
			{
				writer.WriteLine($"{customassemblytxt}:{customtxt}");
			}

		}

		private void ThirteenButton5_Click(object sender, EventArgs e)
		{
			string path = Directory.GetCurrentDirectory() + "\\Configs\\CustomRenamer.rzy";

			using (StreamWriter writer = new StreamWriter(File.OpenWrite(path)))
			{
				writer.WriteLine("Rzy-Protector CreeperᅠᅠᅠᅠAww ManᅠᅠᅠᅠᅠᅠFuck skiddersᅠᅠᅠᅠᅠhttps://rzy.beᅠᅠᅠᅠaw manᅠ");
			}
		}

		private void ThirteenButton8_Click(object sender, EventArgs e)
		{

		}


		private void UnCheckAllProt()
		{
			antihttpcheck.Checked = false;
			antifiddlercheck.Checked = false;
			antidnspycheck.Checked = false;
			processmonitorcheck.Checked = false;
			fakeobfuscatorcheck.Checked = false;
			modulefloodcheck.Checked = false;
			md5check.Checked = false;
			invalidmetadatacheck.Checked = false;
			hidemethodscheck.Checked = false;
			admincheck.Checked = false;
			stackunderflowcheck.Checked = false;
			modulerenamercheck.Checked = false;
			lcltofieldscheck.Checked = false;
			fakenativecheck.Checked = false;
			erasecheck.Checked = false;
			renamercheck.Checked = false;
			resourcescheck.Checked = false;
			refproxycheck.Checked = false;
			junkcheck.Checked = false;
			ctrlflowcheck.Checked = false;
			constantscheck.Checked = false;
			callicheck.Checked = false;
			antiwatermarkcheck.Checked = false;
			antimemoryeditcheck.Checked = false;
			antidumpcheck.Checked = false;
			antitampercheck.Checked = false;
			antidebugcheck.Checked = false;
			antide4dotcheck.Checked = false;
			antiildasmcheck.Checked = false;
			antivmcheck.Checked = false;
		}
		private void ThirteenButton10_Click(object sender, EventArgs e)
		{
			ConfuserProject proj = new ConfuserProject();
			try
			{
				XmlDocument xmlDoc = new XmlDocument();
				xmlDoc.Load($"{Directory.GetCurrentDirectory()}\\Projects\\{listBox1.SelectedItem}");
				proj.Load(xmlDoc);
				ProjectVM project = new ProjectVM(proj, $"{Directory.GetCurrentDirectory()}\\Projects\\{listBox1.SelectedItem}");

				foreach (ProjectRuleVM r in project.Rules)
				{
					foreach (ProjectSettingVM<Protection> s in r.Protections)
					{

						UnCheckAllProt();
						string name = s.Id;

						if (name == "anti vm")
						{
							antivmcheck.Checked = true;
						}
						if (name == "anti ildasm")
						{
							antiildasmcheck.Checked = true;
						}
						if (name == "anti de4dot")
						{
							antide4dotcheck.Checked = true;
						}
						if (name == "anti debug")
						{
							antidebugcheck.Checked = true;
						}
						if (name == "anti tamper")
						{
							antitampercheck.Checked = true;
						}
						if (name == "anti dump")
						{
							antidumpcheck.Checked = true;
						}
						if (name == "anti watermark")
						{
							antiwatermarkcheck.Checked = true;
						}
						if (name == "memory protection")
						{
							antimemoryeditcheck.Checked = true;
						}
						if (name == "calli protection")
						{
							callicheck.Checked = true;
						}
						if (name == "constants")
						{
							constantscheck.Checked = true;
						}
						if (name == "ctrl flow")
						{
							ctrlflowcheck.Checked = true;
						}
						if (name == "Junk")
						{
							junkcheck.Checked = true;
						}
						if (name == "ref proxy")
						{
							refproxycheck.Checked = true;
						}
						if (name == "resources")
						{
							resourcescheck.Checked = true;
						}
						if (name == "rename")
						{
							renamercheck.Checked = true;
						}
						if (name == "Mutate Constants")
						{
							mutateconstantscheck.Checked = true;
						}
						if (name == "erase headers")
						{
							erasecheck.Checked = true;
						}
						if (name == "fake native")
						{
							fakenativecheck.Checked = true;
						}
						if (name == "lcltofield")
						{
							lcltofieldscheck.Checked = true;
						}
						if (name == "Rename Module")
						{
							modulerenamercheck.Checked = true;
						}
						if (name == "stack underflow")
						{
							stackunderflowcheck.Checked = true;
						}
						if (name == "force elevation")
						{
							admincheck.Checked = true;
						}
						if (name == "Hide Methods")
						{
							hidemethodscheck.Checked = true;
						}
						if (name == "invalid metadata")
						{
							invalidmetadatacheck.Checked = true;
						}
						if (name == "MD5 Hash Check")
						{
							md5check.Checked = true;
						}
						if (name == "module flood")
						{
							modulefloodcheck.Checked = true;
						}
						if (name == "fake obfuscator")
						{
							fakeobfuscatorcheck.Checked = true;
						}
						if (name == "process monitor")
						{
							processmonitorcheck.Checked = true;
						}
						if (name == "anti dnspy")
						{
							antidnspycheck.Checked = true;
						}
						if (name == "anti fiddler")
						{
							antifiddlercheck.Checked = true;
						}
						if (name == "anti http debugger")
						{
							antihttpcheck.Checked = true;
						}
					}
				}
			}
			catch { }
		}

		private void ThirteenButton9_Click(object sender, EventArgs e)
		{

			listBox1.Items.Clear();

			DirectoryInfo d = new DirectoryInfo($"{Directory.GetCurrentDirectory()}\\Projects");
			FileInfo[] Files = d.GetFiles("*.rzyproj");
			foreach (FileInfo file in Files)
			{
				//str = str + ", " + file.Name;
				listBox1.Items.Add(file.Name);
			}
		}

		private void ThirteenButton12_Click(object sender, EventArgs e)
		{
			if(textBox1.Text.Length < 2)
			{
				MessageBox.Show("Title of the projects need to be > than 2 letters.");
			}
			else
			{
				listBox1.Items.Add(textBox1.Text + ".rzyproj");
				ConfuserProject proj = new ConfuserProject();
				try
				{
					proj.BaseDirectory = Path.GetDirectoryName(thirteenTextBox1.Text);
					proj.OutputDirectory = Path.Combine(Path.GetDirectoryName(thirteenTextBox1.Text) + @"\RzyProtector");
				}
				catch { }
				ProjectModule module = new ProjectModule();
				module.Path = Path.GetFileName(thirteenTextBox1.Text);
				proj.Add(module);

				Rule rule = new Rule("true", ProtectionPreset.None, false);




				if (antivmcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("avm"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antiildasmcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("ildasm"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antide4dot.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("de4dot"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antidnspy.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("dnspy"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antitampercheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("tamper"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antidumpcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventdump"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antidebugcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventdebug"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antimemoryeditcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("memory"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antiwatermarkcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("watermark"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (callicheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("calliprot"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (constantscheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("const"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (ctrlflowcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("control flow"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (junkcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("addjunk"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (refproxycheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("proxy"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (resourcescheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("resources"), SettingItemAction.Add);
					rule.Add(protection);
				}
				
				if (erasecheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("erase"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (fakenativecheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("native"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (lcltofieldscheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("local to field"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (modulerenamercheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("modulerenamer"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (stackunderflowcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("stack"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (admincheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("forceadmin"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (hidemethodscheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("methodshide"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (invalidmetadatacheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("metadata"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (md5check.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("checksum"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (modulefloodcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("floodmodule"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (fakeobfuscatorcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("fakeobf"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (renamercheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("renamer"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antifiddlercheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventfiddler"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (antihttpcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("preventhttp"), SettingItemAction.Add);
					rule.Add(protection);
				}
				if (processmonitorcheck.Checked)
				{
					SettingItem<Protection> protection = new SettingItem<Protection>(TrinitySeal.Seal.Var("processmonitor"), SettingItemAction.Add);
					rule.Add(protection);
				}


				proj.PluginPaths.Clear();
				proj.Rules.Add(rule);

				XmlDocument doc = proj.Save();
				doc.Save($"{Directory.GetCurrentDirectory()}\\Projects\\{textBox1.Text}.rzyproj");
			}
		}

		private void BunifuFlatButton5_Click(object sender, EventArgs e)
		{
			flatGroupBox4.Show();
		}


		private void ThirteenButton8_Click_1(object sender, EventArgs e)
		{
			Process.Start("explorer.exe", Path.Combine(Environment.CurrentDirectory, "Projects"));
		}
	}
}
