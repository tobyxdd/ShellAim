using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShellAim
{
    public partial class FormMenu : Form
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        private struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        private IntPtr gameWindowHandle;

        private Rect gameWindowRect;

        private const int WM_HOTKEY = 0x0312;

        private bool moveable = false;

        private Point downPos;

        private FormOverlay formOverlay = new FormOverlay();

        private Color overlayColor = Color.LightGreen;

        private float screenRatioX, screenRatioY;

        private int displayAngle, realAngle = 60;

        private int playerX = 200, playerY = 200;

        private int velocity = 60;

        private bool underGroundShot = false;

        public FormMenu(IntPtr handle)
        {
            InitializeComponent();

            Text = Utils.RandomString();

            //SetStyle(ControlStyles.UserPaint, true);
            //SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);

            gameWindowHandle = handle;

            RegisterFormHotkeys();

            formOverlay.Paint += FormOverlay_Paint;
        }

        private void FormOverlay_Paint(object sender, PaintEventArgs e)
        {
            DrawOverlay(e.Graphics);
        }

        private void RefreshOverlay()
        {
            formOverlay.Invalidate();
        }

        private void RegisterFormHotkeys()
        {
            RegisterHotKey(Handle, 101, 0, (int)Keys.Tab);
            RegisterHotKey(Handle, 102, 2, (int)Keys.Tab);

            RegisterHotKey(Handle, 103, 0, (int)Keys.NumPad2);

            RegisterHotKey(Handle, 104, 0, (int)Keys.NumPad4);
            RegisterHotKey(Handle, 105, 0, (int)Keys.NumPad5);
            RegisterHotKey(Handle, 106, 0, (int)Keys.NumPad6);
            RegisterHotKey(Handle, 107, 0, (int)Keys.NumPad8);
        }

        private void UnregisterFormHotkeys()
        {
            UnregisterHotKey(Handle, 101);
            UnregisterHotKey(Handle, 102);
            UnregisterHotKey(Handle, 103);
            UnregisterHotKey(Handle, 104);
            UnregisterHotKey(Handle, 105);
            UnregisterHotKey(Handle, 106);
            UnregisterHotKey(Handle, 107);
        }

        private void SetGameWindowForeground()
        {
            ShowWindow(gameWindowHandle, 4);
            SetForegroundWindow(gameWindowHandle);
        }

        private void ToggleOverlay()
        {
            if (formOverlay.Visible)
            {
                timer.Stop();
                formOverlay.Hide();
                labelStatus.ForeColor = Color.Red;
                labelStatus.Text = "Disabled";
            }
            else
            {
                timer.Start();
                formOverlay.Show();
                labelStatus.ForeColor = Color.Green;
                labelStatus.Text = "Enabled";
                SetGameWindowForeground();
            }
        }

        private void ToggleHelp()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_HOTKEY)
            {
                switch (m.WParam.ToInt32())
                {
                    case 101:
                        ToggleOverlay();
                        break;
                    case 102:
                        ToggleHelp();
                        break;
                    case 103:
                        underGroundShot = !underGroundShot;
                        RefreshOverlay();
                        break;
                    case 104:
                        realAngle++;
                        if (realAngle >= 180) realAngle = 1;
                        RefreshOverlay();
                        break;
                    case 105:
                        if (velocity > 0) velocity--;
                        RefreshOverlay();
                        break;
                    case 106:
                        realAngle--;
                        if (realAngle <= 0) realAngle = 179;
                        RefreshOverlay();
                        break;
                    case 107:
                        if (velocity < 100) velocity++;
                        RefreshOverlay();
                        break;

                }
            }
        }

        private void FormMenu_MouseDown(object sender, MouseEventArgs e)
        {
            downPos = new Point(Cursor.Position.X - Location.X, Cursor.Position.Y - Location.Y);
            moveable = true;
        }

        private void FormMenu_MouseUp(object sender, MouseEventArgs e)
        {
            moveable = false;
        }

        private void FormMenu_MouseMove(object sender, MouseEventArgs e)
        {
            if (moveable)
            {
                Location = new Point(Cursor.Position.X - downPos.X, Cursor.Position.Y - downPos.Y);
            }
        }

        private void ButtonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void FormMenu_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterFormHotkeys();
        }

        private void UpdateDisplayAngle()
        {
            if (realAngle > 90)
            {
                displayAngle = 180 - realAngle;
            }
            else if (realAngle == 0)
            {
                displayAngle = 180;
            }
            else
            {
                displayAngle = realAngle;
            }
        }

        private void DrawOverlay(Graphics graphics)
        {
            //graphics.Clear(formOverlay.BackColor);

            //graphics.SmoothingMode = SmoothingMode.HighQuality;
            //graphics.TextRenderingHint = TextRenderingHint.SingleBitPerPixel;

            graphics.DrawString("ShellAim Overlay", new Font(Constants.OVERLAY_FONT, 14), Brushes.LightGray, 40, 60);

            UpdateDisplayAngle();

            float rad = (float)(realAngle * Math.PI / 180);

            float maxHeightY = (float)(velocity * velocity * Math.Sin(rad) * Math.Sin(rad) / 20 * 1.67 * screenRatioX);
            float a = (float)(4 * maxHeightY / Math.Tan(rad));
            float c = (float)(-Math.Tan(rad) / a * 1.02);

            float maxHeightX = playerX + a / 2;

            if (underGroundShot) c = -c;

            int x = 0, y = 0;

            int convertedY = -playerY + formOverlay.Height;

            try
            {

                Point[] points = new Point[formOverlay.Width];

                while (x < formOverlay.Width)
                {
                    y = (int)(c * ((x - playerX) - a) * (x - playerX));
                    points[x] = new Point(x, -y + convertedY);
                    x++;
                }

                int minPoint, maxPoint;
                if (maxHeightX > playerX)
                {
                    minPoint = playerX;
                    maxPoint = formOverlay.Width;
                }
                else
                {
                    minPoint = 0;
                    maxPoint = playerX;
                }

                Point[] pointsNot = new Point[maxPoint - minPoint];
                x = 0;
                if (maxHeightX > playerX)
                {
                    while (x < maxPoint - minPoint)
                    {
                        pointsNot[x] = points[formOverlay.Width - x - 1];
                        x++;
                    }
                }
                else
                {
                    while (x < maxPoint - minPoint)
                    {
                        pointsNot[x] = points[x];
                        x++;
                    }
                }

                graphics.DrawCurve(new Pen(overlayColor, 2), pointsNot);
            }
            catch (Exception e)
            {
                graphics.DrawString("Unable to render", new Font(Constants.OVERLAY_FONT, 12), Brushes.Red, 40, 100);
            }

            graphics.FillEllipse(new SolidBrush(overlayColor), new Rectangle(playerX - 5, convertedY - 5, 10, 10));

            StringFormat sf = new StringFormat();
            sf.Alignment = StringAlignment.Center;

            graphics.DrawString(velocity + ", " + displayAngle, new Font(Constants.OVERLAY_FONT, 12), new SolidBrush(overlayColor), playerX, convertedY + 20, sf);

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (formOverlay.Visible)
            {
                if (GetForegroundWindow() != gameWindowHandle)
                {
                    if (Utils.GetGameHandler() == IntPtr.Zero)
                    {
                        Application.Exit();
                    }
                    formOverlay.Hide();
                }
                else
                {
                    if (GetAsyncKeyState(Keys.RButton) != 0)
                    {
                        playerX = Cursor.Position.X - formOverlay.Left;
                        playerY = -(Cursor.Position.Y - formOverlay.Top) + formOverlay.Height;
                        UpdateOverlaySizePos(true);
                    }
                    else
                    {
                        UpdateOverlaySizePos(false);
                    }
                }
            }
            else
            {
                if (GetForegroundWindow() == gameWindowHandle)
                {
                    formOverlay.Show();
                    UpdateOverlaySizePos(false);
                }
            }
        }

        private bool RefreshGameWindowRect()
        {
            Rect tmpRect;
            GetWindowRect(gameWindowHandle, out tmpRect);
            if (tmpRect.top == gameWindowRect.top &&
                tmpRect.bottom == gameWindowRect.bottom &&
                tmpRect.left == gameWindowRect.left &&
                tmpRect.right == gameWindowRect.right)
            {
                return false;
            }
            else
            {
                gameWindowRect = tmpRect;
                return true;
            }
        }

        private void UpdateOverlaySizePos(bool forceRefreshOverlay)
        {
            if (RefreshGameWindowRect() || forceRefreshOverlay)
            {
                int gameWidth = Math.Abs(gameWindowRect.right - gameWindowRect.left);
                int gameHeight = Math.Abs(gameWindowRect.bottom - gameWindowRect.top);
                formOverlay.Size = new Size(gameWidth, gameHeight);
                formOverlay.Location = new Point(gameWindowRect.left, gameWindowRect.top);

                screenRatioX = gameWidth / 1280f;
                screenRatioY = gameHeight / 759f;

                RefreshOverlay();
            }
        }
    }
}
