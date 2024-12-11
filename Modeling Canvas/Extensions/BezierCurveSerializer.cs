using Modeling_Canvas.Models;
using Modeling_Canvas.UIElements;
using Newtonsoft.Json;
using System.IO;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.Media.Animation;

public static class BezierCurveSerializer
{
    // Serialize and save to a file
    public static void SerializeToFile(this BezierCurve bezierCurve, string filePath)
    {
        var dto = new BezierCurveDto
        {
            Points = bezierCurve.Points.Select(p => p.Position).ToList(),
            ControlPrevPoints = bezierCurve.Points.Select(p => p.ControlPrevPoint.Position).ToList(),
            ControlNextPoints = bezierCurve.Points.Select(p => p.ControlNextPoint.Position).ToList(),
            Fill = bezierCurve.Fill?.ToString(),
            Stroke = bezierCurve.Stroke?.ToString(),
            StrokeThickness = bezierCurve.StrokeThickness,
            HasAnchorPoint = bezierCurve.HasAnchorPoint,
            AnchorPointPosition = bezierCurve.HasAnchorPoint ? bezierCurve.AnchorPoint?.Position : null,
            AnimationFrames = bezierCurve.AnimationFrames
        };

        var json = JsonConvert.SerializeObject(dto, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    // Load from a file and deserialize
    public static BezierCurve DeserializeFromFile(string filePath, CustomCanvas canvas)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        var json = File.ReadAllText(filePath);
        var dto = JsonConvert.DeserializeObject<BezierCurveDto>(json);

        if (dto == null)
            throw new InvalidOperationException("Deserialization failed!");

        var bezierCurve = new BezierCurve(canvas)
        {
            Fill = !string.IsNullOrEmpty(dto.Fill) ? (Brush)new BrushConverter().ConvertFromString(dto.Fill) : null,
            Stroke = !string.IsNullOrEmpty(dto.Stroke) ? (Brush)new BrushConverter().ConvertFromString(dto.Stroke) : null,
            StrokeThickness = dto.StrokeThickness
        };

        // Додати криву до Canvas
        canvas.Children.Add(bezierCurve);

        // Встановити точки кривої
        for (int i = 0; i < dto.Points.Count; i++)
        {
            bezierCurve.AddBezierPoint(dto.Points[i], dto.ControlPrevPoints[i], dto.ControlNextPoints[i]);
        }

        // Встановити AnchorPoint, якщо HasAnchorPoint == true
        if (dto.HasAnchorPoint && dto.AnchorPointPosition.HasValue)
        {
            bezierCurve.AnchorPoint.Position = dto.AnchorPointPosition.Value;
        }

        // Десеріалізувати AnimationFrames
        if (dto.AnimationFrames != null)
        {
            foreach (var frame in dto.AnimationFrames)
            {
                bezierCurve.InitFrame(frame.Key, frame.Value);
            }
            bezierCurve.UpdateFramesPanel();
        }

        return bezierCurve;
    }
}
