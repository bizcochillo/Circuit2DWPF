using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace Circuit2D.Gui.Model.Interfaces
{
    public class WpfCanvas : ICanvas
    {
        private Canvas _canvas;
        public Canvas DesignCanvas => _canvas;

        private IDrawingPath _drawingPath;

        public void SetDrawingPath(IDrawingPath drawingPath)
        {
            _drawingPath = drawingPath;
        }

        public WpfCanvas(Canvas canvas)
        {
            _canvas = canvas;
            _canvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
        }

        public void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_drawingPath != null) _drawingPath.AddPoint(Mouse.GetPosition(_canvas));
        }

        public T FindElementByType<T>()
        {
            foreach (var element in _canvas.Children)
            {
                if (element is T)
                    return (T) element;
            }

            return default(T);
        }

        public void AddElement(object element)
        {
            if (!(element is UIElement))
                throw new ArgumentException(
                    $"UIElement not suitable to be added to the WPF Canvas. Element type: {element.GetType()}");
            _canvas.Children.Add((UIElement) element);
        }

        public void AddElement(object element, double top, double left)
        {
            if (!(element is UIElement))
                throw new ArgumentException(
                    $"UIElement not suitable to be added to the WPF Canvas. Element type: {element.GetType()}");
            var elementUI = (UIElement) element;
            Canvas.SetTop(elementUI, top);
            Canvas.SetLeft(elementUI, left);
            AddElement(elementUI);
        }

        public void Clear()
        {
            _canvas.Children.Clear();
        }

        public void UnregisterNameAnimation(string name)
        {
            _canvas.UnregisterName(name);
        }

        public bool ContainsElement(object element)
        {
            if (!(element is UIElement))
                throw new ArgumentException(
                    $"UIElement not suitable to be added to the WPF Canvas. Element type: {element.GetType()}");
            return _canvas.Children.Contains((UIElement) element);
        }

        public void RemoveElement(object element)
        {
            if (element == null) return;
            if (!(element is UIElement))
                throw new ArgumentException(
                    $"Element to be removed must be of an UIElement . Element type: {element.GetType()}");
            _canvas.Children.Remove((UIElement) element);
        }

        public void RegisterNameAnimation(string name, object transform)
        {
            _canvas.RegisterName(name, transform);
        }

        public void StartStoryboard(object storyboard, string name)
        {
            if (!(storyboard is Storyboard))
                throw new ArgumentException($"Storyboard type not correct: {storyboard.GetType()}");
            ((Storyboard) storyboard).Begin(_canvas);
            _canvas.UnregisterName(name);
        }
    }
}
