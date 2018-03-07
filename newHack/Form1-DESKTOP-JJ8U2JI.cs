using System;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using Factory = SharpDX.Direct2D1.Factory;
using FontFactory = SharpDX.DirectWrite.Factory;
using Format = SharpDX.DXGI.Format;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using System.Threading;
using System.Runtime.InteropServices;
using static newHack.Aimbot;
using System.Diagnostics;
using System.Net.Http;
using System.Collections.Generic;
using laExternalMulti.Objects.Implementation.CSGO.Data.BSP;
using System.Text;
using System.Text.RegularExpressions;

namespace newHack
{
    public partial class Form1 : Form
    {
        BSP map;
        private WindowRenderTarget device;
        private HwndRenderTargetProperties renderProperties;
        private SolidColorBrush redBrush;
        private SolidColorBrush blueBrush;
        private Factory factory;

        //text fonts
        private TextFormat font, fontSmall;
        private FontFactory fontFactory;
        private const string fontFamily = "Arial";//you can edit this of course
        private const float fontSize = 18.0f;
        private const float fontSizeSmall = 10.0f;
        private int windowHeight = 0;
        private int windowWidth = 0;

        private IntPtr handle;
        private Thread sDX = null;
        //DllImports
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("dwmapi.dll")]
        public static extern void DwmExtendFrameIntoClientArea(IntPtr hWnd, ref int[] pMargins);

        [DllImport("user32.dll")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string setNull, string name);

        //Styles
        public const UInt32 SWP_NOSIZE = 0x0001;
        public const UInt32 SWP_NOMOVE = 0x0002;
        public const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        public float smoothing = 0.1f;
        public static IntPtr HWND_TOPMOST = new IntPtr(-1);
        Controls controls;
        System.Timers.Timer timer = new System.Timers.Timer();
        Menu menu;
        HttpClient client;
        WebBrowser adBrowser;

        int menuOffTicks = 150;

        public Form1()
        {
            this.handle = Handle;
            int initialStyle = GetWindowLong(this.Handle, -20);

            try
            {
                csgoProcess = Process.GetProcessesByName("csgo")[0];
            }
            catch
            {
                sDX?.Abort();
                d?.Abort();
                bhop?.Abort();
            }
            SetWindowLong(this.Handle, -20, initialStyle | 0x80000 | 0x20);
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, 0);

            OnResize(null);
            ShowIcon = false;
            ShowInTaskbar = false;

            timer.Interval = 1;
            timer.Elapsed += (o, e) =>
            {
                menuOffTicks -= 1;
                if (menuOffTicks <= 0)
                {
                    menuOffTicks = 150;
                    showMenu = false;
                    timer.Stop();
                }
            };

            InitializeComponent();
            client = new HttpClient();
            //ad thing

        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            formHandle = Handle;
            this.DoubleBuffered = true;
            this.Width = Screen.PrimaryScreen.Bounds.Width;// set your own size
            this.Height = Screen.PrimaryScreen.Bounds.Height;
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

            //SetLayeredWindowAttributes(this.Handle, 0, 255, 0);// caution directx error

            //Init DirectX
            device = new WindowRenderTarget(factory, new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)), renderProperties);


            redBrush = new SolidColorBrush(device, new RawColor4(1f, 0f, 0f, 1f));
            blueBrush = new SolidColorBrush(device, new RawColor4(0f, 0f, 1f, 1f));
            // Init font's
            font = new TextFormat(fontFactory, fontFamily, fontSize);
            fontSmall = new TextFormat(fontFactory, fontFamily, fontSizeSmall);
            //line = new device.DrawLine;
            WindowState = FormWindowState.Maximized;

            sDX = new Thread(new ParameterizedThreadStart(sDXThread));

            sDX.IsBackground = false;

            //MessageBox.Show("New CS Update, which means ESP and Aimbot don't work. ");
            var loginForm = new LoginForm(this);
            var status = "denied";
            client.Timeout = TimeSpan.FromSeconds(5);
            string responseString = null;
#if DEBUG
            status = "success";
#endif
            while (status != "success")
            {
                loginForm.ShowDialog();
                var values = loginForm.doThing();

                var content = new FormUrlEncodedContent(values);
                HttpResponseMessage response = null;
                try
                {
                    response = await client.PostAsync("http://109.189.178.82:8080/login", content);
                    responseString = await response.Content.ReadAsStringAsync();
                }
                catch
                {
                    MessageBox.Show("Server is offline. The cheat is unavailable.");
                    Process.GetCurrentProcess().Kill();
                    responseString = "Denied access: Server offline.";
                }
                if (responseString.Contains("<html"))
                {
                    responseString = "Server Error.";
                }
                if (responseString != "success")
                {
                    MessageBox.Show("Server response: \n" + responseString);
                }
                status = responseString;
            }
            MessageBox.Show("Server response: \n" + responseString);
            controls = new Controls(this);
            controls.Show();
            sDX.Start();
            adBrowser = new WebBrowser();
            adBrowser.Height = Height / 6;
            adBrowser.Width = Width / 10;
            adBrowser.ScrollBarsEnabled = false;
            adBrowser.Location = new System.Drawing.Point(98 * Width / 100 - adBrowser.Width, Height / 2 - adBrowser.Height / 2);
            adBrowser.Url = new Uri("http://detolly.no/ads");
            System.Windows.Forms.Button closeAdButton = new System.Windows.Forms.Button();
            int size = adBrowser.Width / 10;
            closeAdButton.BackColor = System.Drawing.SystemColors.Control;
            closeAdButton.Text = "X";
            closeAdButton.Font = new System.Drawing.Font("Arial", closeAdButton.Height / 3);
            closeAdButton.Size = new System.Drawing.Size(size, size);
            closeAdButton.Click += (s, o) =>
            {
                Controls.Remove(adBrowser);
                Controls.Remove(closeAdButton);
            };
            closeAdButton.Enabled = true;
            closeAdButton.Location = new System.Drawing.Point(98 * Width / 100 - closeAdButton.Size.Width, adBrowser.Location.Y);
            Controls.Add(closeAdButton);
            Controls.Add(adBrowser);

            Application.Idle += Update;
        }

        protected override void OnPaint(PaintEventArgs e)// create the whole form
        {
            int[] marg = new int[] { 0, 0, Width, Height };
            DwmExtendFrameIntoClientArea(this.Handle, ref marg);
        }

        ~Form1()
        {
            sDX.Abort();
            d.Abort();
            bhop.Abort();
            Application.Exit();
        }

        int getModule(string name)
        {
            try
            {
                Process csgo = Process.GetProcessesByName("csgo")[0];
                foreach (ProcessModule m in csgo.Modules)
                {
                    if (m.ModuleName == name)
                    {
                        return (int)m.BaseAddress;
                    }
                }
            }
            catch
            {
                sDX.Abort();
                d.Abort();
                bhop.Abort();
            }
            return 0;
        }

        void DrawFilledRect(float x, float y, float w, float h, bool enemy)
        {
            device.DrawRectangle(new SharpDX.Mathematics.Interop.RawRectangleF(x, y, x + w, y + h), enemy ? redBrush : blueBrush);
        }

        void DrawBoundingBox(float x, float y, float w, float h, float t, bool enemy)
        {
            DrawFilledRect(x, y, w, t, enemy);
            DrawFilledRect(x, y, t, h, enemy);
            DrawFilledRect(x + w, y, t, h, enemy);
            DrawFilledRect(x, y + h, w + t, t, enemy);
        }

        VAMemory vam;
        int bClient;
        int bEngine;
        public bool holdMouse1 = true;
        public bool aimbottoggle = false;
        public bool esptoggle = false;
        public bool triggertoggle = false;
        public bool bhoptoggle = false;
        public bool wallhacktoggle = false;
        Memory csgo;
        public float currentFov = 1;
        Thread d;
        Thread bhop;
        Process csgoProcess;
        IntPtr formHandle;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

        bool showMenu = false;
        RECT t;
        string lastmap = "";
        bool noflashtoggle = false;
        public int lookforKey = 0x1;
        float randomFloat = 0f;

        void Update(object o, EventArgs e)
        {//once per second
            
        }

        private void sDXThread(object sender)
        {
            showwarning();
            vam = new VAMemory("csgo");
            csgo = new Memory("csgo");
            bEngine = getModule("engine.dll");
            bClient = getModule("client.dll");
            Button[] buttons =
                new Button[] {
                    new ToggleableButton("Aimbot", delegate () { aimbottoggle = !aimbottoggle; }),
                    new ToggleableButton("Wallhack", delegate () { wallhacktoggle = !wallhacktoggle; }),
                    new ToggleableButton("ESP", delegate () { esptoggle = !esptoggle; }),
                    new ToggleableButton("Bhop", delegate () { bhoptoggle = !bhoptoggle; }, true),
                    new ToggleableButton("Triggerbot", delegate () { triggertoggle = !triggertoggle; }),
                    new ToggleableButton("Noflash", delegate() { noflashtoggle = !noflashtoggle; }),
                    //new SliderButton("FOV: %value%", delegate (float f) { randomFloat = f; }, abstractValue: 180f)
                };
            menu = new Menu(buttons, (int)Math.Floor((decimal)(Height * 55.5 / 100)), 0);

            d = new Thread(() =>
            {
                bool c = false;
                while (true)
                {
                    while (c == false)
                    {
                        while (GetAsyncKeyState(0x26) < 0)
                        {
                            //aimbottoggle = !aimbottoggle;
                            if (c == false)
                                menu.shiftSelected(false);
                            c = true;
                        }
                        while (GetAsyncKeyState(0x28) < 0)
                        {
                            //wallhacktoggle = !wallhacktoggle;
                            if (c == false)
                                menu.shiftSelected(true);
                            c = true;
                        }
                        while (GetAsyncKeyState(0x25) < 0 || GetAsyncKeyState(0x27) < 0)
                        {
                            //esptoggle = !esptoggle;
                            if (c == false)
                                menu.activateCurrent();
                            c = true;
                        }
                        if (c == true)
                        {
                            showMenu = true;
                            //start timer
                            if (timer.Enabled == true)
                                timer.Stop();
                            menuOffTicks = 150;
                            timer.Start();
                        }
                    }
                    c = false;
                }
            });
            d.Start();
            bhop = new Thread(() =>
            {
                while (true)
                {
                    jumpy();
                    Thread.Sleep(1);
                }
            });
            bhop.Start();
            Thread checkmap = new Thread(() =>
            {
                while (true)
                {
                    var clientstate = vam.ReadInt32((IntPtr)(bEngine + Offsets.oClientState));
                    var name = vam.ReadByteArray((IntPtr)(clientstate + 0x28C), 32);
                    var stringthing = Encoding.ASCII.GetString(name);
                    stringthing = Regex.Replace(stringthing, @"[^\u0020-\u007F].*$+", string.Empty);
                    var isInGame = vam.ReadInt32((IntPtr)clientstate + 0x108);
                    if ((lastmap == "" || lastmap != stringthing) && isInGame == 6)
                    {
                        string location = Process.GetProcessesByName("csgo")[0].MainModule.FileName;
                        location = location.Substring(0, location.Length - 8);
                        //stringthing = stringthing.Substring(0, stringthing.IndexOf(@"\"));
                        location += @"csgo\maps\" + stringthing + ".bsp";
                        if (stringthing.Length > 0)
                        {
                            map = new BSP(location);
                            lastmap = stringthing;
                        }
                    }
                    yield return 0;
                }
            });
            checkmap.Start();
            while (true)
            {
                device.BeginDraw();
                device.Clear(new RawColor4(1f, 1f, 1f, 0f));
                int id = 0;
                var g2 = GetForegroundWindow();
                GetWindowThreadProcessId(g2, out id);
#if !DEBUG
                if (id != csgoProcess.Id) { device.EndDraw(); continue; }
#endif
                if (showMenu == true)
                    menu.showMenu(device, font);
                t = new RECT();
                GetWindowRect(GetForegroundWindow(), out t);
                int sd = t.Right;
                int st = t.Bottom;

                windowWidth = t.Right - t.Left;
                windowHeight = t.Bottom - t.Top;
                device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Aliased;// you can set another text mode
                //device.DrawText("http://detolly.no | Private Cheat | Copyright 2017.", font, new SharpDX.Mathematics.Interop.RawRectangleF(t.Left + 5, t.Top + 20, t.Right + 600, t.Bottom + 20), redBrush);
                //device.DrawText("F1 | Aimbot: " + (aimbottoggle ? "On." : "Off.") + " | Fov: " + currentFov, font, new SharpDX.Mathematics.Interop.RawRectangleF(t.Left + 5, t.Top + 60, t.Right + 600, t.Bottom + 100), redBrush);
                //device.DrawText("F2 | Wallhack: " + (wallhacktoggle ? "On." : "Off."), font, new SharpDX.Mathematics.Interop.RawRectangleF(t.Left + 5, t.Top + 100, t.Right + 600, t.Bottom + 100), redBrush);
                //device.DrawText("F3 | ESP: " + (esptoggle ? "On." : "Off."), font, new SharpDX.Mathematics.Interop.RawRectangleF(t.Left + 5, t.Top + 140, 600, t.Bottom + 140), redBrush);
                //device.DrawText("F6 | Triggerbot: " + (triggertoggle ? "On." : "Off."), font, new SharpDX.Mathematics.Interop.RawRectangleF(t.Left + 5, t.Top + 220, t.Right + 600, t.Bottom + 220), redBrush);
                //device.DrawText("F4 | Bhop: " + (bhoptoggle ? "On." : "Off."), font, new SharpDX.Mathematics.Interop.RawRectangleF(t.Left + 5, t.Top + 180, t.Right + 600, t.Bottom + 220), redBrush);

                //var g22 = getMatrixFloats2(csgo, bClient);
                //string g22text = "";
                //for (int i = 0; i < g22.Length; i++)
                //{
                //    for (int x = 0; x < g22[i].Length; x++)
                //    {
                //        g22text += g22[i][x] + "\t";
                //    }
                //    g22text += "\r\n";
                //}
                //g22text += "First Length: " + g22.Length + "\tSecond Length: " + g22[0].Length;
                //device.DrawText(
                //    g22text,
                //    font,
                //    new SharpDX.Mathematics.Interop.RawRectangleF(0,100000,100000,windowHeight/2),
                //    blueBrush
                //    );
                //device.DrawText("Player Position: " + getLocalPlayerPosition(vam, bClient).x + ", " + getLocalPlayerPosition(vam, bClient).y + ", " + getLocalPlayerPosition(vam, bClient).z
                //    , font
                //    , new SharpDX.Mathematics.Interop.RawRectangleF(0, windowHeight - 20, 1000, windowHeight)
                //    , redBrush);

                Esp();
                device.EndDraw();
                seethroughthing();
                Aimbot();
                tragger();
                noflash();
                if (performance)
                    Thread.Sleep(5);
            }
        }

        private void Esp()
        {
            if (esptoggle)
            {
                int localplayer = vam.ReadInt32((IntPtr)(bClient + Offsets.oLocalPlayer));
                int playerIndex = vam.ReadInt32((IntPtr)(localplayer + 0x64)) - 1;
                int i = 0;
                do
                {
                    if (playerIndex == i) { i++; continue; }
                    int entity = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (i) * Offsets.oEntityLoopDistance));
                    int dormant = vam.ReadInt32((IntPtr)(entity + Offsets.oDormant));
                    if (isAlive(i, vam, bClient) && !(dormant == 1 || dormant == 2))
                    {
                        Point aimAt = new Point();
                        bool g = WorldToScreen(getPlayerVectorByIndex(vam, bClient, i), ref aimAt, vam, getMatrixFloats2(csgo, bClient), t);
                        if (g && aimAt.x > t.Left && aimAt.x < t.Right && aimAt.y > t.Top && aimAt.y < t.Bottom)
                        {
                            float d = Get3dDistance(getLocalPlayerPosition(vam, bClient), getPlayerVectorByIndex(vam, bClient, i));
                            float width = 20000 / d;
                            float height = 47500 / d;
                            int team = vam.ReadInt32((IntPtr)(entity + Offsets.oTeam));
                            if (team == 3)
                            {
                                DrawBoundingBox(aimAt.x - (width / 2), aimAt.y - height, width, height, 1, false);
                                device.DrawLine(
                                    new RawVector2(windowWidth / 2 + t.Left, windowHeight + t.Top),
                                    new RawVector2(aimAt.x, aimAt.y),
                                    blueBrush,
                                    1f);
                            }
                            else if (team == 2)
                            {
                                DrawBoundingBox(aimAt.x - (width / 2), aimAt.y - height, width, height, 1, true);
                                device.DrawLine(
                                    new RawVector2(windowWidth / 2 + t.Left, windowHeight + t.Top),
                                    new RawVector2(aimAt.x, aimAt.y),
                                    redBrush,
                                    1f);
                            }
                            if (team == 2 || team == 3)
                            {
                                //DrawText("Health: " + , aimAt.x - width/2, aimAt.y - height - height/3);
                                DrawHealthBar(vam.ReadInt32((IntPtr)(entity + Offsets.oHealth)), aimAt.x, aimAt.y, width, height, d);
                                var a = vam.ReadInt32((IntPtr)(bClient + Offsets.dwRadarBase));
                                a = vam.ReadInt32((IntPtr)(a + 0x20));
                                byte[] returnbytes = csgo.Read((IntPtr)(a + 0x1E0 * i + 0x204), 32);
                                byte[] returnbytes2 = vam.ReadByteArray((IntPtr)(a + 0x1E0 * i + 0x204), 32);
                                string asd = System.Text.Encoding.Unicode.GetString(returnbytes2, 0, 32);
                                TextFormat thisFont = new TextFormat(fontFactory, fontFamily, width * 30 / 100);
                                //device.DrawText(asd, thisFont, new RawRectangleF(aimAt.x-width/2,aimAt.y-height-10,Width,Height), new SolidColorBrush(device, new RawColor4(1,1,1,1)));
                            }
                        }
                    }
                    i++;
                } while (i < 65);
            }
        }

        void noflash()
        {
            int localPlayer = vam.ReadInt32((IntPtr)bClient + Offsets.oLocalPlayer);
            if (noflashtoggle)
            {
                if (vam.ReadFloat((IntPtr)localPlayer + Offsets.oFlashMaxAlpha) > 0.0f)
                {
                    vam.WriteFloat((IntPtr)localPlayer + Offsets.oFlashMaxAlpha, 0.0f);
                }
            }
            else if (vam.ReadFloat((IntPtr)localPlayer + Offsets.oFlashMaxAlpha) < 0f + float.Epsilonh)
            {
                vam.WriteFloat((IntPtr)localPlayer + Offsets.oFlashMaxAlpha, 255.0f);
            }
        }

        private static void showwarning()
        {
            MessageBox.Show(@"Warning:
I do not condone the use of cheats either online or offline, especially in competitive games.
By using this cheat, you acknowledge that I'm not responsible for anything that happens to you or your account.
Have fun, and make sure everyone else is having fun too. Nobody likes a cheater.

With that said, by using this software, you're one hundred percent responsible for what consequences might occur to you or anyone else.

I'm distributing this software for educational purposes ONLY.", "Warning", MessageBoxButtons.OK);
        }

        public bool aimbotautoshoot = false;

        void DrawHealthBar(float health, float x, float y, float width, float height, float distance)
        {
            var rect = new RawRectangleF(
                x - (float)Math.Floor(width / 2), y - height - 5, x + (float)Math.Floor(width / 2), y - height - 3);
            device.FillRectangle(rect, redBrush);
            // x2 is between a range, where it goes from x-width/2 to x+width/2
            // what I want to do is get the percentage of lives left and then convert
            // that into pixels and MAP the percentage of lives to x-width/2 to x+width/2.
            // The way I do that is by shifting zero.
            float max = (x + width / 2);
            float min = (x - width / 2);
            float z = max - min;
            float p = (z * health) / 100;
            p = min + p;
            device.FillRectangle(new RawRectangleF(
                x - (float)Math.Floor(width / 2), y - height - 5, p, y - height - 3)
                , new SolidColorBrush(device, new RawColor4(0, 1, 0, 1)));
        }

        [DllImport("user32")]
        public static extern short GetAsyncKeyState(int vKey);

        class Player
        {
            public float r;
            public float g;
            public float b;
            public float a;
            public bool rwo;
            public bool rwuo;

            public Player(float r, float g, float b, float a, bool rwo, bool rwuo)
            {
                this.r = r;
                this.g = g;
                this.b = b;
                this.a = a;
                this.rwo = rwo;
                this.rwuo = rwuo;
            }
        }

        class Enemy : Player
        {
            public Enemy() : base(1, 0, 0, 0.75f, true, false) { }
        }

        class Team : Player
        {
            public Team() : base(0, 0, 1, 0.75f, true, false) { }
        }

        bool setAngles(Angles setAngles2)
        {

            //if (setAngles.x < 180f && setAngles.x > -180f && setAngles.y > -180f && setAngles.y < 180f)
            //{
            Angles currentAngles = getCurrentAngles(vam, bEngine);
            float fov = currentFov;
            Angles setAngles = setAngles2;
            if (setAngles.y < 0) { setAngles.y += 180f; }
            if (currentThing >= 0 || (setAngles.x - currentAngles.x < fov && setAngles.x - currentAngles.x > -fov && setAngles.y - currentAngles.y < fov && setAngles.y - currentAngles.y > -fov))
            {
                setAngles = SmoothAngles(getCurrentAngles(vam, bEngine), setAngles, smoothing);
                vam.WriteFloat((IntPtr)(vam.ReadInt32((IntPtr)(bEngine + Offsets.oClientState)) + Offsets.oViewAngles), setAngles2.x);
                vam.WriteFloat((IntPtr)(vam.ReadInt32((IntPtr)(bEngine + Offsets.oClientState)) + Offsets.oViewAngles + 4), setAngles2.y);
                return true;
            }
            //}
            return false;
        }

        int currentThing = -1;

        int random = 123;
        private string globalflag;

        public Angles SmoothAngles(Angles src, Angles dest, float smooth)
        {
            return src + (dest - src) * smooth;
        }

        [DllImport("user32.dll")]
        public static extern long SetCursorPos(int x, int y);

        public bool spottedNeedsToBeTrue = true;

        public int boneIndex = 8;
        public bool performance = false;

        void Aimbot()
        {
            if (!aimbottoggle) return;
            int forceAttack = bClient + Offsets.oAttack;
            if (GetAsyncKeyState(lookforKey) < 0 || !holdMouse1)
            {
                int playerIndex = vam.ReadInt32((IntPtr)(vam.ReadInt32((IntPtr)(bClient + Offsets.oLocalPlayer)) + 0x64)) - 1;
                float smallestDistance = float.MaxValue;
                int indexOfSmallestDistance = -1;
                var localPlayer2 = GetBonePosition(2, playerIndex, bClient, vam);
                Vector3 localPlayer = getLocalPlayerPosition(vam, bClient);

#region Closest to Crosshair
                for (int j = 0; j < 65; j++)
                {
                    if (j == playerIndex) { continue; }
                    if (isAlive(j, vam, bClient))
                    {
                        if (!isEntityByIdOnMyTeam(playerIndex, j, vam, bClient))
                        {
                            Point aimAt = new Point();
                            bool g = WorldToScreen(getPlayerVectorByIndex(vam, bClient, j), ref aimAt, vam, getMatrixFloats2(csgo, bClient), t);
                            if (aimAt.x != 200)
                            {
                                float currentDistance = float.MaxValue;
                                float WidthDiv2 = (t.Right - t.Left) / 2;
                                float HeightDiv2 = (t.Bottom - t.Top) / 2;
                                currentDistance = (float)Math.Sqrt(Math.Pow(Math.Abs(WidthDiv2 - aimAt.x), 2) + (float)Math.Pow(Math.Abs(aimAt.y - HeightDiv2), 2));
                                //currentDistance = (float)Math.Sqrt(currentDistance);
                                if (currentDistance < smallestDistance)
                                {
                                    localPlayer2 = GetBonePosition(37, playerIndex, bClient, vam);
                                    localPlayer = getLocalPlayerPosition(vam, bClient);
                                    if (map.IsVisible(localPlayer2, GetBonePosition(boneIndex, j, bClient, vam)) || map.IsVisible(localPlayer, GetBonePosition(boneIndex, j, bClient, vam)) || !spottedNeedsToBeTrue)
                                    {
                                        smallestDistance = currentDistance;
                                        indexOfSmallestDistance = j;
                                    }
                                }
                            }
                        }
                    }
                }

                //int entity2 = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (indexOfSmallestDistance) * Offsets.oEntityLoopDistance));
                if (indexOfSmallestDistance != -1)
                {
                    calcang2(playerIndex, localPlayer, GetBonePosition(boneIndex, indexOfSmallestDistance, bClient, vam), vam, bClient, bEngine, currentFov, currentThing, smoothing, this);
                    if (aimbotautoshoot)
                    {
                        vam.WriteInt32((IntPtr)forceAttack, 6);
                    }
                }

#endregion
#region First Index
                //int indexOfPlayerToAimAt = -99;
                //int i = 1;
                //do
                //{
                //    indexOfPlayerToAimAt = i - 1;
                //    if (indexOfPlayerToAimAt == playerIndex) { i++; continue; }
                //    if (currentThing == -1)
                //    {
                //        if (isAlive(indexOfPlayerToAimAt, vam, bClient))
                //        {
                //            if (!isEntityByIdOnMyTeam(playerIndex, indexOfPlayerToAimAt, vam, bClient))
                //            {
                //                int entity = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (indexOfPlayerToAimAt) * Offsets.oEntityLoopDistance));
                //                if (map.IsVisible(localPlayer2, GetBonePosition(boneIndex, indexOfPlayerToAimAt, bClient, vam)) || map.IsVisible(localPlayer, GetBonePosition(boneIndex, indexOfPlayerToAimAt, bClient, vam)) || !spottedNeedsToBeTrue)
                //                {
                //                    //    if ((vam.ReadInt32((IntPtr)(entity + 0x97C)) & ( 1 << playerIndex)) == 1 || !spottedNeedsToBeTrue)
                //                    //{
                //                    Angles angles = calcang2(playerIndex, localPlayer, GetBonePosition(boneIndex, indexOfPlayerToAimAt, bClient, vam), vam, bClient, bEngine, currentFov, currentThing, smoothing, this);
                //                    if (angles.x != 200) currentThing = indexOfPlayerToAimAt;
                //                    if (aimbotautoshoot)
                //                    {
                //                        vam.WriteInt32((IntPtr)forceAttack, 6);
                //                    }
                //                    //}
                //                }
                //            }
                //        }
                //    }
                //    i++;
                //} while (i < 65);
                //currentThing = -1;
#endregion

            }
        }

        void doStuff()
        {
            random += 5;
            if (random > 51111)
            {
                random = 0;
            }
        }

        void DrawText(string text, float x, float y, Brush brush = null)
        {
            device.DrawText(text, font, new SharpDX.Mathematics.Interop.RawRectangleF(x, windowHeight, windowWidth, y), brush != null ? brush : redBrush);
        }

        void seethroughthing()
        {
            if (!wallhacktoggle) return;
            Enemy Enemy = new Enemy();
            Team Friendly = new Team();

            int i = 1;
            doStuff();

            do
            {
                var d = bClient + Offsets.oLocalPlayer;
                int LocalPlayer = vam.ReadInt32((IntPtr)d);

                int Team = vam.ReadInt32((IntPtr)(LocalPlayer + Offsets.oTeam));

                int EntityList = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (i - 1) * Offsets.oEntityLoopDistance));

                int OtherTeam = vam.ReadInt32((IntPtr)(EntityList + Offsets.oTeam));
                int Glow = vam.ReadInt32((IntPtr)(EntityList + Offsets.oGlowIndex));
                int Object = vam.ReadInt32((IntPtr)(bClient + Offsets.oGlowObject));

                if (OtherTeam == 2)
                {
                    int x = Glow * 0x38 + 0x4;
                    int current = Object + x;
                    vam.WriteFloat((IntPtr)current, Enemy.r);

                    x = Glow * 0x38 + 0x8;
                    current = Object + x;
                    vam.WriteFloat((IntPtr)current, Enemy.g);

                    x = Glow * 0x38 + 0xC;
                    current = Object + x;
                    vam.WriteFloat((IntPtr)current, Enemy.b);

                    x = Glow * 0x38 + 0x10;
                    current = Object + x;
                    vam.WriteFloat((IntPtr)current, Enemy.a);

                    x = Glow * 0x38 + 0x24;
                    current = Object + x;
                    vam.WriteBoolean((IntPtr)current, Enemy.rwo);

                    x = Glow * 0x38 + 0x25;
                    current = Object + x;
                    vam.WriteBoolean((IntPtr)current, Enemy.rwuo);
                }
                else if (OtherTeam == 3)
                {
                    int x = Glow * 0x38 + 0x4;
                    int current = Object + x;
                    vam.WriteFloat((IntPtr)current, Friendly.r);

                    x = Glow * 0x38 + 0x8;
                    current = Object + x;
                    vam.WriteFloat((IntPtr)current, Friendly.g);

                    x = Glow * 0x38 + 0xC;
                    current = Object + x;
                    vam.WriteFloat((IntPtr)current, Friendly.b);

                    x = Glow * 0x38 + 0x10;
                    current = Object + x;
                    vam.WriteFloat((IntPtr)current, Friendly.a);

                    x = Glow * 0x38 + 0x24;
                    current = Object + x;
                    vam.WriteBoolean((IntPtr)current, Friendly.rwo);

                    x = Glow * 0x38 + 0x25;
                    current = Object + x;
                    vam.WriteBoolean((IntPtr)current, Friendly.rwuo);
                }
                i++;
            } while (i < 65);
        }

        void tragger()
        {
            if (!triggertoggle) return;
            int forceAttack = bClient + Offsets.oAttack;

            int LocalPlayer = vam.ReadInt32((IntPtr)(bClient + Offsets.oLocalPlayer));
            int Team = vam.ReadInt32((IntPtr)(LocalPlayer + Offsets.oTeam));
            int PlayerInCross = vam.ReadInt32((IntPtr)(LocalPlayer + Offsets.oCrossHairID));

            if (PlayerInCross >= 0 && PlayerInCross < 65)
            {
                int PtrToPIC = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (PlayerInCross - 1) * Offsets.oEntityLoopDistance));
                int PICHealth = vam.ReadInt32((IntPtr)(PtrToPIC + Offsets.oHealth));
                int PICTeam = vam.ReadInt32((IntPtr)(PtrToPIC + Offsets.oTeam));

                if (PICTeam != Team && PICTeam > 1 && PICHealth > 0)
                {
                    vam.WriteInt32((IntPtr)forceAttack, 6);
                }
            }
        }

        void jumpy()
        {
            if (!bhoptoggle) return;
            int fJump = bClient + Offsets.oJump;
            if (GetAsyncKeyState(0x20) < 0)
            {
                var d = bClient + Offsets.oLocalPlayer;
                int LocalPlayer = csgo.Read((IntPtr)d);

                int aFlags = LocalPlayer + Offsets.oFlags;
                var Flags = vam.ReadInt32((IntPtr)aFlags);
                if (Flags == 257 || Flags == 263)
                {
                    vam.WriteInt32((IntPtr)fJump, 6);
                }
                globalflag = Flags.ToString();
            }
        }

        public void DrawLine(SharpDX.Mathematics.Interop.RawColor4 color, Point from, Point to, float strokeWidth = 5f)
        {
            if (device == null)
                throw new SharpDXException("The device was not initialized yet");
            using (SolidColorBrush brush = new SolidColorBrush(device, color))
            {
                device.DrawLine(new SharpDX.Mathematics.Interop.RawVector2(from.x, from.y), new SharpDX.Mathematics.Interop.RawVector2(to.x, to.y), brush, strokeWidth);
            }
        }
    }

    /*
     * 
     * other aimbot code
     * 
     *
     *          float bestDistance = float.PositiveInfinity;
                for (int g = 0; g < 64; g++)
                {
                    Point aimAt = new Point();
                    bool worked = WorldToScreen(getPlayerVectorByIndex(vam, bClient, g), ref aimAt, vam, getMatrixFloats2(csgo, bClient), t);
                    if (worked)
                    {
                        var x1 = aimAt.x;
                        var x2 = Width / 2;
                        var y1 = aimAt.y;
                        var y2 = Height / 2;
                        //this will change relative to where x1 and y1 is.
                        float relativeDistance = 0f;
                        if (x1 > Width / 2 && y1 < Height / 2)
                            relativeDistance = (x1 - x2) * (x1 - x2) + (y2 - y1) * (y2 - y1);
                        else if (x1 > Width / 2 && y1 > Height / 2)
                            relativeDistance = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2);
                        else if (x1 < Width / 2 && y1 > Height / 2)
                            relativeDistance = (y1 - y2) * (y1 - y2) + (x2 - x1) * (x2 - x1);
                        else if (x2 < Width / 2 && y1 < Height / 2)
                            relativeDistance = (y2 - y1) * (y2 - y1) + (x2 - x1) * (x2 - x1);
                        if (relativeDistance < bestDistance)
                        {
                            bestDistance = relativeDistance;
                            indexOfPlayerToAimAt = g;
                        }
                    }
                }
                if (isAlive(indexOfPlayerToAimAt, vam, bClient))
                {
                    if (!isEntityByIdOnMyTeam(playerIndex, indexOfPlayerToAimAt, vam, bClient))
                    {
                        int entity = vam.ReadInt32((IntPtr)(bClient + Offsets.oEntityList + (indexOfPlayerToAimAt) * Offsets.oEntityLoopDistanceTrigger));
                        if (vam.ReadInt32((IntPtr)(entity + Offsets.oSpotted)) == 1)
                        {
                                Angles angles = calcang2(playerIndex, localPlayer, GetBonePosition(indexOfPlayerToAimAt, bClient, vam), vam, bClient, bEngine, currentFov, currentThing, smoothing, this);
                                currentThing = indexOfPlayerToAimAt;
                        }
                    }
                } 
     *
     * 
     * 
     * 
     * 
     */
}
