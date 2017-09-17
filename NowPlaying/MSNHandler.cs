using Sync.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NowPlaying
{
    public interface INowPlayingEvent : IBaseEvent { }

    public struct CurrentPlayingBeatmapChangedEvent : INowPlayingEvent
    {
        public BeatmapEntry NewBeatmap;

        public CurrentPlayingBeatmapChangedEvent(BeatmapEntry beatmap)
        {
            NewBeatmap = beatmap;
        }
    }

    public struct StatusChangeEvent : INowPlayingEvent
    {
        public OSUStatus CurrentStatus { get; private set; }
        public StatusChangeEvent(OSUStatus status)
        {
            CurrentStatus = status;
        }
    }

    public class NowPlayingEvents : BaseEventDispatcher<INowPlayingEvent>
    {
        public static readonly NowPlayingEvents Instance = new NowPlayingEvents();
        private NowPlayingEvents()
        {
            EventDispatcher.Instance.RegisterNewDispatcher(GetType());
        }
    }

    public struct OSUStatus
    {
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Diff { get; set; }
        public string Status { get; set; }
        public string Prefix { get; set; }
        public string Mode { get; set; }

        public static implicit operator OSUStatus(string[] arr)
        {
            try
            {
                string[] result = null;
                if (arr.Length == 2)
                {
                    result = arr[0].Replace("\\0", "\\").Split(new[] { '\\' } ,StringSplitOptions.RemoveEmptyEntries);
                    if(result.Length < 6)
                    {
                        return new OSUStatus();
                    }
                    OSUStatus obj = new OSUStatus
                    {
                        Prefix = result[0],
                        Status = result[2].Split(' ')[0],
                        Artist = result[4],
                        Title = result[3],
                        Mode = result[5]
                    };
                    if(result.Length == 7)
                    {
                        obj.Diff = result[6];
                    }
                    return obj;

                }
                else
                {
                    return new OSUStatus();
                }
            }
            catch (Exception e)
            {
                Sync.Tools.IO.CurrentIO.WriteColor("错误:" + e.Message, ConsoleColor.Red);
                return new OSUStatus();
            }
        }
    }


    internal static class NativeMethods
    {
        internal delegate IntPtr WNDPROC(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal class WNDCLASS
        {
            public int style = 0;
            public WNDPROC lpfnWndProc = null;
            public int cbClsExtra = 0;
            public int cbWndExtra = 0;
            public IntPtr hInstance = IntPtr.Zero;
            public IntPtr hIcon = IntPtr.Zero;
            public IntPtr hCursor = IntPtr.Zero;
            public IntPtr hbrBackground = IntPtr.Zero;
            public string lpszMenuName = null;
            public string lpszClassName = null;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        internal struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr RegisterClass(WNDCLASS wc);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle, string lpszClassName, string lpszWindowName, int style, int x, int y, int width, int height, IntPtr hWndParent, IntPtr hMenu, IntPtr hInst, IntPtr lpParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DestroyWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern IntPtr DefWindowProcW(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }

    public class MSNHandler : IDisposable
    {
        private const string CONST_CLASS_NAME = "MsnMsgrUIManager";
        private IntPtr m_hWnd;
        private NativeMethods.WNDCLASS lpWndClass;
        private Thread t;

        public void Dispose()
        {
            DestoryMSNWindow();
            t.Abort();
        }

        public void Load()
        {
            t = new Thread(CreateMSNWindow);
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Name = "ActiveXThread";
        }

        public void StartHandler()
        {
            t.Start();
        }

        #region WIN32Form Implement
        [STAThread]
        private void CreateMSNWindow()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            lpWndClass = new NativeMethods.WNDCLASS
            {
                lpszClassName = CONST_CLASS_NAME,
                lpfnWndProc = new NativeMethods.WNDPROC(WndProc)
            };

            if (NativeMethods.RegisterClass(lpWndClass).ToInt32() == 0 && Marshal.GetLastWin32Error() != 1410)
            {
                Sync.Tools.IO.CurrentIO.WriteColor("无法注册MSN类", ConsoleColor.Red);
                return;
            }
            m_hWnd = NativeMethods.CreateWindowEx(0, CONST_CLASS_NAME, string.Empty, 0, 0, 0, 0, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            if(m_hWnd.ToInt32() > 0)
            {
                Sync.Tools.IO.CurrentIO.WriteColor("MSN类注册成功！", ConsoleColor.Green);
            }

            Application.Run();
            return;
        }

        private bool DestoryMSNWindow()
        {
            if(m_hWnd.ToInt32() > 0)
            {
                return NativeMethods.DestroyWindow(m_hWnd);
            }
            return true;
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            if(msg == 74)
            {
                NativeMethods.COPYDATASTRUCT cb = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.COPYDATASTRUCT));
                string[] info = Marshal.PtrToStringUni(cb.lpData, cb.cbData / 2).Split("\0".ToCharArray(), StringSplitOptions.None);
                OSUStatus stats = info;
                NowPlayingEvents.Instance.RaiseEventAsync(new StatusChangeEvent(stats));
            }

            return NativeMethods.DefWindowProcW(hWnd, msg, wParam, lParam);
        }
        #endregion
    }
}
