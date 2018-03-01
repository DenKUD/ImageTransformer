using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kontur.ImageTransformer.Transformer
{
    public interface IImageTransformer
    {
         Image Transform(Image img,Model.TransformationParametrs parametrs);

    }
}
