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

namespace EmergencyStrobesPatternsEditor
{
    /// <summary>
    /// Interaction logic for PatternPlayerWindow.xaml
    /// </summary>
    public partial class PatternPlayerWindow : Window
    {
        public Pattern Pattern { get; }

        public D3DImage CurrentImage { get; }
        
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

            currentStage = Pattern.Stages[0];
            currentStageIndex = 0;
            
            RenderStage();

            int timeMs = unchecked((int)currentStage.Milliseconds);
            timeMs = Math.Max(timeMs, 1); // if milliseconds equals 0, it freezes the window, it needs to be at least 1 ms

            timer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, timeMs), DispatcherPriority.Normal, OnTimerTick, Dispatcher);
            timer.Start();
        }

        protected override void OnClosed(EventArgs e)
        {
            timer?.Stop();

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

            int timeMs = unchecked((int)currentStage.Milliseconds);
            timeMs = Math.Max(timeMs, 1); // if milliseconds equals 0, it freezes the window, it needs to be at least 1 ms

            timer.Interval = new TimeSpan(0, 0, 0, 0, timeMs);
        }

        private void RenderStage()
        {
            PatternStageType type = currentStage.Type;

            LeftHeadlightEllipse.Visibility = ((type & PatternStageType.LeftHeadlight) == PatternStageType.LeftHeadlight) ? Visibility.Visible : Visibility.Hidden;
            RightHeadlightEllipse.Visibility = ((type & PatternStageType.RightHeadlight) == PatternStageType.RightHeadlight) ? Visibility.Visible : Visibility.Hidden;

            LeftTailLightEllipse.Visibility = ((type & PatternStageType.LeftTailLight) == PatternStageType.LeftTailLight) ? Visibility.Visible : Visibility.Hidden;
            RightTailLightEllipse.Visibility = ((type & PatternStageType.RightTailLight) == PatternStageType.RightTailLight) ? Visibility.Visible : Visibility.Hidden;

            LeftBrakeLightEllipse.Visibility = ((type & PatternStageType.LeftBrakeLight) == PatternStageType.LeftBrakeLight) ? Visibility.Visible : Visibility.Hidden;
            RightBrakeLightEllipse.Visibility = ((type & PatternStageType.RightBrakeLight) == PatternStageType.RightBrakeLight) ? Visibility.Visible : Visibility.Hidden;
        }
    }

}