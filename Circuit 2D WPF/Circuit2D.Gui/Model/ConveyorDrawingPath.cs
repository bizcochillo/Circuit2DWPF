using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Circuit2D.Gui.Model.Interfaces;

namespace Circuit2D.Gui.Model
{
    public class ConveyorDrawingPath : DrawingPathBase
    {
        private const int CONVEYOR_ROUND_SEGMENT_SIZE = 20;
        private const int CONVEYOR_INTERSECTION_LINE_SIZE = 15;
        private const int CONVEYOR_SEGMENT_WIDTH = 5;
        private const int CONVEYOR_SEGMENT_HEIGHT = 15; //TODO: Smell (Duplicate in CurvedConveyorSegment)

        private readonly TrackedCanvasElements _intersectionPoints;
        private readonly TrackedCanvasElements _intersectionLines;
        private readonly TrackedCanvasElements _circleCenterElements;
        private readonly TrackedCanvasElements _roundedSegments;

        private bool _isFirstConveyorPoint = true;
        private StraightConveyorSegment _lastConveyorSegment;
        private readonly List<ConveyorSegmentBase> _conveyorSegments;

        public ConveyorDrawingPath(ICanvas canvas) : base(canvas)
        {
            _intersectionPoints = new TrackedCanvasElements(canvas);
            _intersectionLines = new TrackedCanvasElements(canvas);
            _circleCenterElements = new TrackedCanvasElements(canvas);
            _roundedSegments = new TrackedCanvasElements(canvas);

            _conveyorSegments = new List<ConveyorSegmentBase>();
        }

        public bool IsIntersectionPointsEnabled
        {
            get => _intersectionPoints.IsVisible;
            set => _intersectionPoints.IsVisible = value;
        }

        public bool IsIntersectionLinesEnabled
        {
            get => _intersectionLines.IsVisible;
            set => _intersectionLines.IsVisible = value;
        }

        public bool IsCircleCenterElementsEnabled
        {
            get => _circleCenterElements.IsVisible;
            set => _circleCenterElements.IsVisible = value;
        }

        public bool IsRoundedSegmentsEnabled
        {
            get => _roundedSegments.IsVisible;
            set => _roundedSegments.IsVisible = value;
        }

        public override void AddPoint(Point p)
        {
            base.AddPoint(p);

            // Intersection Points
            if (_isFirstConveyorPoint)
            {
                var straightConveyorSegment = new StraightConveyorSegment();
                straightConveyorSegment.SourceP0 = p;
                straightConveyorSegment.P0 = p;
                _lastConveyorSegment = straightConveyorSegment;
                _isFirstConveyorPoint = false;
                return;
            }

            if (_conveyorSegments.Count == 0)
            {
                _lastConveyorSegment.P1 = p;
                _lastConveyorSegment.SourceP1 = p;
                _conveyorSegments.Add(_lastConveyorSegment);
                TrackConveyorGraphics();
                return;
            }

            var alpha1 =
                _lastConveyorSegment
                    .Alpha; //Math.Atan2(_lastConveyorSegment.P1.Y - _lastConveyorSegment.P0.Y, _lastConveyorSegment.P1.X - _lastConveyorSegment.P0.X);
            double xl, yl;

            xl = _lastConveyorSegment.P1.X - Math.Cos(alpha1) * CONVEYOR_ROUND_SEGMENT_SIZE;
            yl = _lastConveyorSegment.P1.Y - Math.Sin(alpha1) * CONVEYOR_ROUND_SEGMENT_SIZE;
            _lastConveyorSegment.P1 = new Point(xl, yl); // TODO: Integrate in StraightConveyor?            

            var newStraightConveyorSegment = new StraightConveyorSegment();
            newStraightConveyorSegment.SourceP0 = _lastConveyorSegment.SourceP1;
            newStraightConveyorSegment.SourceP1 = p;
            var alpha2 =
                newStraightConveyorSegment
                    .Alpha; // Math.Atan2(p.Y - _lastConveyorSegment.SourceP1.Y, p.X - _lastConveyorSegment.SourceP1.X);
            newStraightConveyorSegment.P0 = new Point(
                newStraightConveyorSegment.SourceP0.X + Math.Cos(alpha2) * CONVEYOR_ROUND_SEGMENT_SIZE,
                newStraightConveyorSegment.SourceP0.Y + Math.Sin(alpha2) * CONVEYOR_ROUND_SEGMENT_SIZE);

            var newCurvedConveyorSegment = new CurvedConveyorSegment();
            newCurvedConveyorSegment.P0 = _lastConveyorSegment.P1;
            newCurvedConveyorSegment.P1 = newStraightConveyorSegment.P0;
            newStraightConveyorSegment.P1 = p;

            // Corner calculations
            newCurvedConveyorSegment.A0 = newStraightConveyorSegment.A;
            newCurvedConveyorSegment.B0 = newStraightConveyorSegment.P0.Y -
                                          newStraightConveyorSegment.A * newStraightConveyorSegment.P0.X;
            newCurvedConveyorSegment.A1 = _lastConveyorSegment.A;
            newCurvedConveyorSegment.B1 = yl - _lastConveyorSegment.A * xl;
            newCurvedConveyorSegment.Alpha0 = _lastConveyorSegment.Alpha;

            if (!newCurvedConveyorSegment.IsValid())
            {
                //If not valid, the entire block is discarded
                _lastConveyorSegment.SourceP1 = p;
                _lastConveyorSegment.P1 = p;
                TrackConveyorGraphics();
                return;
            }

            _intersectionPoints.TrackElement(DrawCircle(_lastConveyorSegment.P1, Brushes.Green));
            _intersectionLines.TrackElement(GetIntersectionLine(_lastConveyorSegment.P1, alpha1));
            _intersectionPoints.TrackElement(DrawCircle(newStraightConveyorSegment.P0, Brushes.Orange));
            _intersectionLines.TrackElement(GetIntersectionLine(newStraightConveyorSegment.P0, alpha2));
            _conveyorSegments.Add(newCurvedConveyorSegment);
            _conveyorSegments.Add(newStraightConveyorSegment);
            newCurvedConveyorSegment.Alpha1 = newStraightConveyorSegment.Alpha;
            _circleCenterElements.TrackElement(DrawCircle(newCurvedConveyorSegment.Solution,
                newCurvedConveyorSegment.Radius, Brushes.Violet));
            _circleCenterElements.TrackElement(DrawCircle(newCurvedConveyorSegment.Solution, Brushes.Red));
            TrackCornerSegmentation(newCurvedConveyorSegment.Solution, newCurvedConveyorSegment.P0,
                newCurvedConveyorSegment.P1, _circleCenterElements);
            _circleCenterElements.TrackElement(DrawRawLine2(newCurvedConveyorSegment.Solution,
                newCurvedConveyorSegment.P0, Brushes.Red));
            _circleCenterElements.TrackElement(DrawRawLine2(newCurvedConveyorSegment.Solution,
                newCurvedConveyorSegment.P1, Brushes.Red));
            _circleCenterElements.TrackElement(
                DrawArc(
                    newCurvedConveyorSegment.P0,
                    newCurvedConveyorSegment.P1,
                    newCurvedConveyorSegment.Radius,
                    SweepDirection.Clockwise, Brushes.Blue));
            _circleCenterElements.TrackElement(
                DrawArc(
                    newCurvedConveyorSegment.P0,
                    newCurvedConveyorSegment.P1,
                    newCurvedConveyorSegment.Radius,
                    SweepDirection.Counterclockwise,
                    Brushes.Cyan));
            _circleCenterElements.TrackElement(
                DrawArc(
                    newCurvedConveyorSegment.P0,
                    newCurvedConveyorSegment.P1,
                    newCurvedConveyorSegment.Radius,
                    newCurvedConveyorSegment.Direction,
                    Brushes.Green));
            _lastConveyorSegment = newStraightConveyorSegment;

            // Rounded Segments
            TrackConveyorGraphics();
        }

        private void TrackConveyorGraphics()
        {
            _roundedSegments.RemoveAll();
            foreach (var conveyorSegment in _conveyorSegments)
            {
                if (conveyorSegment is StraightConveyorSegment)
                {
                    TrackStraightSegmentation((StraightConveyorSegment) conveyorSegment);
                }
                else
                {
                    var segment = (CurvedConveyorSegment) conveyorSegment;
                    TrackCornerSegmentation(segment.Solution, segment.P0, segment.P1, _roundedSegments, false);
                    var radiusModificator = (segment.Direction == SweepDirection.Clockwise ? 1 : -1);
                    _roundedSegments.TrackElement(DrawArc(segment.P0Top, segment.P1Top,
                        segment.Radius + radiusModificator * CONVEYOR_SEGMENT_HEIGHT, segment.Direction, Brushes.Black,
                        1));
                    _roundedSegments.TrackElement(DrawArc(segment.P0Bottom, segment.P1Bottom,
                        segment.Radius - radiusModificator * CONVEYOR_SEGMENT_HEIGHT, segment.Direction, Brushes.Black,
                        1));
                }
            }
        }

        private void TrackStraightSegmentation(StraightConveyorSegment segment)
        {
            var numSegments = (int) Math.Round(
                Math.Sqrt(Math.Pow(segment.P0.X - segment.P1.X, 2) + Math.Pow(segment.P0.Y - segment.P1.Y, 2)) /
                CONVEYOR_SEGMENT_WIDTH);
            var difX = (segment.P1.X - segment.P0.X) / numSegments;
            var difY = (segment.P1.Y - segment.P0.Y) / numSegments;
            double x = segment.P0.X;
            double y = segment.P0.Y;
            for (int index = 0; index < numSegments; index++)
            {
                x = x + difX;
                y = y + difY;
                var pRef = new Point(x, y);
                var pTop = segment.GetTopPoint(pRef, segment.Beta);
                var pBottom = segment.GetBottomPoint(pRef, segment.Beta);
                _roundedSegments.TrackElement(DrawRawLine2(pTop, pBottom, Brushes.Green));
            }

            _roundedSegments.TrackElement(DrawRawLine2(segment.P0Top, segment.P0Bottom, Brushes.Black));
            _roundedSegments.TrackElement(DrawRawLine2(segment.P0Bottom, segment.P1Bottom, Brushes.Black));
            _roundedSegments.TrackElement(DrawRawLine2(segment.P1Bottom, segment.P1Top, Brushes.Black));
            _roundedSegments.TrackElement(DrawRawLine2(segment.P1Top, segment.P0Top, Brushes.Black));
        }

        private Path DrawArc(Point p1, Point p2, int radius, SweepDirection sweepDir, Brush brush,
            int strokeThickness = 3)
        {
            if (radius < 0) return null;
            var arc = new ArcSegment
            {
                Point = p2,
                Size = new Size(radius, radius),
                SweepDirection = sweepDir
            };

            var figure = new PathFigure();
            figure.StartPoint = p1;
            figure.Segments.Add(arc);

            var geo = new PathGeometry();
            geo.Figures.Add(figure);

            var path = new Path();
            path.Stroke = brush;
            path.StrokeThickness = strokeThickness;
            path.Data = geo;
            return path;
        }

        private void TrackCornerSegmentation(Point p0, Point p1, Point p2, TrackedCanvasElements elements,
            bool debug = true)
        {
            const double SEGMENTS = 7.0;
            var radiusMax = Point.Subtract(p1, p0).Length + CONVEYOR_SEGMENT_HEIGHT;
            var radiusMin = Point.Subtract(p1, p0).Length - CONVEYOR_SEGMENT_HEIGHT;
            var alpha1 = Math.Atan2(p1.Y - p0.Y, p1.X - p0.X);
            var alpha2 = Math.Atan2(p2.Y - p0.Y, p2.X - p0.X);
            var absDif = Math.Abs(alpha1 - alpha2);
            string adjusted = "";
            //TODO: To improve the subtraction, we have to normalize the angles into the interval [0, 2*Math.PI]
            if (absDif > Math.PI)
            {
                if (alpha1 > 0)
                {
                    alpha2 = alpha2 + 2.0 * Math.PI;
                }
                else
                {
                    alpha1 = alpha1 + Math.PI * 2.0;
                }

                adjusted = "[AD]";
            }

            var difAlpha = (alpha2 - alpha1) / SEGMENTS;

            Console.WriteLine($"{adjusted}DifAlpha={difAlpha}. Alpha1={alpha1}. Alpha2={alpha2}.");

            double alphaI = alpha1;
            for (int index = 0; index < SEGMENTS; index++)
            {
                alphaI = alphaI + difAlpha;
                if (alphaI > 2.0 * Math.PI)
                    alphaI = alphaI - Math.PI * 2.0;
                var xpMax = p0.X + radiusMax * Math.Cos(alphaI);
                var ypMax = p0.Y + radiusMax * Math.Sin(alphaI);
                var xpMin = p0.X + radiusMin * Math.Cos(alphaI);
                var ypMin = p0.Y + radiusMin * Math.Sin(alphaI);
                if (debug) elements.TrackElement(DrawRawLine2(p0, new Point(xpMax, ypMax), Brushes.Yellow));
                elements.TrackElement(DrawRawLine2(xpMin, ypMin, xpMax, ypMax, Brushes.Green));
            }
        }

        private Line DrawRawLine2(double x1, double y1, double x2, double y2, Brush brush)
        {
            return new Line
            {
                Stroke = brush,
                X1 = x1,
                X2 = x2,
                Y1 = y1,
                Y2 = y2,
                StrokeThickness = 1
            };
        }

        private Line DrawRawLine2(Point p1, Point p2, Brush brush)
        {
            return DrawRawLine2(p1.X, p1.Y, p2.X, p2.Y, brush);
        }

        private Ellipse DrawCircle(Point p, int radius, Brush brush)
        {
            var circle = new Ellipse
            {
                StrokeThickness = 1,
                Stroke = brush,
                Width = radius * 2,
                Height = radius * 2
            };
            Canvas.SetLeft(circle, p.X - radius);
            Canvas.SetTop(circle, p.Y - radius);
            return circle;
        }

        private Line GetIntersectionLine(Point p, double alpha)
        {
            double xp0, yp0, xp1, yp1;
            var beta = alpha - Math.PI / 2.0;
            xp0 = p.X + CONVEYOR_INTERSECTION_LINE_SIZE * Math.Cos(beta);
            yp0 = p.Y + CONVEYOR_INTERSECTION_LINE_SIZE * Math.Sin(beta);
            xp1 = p.X - CONVEYOR_INTERSECTION_LINE_SIZE * Math.Cos(beta);
            yp1 = p.Y - CONVEYOR_INTERSECTION_LINE_SIZE * Math.Sin(beta);
            return new Line
            {
                X1 = xp0,
                Y1 = yp0,
                X2 = xp1,
                Y2 = yp1,
                Stroke = Brushes.Violet
            };
        }

        private List<PathFigure> GetSendingPath()
        {
            var sendingPath = new List<PathFigure>();
            foreach (var segment in _conveyorSegments)
                sendingPath.Add(segment.GetPath());
            return sendingPath;
        }

        private int _conveyorId = 0;

        public override void Animate()
        {
            var path = new Path();
            path.Stroke = Brushes.DarkSlateGray;
            path.Fill = Brushes.Olive;
            //var pathGeometry = PathGeometry.Parse("m 56.071536 16.858636 c -0.86355 0.354204 -4.99977 0.489413 -18.861952 0.616578 l -17.755842 0.162884 -0.114432 1.003904 c -0.06294 0.552148 -0.289324 1.115076 -0.503081 1.250951 -0.213757 0.135875 -3.599424 0.247917 -7.523705 0.248986 l -7.1350564 0.002 -0.803631 -0.867908 c -1.18669 -1.281612 -1.612862 -3.958624 -1.438409 -9.035426 0.132278 -3.8494495 0.2491 -4.5559485 0.969551 -5.8634675 l 0.819152 -1.486648 2.507962 -0.206352 c 3.641502 -0.299619 12.1576984 -0.32118 12.6118434 -0.03193 0.213636 0.136066 0.439519 0.699196 0.501963 1.251399 l 0.113534 1.003934 17.253163 0.190135 c 14.07532 0.155114 17.5308 0.282784 18.7605 0.693148 1.09014 0.36379 1.50726 0.681413 1.50706 1.147576 -6.7e-4 1.32309 -1.70765 1.449845 -19.783269 1.468976 -12.825934 0.01358 -17.203071 0.118583 -17.504696 0.419939 -0.442931 0.442534 -0.549124 4.2108095 -0.136958 4.8599495 0.193719 0.305098 4.491775 0.431031 17.499149 0.512724 14.289944 0.08975 17.470994 0.194032 18.577614 0.60903 1.72187 0.645722 1.89491 1.452649 0.43954 2.049608 z M 8.2173706 9.0064525 c -1.273576 -1.176302 -2.621813 -1.195201 -3.977202 -0.05575 -0.858548 0.721766 -1.031243 1.1092155 -1.031782 2.3148595 -5.36e-4 1.205644 0.171809 1.593248 1.029712 2.315781 1.253588 1.055782 2.592228 1.109446 3.809627 0.15272 1.788634 -1.405646 1.852078 -3.17368 0.169645 -4.7276095 z");
            var pathGeometry = new RectangleGeometry(new Rect(new Point(-15, -7), new Point(15, 7)));
            path.Data = pathGeometry;
            var sendingPath = GetSendingPath();

            // Create a MatrixTransform. This transform
            // will be used to move the button.
            var packageMatrixTransform = new MatrixTransform();
            path.RenderTransform = packageMatrixTransform;

            // Register the transform's name with the page
            // so that it can be targeted by a Storyboard.
            var packageMatrixTransformName = $"PackageMatrixTransform{_conveyorId:0000}";
            _conveyorId++;
            _canvas.RegisterNameAnimation(packageMatrixTransformName, packageMatrixTransform);
            _canvas.AddElement(path);

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();
            foreach (var figure in sendingPath)
            {
                animationPath.Figures.Add(figure);
            }

            // Freeze the PathGeometry for performance benefits.
            animationPath.Freeze();

            // Create a MatrixAnimationUsingPath to move the
            // button along the path by animating
            // its MatrixTransform.
            var matrixAnimation =
                new MatrixAnimationUsingPath
                {
                    PathGeometry = animationPath,
                    Duration = TimeSpan.FromSeconds(5),
                    RepeatBehavior = RepeatBehavior.Forever,
                    DoesRotateWithTangent = true
                };

            // Set the animation's DoesRotateWithTangent property
            // to true so that rotates the rectangle in addition
            // to moving it.

            // Set the animation to target the Matrix property
            // of the MatrixTransform named "ButtonMatrixTransform".
            Storyboard.SetTargetName(matrixAnimation, packageMatrixTransformName);
            Storyboard.SetTargetProperty(matrixAnimation,
                new PropertyPath(MatrixTransform.MatrixProperty));

            // Create a Storyboard to contain and apply the animation.
            Storyboard pathAnimationStoryboard = new Storyboard();
            pathAnimationStoryboard.Children.Add(matrixAnimation);

            // Start the storyboard when the button is loaded.
            path.Loaded += delegate(object s, RoutedEventArgs ev)
            {
                // Start the storyboard.
                _canvas.StartStoryboard(pathAnimationStoryboard, packageMatrixTransformName);
            };
        }
    }
}
