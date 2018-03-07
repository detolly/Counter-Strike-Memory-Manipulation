using System;
using System.Windows.Forms;

namespace newHack
{
    public partial class Controls : Form
    {
        Form1 form;
        public Controls(Form1 form)
        {
            this.form = form;
            InitializeComponent();
#if DEBUG
            checkBox2.Enabled = true;
#endif
            this.FormClosing += (o,e) => {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            };
        }

        ~Controls()
        {
            form.Close();
            Application.Exit();
        }
       

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            form.smoothing = (float)(numericUpDown1.Value / 100);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("100 = Instant Aim (Good for HvH)\r\n1 = Extremely slow and smooth. \r\n\r\n 10 is good and looks semi-legit.", "Help", MessageBoxButtons.OK);
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            form.currentFov = (float)numericUpDown2.Value;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Less than 2 is pretty legit, everything else is just rip account imo", "Help", MessageBoxButtons.OK);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            form.holdMouse1 = checkBox1.Checked ? true : false;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            form.aimbotautoshoot = checkBox2.Checked ? true : false;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //aimbot spot help click
            MessageBox.Show(
                @"Enemies need to be visible (3D) in order for the aimbot to trigger on them.",
                "Help",
                MessageBoxButtons.OK
                );
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            form.spottedNeedsToBeTrue = checkBox3.Checked ? true : false;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            form.boneIndex = comboBox1.SelectedIndex + 1;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //aimbot bone select help
            MessageBox.Show(
                @"This is what the aimbot will aim at.

This may vary from various models, so descriptions may be slightly inaccurate.",
                "Help",
                MessageBoxButtons.OK
                );
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            form.performance = checkBox4.Checked;
        }

        bool lookingForKey = false;

        private void button5_Click(object sender, EventArgs e)
        {
            lookingForKey = true;
            button5.Text = "Press a Key";
        }

        private void Controls_KeyPress(object sender, KeyPressEventArgs e)
        {
            
        }

        private void Controls_KeyDown(object sender, KeyEventArgs e)
        {
            
        }

        private void button5_KeyDown(object sender, KeyEventArgs e)
        {
            if (lookingForKey)
            {
                button5.Text = "" + e.KeyValue + " (" + e.KeyData.ToString() + ")";
                form.lookforKey = e.KeyValue;
                lookingForKey = false;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            button5.Text = "mouse1";
            form.lookforKey = 0x1;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            button5.Text = "mouse2";
            form.lookforKey = 0x2;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("skins.ini");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            form.gotValues = false;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            form.updateOnRefreshModel = checkBox5.Checked;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch(comboBox2.SelectedIndex)
            {
                case 0:
                    form.legitBot = true;
                    form.rageBot = false;
                    break;
                case 1:
                    MessageBox.Show("Warning: This might cause an unexpected untrusted ban because of viewangles being written DIRECTLY to memory. \r\n\r\nYou have been warned!");
                    form.legitBot = false;
                    form.rageBot = true;
                    break;
                default:
                    break;
            }
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            form.aimAtFriends = checkBox6.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            form.careAboutFovInRage = checkBox7.Checked;
        }
    }
}
