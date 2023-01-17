using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace PDF_Locksmith
{
    public partial class Form2 : Form
    {
        public string Password = string.Empty;
        public Form2()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Password = textBox1.Text;
            this.DialogResult = DialogResult.OK;
        }

        public void Clear()
        {
            textBox1.Text = string.Empty;
            Password = string.Empty;
        }
    }
}
