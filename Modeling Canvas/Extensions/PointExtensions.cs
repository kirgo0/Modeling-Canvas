using Modeling_Canvas.Models;
using Modeling_Canvas.UIELements;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Modeling_Canvas.Extensions
{
    public static class PointExtensions
    {
        public static CustomCanvas Canvas { get; set; }
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
            double x = point.X, y = point.Y;
            double w = projective.wX * x + projective.wY * y + projective.wO;
            if (w == 0) return new Point(0, 0); // Avoid division by zero

            double tx = (projective.Xx * x + projective.Yx * y + projective.Ox) / w;
            double ty = (projective.Xy * x + projective.Yy * y + projective.Oy) / w;

            //return new Point(tx, projective.CanvasHeight - ty);
            return new Point(tx, ty);
        }
        public static Point ReverseProjectiveTransformation2(this Point point, ProjectiveModel projective)
        {
            double x = point.X, y = point.Y;

            // Compute the determinant of the matrix
            double determinant =
                projective.Xx * (projective.Yy * projective.wO - projective.Yx * projective.wY) -
                projective.Yx * (projective.Xy * projective.wO - projective.Xx * projective.wY) +
                projective.wX * (projective.Xy * projective.Yx - projective.Xx * projective.Yy);

            if (determinant == 0)
                throw new InvalidOperationException("Projective transformation matrix is non-invertible.");

            // Compute the inverse of the projective transformation matrix
            double invXx = (projective.Yy * projective.wO - projective.Yx * projective.wY) / determinant;
            double invYx = (projective.Yx * projective.wX - projective.Xx * projective.wO) / determinant;
            double invwX = (projective.Xx * projective.wY - projective.Xy * projective.Yx) / determinant;

            double invXy = (projective.wX * projective.Yy - projective.Xy * projective.wO) / determinant;
            double invYy = (projective.Xx * projective.wO - projective.Yx * projective.wX) / determinant;
            double invwY = (projective.Xy * projective.Yx - projective.Xx * projective.Yy) / determinant;

            double invOx = (projective.Xy * (projective.Yx * projective.wY - projective.Yy * projective.wX) +
                            projective.Xx * (projective.Yy * projective.wO - projective.Yx * projective.wY) -
                            projective.Yx * (projective.Xy * projective.wO - projective.Xx * projective.wY)) / determinant;

            double invOy = (projective.Xx * (projective.Yy * projective.wO - projective.Yx * projective.wY) -
                            projective.Yy * (projective.Xy * projective.wO - projective.Xx * projective.wY) +
                            projective.Xy * (projective.Yx * projective.wX - projective.Yy * projective.wX)) / determinant;

            double invwO = (projective.Xx * (projective.Yy * projective.wY - projective.Yx * projective.wX) +
                            projective.Yx * (projective.Xy * projective.wO - projective.Xx * projective.wY) -
                            projective.wX * (projective.Xy * projective.Yx - projective.Xx * projective.Yy)) / determinant;

            // Apply the inverse transformation
            double w = invwX * x + invwY * y + invwO;
            if (w == 0) return new Point(0, 0); // Avoid division by zero

            double originalX = (invXx * x + invYx * y + invOx) / w;
            double originalY = (invXy * x + invYy * y + invOy) / w;

            return new Point(originalX, originalY);
        }

        public static Point ReverseProjectiveTransformation(this Point canvasPoint, ProjectiveModel projective)
        {
            canvasPoint = new Point(canvasPoint.X, canvasPoint.Y);
            double xx = projective.Xx;
            double yx = projective.Yx;
            double ox = projective.Ox;
            double xy = projective.Xy;
            double yy = projective.Yy;
            double oy = projective.Oy;
            double wx = projective.wX;
            double wy = projective.wY;
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

        //public static Point ReverseProjectiveTransformation(this Point transformedPoint, ProjectiveModel projective)
        //{
        //    double Xx = projective.Xx;
        //    double Xy = projective.Xy;
        //    double Ox = projective.Ox;
        //    double Yx = projective.Yx;
        //    double Yy = projective.Yy;
        //    double Oy = projective.Oy;
        //    double Wx = projective.wX;
        //    double Wy = projective.wY;
        //    double Wo = projective.wO;

        //    // Invert the matrix for projective transformation
        //    double determinant = Xx * (Yy * Wo - Oy * Wy) - Xy * (Yx * Wo - Oy * Wx) + Ox * (Yx * Wy - Yy * Wx);

        //    double invXx = (Yy * Wo - Oy * Wy) / determinant;
        //    double invXy = (Ox * Wy - Xy * Wo) / determinant;
        //    double invOx = (Xy * Oy - Ox * Yy) / determinant;
        //    double invYx = (Oy * Wx - Yx * Wo) / determinant;
        //    double invYy = (Xx * Wo - Ox * Wx) / determinant;
        //    double invOy = (Ox * Yx - Xx * Oy) / determinant;
        //    double invWx = (Yx * Wy - Yy * Wx) / determinant;
        //    double invWy = (Xy * Wx - Xx * Wy) / determinant;
        //    double invWo = (Xx * Yy - Xy * Yx) / determinant;

        //    double w = invWx * transformedPoint.X + invWy * transformedPoint.Y + invWo;
        //    double x = (invXx * transformedPoint.X + invXy * transformedPoint.Y + invOx) / w;
        //    double y = (invYx * transformedPoint.X + invYy * transformedPoint.Y + invOy) / w;

        //    return new Point(x, y);
        //}



    }
}
