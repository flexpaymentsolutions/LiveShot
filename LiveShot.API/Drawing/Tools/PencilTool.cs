﻿using System.Windows;
using System.Windows.Input;
using System.Windows.Shapes;

namespace LiveShot.API.Drawing.Tools
{
    public class PencilTool : DrawingTool
    {
        private Point? _currentPoint;

        public override CanvasTool Tool => CanvasTool.Pencil;

        public override void OnMouseLeftButtonDown(MouseButtonEventArgs e, System.Windows.Controls.Canvas canvas)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _currentPoint = e.GetPosition(canvas);
        }

        public override void OnMouseLeftButtonUp(MouseButtonEventArgs e, System.Windows.Controls.Canvas canvas)
        {
            _currentPoint = null;
        }

        public override void OnMouseMove(MouseEventArgs e, System.Windows.Controls.Canvas canvas)
        {
            if (_currentPoint is null || e.LeftButton != MouseButtonState.Pressed) return;

            var point = _currentPoint.Value;

            Line line = new()
            {
                Stroke = Color,
                X1 = point.X,
                Y1 = point.Y,
                X2 = e.GetPosition(canvas).X,
                Y2 = e.GetPosition(canvas).Y
            };

            _currentPoint = e.GetPosition(canvas);

            canvas.Children.Add(line);
        }
    }
}