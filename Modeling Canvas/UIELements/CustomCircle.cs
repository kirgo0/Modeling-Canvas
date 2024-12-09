﻿using Modeling_Canvas.Extensions;
using Modeling_Canvas.Models;
using Modeling_Canvas.UIElements.Interfaces;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Modeling_Canvas.UIElements
{
    public partial class CustomCircle : GroupableElement
    {
        public double MinRadiusValue { get; set; } = 0.5;

        private double _maxRadiusValue = 10;

        public int Precision { get; set; } = 100;

        private double _radius = 5;

        public double MaxRadiusValue
        {
            get => _maxRadiusValue;
            set
            {
                if (_maxRadiusValue != value)
                {
                    _maxRadiusValue = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Radius
        {
            get => _radius;
            set
            {
                if (_radius != value)
                {
                    if (value <= 0.5) _radius = 0.5;
                    else _radius = Math.Round(value, 3);
                    OnPropertyChanged();
                }
            }
        }

        public Point Center
        {
            get => CenterPoint.Position;
            set => CenterPoint.Position = value;
        }

        public DraggablePoint CenterPoint { get; set; }

        public DraggablePoint RadiusPoint { get; set; }

        public double RadiusControlDistance { get; set; } = 1;

        public CustomCircle(CustomCanvas canvas, bool hasAnchorPoint = true) : base(canvas, hasAnchorPoint)
        {
            canvas.MouseWheel += (o, e) => MaxRadiusValue = CalculateMaxCircleRadius();
            Initialized += (o, e) => MaxRadiusValue = CalculateMaxCircleRadius();
        }
        protected override void InitChildren()
        {
            RadiusPoint = new DraggablePoint(Canvas, false)
            {
                Radius = 8,
                Opacity = 0.7,
                OverrideMoveAction = RadiusPointMoveAction,
                MouseLeftButtonDownAction = OnPointMouseLeftButtonDown,
                IsSelectable = false
                //OverrideRenderControlPanelAction = true
            };
            Canvas.Children.Add(RadiusPoint);
            Panel.SetZIndex(RadiusPoint, Canvas.Children.Count + 1);


            CenterPoint = new DraggablePoint(Canvas, false)
            {
                Radius = 3,
                OverrideMoveAction = CenterPointMoveAction,
                //OverrideRenderControlPanelAction = true,
                Position = new Point(0, 0),
                IsSelectable = false
            };
            Canvas.Children.Add(CenterPoint);
            Panel.SetZIndex(CenterPoint, Canvas.Children.Count + 1);
            base.InitChildren();
        }

        protected override void OnRender(DrawingContext dc)
        {
            CenterPoint.Visibility = ControlsVisibility;
            RadiusPoint.Visibility = ControlsVisibility;
            RadiusPoint.Position = new Point(Center.X + (Radius + RadiusControlDistance) * Math.Cos(Helpers.DegToRad(0)), Center.Y - Radius * Math.Sin(0));
            CenterPoint.Position = Center;
            base.OnRender(dc);
        }

        protected override void DefaultRender(DrawingContext dc)
        {
            dc.DrawCircle(Canvas, Fill, StrokePen, CenterPoint.PixelPosition, Radius * UnitSize, Precision, 10);
        }

        protected override void InitControlPanel()
        {
            base.InitControlPanel();
            AddCenterControls();
            AddFillColorControls();
            AddStrokeColorControls();
            AddStrokeThicknessControls();
            AddRadiusControls();
        }

        //protected override void RenderControlPanel()
        //{
        //    base.RenderControlPanel();
        //    AddCenterControls();
        //    AddFillColorControls();
        //    AddStrokeColorControls();
        //    AddStrokeThicknessControls();
        //    AddRadiusControls();
        //}


        protected override Point GetAnchorDefaultPosition() => Center;

        public override Point GetTopLeftPosition() => new Point(Center.X - Radius - StrokeThickness / UnitSize, Center.Y + Radius + StrokeThickness / UnitSize);

        public override Point GetBottomRightPosition() => new Point(Center.X + Radius + StrokeThickness / UnitSize, Center.Y - Radius - StrokeThickness / UnitSize);

        public event EventHandler<RadiusChangeEventArgs> OnRadiusChange;

        public virtual void RadiusPointMoveAction(DraggablePoint point, Vector offset)
        {
            var previousRadius = Radius;
            if (SnappingEnabled)
            {
                Radius = Helpers.SnapValue(Radius + offset.X / UnitSize);
            }
            else
            {
                Radius += offset.X / UnitSize;
            }
            OnRadiusChange?.Invoke(this, new RadiusChangeEventArgs(previousRadius, Radius, offset.X));
        }
        
        public virtual void CenterPointMoveAction(DraggablePoint point, Vector offset)
        {
            MoveElement(offset);
        }

        public bool OverrideMoveAction { get; set; } = false;
        public Action<Vector>? MoveAction;

        public override void MoveElement(Vector offset)
        {
            MoveAction?.Invoke(offset);
            if (OverrideMoveAction) return;

            if (InputManager.AnyKeyButShiftPressed) return;

            Center = SnappingEnabled ? Center.OffsetAndSpanPoint(offset) : Center.OffsetPoint(offset);

            base.MoveElement(offset);
        }

        public bool OverrideRotateAction { get; set; } = false;
        public Action<Point, double>? RotateAction;
        public override void RotateElement(Point anchorPoint, double degrees)
        {
            RotateAction?.Invoke(anchorPoint, degrees);
            if (OverrideRotateAction) return;
            Center = Center.RotatePoint(anchorPoint, degrees);
        }

        public bool OverrideScaleAction { get; set; } = false;
        public Action<Point, Vector, double>? ScaleAction;
        public override void ScaleElement(Point anchorPoint, Vector scaleVector, double scaleFactor)
        {
            ScaleAction?.Invoke(anchorPoint, scaleVector, scaleFactor);
            if (OverrideScaleAction) return;
            Center = Center.ScalePoint(anchorPoint, scaleVector);
            Radius *= scaleFactor;
        }

        public double CalculateMaxCircleRadius()
        {
            return Math.Max(Canvas.ActualHeight / 2, Canvas.ActualWidth / 2) / UnitSize;
        }

        public override string ToString()
        {
            return $"X: {Center.X} \nY: {Center.Y} \nRadius: {Radius}";
        }

    }
}
