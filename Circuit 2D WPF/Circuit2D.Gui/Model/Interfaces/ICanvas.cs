using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuit2D.Gui.Model.Interfaces
{
    public interface ICanvas
    {
        void Clear();
        T FindElementByType<T>();
        void SetDrawingPath(IDrawingPath drawingPath);
        bool ContainsElement(object element);
        void RemoveElement(object element);
        void AddElement(object element);
        void AddElement(object element, double top, double left);
        void RegisterNameAnimation(string name, object transform);
        void UnregisterNameAnimation(string name);
        void StartStoryboard(object storyboard, string name);
    }
}
