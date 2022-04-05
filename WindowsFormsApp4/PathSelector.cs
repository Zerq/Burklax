using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp4 {
    public partial class PathSelector : UserControl {
        public PathSelector() {
            InitializeComponent();
        }

        public string Directory { get { return this.textBox1.Text; } }

        private void button_Click(object sender, EventArgs e) {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) {
                this.textBox1.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void textBox1_Validating(object sender, CancelEventArgs e) {
            if (!System.IO.Directory.Exists(textBox1.Text)) {
                textBox1.Invalidate();
                textBox1.ForeColor = Color.Red;
            } else {
                textBox1.ForeColor = Color.Black;
            }
            textBox1.Refresh();
        }
    }
}
