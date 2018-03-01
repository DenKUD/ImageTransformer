using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Kontur.ImageTransformer.Transformer.Model
{
   public class TransformationParametrs
    {
       public Flip Flip = Flip.None;
       public Rotation Rotation=Rotation.None;
       public int TopLeftConerX;
       public int TopLeftConerY;
       public int Width;
       public int Height;
       public static TransformationParametrs Parse(string paramString)
       {
            TransformationParametrs result = new TransformationParametrs();
            List<string> Params = new List<string>(paramString.Split(' '));
            string rotateFlipParametrs = Params[0];
            /*
            switch (rotateFlipParametrs)
            {
                case ("​rotate-cw") : result.Rotation = Rotation.Clockwise ; break;
                case "rotate-ccw​": result.Rotation = Rotation.CounterClockwise; break;
                case "flip-v"   : result.Flip = Flip.Vertical; break;
                case ("flip-h")   : result.Flip = Flip.Horizontal; break;
                default: throw new ArgumentException("Неверный формат строки");
            }
            */
            if (rotateFlipParametrs.Equals("rotate-cw")) result.Rotation = Rotation.Clockwise;
            else if (rotateFlipParametrs.Equals("rotate-ccw")) result.Rotation = Rotation.CounterClockwise;
            else if (rotateFlipParametrs.Equals( "flip-v")) result.Flip = Flip.Vertical;
            else if (rotateFlipParametrs.Equals ("flip-h")) result.Flip = Flip.Horizontal;
            else throw new ArgumentException("Неверный формат строки");

            List<string> coords = new List<string>(Params[1].Split(','));
            try
            {
                result.TopLeftConerX = int.Parse(coords[0]);
                result.TopLeftConerY = int.Parse(coords[1]);
                result.Width = int.Parse(coords[2]);
                result.Height = int.Parse(coords[3]);
            }
            catch(FormatException fe)
            {
                throw new ArgumentException("Неверный формат строки");
            }

            return result;
       }
    }
}
