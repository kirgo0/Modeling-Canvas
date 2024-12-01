using Modeling_Canvas.Models;
using Modeling_Canvas.UIELements;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Modeling_Canvas.Extensions
{
    public static class PointExtensions
    {
        public static CustomCanvas Canvas { get; set; }

        public static Point AddCanvasOffsets(this Point p)
        {
            return new Point(p.X + Canvas.XOffset, p.Y - Canvas.YOffset);
        }

        public static Point RotatePoint(this Point point1, Point point2, double degrees)
        {
            // Calculate rotation
            double dx = point1.X - point2.X;
            double dy = point1.Y - point2.Y;
            double radians = Helpers.DegToRad(degrees);

            double rotatedX = Math.Cos(radians) * dx - Math.Sin(radians) * dy + point2.X;
            double rotatedY = Math.Sin(radians) * dx + Math.Cos(radians) * dy + point2.Y;

            // Update point position
            return new Point(Math.Round(rotatedX, Canvas.RotationPrecision), Math.Round(rotatedY, Canvas.RotationPrecision));
        }

        public static Point OffsetPoint(this Point point, Vector offset)
        {
            return new Point(
                point.X + offset.X / Canvas.UnitSize,
                point.Y - offset.Y / Canvas.UnitSize
                );
        }

        public static Point OffsetAndSpanPoint(this Point point, Vector offset)
        {
            return new Point(
                Helpers.SnapValue(point.X + offset.X / Canvas.UnitSize),
                Helpers.SnapValue(point.Y - offset.Y / Canvas.UnitSize)
            );
        }

        public static Point ScalePoint(this Point point, Point anchor, Vector scaleVector)
        {
            // Apply scaling transformation
            double newX = anchor.X + scaleVector.X * (point.X - anchor.X);
            double newY = anchor.Y + scaleVector.Y * (point.Y - anchor.Y);

            // Update the point in the list
            return new Point(newX, newY);
        }
        public static Point ApplyAffineTransformation(this Point point, AffineModel affine)
        {
            double Xx = affine.Xx;
            double Xy = -affine.Xy;
            double Yx = -affine.Yx;
            double Yy = affine.Yy;
            double Ox = affine.Ox;
            double Oy = affine.Oy;

            return new Point(
                Xx * point.X + Yx * point.Y + Ox,
                Xy * point.X + Yy * point.Y + Oy
            );
        }
        public static Point ReverseAffineTransformation(this Point transformedPoint, AffineModel affine)
        { 
            transformedPoint = new Point(transformedPoint.X, transformedPoint.Y);   
            double Xx = affine.Xx;
            double Xy = -affine.Yx;
            double Yx = -affine.Xy;
            double Yy = affine.Yy;
            double Ox = affine.Ox;
            double Oy = affine.Oy;
            // Calculate the determinant of the 2x2 matrix
            double determinant = Xx * Yy - Xy * Yx;

            // Compute the inverse of the 2x2 matrix
            double invXx = Yy / determinant;
            double invXy = -Xy / determinant;
            double invYx = -Yx / determinant;
            double invYy = Xx / determinant;

            // Adjust the offset
            double invOx = -(invXx * Ox + invXy * Oy);
            double invOy = -(invYx * Ox + invYy * Oy);

            // Apply the inverse transformation to the point

            return new Point(
                invXx * transformedPoint.X + invXy * transformedPoint.Y + invOx,
                invYx * transformedPoint.X + invYy * transformedPoint.Y + invOy
            );
        }

        public static Point ApplyProjectiveTransformation(this Point point, ProjectiveModel projective)
        {
            // Витягуємо значення параметрів із об'єкта ProjectiveModel у локальні змінні
            double xx = projective.Xx;
            double yx = -projective.Yx;
            double ox = projective.Ox;
            double xy = -projective.Xy;
            double yy = projective.Yy;
            double oy = projective.Oy;
            double wx = projective.wX;
            double wy = -projective.wY;
            double wo = projective.wO;

            // Поточні координати точки
            double x = point.X;
            double y = point.Y;

            // Обчислення знаменника w
            double w = wx * x + wy * y + wo;
            if (w == 0)
                return new Point(0, 0); // Уникаємо ділення на нуль

            // Обчислення трансформованих координат
            double tx = (xx * x + yx * y + ox) / w;
            double ty = (xy * x + yy * y + oy) / w;

            return new Point(tx, ty);
        }

        public static Point ReverseProjectiveTransformation(this Point canvasPoint, ProjectiveModel projective)
        {
            canvasPoint = new Point(canvasPoint.X, canvasPoint.Y);
            double xx = projective.Xx;
            double yx = -projective.Yx;
            double ox = projective.Ox;
            double xy = -projective.Xy;
            double yy = projective.Yy;
            double oy = projective.Oy;
            double wx = projective.wX;
            double wy = -projective.wY;
            double wo = projective.wO;

            // The equation is:
            // canvasX = (xx * x + yx * y + ox) / (wx * x + wy * y + wo)
            // canvasY = (xy * x + yy * y + oy) / (wx * x + wy * y + wo)
            // To reverse it, we solve for x and y.

            // Let canvasX = u, canvasY = v
            double u = canvasPoint.X;
            double v = canvasPoint.Y;

            // Set up the system of equations:
            // u * (wx * x + wy * y + wo) = xx * x + yx * y + ox
            // v * (wx * x + wy * y + wo) = xy * x + yy * y + oy

            // Rearrange into:
            // (xx - u * wx) * x + (yx - u * wy) * y = u * wo - ox
            // (xy - v * wx) * x + (yy - v * wy) * y = v * wo - oy

            // Coefficients for the system of linear equations:
            double a1 = xx - u * wx;
            double b1 = yx - u * wy;
            double c1 = u * wo - ox;

            double a2 = xy - v * wx;
            double b2 = yy - v * wy;
            double c2 = v * wo - oy;

            // Solve the 2x2 system of linear equations using the determinant method:
            double determinant = a1 * b2 - a2 * b1;

            //if (Math.Abs(determinant) < 1e-10)
            //    throw new InvalidOperationException("The system of equations is singular and cannot be solved.");

            // Calculate the original point (x, y)
            double x = (c1 * b2 - c2 * b1) / determinant;
            double y = (a1 * c2 - a2 * c1) / determinant;

            //return new Point(x, projective.CanvasHeight - y);
            return new Point(x, y);
        }



    }
}
