using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetworkParall
{
    public partial class UserCode : Form
    {
        public UserCode()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (richTextBox1.Text != "") { Data.Value = richTextBox1.Text; this.Close(); }
            else MessageBox.Show("Empty source code!");
        }
    }
}

