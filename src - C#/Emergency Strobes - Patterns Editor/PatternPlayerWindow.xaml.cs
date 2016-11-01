using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Windows.Threading;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Multimedia;
using SharpDX.Direct3D9;

namespace EmergencyStrobesPatternsEditor
{
    /// <summary>
    /// Interaction logic for PatternPlayerWindow.xaml
    /// </summary>
    public partial class PatternPlayerWindow : Window
    {
        public Pattern Pattern { get; }

        public D3DImage CurrentImage { get; }

        private DirectXPatternRenderer renderer;
        private DispatcherTimer timer;

        private Pattern.Stage currentStage;
        private int currentStageIndex;

        public PatternPlayerWindow(Pattern pattern)
        {
            if (pattern.Stages == null || pattern.Stages.Length <= 0)
                throw new ArgumentException("The Stages array is null or empty.", nameof(pattern));

            Pattern = pattern;


            InitializeComponent();

            Title = "Reproducing " + Pattern.Name;
            
            renderer = new DirectXPatternRenderer(274, 551);

            currentStage = Pattern.Stages[0];
            currentStageIndex = 0;

            RenderStage();

            timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, unchecked((int)currentStage.Milliseconds)), DispatcherPriority.Render, OnTimerTick, Dispatcher);
            timer.Start();
        }



        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();
            renderer?.Dispose();

            base.OnClosed(e);
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            int newStageIndex = currentStageIndex + 1;
            if (newStageIndex >= Pattern.Stages.Length)
                newStageIndex = 0;

            currentStage = Pattern.Stages[newStageIndex];
            currentStageIndex = newStageIndex;
            RenderStage();
            timer.Interval = new TimeSpan(0, 0, 0, 0, unchecked((int)currentStage.Milliseconds));
        }

        private void RenderStage()
        {
            D3DImage.Lock();
            renderer.RenderPatternStageToSurface(currentStage);
            D3DImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, renderer.SurfacePointer);
            D3DImage.AddDirtyRect(new Int32Rect(0, 0, D3DImage.PixelWidth, D3DImage.PixelHeight));
            D3DImage.Unlock();
        }


        internal class DirectXPatternRenderer : IDisposable
        {
            Direct3DEx dx;
            DeviceEx device;
            Surface surface;
            Surface backBuffer;
            IntPtr surfacePointer;

            public IntPtr SurfacePointer { get { return surfacePointer; } }

            public DirectXPatternRenderer(int width, int height)
            {
                dx = new Direct3DEx();

                HwndSource hwnd = new HwndSource(0, 0, 0, 0, 0, width, height, "DirectXControl", IntPtr.Zero);

                PresentParameters presentParams = new PresentParameters()
                {
                    BackBufferCount = 1,
                    BackBufferFormat = Format.A8R8G8B8,
                    BackBufferWidth = width,
                    BackBufferHeight = height,
                    DeviceWindowHandle = hwnd.Handle,
                    PresentationInterval = PresentInterval.Immediate,
                    Windowed = true,
                    SwapEffect = SwapEffect.Discard,
                };

                device = new DeviceEx(dx, 0, DeviceType.Hardware, hwnd.Handle, CreateFlags.HardwareVertexProcessing, presentParams);
                backBuffer = device.GetRenderTarget(0);

                surface = Surface.CreateRenderTarget(device, width, height, Format.A8R8G8B8, MultisampleType.None, 1, false);
                surfacePointer = surface.NativePointer;
            }

            public void RenderPatternStageToSurface(Pattern.Stage stage)
            {

                byte r = 0, g = 0, b = 0;

                // TODO: draw proper police vehicle with lights depending on the pattern
                PatternStageType type = stage.Type;
                if ((type & PatternStageType.LeftHeadlight) == PatternStageType.LeftHeadlight)
                    r += 127;
                if((type & PatternStageType.RightHeadlight) == PatternStageType.RightHeadlight)
                    r += 127;

                if ((type & PatternStageType.LeftTailLight) == PatternStageType.LeftTailLight)
                    g += 127;
                if ((type & PatternStageType.RightTailLight) == PatternStageType.RightTailLight)
                    g += 127;

                if ((type & PatternStageType.LeftBrakeLight) == PatternStageType.LeftBrakeLight)
                    b += 127;
                if ((type & PatternStageType.RightBrakeLight) == PatternStageType.RightBrakeLight)
                    b += 127;


                device.SetRenderTarget(0, surface);
                device.Clear(ClearFlags.Target, new SharpDX.Mathematics.Interop.RawColorBGRA(b, g, r, 255), 1.0f, 0);
                device.BeginScene();
                device.EndScene();
            }


            public void Dispose()
            {
                surface.Dispose();
                device.Dispose();
                dx.Dispose();
            }
        }
    }

}