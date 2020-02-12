using System;
using System.Linq;
using System.Windows.Forms;
using static CWishlist_win.Languages;

namespace CWishlist_win
{
    partial class LanguageSelectionDialog : Form
    {
        public LanguageSelectionDialog(string title)
        {
            InitializeComponent();
            Text = title;
            foreach (lang l in langs.Keys)
                listBox1.Items.Add(l.name);
        }

        void button1_Click(object _, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
                selected = langs.Keys.ElementAt(listBox1.SelectedIndex);
            Close();
        }
    }
}
