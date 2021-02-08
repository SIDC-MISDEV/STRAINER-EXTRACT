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
        delegate void SetLabelText(Form f, Control c, string text);
        delegate void SetProgress(Form f, ProgressBar c, int value, int maxVal);

        public static void SetControlState(Form f, Control c, bool stat)
        {
            if (c.InvokeRequired)
            {
                SetButtonState d = new SetButtonState(SetControlState);
                f.Invoke(d, new object[] { f, c, stat });
            }
            else
            {
                c.Enabled = stat;
            }
        }

        public static void SetLabel(Form f, Control c, string val)
        {
            if (c.InvokeRequired)
            {
                SetLabelText d = new SetLabelText(SetLabel);
                f.Invoke(d, new object[] { f, c, val });
            }
            else
            {
                c.Text = val;
            }
        }

        //public static void SetValue(Form f, ProgressBar c, int value, int maxVal)
        //{
        //    if (c.InvokeRequired)
        //    {
        //        SetProgress d = new SetProgress(SetValue);
        //        f.Invoke(d, new object[] { f, c, value, maxVal });
                
        //    }
        //    else
        //    {
        //        c.Maximum = maxVal;
        //        c.Value = value;
                
        //    }
        //}


    }
}
