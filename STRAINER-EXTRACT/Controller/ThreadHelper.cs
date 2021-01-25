using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace STRAINER_EXTRACT.Controller
{
    public class ThreadHelper
    {
        delegate void SetButtonState(Form f, Control c, bool stat);

        public static void SetButtonEnable(Form f, Control c, bool stat)
        {
            if (c.InvokeRequired)
            {
                SetButtonState d = new SetButtonState(SetButtonEnable);
                f.Invoke(d, new object[] { f, c, stat });
            }
            else
            {
                c.Enabled = stat;
            }
        }
    }
}
