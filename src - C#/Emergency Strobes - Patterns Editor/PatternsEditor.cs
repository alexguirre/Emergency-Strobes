namespace EmergencyStrobesPatternsEditor
{
    // System
    using System;
    using System.IO;
    using System.Windows;
    using System.Linq;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    // MS
    using Microsoft.Win32;

    internal class PatternsEditor
    {
        public static PatternsEditor Instance { get; private set; }

        public MainWindowUIManager UI { get; }

        private bool isEditingFile;
        public bool IsEditingFile
        {
            get { return isEditingFile; }
            private set
            {
                if (value == isEditingFile)
                    return;
                isEditingFile = value;
                UI.AreFileDependantControlsEnabled = isEditingFile;
            }
        }
        //public bool HasMadeChangesSinceLastSave { get; private set; }
        
        public string CurrentFileName { get; private set; }

        int currentPatternIndex;

        public PatternsEditor(MainWindow window)
        {
            Instance = this;

            UI = new MainWindowUIManager(window);
        }

        public void SetUp()
        {
            UI.Window.Closing += OnWindowClosing;
            UI.Window.OpenMenuItem.Click += OnOpenMenuItemClick;
            UI.Window.NewMenuItem.Click += OnNewMenuItemClick;
            UI.Window.SaveMenuItem.Click += OnSaveMenuItemClick;
            UI.Window.AboutMenuItem.Click += OnAboutMenuItemClick;
            UI.Window.AddPatternButton.Click += OnAddPatternButtonClick;
            UI.Window.CurrentPatternNameTextBox.TextChanged += OnCurrentPatternNameTextBoxChanged;
            UI.PropertyChanged += OnUIPropertyChanged;
            UI.Window.ReproducePatternButton.Click += OnReproducePatternButtonClick;
        }

        private void OnAboutMenuItemClick(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }

        private void OnReproducePatternButtonClick(object sender, RoutedEventArgs e)
        {
            Pattern pattern = new Pattern(UI.SelectedPattern.Name, UI.Window.MultiStagesEditor.GetStages());

            if (pattern.Stages == null || pattern.Stages.Length <= 0)
            {
                MessageBox.Show("The selected pattern doesn't have any stages, can't reproduce.", "Empty pattern", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            new PatternPlayerWindow(pattern).ShowDialog();
        }

        private void OnWindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (IsEditingFile/* || HasMadeChangesSinceLastSave*/)
            {
                MessageBoxResult result = MessageBox.Show($@"Save file ""{CurrentFileName}"" ?", "Save", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    default:
                    case MessageBoxResult.Cancel:
                        e.Cancel = true;
                        return;
                    case MessageBoxResult.Yes:
                        SaveCurrentFile();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        private void OnCurrentPatternNameTextBoxChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UI.SelectedPattern.Name = UI.Window.CurrentPatternNameTextBox.Text;
        }
        
        private void OnUIPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(UI.SelectedPattern))
            {
                if (currentPatternIndex != -1 && currentPatternIndex < UI.CurrentPatterns.Count)
                {
                    UI.CurrentPatterns[currentPatternIndex].Stages = UI.Window.MultiStagesEditor.GetStages();
                }

                UI.Window.MultiStagesEditor.RemoveStageEditors();

                if (UI.SelectedPattern != null)
                {
                    UI.Window.CurrentPatternNameTextBox.Text = UI.SelectedPattern.Name;

                    for (int i = 0; i < UI.SelectedPattern.Stages.Length; i++)
                    {
                        UI.Window.MultiStagesEditor.AddStageEditor(UI.SelectedPattern.Stages[i]);
                    }
                    currentPatternIndex = UI.CurrentPatterns.IndexOf(UI.SelectedPattern);
                }
                else
                {
                    currentPatternIndex = -1;
                }
            }
        }

        private void OnAddPatternButtonClick(object sender, RoutedEventArgs e)
        {
            if (IsEditingFile)
            {
                AddNewPattern();
            }
        }

        private void OnNewMenuItemClick(object sender, RoutedEventArgs e)
        {
            if (IsEditingFile/* || HasMadeChangesSinceLastSave*/)
            {
                MessageBoxResult result = MessageBox.Show($@"Save file ""{CurrentFileName}"" ?", "Save", MessageBoxButton.YesNoCancel);
                switch (result)
                {
                    default:
                    case MessageBoxResult.Cancel: return;
                    case MessageBoxResult.Yes:
                        SaveCurrentFile();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }

            currentPatternIndex = -1;
            UI.CurrentPatterns = new ObservableCollection<PatternWrapper>();
            AddNewPattern();
            CurrentFileName = "new patterns file";
            IsEditingFile = true;
            UI.SelectedPattern = null;
            currentPatternIndex = -1;
        }

        private void OnOpenMenuItemClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ValidateNames = true;
            openFileDialog.Title = "Open";
            openFileDialog.Multiselect = false;
            openFileDialog.Filter = "XML Files | *.xml";
            openFileDialog.AddExtension = true;
            openFileDialog.DefaultExt = "xml";

            bool? result = openFileDialog.ShowDialog();
            if (result != null && result.HasValue && result.Value)
            {
                currentPatternIndex = -1;
                CurrentFileName = openFileDialog.FileName;
                UI.CurrentPatterns = new ObservableCollection<PatternWrapper>(PatternsIO.LoadFrom(CurrentFileName).Select(p => new PatternWrapper(p)).ToList());
                IsEditingFile = true;
            }
        }

        private void OnSaveMenuItemClick(object sender, RoutedEventArgs e)
        {
            SaveCurrentFile();
        }

        private void SaveCurrentFile()
        {
            if (!Path.IsPathRooted(CurrentFileName))
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.ValidateNames = true;
                saveFileDialog.Title = "Save";
                saveFileDialog.OverwritePrompt = true;
                saveFileDialog.Filter = "XML Files | *.xml";
                saveFileDialog.AddExtension = true;
                saveFileDialog.DefaultExt = "xml";

                bool? result = saveFileDialog.ShowDialog();
                if (result != null && result.HasValue && result.Value)
                {
                    CurrentFileName = saveFileDialog.FileName;
                }
                else
                {
                    return;
                }
            }

            if (currentPatternIndex != -1 && currentPatternIndex < UI.CurrentPatterns.Count)
            {
                UI.CurrentPatterns[currentPatternIndex].Stages = UI.Window.MultiStagesEditor.GetStages();
            }
            PatternsIO.SaveTo(CurrentFileName, UI.CurrentPatterns.Select(w => w.Pattern).ToArray());
            //HasMadeChangesSinceLastSave = false;
        }

        private void AddNewPattern()
        {
            ObservableCollection<PatternWrapper> currentPatterns = UI.CurrentPatterns;
            ObservableCollection<PatternWrapper> newPatterns = new ObservableCollection<PatternWrapper>(currentPatterns);

            string newName = "new";
            int i = 1;
            while (UI.CurrentPatterns.Any(p => p.Name == newName))
            {
                newName = "new " + i;
                i++;
            }
            newPatterns.Add(new PatternWrapper(new Pattern() { Name = newName, Stages = new Pattern.Stage[0] }));

            UI.CurrentPatterns = newPatterns;
            UI.SelectedPattern = newPatterns.Last();

            //HasMadeChangesSinceLastSave = true;
        }
    }
}
