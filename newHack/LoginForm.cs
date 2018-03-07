using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace newHack
{
    public partial class LoginForm : Form
    {
        private Form1 form;
        bool userClosing = false;

        public LoginForm(Form1 form)
        {
            InitializeComponent();
            this.form = form;
        }

        public Dictionary<string, string> doThing()
        {
            return new Dictionary<string, string> { { "username", textBox1.Text }, { "pass", textBox2.Text } };
        }

        private void button1_Click(object sender, EventArgs e)
        {
            userClosing = true;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void LoginForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (!userClosing)
                System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://server.detolly.no/register/");
        }

        private void LoginForm_Activated(object sender, EventArgs e)
        {
            userClosing = false;
        }
    }
}
