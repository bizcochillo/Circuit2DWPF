using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Circuit2D.Gui.Model.Interfaces;

namespace Circuit2D.Gui.Model
{
    public class TrackedCanvasElements
    {
        private ICanvas _canvas;

        private readonly List<UIElement> _visibleElements = new List<UIElement>();
        private readonly List<UIElement> _hiddenElements = new List<UIElement>();

        public TrackedCanvasElements(ICanvas canvas)
        {
            _canvas = canvas;
        }

        private bool _isVisible;
        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                RefreshElements();
            }
        }

        private void RefreshElements()
        {
            if (_isVisible)
            {
                foreach (var element in _hiddenElements)
                {
                    if (!_canvas.ContainsElement(element))
                    {
                        _canvas.AddElement(element);
                        _visibleElements.Add(element);
                    }
                }
                _hiddenElements.Clear();
            }
            else
            {
                foreach (var element in _visibleElements)
                {
                    if (_canvas.ContainsElement(element))
                    {
                        _canvas.RemoveElement(element);
                        _hiddenElements.Add(element);
                    }
                }
                _visibleElements.Clear();
            }
        }

        public void TrackElement(UIElement element)
        {
            if (element is null) return;
            if (_isVisible)
            {
                _canvas.AddElement(element);
                _visibleElements.Add(element);
            }
            else
            {
                _hiddenElements.Add(element);
            }
        }

        public void RemoveElement(UIElement element)
        {
            _visibleElements.Remove(element);
            _hiddenElements.Remove(element);
            _canvas.RemoveElement(element);
        }

        internal void RemoveAll()
        {
            foreach (var element in _hiddenElements)
                _canvas.RemoveElement(element);
            foreach (var element in _visibleElements)
                _canvas.RemoveElement(element);
            _hiddenElements.Clear();
            _visibleElements.Clear();
        }
    }
}
