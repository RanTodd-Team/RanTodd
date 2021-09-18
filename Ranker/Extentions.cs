using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Ranker
{
    public class Extentions
    {
        // By @Ahmed605

        public static IPathCollection GoodBuildCorners(int imageWidth, int imageHeight, float cornerRadius)
        {
            var rect1 = new RectangularPolygon(cornerRadius / 2, imageHeight - cornerRadius, cornerRadius / 2, imageHeight - (cornerRadius / 2));
            var rect2 = new RectangularPolygon(imageWidth - (cornerRadius / 2), imageHeight - cornerRadius, cornerRadius / 2, imageHeight - (cornerRadius / 2));
            var rect3 = new RectangularPolygon(cornerRadius / 2, 0, imageWidth - cornerRadius, imageHeight);

            IPath corner1 = new EllipsePolygon(cornerRadius / 2, cornerRadius / 2, cornerRadius / 2);
            IPath corner2 = new EllipsePolygon(cornerRadius / 2, imageHeight - (cornerRadius / 2), cornerRadius / 2);
            IPath corner3 = new EllipsePolygon(imageWidth - (cornerRadius / 2), cornerRadius / 2, cornerRadius / 2);
            IPath corner4 = new EllipsePolygon(imageWidth - (cornerRadius / 2), imageHeight - (cornerRadius / 2), cornerRadius / 2);

            return new PathCollection(corner1, corner2, corner3, corner4, rect1, rect2, rect3);
        }

        public static Image RoundCorners(Image<Rgba32> image, int? radius)
        {
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            int cornerRadius = radius ?? ((imageWidth + imageHeight) / 2 / 2);
            var rect = new RectangularPolygon(-0.5f, -0.5f, cornerRadius, cornerRadius);

            IPath cornerTopLeft = rect.Clip(new EllipsePolygon(cornerRadius - 0.5f, cornerRadius - 0.5f, cornerRadius));

            float rightPos = imageWidth - cornerTopLeft.Bounds.Width + 1;
            float bottomPos = imageHeight - cornerTopLeft.Bounds.Height + 1;

            IPath cornerTopRight = cornerTopLeft.RotateDegree(90).Translate(rightPos, 0);
            IPath cornerBottomLeft = cornerTopLeft.RotateDegree(-90).Translate(0, bottomPos);
            IPath cornerBottomRight = cornerTopLeft.RotateDegree(180).Translate(rightPos, bottomPos);

            var newImage = image;
            foreach (var c in new PathCollection(cornerTopLeft, cornerBottomLeft, cornerTopRight, cornerBottomRight))
            {
                newImage.Mutate(x => x.Fill(Color.Black, c));
            }
            return newImage;
        }
    }
}
