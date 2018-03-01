using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Kontur.ImageTransformer.Transformer;

namespace Kontur.ImageTransformer.Transformer.Test
{
    class Program
    {
        static void Main(string[] args) // usage transformerTest.exe PathToImage flip/Rotation x y width height
        {
            string path = args[0];
            string paramString = args[1] +' '+ args[2];
            Transformer.Model.TransformationParametrs transformParametrs = Transformer.Model.TransformationParametrs.Parse(paramString);
            Bitmap image = new Bitmap(path);
            ImageTransformer transformer = new ImageTransformer();
            Bitmap result= (Bitmap) transformer.Transform(image, transformParametrs);
            result.Save("out.png");

        }
    }
}
