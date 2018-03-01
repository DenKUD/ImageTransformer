using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Transformer.Model;

namespace Kontur.ImageTransformer.Transformer
{
    public class ImageTransformer : IImageTransformer
    {
        public Image Transform(Image img, TransformationParametrs parametrs)
        {
            Bitmap result = new Bitmap(img);
            RotateFlipType rotation;
            rotation = RotateFlipType.RotateNoneFlipNone;
            if (parametrs.Rotation == Rotation.Clockwise) rotation = RotateFlipType.Rotate90FlipNone;
            if (parametrs.Rotation == Rotation.CounterClockwise) rotation = RotateFlipType.Rotate270FlipNone;
            if (parametrs.Flip == Flip.Horizontal) rotation = RotateFlipType.RotateNoneFlipX;
            if (parametrs.Flip == Flip.Vertical) rotation = RotateFlipType.RotateNoneFlipY;
            result.RotateFlip(rotation);
            result = Cut(result, parametrs);
            return result;
        }
        private Bitmap Cut(Bitmap img, TransformationParametrs parametrs)
        {
            Bitmap result = new Bitmap(parametrs.Width, parametrs.Height);
            int startXCoord = parametrs.TopLeftConerX;
            int startYCoord = parametrs.TopLeftConerY;
            int width = parametrs.Width;
            int heidht = parametrs.Height;
            bool isEmpty = true;
            
            for(int yCoord= startYCoord; yCoord< startYCoord + width; yCoord++)
                for (int xCoord=startXCoord; xCoord< startXCoord +heidht; xCoord++)
                {
                    Color pixelColour = new Color();
                    //try
                    //{
                         pixelColour = img.GetPixel(xCoord, yCoord);
                        isEmpty = false;
                    //}
                    /*
                    catch (ArgumentOutOfRangeException)
                    {
                        pixelColour = Color.Transparent;
                    }
                    */
                    result.SetPixel(xCoord - startXCoord, yCoord - startYCoord, pixelColour);
                }
            if(isEmpty)
            {
                throw new ArgumentException("Пустая область");
            }
            return result;
        }
    }
}
