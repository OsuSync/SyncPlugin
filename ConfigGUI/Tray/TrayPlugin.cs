using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Sync.Plugins;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace ConfigGUI.Tray
{
    public class TrayPlugin : Plugin
    {
        private const string PLUGIN_NAME = "Tray";
        private const string PLUGIN_AUTHOR = "KedamaOvO";

        private Thread _winThread;
        private bool _quit = false;

        private bool _visible = true;

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        public static void SetConsoleWindowVisibility(bool visible)
        {
            IntPtr hWnd = FindWindow(null, Console.Title);
            if (hWnd != IntPtr.Zero)
            {
                if (visible) ShowWindow(hWnd, 1); //1 = SW_SHOWNORMAL           
                else ShowWindow(hWnd, 0); //0 = SW_HIDE               
            }
        }

        public TrayPlugin() : base(PLUGIN_NAME, PLUGIN_AUTHOR)
        {
        }

        private ContextMenuStrip GetMenu()
        {
            var contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add(DefaultLanguage.TRAY_HIDE_SHOW, null,(s,e)=>SetConsoleWindowVisibility(_visible = !_visible));
            contextMenu.Items.Add(DefaultLanguage.TRAY_OPEN_SYNC_FOLDER, null,
                (s, e) => System.Diagnostics.Process.Start("explorer.exe", AppDomain.CurrentDomain.BaseDirectory));

            contextMenu.Items.Add(DefaultLanguage.TRAY_CONFIG, null, (s, e) => getHoster().Commands.Dispatch.invoke("config", null));
            contextMenu.Items.Add(DefaultLanguage.TRAY_EXIT, null, (s, e) => getHoster().Commands.Dispatch.invoke("exit", null));
            return contextMenu;
        }

        public override void OnExit()
        {
            _quit = true;
            _winThread?.Join();
        }

        public override void OnEnable()
        {
            _winThread = new Thread(() =>
            {
                using (NotifyIcon notifyIcon = new NotifyIcon())
                {
                    notifyIcon.DoubleClick += (s, e) =>
                    {
                        _visible = !_visible;
                        SetConsoleWindowVisibility(_visible);
                    };
                    notifyIcon.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                    notifyIcon.Visible = true;
                    notifyIcon.Text = Application.ProductName;
                    notifyIcon.ContextMenuStrip = GetMenu();
                    while (!_quit)
                    {
                        Application.DoEvents();
                        Thread.Sleep(100);
                    }
                }
            })
            {
                Name = "STA Tray Thread"
            };
            
            _winThread.SetApartmentState(ApartmentState.STA);
            _winThread.Start();
        }
    }
}
