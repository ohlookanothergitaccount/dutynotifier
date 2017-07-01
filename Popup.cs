using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX.Direct2D1;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.DXGI;
using System.Diagnostics;

namespace dutynotifier {
    public partial class Popup : Form {

        #region ida.native.win32
        [Flags]
        public enum ProcessAccessFlags : uint {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VirtualMemoryOperation = 0x00000008,
            VirtualMemoryRead = 0x00000010,
            VirtualMemoryWrite = 0x00000020,
            DuplicateHandle = 0x00000040,
            CreateProcess = 0x000000080,
            SetQuota = 0x00000100,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            QueryLimitedInformation = 0x00001000,
            Synchronize = 0x00100000
        }


        [StructLayout( LayoutKind.Sequential )]
        public struct RECT {
            private int _Left;
            private int _Top;
            private int _Right;
            private int _Bottom;

            public RECT( RECT Rectangle ) : this( Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom ) {
            }
            public RECT( int Left, int Top, int Right, int Bottom ) {
                _Left = Left;
                _Top = Top;
                _Right = Right;
                _Bottom = Bottom;
            }

            public int X {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Y {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Left {
                get { return _Left; }
                set { _Left = value; }
            }
            public int Top {
                get { return _Top; }
                set { _Top = value; }
            }
            public int Right {
                get { return _Right; }
                set { _Right = value; }
            }
            public int Bottom {
                get { return _Bottom; }
                set { _Bottom = value; }
            }
            public int Height {
                get { return _Bottom - _Top; }
                set { _Bottom = value + _Top; }
            }
            public int Width {
                get { return _Right - _Left; }
                set { _Right = value + _Left; }
            }
            public System.Drawing.Point Location {
                get { return new System.Drawing.Point( Left, Top ); }
                set {
                    _Left = value.X;
                    _Top = value.Y;
                }
            }
            public Size Size {
                get { return new Size( Width, Height ); }
                set {
                    _Right = value.Width + _Left;
                    _Bottom = value.Height + _Top;
                }
            }

            [DllImport( "dwmapi.dll", SetLastError = true )]
            public static extern void DwmExtendFrameIntoClientArea( IntPtr hWnd, ref int[] pMargins );

            public static implicit operator System.Drawing.Rectangle( RECT Rectangle ) {
                return new System.Drawing.Rectangle( Rectangle.Left, Rectangle.Top, Rectangle.Width, Rectangle.Height );
            }
            public static implicit operator RECT( System.Drawing.Rectangle Rectangle ) {
                return new RECT( Rectangle.Left, Rectangle.Top, Rectangle.Right, Rectangle.Bottom );
            }
            public static bool operator ==( RECT Rectangle1, RECT Rectangle2 ) {
                return Rectangle1.Equals( Rectangle2 );
            }
            public static bool operator !=( RECT Rectangle1, RECT Rectangle2 ) {
                return !Rectangle1.Equals( Rectangle2 );
            }

            public override string ToString() {
                return "{Left: " + _Left + "; " + "Top: " + _Top + "; Right: " + _Right + "; Bottom: " + _Bottom + "}";
            }

            public override int GetHashCode() {
                return ToString().GetHashCode();
            }

            public bool Equals( RECT Rectangle ) {
                return Rectangle.Left == _Left && Rectangle.Top == _Top && Rectangle.Right == _Right && Rectangle.Bottom == _Bottom;
            }

            public override bool Equals( object Object ) {
                if( Object is RECT ) {
                    return Equals( ( RECT )Object );
                } else if( Object is System.Drawing.Rectangle ) {
                    return Equals( new RECT( ( System.Drawing.Rectangle )Object ) );
                }

                return false;
            }
        }

        [DllImport( "dwmapi.dll", SetLastError = true )]
        public static extern void DwmExtendFrameIntoClientArea( IntPtr hWnd, ref int[] pMargins );

        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern IntPtr OpenProcess( ProcessAccessFlags processAccess, bool bInheritHandle, int processId );

        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool CloseHandle( System.IntPtr handle );
        [DllImport( "kernel32.dll", SetLastError = true )]
        public static extern bool ReadProcessMemory( System.IntPtr hProcess, System.IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead );
        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern IntPtr GetForegroundWindow();

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool SetForegroundWindow( IntPtr hWnd );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool GetWindowRect( IntPtr hWnd, out RECT lpRect );
        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern bool ClientToScreen( IntPtr hWnd, ref POINT lpPoint );

        [DllImport( "user32.dll" )]
        public static extern bool PrintWindow( IntPtr hWnd, IntPtr hdcBlt, int nFlags );

        [DllImport( "user32.dll", CharSet = CharSet.Auto, SetLastError = true )]
        public static extern int GetSystemMetrics( int id );
        const int SM_CXSIZEFRAME = 32;
        const int SM_CYSIZEFRAME = 33;
        const int SM_CYCAPTION = 4;
        #endregion

        #region initialize

        public Popup() {
            InitializeComponent();

            this.DoubleBuffered = true;
            this.SetStyle( ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor, true );
            this.TopMost = true;

            this.InitializeSharpDx();
        }

        public struct POINT {
            public int X, Y;
        }

        protected override void OnPaint( PaintEventArgs e ) {
            int[] marg = new int[] { 0, 0, Width, Height };
            DwmExtendFrameIntoClientArea( this.Handle, ref marg );

            e.Graphics.FillRectangle( Brushes.White, 17, 53, 463 - 17, 420 - 53 );
        }

        protected SharpDX.Direct2D1.Factory Factory { get; set; }
        protected SharpDX.DirectWrite.Factory FontFactory { get; set; }
        protected SharpDX.Direct2D1.WindowRenderTarget Device { get; set; }
        protected SharpDX.DirectWrite.TextFormat TimeTextFormat { get; set; }
        protected SharpDX.Direct2D1.SolidColorBrush TimeBrush { get; set; }
        protected SharpDX.Direct2D1.Bitmap DutyEntryBackgroundImage { get; set; }

        private void InitializeSharpDx() {
            this.Factory = new SharpDX.Direct2D1.Factory();
            this.FontFactory = new SharpDX.DirectWrite.Factory();

            var renderProp = new SharpDX.Direct2D1.HwndRenderTargetProperties() {
                Hwnd = this.Handle,
                PixelSize = new SharpDX.Size2( this.Width, this.Height ),
                PresentOptions = SharpDX.Direct2D1.PresentOptions.None
            };

            this.Device = new SharpDX.Direct2D1.WindowRenderTarget(
                this.Factory,
                new SharpDX.Direct2D1.RenderTargetProperties(
                    new SharpDX.Direct2D1.PixelFormat(
                        SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                        SharpDX.Direct2D1.AlphaMode.Premultiplied )
                    ),
                renderProp );

            this.Device.TextAntialiasMode = SharpDX.Direct2D1.TextAntialiasMode.Cleartype;

            this.TimeTextFormat = new SharpDX.DirectWrite.TextFormat( this.FontFactory, "Arial", SharpDX.DirectWrite.FontWeight.DemiBold, SharpDX.DirectWrite.FontStyle.Normal, 21.35f );

            this.TimeBrush = new SharpDX.Direct2D1.SolidColorBrush( this.Device, new SharpDX.Color4( 168 / 255.0f, 58 / 255.0f, 58 / 255.0f, 1.0f ) );

            this.DutyEntryBackgroundImage = ConvertBitmap( this.Device, dutynotifier.Properties.Resources.background );

        }

        public static SharpDX.Direct2D1.Bitmap ConvertBitmap( RenderTarget renderTarget, System.Drawing.Bitmap bitmap ) {
            // Loads from file using System.Drawing.Image
            var sourceArea = new System.Drawing.Rectangle( 0, 0, bitmap.Width, bitmap.Height );
            var bitmapProperties = new BitmapProperties( new PixelFormat( Format.R8G8B8A8_UNorm, AlphaMode.Premultiplied ) );
            var size = new Size2( bitmap.Width, bitmap.Height );

            // Transform pixels from BGRA to RGBA
            int stride = bitmap.Width * sizeof( int );
            using( var tempStream = new DataStream( bitmap.Height * stride, true, true ) ) {
                // Lock System.Drawing.Bitmap
                var bitmapData = bitmap.LockBits( sourceArea, System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb );

                // Convert all pixels 
                for( int y = 0; y < bitmap.Height; y++ ) {
                    int offset = bitmapData.Stride * y;
                    for( int x = 0; x < bitmap.Width; x++ ) {
                        // Not optimized 
                        byte B = Marshal.ReadByte( bitmapData.Scan0, offset++ );
                        byte G = Marshal.ReadByte( bitmapData.Scan0, offset++ );
                        byte R = Marshal.ReadByte( bitmapData.Scan0, offset++ );
                        byte A = Marshal.ReadByte( bitmapData.Scan0, offset++ );
                        int rgba = R | ( G << 8 ) | ( B << 16 ) | ( A << 24 );
                        tempStream.Write( rgba );
                    }

                }
                bitmap.UnlockBits( bitmapData );
                tempStream.Position = 0;

                return new SharpDX.Direct2D1.Bitmap( renderTarget, size, tempStream, stride, bitmapProperties );
            }
        }

        private System.Threading.Thread renderThread;

        private void Popup_Load( object sender, EventArgs e ) {
            this.renderThread = new System.Threading.Thread( this.RenderThread ) {
                IsBackground = true
            };

            this.renderThread.Start();
        }

        #endregion

        #region mouseinput
        private bool trace = false, moved = false;
        System.Drawing.Point refPoint;
        IntPtr lastTargetWindow;

        private void Popup_MouseUp( object sender, MouseEventArgs e ) {
            trace = false;
            if( !moved ) {
                SetForegroundWindow( this.lastTargetWindow );
                this.Visible = false;
            }
        }

        private void Popup_MouseDown( object sender, MouseEventArgs e ) {
            trace = true;
            moved = false;
            this.refPoint = e.Location;
        }

        private void Popup_MouseMove( object sender, MouseEventArgs e ) {
            if( trace ) {
                var p2s = new System.Drawing.Point( e.X + this.Location.X - refPoint.X, e.Y + this.Location.Y - refPoint.Y );
                this.Location = p2s;
            }
        }

        #endregion


        private void Draw( int timeLeft, SharpDX.Direct2D1.Bitmap client, System.Drawing.Point popupLoc ) {
            const int bgtop = 53;
            const int bgleft = 18;
            const int bgright = 462;

            //draw
            this.Device.BeginDraw();
            this.Device.Clear( new SharpDX.Color4() );

            this.Device.DrawBitmap( this.DutyEntryBackgroundImage, 1.0f, SharpDX.Direct2D1.BitmapInterpolationMode.Linear );

            if( client != null ) {

                float w;
                float h;

                //upper part
                h = 308 - bgtop;
                w = bgright - bgleft;
                this.Device.DrawBitmap(
                    client,
                    new SharpDX.RectangleF( bgleft, bgtop, w, h ), //dst
                    1.0f,
                    BitmapInterpolationMode.Linear,
                    new SharpDX.RectangleF( popupLoc.X, popupLoc.Y, w, h ) //src
                    );

                //center icon
                h = 80 - 38;
                w = 258 - 226;
                this.Device.DrawBitmap(
                    client,
                    new SharpDX.RectangleF( 226, 38, w, h ), //dst
                    1.0f,
                    BitmapInterpolationMode.Linear,
                    new SharpDX.RectangleF( popupLoc.X + ( 226 - bgleft ), popupLoc.Y + ( 38 - bgtop ), w, h ) //src
                    );

                //class
                h = 360 - 308;
                w = 730 - 444;
                this.Device.DrawBitmap(
                    client,
                    new SharpDX.RectangleF( bgleft, 308, w, h ), //dst
                    1.0f,
                    BitmapInterpolationMode.Linear,
                    new SharpDX.RectangleF( popupLoc.X, popupLoc.Y + ( 308 - bgtop ), w, h ) //src
                    );

                //draw time
                this.Device.DrawText( "0:" + timeLeft.ToString( "00" ), this.TimeTextFormat, new SharpDX.RectangleF( 794 - 444 + bgleft, 459 - 178 + bgtop, 100, 30 ), this.TimeBrush );
            }

            this.Device.EndDraw();
        }

        System.Drawing.Point popupLoc = new System.Drawing.Point();
        private void RenderThread() {
            int i = 0;
            SharpDX.Direct2D1.Bitmap bmp = null;

            PopupState popupstate = PopupState.NotVisible;

            while( true ) {
                int seconds;
                System.Drawing.Point wndLoc;
                System.Drawing.Point newpopupLoc;

                //update
                Process p = this.Update( out seconds, ref popupstate, out newpopupLoc );

                if( p != null ) {
                    lastTargetWindow = p.MainWindowHandle;

                    switch( popupstate ) {
                        case PopupState.Visible:
                            if( bmp == null ) {
                                //get image from client
                                bmp = ConvertBitmap( this.Device, ( System.Drawing.Bitmap )PrintWindow( p.MainWindowHandle, out wndLoc ).Clone() );

                                popupLoc = new System.Drawing.Point( newpopupLoc.X + 10, newpopupLoc.Y + 36 );
                                
                                //add offsets by window frame
                                POINT clientCoord = new POINT();
                                ClientToScreen( p.MainWindowHandle, ref clientCoord );

                                popupLoc.X += clientCoord.X - wndLoc.X;
                                popupLoc.Y += clientCoord.Y - wndLoc.Y;
                                
                                //position popup at same position as ingame
                                this.Invoke( new Action( () => {
                                    Location = new System.Drawing.Point( wndLoc.X + popupLoc.X - 17, wndLoc.Y + popupLoc.Y - 53 );
                                } ) );
                            }

                            if( !this.Visible ) {
                                this.Invoke( new Action( () => {
                                    this.Visible = true;
                                } ) );
                            }

                            this.Draw( seconds, bmp, popupLoc );
                            break;
                        case PopupState.NotVisible:
                        case PopupState.Locked:
                            if( null != bmp ) {
                                bmp.Dispose();
                                bmp = null;
                            }
                            if( this.Visible ) {
                                this.Invoke( new Action( () => {
                                    this.Visible = false;
                                } ) );
                            }
                            break;

                    }
                } else {
                    if( this.Visible ) {
                        this.Invoke( new Action( () => {
                            this.Visible = false;
                        } ) );
                    }
                }

                System.Threading.Thread.Sleep( 1000 );
            }
        }

        enum PopupState {
            NotVisible = 0,
            Visible = 1,
            Locked = 2
        }

        static int lastTime = 0;
        private Process Update( out int seconds, ref PopupState popupstate, out System.Drawing.Point popupLoc ) {
            seconds = 0;
            popupLoc = new System.Drawing.Point();

            Process[] processes = Process.GetProcessesByName( "ffxiv_dx11" );
            if( processes.Length == 0 ) return null;

            Process p = processes[0];

            seconds = this.ReadEntryTime( p );

            bool isActive = p.MainWindowHandle.Equals( GetForegroundWindow() );

            if( isActive ) {
                //window was active 
                popupstate = PopupState.Locked; //hide
            } else {
                if( seconds > 0 && lastTime == 0 && popupstate == PopupState.NotVisible ) {
                    popupLoc = this.ReadPopupLocation( p );
                                        
                    popupstate = PopupState.Visible;
                } else if( seconds == 0 && popupstate == PopupState.Locked ) {
                    popupstate = PopupState.NotVisible;
                }

                lastTime = seconds;
            }

            return p;
        }

        public static System.Drawing.Bitmap PrintWindow( IntPtr hwnd, out System.Drawing.Point location ) {
            RECT rc;
            GetWindowRect( hwnd, out rc );
            location = rc.Location;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap( rc.Width, rc.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb );
            System.Drawing.Graphics gfxBmp = System.Drawing.Graphics.FromImage( bmp );
            IntPtr hdcBitmap = gfxBmp.GetHdc();

            PrintWindow( hwnd, hdcBitmap, 0 );

            gfxBmp.ReleaseHdc( hdcBitmap );
            gfxBmp.Dispose();

            return bmp;
        }

        private readonly int[] timeOffsets = new int[] { 0x178B018, 0x38, 0x18, 0x48, 0x20, 0xa0 * 0x4 };
        private readonly int[] locOffsets = new int[] { 0x178B018, 0x20, 0x9150, 0x1BC };
        private int ReadEntryTime( System.Diagnostics.Process p ) {
            IntPtr ptr = p.MainModule.BaseAddress;
            IntPtr pHandle = IntPtr.Zero;

            try {
                pHandle = OpenProcess( ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryOperation | ProcessAccessFlags.VirtualMemoryRead, false, p.Id );
                if( pHandle.Equals( IntPtr.Zero ) ) return -1;

                byte[] buff = new byte[IntPtr.Size];
                int read;

                ptr = ptr + timeOffsets[0];
                for( int i = 1; i < timeOffsets.Length; i++ ) {
                    if( !ReadProcessMemory( pHandle, ptr, buff, IntPtr.Size, out read ) ) return -1;
                    ptr = new IntPtr( ( buff.Length == 8 ) ? BitConverter.ToInt64( buff, 0 ) : BitConverter.ToInt32( buff, 0 ) );
                    ptr = ptr + timeOffsets[i];
                }

                if( !ReadProcessMemory( pHandle, ptr, buff, IntPtr.Size, out read ) ) return -1;
                return BitConverter.ToInt32( buff, 0 );
            } finally {
                if( !pHandle.Equals( IntPtr.Zero ) ) {
                    CloseHandle( pHandle );
                }
            }
        }
        private System.Drawing.Point ReadPopupLocation( System.Diagnostics.Process p ) {
            IntPtr ptr = p.MainModule.BaseAddress;
            IntPtr pHandle = IntPtr.Zero;

            try {
                pHandle = OpenProcess( ProcessAccessFlags.QueryInformation | ProcessAccessFlags.VirtualMemoryOperation | ProcessAccessFlags.VirtualMemoryRead, false, p.Id );
                if( pHandle.Equals( IntPtr.Zero ) ) return new System.Drawing.Point();

                byte[] buff = new byte[IntPtr.Size];
                int read;

                ptr = ptr + locOffsets[0];
                for( int i = 1; i < locOffsets.Length; i++ ) {
                    if( !ReadProcessMemory( pHandle, ptr, buff, IntPtr.Size, out read ) ) return new System.Drawing.Point();
                    ptr = new IntPtr( ( buff.Length == 8 ) ? BitConverter.ToInt64( buff, 0 ) : BitConverter.ToInt32( buff, 0 ) );
                    ptr = ptr + locOffsets[i];
                }

                if( !ReadProcessMemory( pHandle, ptr, buff, IntPtr.Size, out read ) ) return new System.Drawing.Point();
                return new System.Drawing.Point( BitConverter.ToInt16( buff, 0 ), BitConverter.ToInt16( buff, 2 ) );
            } finally {
                if( !pHandle.Equals( IntPtr.Zero ) ) {
                    CloseHandle( pHandle );
                }
            }
        }
    }
}
