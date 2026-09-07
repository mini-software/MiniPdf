using MiniPdf.Drawing.Metafile.Emf;

namespace MiniPdf.Drawing.Metafile.Emf.Records.DrawingRecordTypes
{
    using System;
    using System.IO;
    using MiniPdf.Drawing.Metafile.Emf.Enumerations;
    using MiniPdf.Drawing.Metafile.Emf.Objects;
    using MiniPdf.Drawing.Metafile.Wmf.Objects;

    internal class EMR_GRADIENTFILL : Record
    {
        public RectL Bounds = new();

        public uint VertexCount;

        public uint GradientCount;

        public GradientFill Mode;

        public TriVertex[] Vertices { get; set; } = Array.Empty<TriVertex>();

        public GradientRectangle[] GradientRectangles { get; set; } = Array.Empty<GradientRectangle>();

        public GradientTriangle[] GradientTriangles { get; set; } = Array.Empty<GradientTriangle>();

        public EMR_GRADIENTFILL()
        {
            Header = new RecordHeader { Type = RecordType.EMR_GRADIENTFILL };
        }

        public override void Read(BinaryReader reader)
        {
            Bounds.Read(reader);
            VertexCount = reader.ReadUInt32();
            GradientCount = reader.ReadUInt32();
            Mode = (GradientFill)reader.ReadUInt32();

            Vertices = new TriVertex[VertexCount];
            for (int i = 0; i < VertexCount; i++)
            {
                var vertex = new TriVertex();
                vertex.Read(reader);
                Vertices[i] = vertex;
            }

            if (Mode == GradientFill.GradientFillTriangle)
            {
                GradientTriangles = new GradientTriangle[GradientCount];
                for (int i = 0; i < GradientCount; i++)
                {
                    var triangle = new GradientTriangle();
                    triangle.Read(reader);
                    GradientTriangles[i] = triangle;
                }
            }
            else
            {
                GradientRectangles = new GradientRectangle[GradientCount];
                for (int i = 0; i < GradientCount; i++)
                {
                    var rectangle = new GradientRectangle();
                    rectangle.Read(reader);
                    GradientRectangles[i] = rectangle;
                }
            }
        }

        public override void Write(BinaryWriter writer)
        {
            uint vertexCount = VertexCount == 0 ? (uint)Vertices.Length : VertexCount;

            Bounds.Write(writer);
            writer.Write(vertexCount);

            if (Mode == GradientFill.GradientFillTriangle)
            {
                uint gradientCount = GradientCount == 0 ? (uint)GradientTriangles.Length : GradientCount;
                writer.Write(gradientCount);
                writer.Write((uint)Mode);
                for (int i = 0; i < vertexCount; i++)
                {
                    Vertices[i].Write(writer);
                }

                for (int i = 0; i < gradientCount; i++)
                {
                    GradientTriangles[i].Write(writer);
                }
            }
            else
            {
                uint gradientCount = GradientCount == 0 ? (uint)GradientRectangles.Length : GradientCount;
                writer.Write(gradientCount);
                writer.Write((uint)Mode);
                for (int i = 0; i < vertexCount; i++)
                {
                    Vertices[i].Write(writer);
                }

                for (int i = 0; i < gradientCount; i++)
                {
                    GradientRectangles[i].Write(writer);
                }
            }
        }
    }
}
