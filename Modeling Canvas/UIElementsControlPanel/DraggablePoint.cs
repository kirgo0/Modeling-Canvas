﻿using System.Windows;

namespace Modeling_Canvas.UIElements
{
    public partial class DraggablePoint
    {
        protected virtual void AddPointControls()
        {
            AddDefaultPointControls(
                "Point",
                this,
                "Position.X",
                "Position.Y",
                (x) =>
                {
                    Position = new Point(x, Position.Y);
                    InvalidateCanvas();
                },
                (y) =>
                {
                    Position = new Point(Position.X, y);
                    InvalidateCanvas();
                }
            );
        }
    }
}