using Modeling_Canvas.Models;
using Modeling_Canvas.UIElements;
using System.Diagnostics;
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
            var x = point.X;
            var y = Canvas.ActualHeight - point.Y;

            double Xx = affine.Xx;
            double Xy = affine.Xy;
            double Yx = affine.Yx;
            double Yy = affine.Yy;
            double Ox = affine.Ox * Canvas.UnitSize;
            double Oy = affine.Oy * Canvas.UnitSize;

            return new Point(
                Xx * x + Yx * y + Ox,
                Canvas.ActualHeight - (Xy * x + Yy * y + Oy)
            );
        }
        public static Point ReverseAffineTransformation(this Point transformedPoint, AffineModel affine)
        { 
            transformedPoint = new Point(transformedPoint.X, Canvas.ActualHeight - transformedPoint.Y);   
            double Xx = affine.Xx;
            double Xy = affine.Yx;
            double Yx = affine.Xy;
            double Yy = affine.Yy;
            double Ox = affine.Ox * Canvas.UnitSize;
            double Oy = affine.Oy * Canvas.UnitSize;
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
                Canvas.ActualHeight - (invYx * transformedPoint.X + invYy * transformedPoint.Y + invOy)
            );
        }

        public static Point ApplyProjectiveTransformation(this Point point, ProjectiveModel projective)
        {
            // Витягуємо значення параметрів із об'єкта ProjectiveModel у локальні змінні
            double xx = projective.Xx;
            double yx = projective.Yx * Canvas.UnitSize;
            double ox = projective.Ox * Canvas.UnitSize;
            double xy = projective.Xy * Canvas.UnitSize;
            double yy = projective.Yy;
            double oy = projective.Oy * Canvas.UnitSize;
            double wx = projective.wX / 10;
            double wy = projective.wY / 10;
            double wo = projective.wO; 

            // | Xx / m00 | Xy / m01 | wX / m02 | 
            // | Yx / m10 | Yy / m11 | wY / m12 |
            // | Ox / m20 | Oy / m21 | wO / m22 |

            //      x * m00 + y * m10 + m20
            // x* = -----------------------
            //      x * m02 + y * m12 + m22

            //      x * m01 + y * m11 + m21
            // y* = -----------------------
            //      x * m02 + y * m12 + m22


            // Поточні координати точки
            double x = point.X;
            double y = Canvas.ActualHeight - point.Y;

            // Обчислення знаменника w

            //       x * m02 + y * m12 + m22
            double w = x * wx + y * wy + wo;
            if (w == 0)
                return new Point(0, 0);

            // Обчислення трансформованих координат
            //           x* m00 +y * m10 + m20
            double tx = (x * xx + y * yx + ox) / w;
            //          x * m02 + y * m12 + m22
            double ty = (x * xy + y * yy + oy) / w;

            return new Point(tx, Canvas.ActualHeight - ty);
        } 

        public static Point ReverseProjectiveTransformation(this Point canvasPoint, ProjectiveModel projective)
        {
            double xx = projective.Xx;
            double yx = projective.Yx * Canvas.UnitSize;
            double ox = projective.Ox * Canvas.UnitSize;
            double xy = projective.Xy * Canvas.UnitSize;
            double yy = projective.Yy;
            double oy = projective.Oy * Canvas.UnitSize;
            double wx = projective.wX / 10;
            double wy = projective.wY / 10;
            double wo = projective.wO; 

            // The equation is:
            // canvasX = (xx * x + yx * y + ox) / (wx * x + wy * y + wo)
            // canvasY = (xy * x + yy * y + oy) / (wx * x + wy * y + wo)
            // To reverse it, we solve for x and y.

            // Let canvasX = u, canvasY = v
            double u = canvasPoint.X;
            double v = Canvas.ActualHeight - canvasPoint.Y;

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
            return new Point(x, Canvas.ActualHeight - y);
        }



    }
}
