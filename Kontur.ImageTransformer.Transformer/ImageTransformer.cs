using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kontur.ImageTransformer.Transformer.Model;
using System.Drawing.Imaging;
using ImageProcessor;
using System.IO;

namespace Kontur.ImageTransformer.Transformer
{
    public class ImageTransformer : IImageTransformer
    {
        public byte[] Transform(byte[] img, TransformationParametrs parametrs)
        {
            byte[] result;
            Rectangle cropRectangle = new Rectangle(parametrs.TopLeftConerX, parametrs.TopLeftConerY, parametrs.Width, parametrs.Height);
            if(cropRectangle.Width==0||cropRectangle.Height==0) throw new ArgumentOutOfRangeException("Пустая область");
            using (MemoryStream inStream = new MemoryStream(img))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream);
                        if (parametrs.Flip != Flip.None)
                            if (parametrs.Flip == Flip.Horizontal) imageFactory.Flip(false);
                            else imageFactory.Flip(true);
                        if (parametrs.Rotation != Rotation.None)
                            if (parametrs.Rotation == Rotation.Clockwise) imageFactory.Rotate(90);
                            else imageFactory.Rotate(-90);
                        try
                        {
                            imageFactory.Crop(cropRectangle);
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            throw new ArgumentOutOfRangeException("Пустая область");
                        }
                        imageFactory.Save(outStream);
                    }
                    // Do something with the stream.
                    result=outStream.ToArray();
                }
            }
            return result;
        }

        
    }
}
