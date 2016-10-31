namespace EmergencyStrobesPatternsEditor
{
    // System
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media.Animation;
    using System.Windows.Threading;
    using System.ComponentModel;
    using System.Collections.ObjectModel;

    internal class MainWindowUIManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow Window { get; }

        public Dispatcher Dispatcher { get { return Window.Dispatcher; } }

        private string windowTitle = "Emergency Strobes - Patterns Editor";
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                if (value == windowTitle)
                    return;
                windowTitle = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        //private bool isWindowExtended;
        //public bool IsWindowExtended
        //{
        //    get { return isWindowExtended; }
        //    set
        //    {
        //        if (value == isWindowExtended)
        //            return;
        //        isWindowExtended = value;
        //        ExtendWindowAnimation(isWindowExtended);
        //        OnPropertyChanged(nameof(IsWindowExtended));
        //    }
        //}

        private ObservableCollection<PatternWrapper> currentPatterns;
        public ObservableCollection<PatternWrapper> CurrentPatterns
        {
            get { return currentPatterns; }
            set
            {
                if (value == currentPatterns)
                    return;
                currentPatterns = value;
                OnPropertyChanged(nameof(CurrentPatterns));
            }
        }

        private PatternWrapper selectedPattern;
        public PatternWrapper SelectedPattern
        {
            get { return selectedPattern; }
            set
            {
                selectedPattern = value;
                OnPropertyChanged(nameof(SelectedPattern));
            }
        }

        private bool areFileDependantControlsEnabled;
        public bool AreFileDependantControlsEnabled
        {
            get { return areFileDependantControlsEnabled; }
            set
            {
                if (areFileDependantControlsEnabled == value)
                    return;
                areFileDependantControlsEnabled = value;
                OnPropertyChanged(nameof(AreFileDependantControlsEnabled));
            }
        }

        public MainWindowUIManager(MainWindow window)
        {
            Window = window;
            Window.DataContext = this;

            //Window.Loaded += OnWindowLoaded;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



        //private void OnWindowLoaded(object sender, RoutedEventArgs e)
        //{
        //    origWindowStartWidth = Window.Width;
        //    origWindowMaxWidth = Window.MaxWidth;
        //    Window.Loaded -= OnWindowLoaded;
        //}

        //private double origWindowStartWidth;
        //private double origWindowMaxWidth;
        //protected void ExtendWindowAnimation(bool extend)
        //{
        //    const double widthToAnim = 280;

        //    double from = Window.Width;
        //    double to = Window.Width + (extend ? widthToAnim : -widthToAnim);

        //    if (extend)
        //    {
        //        if (to > origWindowStartWidth + widthToAnim)
        //            to = origWindowStartWidth + widthToAnim;
        //        Window.MaxWidth = origWindowMaxWidth + 280;
        //    }
        //    else
        //    {
        //        if (to < origWindowStartWidth)
        //            to = origWindowStartWidth;
        //    }

        //    DoubleAnimation anim = new DoubleAnimation(from, to, new TimeSpan(0, 0, 0, 1, 100));
        //    if (!extend)
        //        anim.Completed += (s, e) => { Window.MaxWidth = origWindowMaxWidth; };

        //    Window.BeginAnimation(MainWindow.WidthProperty, anim);
        //}
    }
}
