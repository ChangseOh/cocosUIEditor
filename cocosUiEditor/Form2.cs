using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cocosUiEditor
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }
        public Form2(string defaultName, int width, int height)
        {
            InitializeComponent();
            passName = defaultName;
            textBox1.Text = defaultName;
            passWidth = width.ToString();
            passHeight = height.ToString();
            numericUpDown1.Value = (decimal)width;
            numericUpDown2.Value = (decimal)height;
        }

        public string passName;
        public string passWidth;
        public string passHeight;
        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length == 0)
            {
                MessageBox.Show("Input Project Name");
                return;
            }

            //create
            passName = textBox1.Text;
            passWidth = numericUpDown1.Value.ToString();
            passHeight = numericUpDown2.Value.ToString();

            this.Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //preset 1
            numericUpDown1.Value = 480;
            numericUpDown2.Value = 800;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            //preset 2
            numericUpDown1.Value = 720;
            numericUpDown2.Value = 1280;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            //preset 3
            numericUpDown1.Value = 1080;
            numericUpDown2.Value = 1920;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //swap
            decimal temp = numericUpDown1.Value;
            numericUpDown1.Value = numericUpDown2.Value;
            numericUpDown2.Value = temp;
        }

    }
}
