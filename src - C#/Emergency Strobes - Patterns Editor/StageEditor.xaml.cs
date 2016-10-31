﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmergencyStrobesPatternsEditor
{
    /// <summary>
    /// Interaction logic for StageEditor.xaml
    /// </summary>
    public partial class StageEditor : UserControl
    {
        public int Milliseconds
        {
            get { return (int)GetValue(MillisecondsProperty); }
            set { SetValue(MillisecondsProperty, value); }
        }

        public PatternStageType Type
        {
            get { return (PatternStageType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        public Pattern.Stage Stage
        {
            get { return new Pattern.Stage(Type, (uint)Milliseconds); }
            set
            {
                Milliseconds = (int)value.Milliseconds;
                Type = value.Type;
            }
        }


        private Dictionary<PatternStageType, CheckBox> checkBoxesByStageType;

        public StageEditor()
        {
            InitializeComponent();

            StackPanel dropDownContentPanel = StageTypesDropDown.DropDownContent as StackPanel;
            if (dropDownContentPanel != null)
            {
                IEnumerable<CheckBox> checkBoxes= dropDownContentPanel.Children.OfType<CheckBox>();
                checkBoxesByStageType = new Dictionary<PatternStageType, CheckBox>(checkBoxes.Count());
                foreach (CheckBox cb in checkBoxes)
                {
                    checkBoxesByStageType[(PatternStageType)cb.Content] = cb;
                }
            }
        }

        protected void OnStageTypeCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;

            if (cb != null)
            {
                PatternStageType type = (PatternStageType)cb.Content;
                if (type == PatternStageType.All)
                {
                    bool isChecked = cb.IsChecked.GetValueOrDefault();
                    foreach (var item in checkBoxesByStageType)
                    {
                        if (item.Key != PatternStageType.All)
                        {
                            item.Value.IsChecked = isChecked;
                        }
                    }
                }
                else
                {
                    bool isChecked = cb.IsChecked.GetValueOrDefault();
                    if (!isChecked && (PatternStageType)cb.Content != PatternStageType.All)
                    {
                        checkBoxesByStageType[PatternStageType.All].Unchecked -= OnStageTypeCheckBoxChecked;
                        checkBoxesByStageType[PatternStageType.All].IsChecked = false;
                        checkBoxesByStageType[PatternStageType.All].Unchecked += OnStageTypeCheckBoxChecked;
                    }
                    else if (checkBoxesByStageType.Where(i => i.Key != PatternStageType.All).All(i => i.Value.IsChecked.GetValueOrDefault()))
                    {
                        checkBoxesByStageType[PatternStageType.All].IsChecked = true;
                    }
                }

                PatternStageType types = GetSelectedStageTypeFromCheckBoxes();
                Type = types;
            }
        }

        protected PatternStageType GetSelectedStageTypeFromCheckBoxes()
        {
            PatternStageType type = PatternStageType.None;
            foreach (var item in checkBoxesByStageType)
            {
                if (item.Value.IsChecked.GetValueOrDefault())
                    type |= item.Key;
            }
            return type;
        }

        #region DependencyProperties
        public static readonly DependencyProperty MillisecondsProperty = DependencyProperty.Register(nameof(Milliseconds), typeof(int), typeof(StageEditor), new PropertyMetadata(0), (v) => (int)v >= 0);
        public static readonly DependencyProperty TypeProperty = DependencyProperty.Register(nameof(Type), typeof(PatternStageType), typeof(StageEditor), new PropertyMetadata(PatternStageType.None));
        #endregion
    }
}
