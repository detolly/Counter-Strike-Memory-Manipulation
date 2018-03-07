using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;
using SharpDX;
using SharpDX.DirectWrite;
using System.Threading;
using System.Runtime.InteropServices;

namespace newHack
{
    public partial class Form1 : Form
    {
        private WindowRenderTarget device;
        private HwndRenderTargetProperties renderProperties;
        private SolidColorBrush solidColorBrush;
        private Factory factory;

        //text fonts
        private TextFormat font, fontSmall;
        private FontFactory fontFactory;
        private const string fontFamily = "Arial";//you can edit this of course
        private const float fontSize = 12.0f;
        private const float fontSizeSmall = 10.0f;

        private IntPtr handle;
        private Thread sDX = null;
        //DllImports
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern bool GetWindowInfo(IntPtr hWnd, );

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("dwmapi.dll")]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);
        //Styles
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        public static IntPtr HWND_TOPMOST = new IntPtr(-1);

        public Form1()
        {
            this.handle = Handle;
            int initialStyle = GetWindowLong(this.Handle, -20);
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
            OnResize(null);

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.DoubleBuffered = true;
            this.Width = 1920;// set your own size
            this.Height = 1080;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |// this reduce the flicker
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true);
            this.TopMost = true;
            this.Visible = true;

            factory = new Factory();
            fontFactory = new FontFactory();
            renderProperties = new HwndRenderTargetProperties()
            {
                Hwnd = this.Handle,
                PixelSize = new Size2(Width, Height),
                PresentOptions = PresentOptions.None
            };

            //SetLayeredWindowAttributes(this.Handle, 0, 255, Managed.LWA_ALPHA);// caution directx error

            //Init DirectX
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);

            solidColorBrush = new SolidColorBrush(device, Color.White);
            // Init font's
            font = new TextFormat(fontFactory, fontFamily, fontSize);
            fontSmall = new TextFormat(fontFactory, fontFamily, fontSizeSmall);
            //line = new device.DrawLine;

            sDX = new Thread(new ParameterizedThreadStart(sDXThread));

            sDX.Priority = ThreadPriority.Highest;
            sDX.IsBackground = true;
            sDX.Start();
        }
        protected override void OnPaint(PaintEventArgs e)// create the whole form
        {
            int[] marg = new int[] { 0, 0, Width, Height };
            DwmExtendFrameIntoClientArea(this.Handle, ref marg);
        }

        private void sDXThread(object sender)
        {
            while (true)
            {
                device.BeginDraw();
                device.Clear(Color.Transparent);
                device.TextAntialiasMode = TextAntialiasMode.Aliased;// you can set another text mode

                
                //place your rendering things here

                device.EndDraw();
            }

            //whatever you want
        }
    }
}
