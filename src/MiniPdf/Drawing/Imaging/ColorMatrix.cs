using System;

namespace MiniPdf.Drawing.Imaging
{
    /// <summary>
    /// A 5×5 float matrix that defines a linear transformation applied to every pixel's
    /// (R, G, B, A, W) tuple during image rendering.  Row 4 (W) is the bias / translation row.
    /// Column order is R, G, B, A, W.
    /// </summary>
    public sealed class ColorMatrix
    {
        private readonly float[,] _m;

        /// <summary>
        /// Initializes a new <see cref="ColorMatrix"/> with an identity matrix.
        /// </summary>
        public ColorMatrix()
        {
            _m = new float[5, 5];
            // identity
            _m[0, 0] = _m[1, 1] = _m[2, 2] = _m[3, 3] = _m[4, 4] = 1f;
        }

        /// <summary>
        /// Initializes a new <see cref="ColorMatrix"/> from a 2D float array.
        /// </summary>
        /// <param name="newColorMatrix">The matrix values.</param>
        public ColorMatrix(float[][] newColorMatrix)
        {
            if (newColorMatrix == null) throw new ArgumentNullException(nameof(newColorMatrix));
            _m = new float[5, 5];
            for (int r = 0; r < 5 && r < newColorMatrix.Length; r++)
                for (int c = 0; c < 5 && c < newColorMatrix[r].Length; c++)
                    _m[r, c] = newColorMatrix[r][c];
        }

        /// <summary>
        /// Gets or sets the matrix element at the specified row and column.
        /// </summary>
        /// <param name="row">The row index (0-4).</param>
        /// <param name="col">The column index (0-4).</param>
        /// <returns>The matrix value.</returns>
        public float this[int row, int col]
        {
            get => _m[row, col];
            set => _m[row, col] = value;
        }

        // Named properties (GDI+ API compatibility)
        /// <summary>
        /// Gets or sets the matrix element at row 0, column 0.
        /// </summary>
        public float Matrix00 { get => _m[0, 0]; set => _m[0, 0] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 0, column 1.
        /// </summary>
        public float Matrix01 { get => _m[0, 1]; set => _m[0, 1] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 0, column 2.
        /// </summary>
        public float Matrix02 { get => _m[0, 2]; set => _m[0, 2] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 0, column 3.
        /// </summary>
        public float Matrix03 { get => _m[0, 3]; set => _m[0, 3] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 0, column 4.
        /// </summary>
        public float Matrix04 { get => _m[0, 4]; set => _m[0, 4] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 1, column 0.
        /// </summary>
        public float Matrix10 { get => _m[1, 0]; set => _m[1, 0] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 1, column 1.
        /// </summary>
        public float Matrix11 { get => _m[1, 1]; set => _m[1, 1] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 1, column 2.
        /// </summary>
        public float Matrix12 { get => _m[1, 2]; set => _m[1, 2] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 1, column 3.
        /// </summary>
        public float Matrix13 { get => _m[1, 3]; set => _m[1, 3] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 1, column 4.
        /// </summary>
        public float Matrix14 { get => _m[1, 4]; set => _m[1, 4] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 2, column 0.
        /// </summary>
        public float Matrix20 { get => _m[2, 0]; set => _m[2, 0] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 2, column 1.
        /// </summary>
        public float Matrix21 { get => _m[2, 1]; set => _m[2, 1] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 2, column 2.
        /// </summary>
        public float Matrix22 { get => _m[2, 2]; set => _m[2, 2] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 2, column 3.
        /// </summary>
        public float Matrix23 { get => _m[2, 3]; set => _m[2, 3] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 2, column 4.
        /// </summary>
        public float Matrix24 { get => _m[2, 4]; set => _m[2, 4] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 3, column 0.
        /// </summary>
        public float Matrix30 { get => _m[3, 0]; set => _m[3, 0] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 3, column 1.
        /// </summary>
        public float Matrix31 { get => _m[3, 1]; set => _m[3, 1] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 3, column 2.
        /// </summary>
        public float Matrix32 { get => _m[3, 2]; set => _m[3, 2] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 3, column 3.
        /// </summary>
        public float Matrix33 { get => _m[3, 3]; set => _m[3, 3] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 3, column 4.
        /// </summary>
        public float Matrix34 { get => _m[3, 4]; set => _m[3, 4] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 4, column 0.
        /// </summary>
        public float Matrix40 { get => _m[4, 0]; set => _m[4, 0] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 4, column 1.
        /// </summary>
        public float Matrix41 { get => _m[4, 1]; set => _m[4, 1] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 4, column 2.
        /// </summary>
        public float Matrix42 { get => _m[4, 2]; set => _m[4, 2] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 4, column 3.
        /// </summary>
        public float Matrix43 { get => _m[4, 3]; set => _m[4, 3] = value; }

        /// <summary>
        /// Gets or sets the matrix element at row 4, column 4.
        /// </summary>
        public float Matrix44 { get => _m[4, 4]; set => _m[4, 4] = value; }
    }
}
