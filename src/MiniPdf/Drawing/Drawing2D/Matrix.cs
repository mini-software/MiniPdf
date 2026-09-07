using MiniPdf.Drawing.Geometry;
using System;

namespace MiniPdf.Drawing.Drawing2D
{
    /// <summary>
    /// Represents a 2D affine transformation matrix.
    /// Internal storage: [M11, M12, M21, M22, DX, DY] (GDI+ element order).
    /// Row-vector transform: x' = x*M11 + y*M21 + DX,  y' = x*M12 + y*M22 + DY.
    /// </summary>
    /// <remarks>
    /// A <see cref="Matrix"/> can be used to transform coordinates when drawing with <see cref="Graphics"/>.
    /// It supports translation, scaling, rotation, and shearing operations.
    /// </remarks>
    public sealed class Matrix : IDisposable
    {
        // Index: 0=M11, 1=M12, 2=M21, 3=M22, 4=DX, 5=DY
        private float[] _elements;

        /// <summary>
        /// Initializes a new <see cref="Matrix"/> as the identity matrix.
        /// </summary>
        public Matrix()
        {
            _elements = new float[] { 1f, 0f, 0f, 1f, 0f, 0f };
        }

        /// <summary>
        /// Initializes a new <see cref="Matrix"/> with the specified elements.
        /// </summary>
        /// <param name="m11">The element at row 1, column 1.</param>
        /// <param name="m12">The element at row 1, column 2.</param>
        /// <param name="m21">The element at row 2, column 1.</param>
        /// <param name="m22">The element at row 2, column 2.</param>
        /// <param name="dx">The horizontal translation component.</param>
        /// <param name="dy">The vertical translation component.</param>
        public Matrix(float m11, float m12, float m21, float m22, float dx, float dy)
        {
            _elements = new float[] { m11, m12, m21, m22, dx, dy };
        }

        /// <summary>
        /// Creates a matrix that maps <paramref name="rect"/> to the parallelogram
        /// defined by three destination points (top-left, top-right, bottom-left).
        /// </summary>
        public Matrix(RectangleF rect, PointF[] plgpts)
        {
            if (plgpts == null) throw new ArgumentNullException(nameof(plgpts));
            if (plgpts.Length != 3) throw new ArgumentException("plgpts must have exactly 3 points.", nameof(plgpts));

            // Map rect corners (top-left→plgpts[0], top-right→plgpts[1], bottom-left→plgpts[2])
            float m11 = (plgpts[1].X - plgpts[0].X) / rect.Width;
            float m12 = (plgpts[1].Y - plgpts[0].Y) / rect.Width;
            float m21 = (plgpts[2].X - plgpts[0].X) / rect.Height;
            float m22 = (plgpts[2].Y - plgpts[0].Y) / rect.Height;
            float dx  = plgpts[0].X - rect.X * m11 - rect.Y * m21;
            float dy  = plgpts[0].Y - rect.X * m12 - rect.Y * m22;
            _elements = new float[] { m11, m12, m21, m22, dx, dy };
        }

        /// <summary>Returns a copy of the six elements [M11, M12, M21, M22, DX, DY].</summary>
        public float[] Elements => (float[])_elements.Clone();

        /// <summary>
        /// Gets the horizontal translation component of this matrix.
        /// </summary>
        public float OffsetX => _elements[4];

        /// <summary>
        /// Gets the vertical translation component of this matrix.
        /// </summary>
        public float OffsetY => _elements[5];

        /// <summary>
        /// Gets a value indicating whether this matrix is the identity matrix.
        /// </summary>
        public bool IsIdentity
            => _elements[0] == 1f && _elements[1] == 0f
            && _elements[2] == 0f && _elements[3] == 1f
            && _elements[4] == 0f && _elements[5] == 0f;

        /// <summary>
        /// Gets a value indicating whether this matrix is invertible.
        /// </summary>
        public bool IsInvertible
        {
            get
            {
                float det = _elements[0] * _elements[3] - _elements[1] * _elements[2];
                return det != 0f;
            }
        }

        /// <summary>
        /// Resets this matrix to the identity matrix.
        /// </summary>
        public void Reset()
        {
            _elements[0] = 1f; _elements[1] = 0f;
            _elements[2] = 0f; _elements[3] = 1f;
            _elements[4] = 0f; _elements[5] = 0f;
        }

        /// <summary>
        /// Multiplies this matrix by <paramref name="matrix"/> in the specified order.
        /// Prepend: result = matrix * this  (matrix applied first to points).
        /// Append:  result = this * matrix  (matrix applied after this to points).
        /// </summary>
        public void Multiply(Matrix matrix, MatrixOrder order = MatrixOrder.Prepend)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));
            MultiplyDirect(
                matrix._elements[0], matrix._elements[1],
                matrix._elements[2], matrix._elements[3],
                matrix._elements[4], matrix._elements[5],
                order);
        }

        /// <summary>
        /// Rotates this matrix by the specified angle.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void Rotate(float angle, MatrixOrder order = MatrixOrder.Prepend)
        {
            double rad = angle * (Math.PI / 180.0);
            float cos = (float)Math.Cos(rad);
            float sin = (float)Math.Sin(rad);
            // Rotation matrix in GDI+ row-vector order: [cos, sin, -sin, cos, 0, 0]
            MultiplyDirect(cos, sin, -sin, cos, 0f, 0f, order);
        }

        /// <summary>
        /// Rotates this matrix by the specified angle around the specified point.
        /// </summary>
        /// <param name="angle">The angle of rotation in degrees.</param>
        /// <param name="center">The center point of rotation.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void RotateAt(float angle, PointF center, MatrixOrder order = MatrixOrder.Prepend)
        {
            if (order == MatrixOrder.Prepend)
            {
                Translate(center.X, center.Y, MatrixOrder.Prepend);
                Rotate(angle, MatrixOrder.Prepend);
                Translate(-center.X, -center.Y, MatrixOrder.Prepend);
            }
            else
            {
                Translate(-center.X, -center.Y, MatrixOrder.Append);
                Rotate(angle, MatrixOrder.Append);
                Translate(center.X, center.Y, MatrixOrder.Append);
            }
        }

        /// <summary>
        /// Translates this matrix by the specified offsets.
        /// </summary>
        /// <param name="offsetX">The horizontal translation.</param>
        /// <param name="offsetY">The vertical translation.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void Translate(float offsetX, float offsetY, MatrixOrder order = MatrixOrder.Prepend)
        {
            MultiplyDirect(1f, 0f, 0f, 1f, offsetX, offsetY, order);
        }

        /// <summary>
        /// Scales this matrix by the specified factors.
        /// </summary>
        /// <param name="scaleX">The horizontal scale factor.</param>
        /// <param name="scaleY">The vertical scale factor.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void Scale(float scaleX, float scaleY, MatrixOrder order = MatrixOrder.Prepend)
        {
            MultiplyDirect(scaleX, 0f, 0f, scaleY, 0f, 0f, order);
        }

        /// <summary>
        /// Shears this matrix by the specified factors.
        /// </summary>
        /// <param name="shearX">The horizontal shear factor.</param>
        /// <param name="shearY">The vertical shear factor.</param>
        /// <param name="order">The order of multiplication (prepend or append).</param>
        public void Shear(float shearX, float shearY, MatrixOrder order = MatrixOrder.Prepend)
        {
            MultiplyDirect(1f, shearY, shearX, 1f, 0f, 0f, order);
        }

        /// <summary>
        /// Inverts this matrix.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown when the matrix is not invertible.</exception>
        public void Invert()
        {
            float det = _elements[0] * _elements[3] - _elements[1] * _elements[2];
            if (det == 0f)
                throw new InvalidOperationException("Matrix is not invertible.");
            float invDet = 1f / det;
            float m11 =  _elements[3] * invDet;
            float m12 = -_elements[1] * invDet;
            float m21 = -_elements[2] * invDet;
            float m22 =  _elements[0] * invDet;
            float dx  = (_elements[2] * _elements[5] - _elements[3] * _elements[4]) * invDet;
            float dy  = (_elements[1] * _elements[4] - _elements[0] * _elements[5]) * invDet;
            _elements = new float[] { m11, m12, m21, m22, dx, dy };
        }

        /// <summary>
        /// Transforms an array of points by this matrix.
        /// </summary>
        /// <param name="pts">The array of points to transform.</param>
        public void TransformPoints(PointF[] pts)
        {
            if (pts == null) throw new ArgumentNullException(nameof(pts));
            float m11 = _elements[0], m12 = _elements[1];
            float m21 = _elements[2], m22 = _elements[3];
            float dx  = _elements[4], dy  = _elements[5];
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X, y = pts[i].Y;
                pts[i] = new PointF(x * m11 + y * m21 + dx, x * m12 + y * m22 + dy);
            }
        }

        /// <summary>
        /// Transforms an array of points by this matrix.
        /// </summary>
        /// <param name="pts">The array of points to transform.</param>
        public void TransformPoints(Point[] pts)
        {
            if (pts == null) throw new ArgumentNullException(nameof(pts));
            float m11 = _elements[0], m12 = _elements[1];
            float m21 = _elements[2], m22 = _elements[3];
            float dx  = _elements[4], dy  = _elements[5];
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X, y = pts[i].Y;
                pts[i] = new Point(
                    (int)Math.Round(x * m11 + y * m21 + dx),
                    (int)Math.Round(x * m12 + y * m22 + dy));
            }
        }

        /// <summary>
        /// Transforms an array of vectors by this matrix, ignoring the translation components.
        /// </summary>
        /// <param name="pts">The array of vectors to transform.</param>
        public void TransformVectors(PointF[] pts)
        {
            if (pts == null) throw new ArgumentNullException(nameof(pts));
            float m11 = _elements[0], m12 = _elements[1];
            float m21 = _elements[2], m22 = _elements[3];
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].X, y = pts[i].Y;
                pts[i] = new PointF(x * m11 + y * m21, x * m12 + y * m22);
            }
        }

        /// <summary>
        /// Creates an exact copy of this <see cref="Matrix"/>.
        /// </summary>
        /// <returns>A new <see cref="Matrix"/> with the same elements.</returns>
        public Matrix Clone()
            => new Matrix(_elements[0], _elements[1], _elements[2], _elements[3], _elements[4], _elements[5]);

        /// <summary>
        /// Releases all resources used by this <see cref="Matrix"/>.
        /// </summary>
        public void Dispose() { /* all managed */ }

        // -----------------------------------------------------------------------
        // Private helpers
        // -----------------------------------------------------------------------

        /// <summary>
        /// Core multiply without allocating a temporary Matrix object.
        /// b = the incoming operation matrix; a = this (current matrix).
        /// Prepend: result = b * a   (b applied first to points)
        /// Append:  result = a * b   (b applied after a to points)
        /// </summary>
        private void MultiplyDirect(
            float bm11, float bm12, float bm21, float bm22, float bdx, float bdy,
            MatrixOrder order)
        {
            float[] a = _elements;
            float r0, r1, r2, r3, r4, r5;

            if (order == MatrixOrder.Prepend)
            {
                // result = b * a
                r0 = bm11 * a[0] + bm12 * a[2];
                r1 = bm11 * a[1] + bm12 * a[3];
                r2 = bm21 * a[0] + bm22 * a[2];
                r3 = bm21 * a[1] + bm22 * a[3];
                r4 = bdx  * a[0] + bdy  * a[2] + a[4];
                r5 = bdx  * a[1] + bdy  * a[3] + a[5];
            }
            else
            {
                // result = a * b
                r0 = a[0] * bm11 + a[1] * bm21;
                r1 = a[0] * bm12 + a[1] * bm22;
                r2 = a[2] * bm11 + a[3] * bm21;
                r3 = a[2] * bm12 + a[3] * bm22;
                r4 = a[4] * bm11 + a[5] * bm21 + bdx;
                r5 = a[4] * bm12 + a[5] * bm22 + bdy;
            }

            _elements = new float[] { r0, r1, r2, r3, r4, r5 };
        }
    }
}
