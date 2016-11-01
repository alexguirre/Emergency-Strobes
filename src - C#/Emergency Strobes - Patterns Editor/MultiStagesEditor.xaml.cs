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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EmergencyStrobesPatternsEditor
{
    /// <summary>
    /// Interaction logic for StagesEditor.xaml
    /// </summary>
    public partial class MultiStagesEditor : UserControl
    {
        public MultiStagesEditor()
        {

            InitializeComponent();

            StageEditorsDragCanvas.Loaded += (s, e) =>
            {
                SetAllStageEditorsAppropriateYPositions();
                CenterStageEditors();
                SetDragCanvasFitStageEditors();
            };
        }

        public void RemoveStageEditors()
        {
            StageEditorsDragCanvas.Children.Clear();
        }

        public StageEditor AddStageEditor()
        {
            StageEditor editor = new StageEditor();
            editor.Width = DefaultStageEditorMaxWidth;
            editor.Height = DefaultStageEditorHeight;

            int index = StageEditorsDragCanvas.Children.Add(editor);
            SetStageEditorAppropriateYPosition(index);
            CenterStageEditor(index);
            SetDragCanvasFitStageEditors();
            return editor;
        }

        public StageEditor AddStageEditor(Pattern.Stage stage)
        {
            StageEditor editor = AddStageEditor();
            editor.Stage = stage;
            return editor;
        }

        public Pattern.Stage[] GetStages()
        {
            Pattern.Stage[] stages = new Pattern.Stage[StageEditorsDragCanvas.Children.Count];
            for (int i = 0; i < StageEditorsDragCanvas.Children.Count; i++)
            {
                stages[i] = ((StageEditor)StageEditorsDragCanvas.Children[i]).Stage;
            }
            return stages;
        }

        private void OnBorderIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                if ((bool)e.NewValue)
                {
                    border.Opacity = 1.0;
                }
                else
                {
                    border.Opacity = 0.5;
                }
            }
        }

        private void OnDragCanvasElementDropped(object sender, UIElement droppedElement)
        {
            double y = Canvas.GetTop(droppedElement);
            int closestIndex = GetClosestIndexedPositionForY(y);
            int droppedElementIndex = StageEditorsDragCanvas.Children.IndexOf(droppedElement);

            if (droppedElementIndex != -1)
            {
                if (closestIndex != droppedElementIndex)
                {
                    if (closestIndex > StageEditorsDragCanvas.Children.Count - 1)
                    {
                        // add the dropped element to the end of the list 
                        StageEditorsDragCanvas.Children.Remove(droppedElement);
                        StageEditorsDragCanvas.Children.Add(droppedElement);
                    }
                    else
                    {
                        // swap the editors
                        UIElement temp = StageEditorsDragCanvas.Children[closestIndex];
                        StageEditorsDragCanvas.Children.Remove(droppedElement);
                        StageEditorsDragCanvas.Children.Insert(closestIndex, droppedElement);
                        StageEditorsDragCanvas.Children.Remove(temp);
                        StageEditorsDragCanvas.Children.Insert(droppedElementIndex, temp);
                    }
                }

                SetAllStageEditorsAppropriateYPositions();
                CenterStageEditors();
            }

            Console.WriteLine(closestIndex);
        }

        private int GetClosestIndexedPositionForY(double y)
        {
            int currentIndex = -1;
            int closestIndex = -1;

            double prevDistance = Double.MaxValue;
            double distance = Double.MaxValue;

            do
            {
                prevDistance = distance;

                currentIndex++;
                double yPos = GetEditorYPositionForIndex(currentIndex);

                double dist = Math.Abs(yPos - y);

                if (prevDistance < dist)
                    break;

                closestIndex = currentIndex;
                distance = dist;

                //Console.WriteLine($"GetClosestIndexedPositionForY({y}):  CurrentIndex:{currentIndex}  ClosestIndex:{closestIndex}  Dist:{dist}  Distance:{distance}  PrevDistance:{prevDistance}");
            } while (true);

            return closestIndex;
        }

        private double GetEditorYPositionForIndex(int index)
        {
            double y = 0.0;
            y += (DefaultStageEditorHeight * index) + (2.5 * (index + 1));
            return y;
        }

        private void SetAllStageEditorsAppropriateYPositions()
        {
            for (int i = 0; i < StageEditorsDragCanvas.Children.Count; i++)
            {
                SetStageEditorAppropriateYPosition(i);
            }
        }

        // returns new Y position
        private void SetStageEditorAppropriateYPosition(int index)
        {
            UIElement editor = StageEditorsDragCanvas.Children[index];
            double y = GetEditorYPositionForIndex(index);
            Canvas.SetTop(editor, y);
        }

        private void SetDragCanvasFitStageEditors()
        {
            // increase the height of the canvas to be able to scroll
            double lastEditorY = GetEditorYPositionForIndex(StageEditorsDragCanvas.Children.Count - 1);
            double lastEditorBottom = lastEditorY + DefaultStageEditorHeight;
            if (lastEditorBottom > StageEditorsDragCanvas.ActualHeight)
                StageEditorsDragCanvas.Height = StageEditorsDragCanvas.ActualHeight + (lastEditorBottom - StageEditorsDragCanvas.ActualHeight) + 2.5;
        }

        private void CenterStageEditor(int index)
        {
            double canvasWidth = StageEditorsDragCanvas.ActualWidth;

            double editorsWidth = DefaultStageEditorMaxWidth > canvasWidth ? canvasWidth : DefaultStageEditorMaxWidth;
            double x = canvasWidth / 2 - editorsWidth / 2;

            StageEditor editor = (StageEditor)StageEditorsDragCanvas.Children[index];
            if (editor.Width != editorsWidth)
                editor.Width = editorsWidth;
            Canvas.SetLeft(editor, x);
        }

        private void CenterStageEditors()
        {

            double canvasWidth = StageEditorsDragCanvas.ActualWidth;

            double editorsWidth = DefaultStageEditorMaxWidth > canvasWidth ? canvasWidth : DefaultStageEditorMaxWidth;
            double x = canvasWidth / 2 - editorsWidth / 2;

            for (int i = 0; i < StageEditorsDragCanvas.Children.Count; i++)
            {
                StageEditor editor = (StageEditor)StageEditorsDragCanvas.Children[i];
                if (editor.Width != editorsWidth)
                    editor.Width = editorsWidth;
                Canvas.SetLeft(editor, x);
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.WidthChanged)
            {
                CenterStageEditors();
            }
        }

        public const double DefaultStageEditorHeight = 60, DefaultStageEditorMaxWidth = 320;

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            AddStageEditor();
        }
    }
}
