using System.IO;

namespace Kontur.ImageTransformer.Transformer.Test
{
    class Program
    {
        static void Main(string[] args) // usage transformerTest.exe PathToImage flip/Rotation x y width height
        {
            string path = args[0];
            string paramString = args[1] +' '+ args[2];
            Transformer.Model.TransformationParametrs transformParametrs = Transformer.Model.TransformationParametrs.Parse(paramString);
            byte[] image = File.ReadAllBytes(path);

            ImageTransformer transformer = new ImageTransformer();
            byte[] result= transformer.Transform(image, transformParametrs);
            File.WriteAllBytes("out.png", result);

        }
    }
}
