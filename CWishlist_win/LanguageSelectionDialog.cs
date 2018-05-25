using System;
using System.Linq;
using System.Windows.Forms;

namespace CWishlist_win
{
    partial class LanguageSelectionDialog : Form
    {
        public LanguageSelectionDialog(string title)
        {
            InitializeComponent();
            Text = title;
            foreach (lang l in LanguageProvider.langs.Keys)
                listBox1.Items.Add(l.name);
        }

        void button1_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                LanguageProvider.selected = LanguageProvider.langs.Keys.ToArray()[listBox1.SelectedIndex];
            Close();
        }
    }
}
