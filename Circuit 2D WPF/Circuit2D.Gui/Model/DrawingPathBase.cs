using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Circuit2D.Gui.Model.Interfaces;

namespace Circuit2D.Gui.Model
{
    public abstract class DrawingPathBase : IDrawingPath
    {
        protected ICanvas _canvas;

        protected readonly TrackedCanvasElements _markPoints;
        protected readonly TrackedCanvasElements _traceLines;

        protected bool _isSegmentMarkingInitialized = false;
        protected Point _lastMarkedPoint;

        public virtual void Animate()
        {
        }

        public DrawingPathBase(ICanvas canvas)
        {
            _canvas = canvas;

            _markPoints = new TrackedCanvasElements(canvas);
            _traceLines = new TrackedCanvasElements(canvas);
        }

        public bool IsMarkingEnabled
        {
            get => _markPoints.IsVisible;
            set => _markPoints.IsVisible = value;
        }

        public bool IsLineTracingEnabled
        {
            get => _traceLines.IsVisible;
            set => _traceLines.IsVisible = value;
        }

        protected void RefreshElements(bool isEnabled, List<UIElement> visibleElements, List<UIElement> hiddenElements)
        {
            if (isEnabled)
            {
                foreach (var element in hiddenElements)
                {
                    if (!_canvas.ContainsElement(element))
                    {
                        _canvas.AddElement(element);
                        visibleElements.Add(element);
                    }
                }

                hiddenElements.Clear();
            }
            else
            {
                foreach (var element in visibleElements)
                {
                    if (_canvas.ContainsElement(element))
                    {
                        _canvas.RemoveElement(element);
                        hiddenElements.Add(element);
                    }
                }

                visibleElements.Clear();
            }
        }

        public virtual void AddPoint(Point p)
        {
            var mark = new Ellipse
            {
                Fill = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0)),
                StrokeThickness = 2,
                Stroke = Brushes.White,
                Width = 4,
                Height = 4
            };
            Canvas.SetLeft(mark, p.X - 4);
            Canvas.SetTop(mark, p.Y - 4);

            // Mark point            
            _markPoints.TrackElement(mark);

            if (!_isSegmentMarkingInitialized)
            {
                _isSegmentMarkingInitialized = true;
                _lastMarkedPoint = p;
                return;
            }

            // Trace Line
            var line = new Line
            {
                Stroke = Brushes.LightSteelBlue,
                X1 = _lastMarkedPoint.X,
                X2 = p.X,
                Y1 = _lastMarkedPoint.Y,
                Y2 = p.Y,
                StrokeThickness = 2
            };
            _traceLines.TrackElement(line);

            _lastMarkedPoint = p;
        }

        protected void TrackElementToAdd(bool isVisible, UIElement element, List<UIElement> visibleElements,
            List<UIElement> hiddenElements)
        {
            if (isVisible)
            {
                _canvas.AddElement(element);
                visibleElements.Add(element);
            }
            else
            {
                hiddenElements.Add(element);
            }
        }

        protected Ellipse DrawCircle(Point p, Brush brush)
        {
            var circle = new Ellipse
            {
                StrokeThickness = 2,
                Stroke = brush,
                Width = 8,
                Height = 8
            };
            Canvas.SetLeft(circle, p.X - 4);
            Canvas.SetTop(circle, p.Y - 4);
            return circle;
        }

        protected Ellipse DrawCircle(double x, double y, Brush brush)
        {
            return DrawCircle(new Point(x, y), brush);
        }
    }
}
