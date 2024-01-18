using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace chat_TUSAPP
{
    internal class TemaManager
    {
        public static void UygulaAcikMod(Control form)
        {
            // Açık Mod teması için renkleri ayarla
            form.BackColor = SystemColors.Control;
            foreach (Control control in form.Controls)
            {
                control.BackColor = SystemColors.Control;
                if (control is TextBox)
                {
                    ((TextBox)control).ForeColor = SystemColors.ControlText;
                }
            }
        }

        public static void UygulaKoyuMod(Control form)
        {
            // Koyu Mod teması için renkleri ayarla
            form.BackColor = Color.DarkGray;
            foreach (Control control in form.Controls)
            {
                control.BackColor = Color.DarkGray;
                if (control is TextBox)
                {
                    ((TextBox)control).ForeColor = Color.White;
                }
            }
        }
    }
}
