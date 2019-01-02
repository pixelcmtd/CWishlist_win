using System.Windows.Forms;

namespace CWishlist_win
{
    public partial class TaskManager : Form
    {
        Form1 src_form;

        public TaskManager(Form1 src_form)
        {
            InitializeComponent();
            this.src_form = src_form;
        }

        void TaskManager_Paint(object sender, PaintEventArgs e)
        {
            listBox1.Items.Clear();
            task[] t;
            lock (src_form.thread_manager.wtask_mutex)
                t = src_form.thread_manager.tasks.ToArray();
            foreach(task u in t)
                listBox1.Items.Add(u.task_mgr_fmt());
        }
    }
}
