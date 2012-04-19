using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;

namespace RPQ
{
    public partial class Options : Form
    {
        public Options()
        {
            InitializeComponent();
        }

        private void btnRegistr_Click(object sender, EventArgs e)
        {
            Register("jpegfile", "ConvertImage", "ConvertImage", string.Format(
                    "\"{0}\" \"%L\"", Application.ExecutablePath));
 
        }

        public static void Register(string fileType,
       string shellKeyName, string menuText, string menuCommand)
        {
            // create path to registry location
            string regPath = string.Format(@"{0}\shell\{1}",
                                           fileType, shellKeyName);

            // add context menu to the registry
            using (RegistryKey key =
                   Registry.ClassesRoot.CreateSubKey(regPath))
            {
                key.SetValue(null, menuText);
            }

            // add command that is invoked to the registry
            using (RegistryKey key = Registry.ClassesRoot.CreateSubKey(
                string.Format(@"{0}\command", regPath)))
            {
                key.SetValue(null, menuCommand);
            }
        }

        public static void Unregister(string fileType, string shellKeyName)
        {
            Debug.Assert(!string.IsNullOrEmpty(fileType) &&
                !string.IsNullOrEmpty(shellKeyName));

            // path to the registry location
            string regPath = string.Format(@"{0}\shell\{1}",
                                           fileType, shellKeyName);

            // remove context menu from the registry
            Registry.ClassesRoot.DeleteSubKeyTree(regPath);
        }

        private void btnUnregistr_Click(object sender, EventArgs e)
        {
            Unregister(".png", "ConvertImage");
        }

    }
}
