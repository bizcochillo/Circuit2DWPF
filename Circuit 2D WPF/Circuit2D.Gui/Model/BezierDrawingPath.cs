using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Circuit2D.Gui.Model.Interfaces;

namespace Circuit2D.Gui.Model
{
    public class BezierDrawingPath: DrawingPathBase
    {
        private List<BezierLineBasicInfoDto> _bezierInfo;
        private bool _isFirstBezierPoint = true;
        private Point _previousBezierPoint;
        private BezierLineBasicInfoDto _previousBezierLine;
        private Path _previousBezierPath;

        private TrackedCanvasElements _dottedReferenceLines;
        private TrackedCanvasElements _controlPoints;
        private TrackedCanvasElements _bezierSegments;

        public BezierDrawingPath(ICanvas canvas) : base(canvas)
        {
            _bezierInfo = new List<BezierLineBasicInfoDto>();

            _dottedReferenceLines = new TrackedCanvasElements(canvas);
            _controlPoints = new TrackedCanvasElements(canvas); ;
            _bezierSegments = new TrackedCanvasElements(canvas); ;
        }

        public bool IsDottedReferenceLinesEnabled
        {
            get => _dottedReferenceLines.IsVisible;
            set => _dottedReferenceLines.IsVisible = value;
        }

        public bool IsControlPointsEnabled
        {
            get => _controlPoints.IsVisible;
            set => _controlPoints.IsVisible = value;
        }

        public bool IsBezierPathEnabled
        {
            get => _bezierSegments.IsVisible;
            set => _bezierSegments.IsVisible = value;
        }

        public override void AddPoint(Point p)
        {
            base.AddPoint(p);
            // first point
            if (_isFirstBezierPoint)
            {
                var firstLine = new BezierLineBasicInfoDto();
                firstLine.Start = p;
                _isFirstBezierPoint = false;
                _previousBezierPoint = p;
                _bezierInfo.Add(firstLine);
                return;
            }
            // second point
            if (_previousBezierLine is null)
            {
                _bezierInfo[0].End = p;
                _previousBezierLine = _bezierInfo[0];
                return;
            }
            var newLine = new BezierLineBasicInfoDto();
            newLine.Start = _previousBezierLine.End;
            newLine.End = p;
            _bezierInfo.Add(newLine);
            // Dotted reference line
            var line = new Line
            {
                Stroke = Brushes.Blue,
                StrokeDashArray = new DoubleCollection(new[] { 2.0, 2.0 }),
                X1 = _previousBezierLine.Start.X,
                X2 = newLine.End.X,
                Y1 = _previousBezierLine.Start.Y,
                Y2 = newLine.End.Y,
                StrokeThickness = 1
            };
            _dottedReferenceLines.TrackElement(line);
            // Bezier Control Points
            var p0 = _previousBezierLine.Start;
            var p2 = newLine.End;

            var dx = p2.X - p0.X;
            var dy = p2.Y - p0.Y;
            var d = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
            var ux = dx / d;
            var uy = dy / d;

            var u1 = new Point(-ux, uy);
            var u2 = new Point(ux, -uy);

            var angle1 = Math.Atan2(u1.Y, u1.X) + Math.PI / 2;
            var angle2 = Math.Atan2(u2.Y, u2.X) + Math.PI / 2;

            var controlPoint1 = new Point
            (
                _previousBezierLine.End.X + Math.Sin(angle1) * (d * 0.2),
                _previousBezierLine.End.Y + Math.Cos(angle1) * (d * 0.2)
            );
            var controlPoint2 = new Point
            (
                _previousBezierLine.End.X + Math.Sin(angle2) * (d * 0.2),
                _previousBezierLine.End.Y + Math.Cos(angle2) * (d * 0.2)
            );
            var lineControlPoints = new Line
            {
                Stroke = Brushes.Green,
                StrokeDashArray = new DoubleCollection(new[] { 2.0, 2.0 }),
                X1 = controlPoint1.X,
                X2 = controlPoint2.X,
                Y1 = controlPoint1.Y,
                Y2 = controlPoint2.Y,
                StrokeThickness = 1
            };
            _previousBezierLine.AddControlPoint(controlPoint1);
            newLine.AddControlPoint(controlPoint2);

            _controlPoints.TrackElement(lineControlPoints);
            _controlPoints.TrackElement(DrawCircle(controlPoint1, Brushes.Red));
            _controlPoints.TrackElement(DrawCircle(controlPoint2, Brushes.Yellow));
            // Bezier Curve.
            _bezierSegments.RemoveElement(_previousBezierPath);
            _canvas.RemoveElement(_previousBezierPath);
            _bezierSegments.TrackElement(DrawBezierCurves(_previousBezierLine));
            var newBezierPath = DrawBezierCurves(newLine);
            _bezierSegments.TrackElement(newBezierPath);
            _previousBezierPath = newBezierPath;
            _previousBezierLine = newLine;
        }

        private Path DrawBezierCurves(BezierLineBasicInfoDto bezierInfoSegment)
        {
            var pthFigure = new PathFigure();
            pthFigure.StartPoint = bezierInfoSegment.Start;

            var bezierSegment = CreateBezierSegment(bezierInfoSegment);
            var myPathSegmentCollection = new PathSegmentCollection();
            myPathSegmentCollection.Add(bezierSegment);

            pthFigure.Segments = myPathSegmentCollection;

            var pthFigureCollection = new PathFigureCollection();
            pthFigureCollection.Add(pthFigure);

            var pthGeometry = new PathGeometry();
            pthGeometry.Figures = pthFigureCollection;

            var arcPath = new Path();
            arcPath.Stroke = new SolidColorBrush(Colors.Black);
            arcPath.StrokeThickness = 1;
            arcPath.Data = pthGeometry;

            return arcPath;
        }

        private PathSegment CreateBezierSegment(BezierLineBasicInfoDto line)
        {
            if (line.ControlPoints.Count == 1)
                return new QuadraticBezierSegment
                {
                    Point1 = line.ControlPoints[0],
                    Point2 = line.End
                };
            return new BezierSegment
            {
                Point1 = line.ControlPoints[0],
                Point2 = line.ControlPoints[1],
                Point3 = line.End
            };
        }

        VehicleType _truckType;

        public void SetTruckType(VehicleType truckType)
        {
            _truckType = truckType;
        }

        private Geometry GetGeometry()
        {
            switch (_truckType)
            {
                case VehicleType.Truck:
                    return Geometry.Parse("m 70.667324 0.22077406 c 1.31438 0.54151689 2.302749 4.51279834 2.302749 9.25247174 0 3.9944992 -0.56898 6.7913342 -1.698944 8.3512052 l -0.805157 1.111489 h -2.17623 c -1.196928 0 -4.794097 0.08153 -7.993707 0.181175 -5.283023 0.164529 -5.861469 0.137177 -6.296345 -0.297693 -0.263376 -0.263376 -0.478864 -0.840629 -0.478864 -1.282783 0 -1.085743 -0.729776 -1.424174 -2.852413 -1.322796 -1.680123 0.08024 -1.73263 0.105938 -1.871929 0.916096 l -0.143327 0.833539 -11.180646 0.07262 -11.180646 0.07261 v -0.663747 c 0 -0.974073 -0.615827 -1.330837 -2.130289 -1.234124 -1.246859 0.07962 -1.34986 0.143462 -1.482952 0.919105 l -0.143038 0.833539 -11.180647 0.07261 -11.18064327 0.07261 V 9.3502385 0.59177389 L 11.357612 0.6643935 22.54093 0.73701311 22.679865 1.7094758 c 0.134701 0.9428733 0.179722 0.9750659 1.480279 1.0580282 1.571955 0.1002722 2.131723 -0.2602783 2.131723 -1.373049 V 0.59178547 l 11.183316 0.0726196 11.18332 0.0726196 0.09453 0.83263523 c 0.122546 1.0793101 0.420144 1.2418269 2.283517 1.2469577 1.832696 0.00492 2.463079 -0.3823879 2.727602 -1.6763978 l 0.19602 -0.95888847 8.185174 -0.0496581 c 4.501847 -0.0272178 8.336734 0.0127395 8.521969 0.089124 z M 26.708647 9.3502385 v 8.0575465 l 10.766547 0.07273 10.76655 0.07272 V 9.5387341 c 0 -4.4079746 -0.08481 -8.0993173 -0.188509 -8.2029855 C 47.949581 1.232089 43.10462 1.1799987 37.286688 1.2199858 l -10.578063 0.072707 z m -26.11756171 0 V 17.407785 H 11.288171 21.985258 l 0.07398 -7.7759487 c 0.04068 -4.27677 0.01274 -7.9354136 -0.06202 -8.1303167 C 21.88959 1.2210308 19.644061 1.1623241 11.226158 1.2199305 L 0.5910824 1.2926948 Z M 49.236932 6.3236547 c -0.613756 0.2355215 -0.652871 5.7967353 -0.04318 6.1385503 0.217984 0.122205 1.24948 0.159677 2.292235 0.08327 l 1.895912 -0.138923 0.07926 -2.8532605 C 53.537451 6.806021 53.516807 6.6905564 52.905466 6.4451294 52.173193 6.1511503 49.884012 6.0753514 49.236945 6.323662 Z m -25.931904 0.00637 c -0.59975 0.1607274 -0.625152 0.2904059 -0.625152 3.1914342 V 12.54547 h 1.63735 c 1.9622 0 1.974653 -0.02019 1.974653 -3.1981127 0 -3.0294388 -0.616695 -3.6524065 -2.986851 -3.0172313 z");
                case VehicleType.Forklift:
                    return Geometry.Parse("m 56.071536 16.858636 c -0.86355 0.354204 -4.99977 0.489413 -18.861952 0.616578 l -17.755842 0.162884 -0.114432 1.003904 c -0.06294 0.552148 -0.289324 1.115076 -0.503081 1.250951 -0.213757 0.135875 -3.599424 0.247917 -7.523705 0.248986 l -7.1350564 0.002 -0.803631 -0.867908 c -1.18669 -1.281612 -1.612862 -3.958624 -1.438409 -9.035426 0.132278 -3.8494495 0.2491 -4.5559485 0.969551 -5.8634675 l 0.819152 -1.486648 2.507962 -0.206352 c 3.641502 -0.299619 12.1576984 -0.32118 12.6118434 -0.03193 0.213636 0.136066 0.439519 0.699196 0.501963 1.251399 l 0.113534 1.003934 17.253163 0.190135 c 14.07532 0.155114 17.5308 0.282784 18.7605 0.693148 1.09014 0.36379 1.50726 0.681413 1.50706 1.147576 -6.7e-4 1.32309 -1.70765 1.449845 -19.783269 1.468976 -12.825934 0.01358 -17.203071 0.118583 -17.504696 0.419939 -0.442931 0.442534 -0.549124 4.2108095 -0.136958 4.8599495 0.193719 0.305098 4.491775 0.431031 17.499149 0.512724 14.289944 0.08975 17.470994 0.194032 18.577614 0.60903 1.72187 0.645722 1.89491 1.452649 0.43954 2.049608 z M 8.2173706 9.0064525 c -1.273576 -1.176302 -2.621813 -1.195201 -3.977202 -0.05575 -0.858548 0.721766 -1.031243 1.1092155 -1.031782 2.3148595 -5.36e-4 1.205644 0.171809 1.593248 1.029712 2.315781 1.253588 1.055782 2.592228 1.109446 3.809627 0.15272 1.788634 -1.405646 1.852078 -3.17368 0.169645 -4.7276095 z");
                default:
                    throw new NotImplementedException();

            }
        }

        private int _forkliftId = 0;
        public override void Animate()
        {
            var path = new Path { Stroke = Brushes.Red, Fill = Brushes.DeepPink };
            Geometry pathGeometry = GetGeometry();
            //var pathGeometry = Geometry.Parse("m 56.071536 16.858636 c -0.86355 0.354204 -4.99977 0.489413 -18.861952 0.616578 l -17.755842 0.162884 -0.114432 1.003904 c -0.06294 0.552148 -0.289324 1.115076 -0.503081 1.250951 -0.213757 0.135875 -3.599424 0.247917 -7.523705 0.248986 l -7.1350564 0.002 -0.803631 -0.867908 c -1.18669 -1.281612 -1.612862 -3.958624 -1.438409 -9.035426 0.132278 -3.8494495 0.2491 -4.5559485 0.969551 -5.8634675 l 0.819152 -1.486648 2.507962 -0.206352 c 3.641502 -0.299619 12.1576984 -0.32118 12.6118434 -0.03193 0.213636 0.136066 0.439519 0.699196 0.501963 1.251399 l 0.113534 1.003934 17.253163 0.190135 c 14.07532 0.155114 17.5308 0.282784 18.7605 0.693148 1.09014 0.36379 1.50726 0.681413 1.50706 1.147576 -6.7e-4 1.32309 -1.70765 1.449845 -19.783269 1.468976 -12.825934 0.01358 -17.203071 0.118583 -17.504696 0.419939 -0.442931 0.442534 -0.549124 4.2108095 -0.136958 4.8599495 0.193719 0.305098 4.491775 0.431031 17.499149 0.512724 14.289944 0.08975 17.470994 0.194032 18.577614 0.60903 1.72187 0.645722 1.89491 1.452649 0.43954 2.049608 z M 8.2173706 9.0064525 c -1.273576 -1.176302 -2.621813 -1.195201 -3.977202 -0.05575 -0.858548 0.721766 -1.031243 1.1092155 -1.031782 2.3148595 -5.36e-4 1.205644 0.171809 1.593248 1.029712 2.315781 1.253588 1.055782 2.592228 1.109446 3.809627 0.15272 1.788634 -1.405646 1.852078 -3.17368 0.169645 -4.7276095 z");

            path.Data = pathGeometry;

            // Create a MatrixTransform. This transform
            // will be used to move the button.
            var forkliftMatrixTransform = new MatrixTransform();
            path.RenderTransform = forkliftMatrixTransform;

            // Register the transform's name with the page
            // so that it can be targeted by a Storyboard.
            var forkliftMatrixTransformName = $"ForkliftMatrixTransform{_forkliftId:0000}";
            _canvas.RegisterNameAnimation(forkliftMatrixTransformName, forkliftMatrixTransform);
            _forkliftId++;

            _canvas.AddElement(path);

            // Create the animation path.
            PathGeometry animationPath = new PathGeometry();

            var newPath = new Path { Stroke = Brushes.DarkGreen };
            var pathGeo = new PathGeometry();

            foreach (var line in _bezierInfo)
            {
                var pthFigure = new PathFigure { StartPoint = line.Start };

                var bezierSegment = CreateBezierSegment(line);
                var myPathSegmentCollection = new PathSegmentCollection();
                myPathSegmentCollection.Add(bezierSegment);

                pthFigure.Segments = myPathSegmentCollection;

                var pthFigureCollection = new PathFigureCollection();
                pthFigureCollection.Add(pthFigure);

                var pthGeometry = new PathGeometry();
                pthGeometry.Figures = pthFigureCollection;
                pathGeo.Figures.Add(pthFigure);
                animationPath.Figures.Add(pthFigure);
            }

            newPath.Data = pathGeo;
            //_canvas.Children.Add(newPath);


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
            Storyboard.SetTargetName(matrixAnimation, forkliftMatrixTransformName);
            Storyboard.SetTargetProperty(matrixAnimation,
                new PropertyPath(MatrixTransform.MatrixProperty));

            // Create a Storyboard to contain and apply the animation.
            Storyboard pathAnimationStoryboard = new Storyboard();
            pathAnimationStoryboard.Children.Add(matrixAnimation);

            // Start the storyboard when the button is loaded.
            path.Loaded += delegate (object s, RoutedEventArgs ev)
            {
                // Start the storyboard.
                _canvas.StartStoryboard(pathAnimationStoryboard, forkliftMatrixTransformName);
            };
        }
    }
}