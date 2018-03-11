namespace Kontur.ImageTransformer.Transformer
{
    public interface IImageTransformer
    {
         byte[] Transform(byte[] img,Model.TransformationParametrs parametrs);

    }
}
