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
            // Перерасчет координат обрезки
            if (result.TopLeftConerX < 0 && result.Width>0)
            {
                result.Width = result.Width + result.TopLeftConerX;
                result.TopLeftConerX = 0;
            }
            if (result.TopLeftConerY < 0 && result.Height > 0)
            {
                result.Height = result.Height + result.TopLeftConerY;
                result.TopLeftConerY = 0;
            }
            if (result.TopLeftConerX > 0 && result.Width < 0)
            {
                result.TopLeftConerX = result.TopLeftConerX + result.Width;
                result.Width *= -1;
            }
            if (result.TopLeftConerY > 0 && result.Height < 0)
            {
                result.TopLeftConerY = result.TopLeftConerY + result.Height;
                result.Height *= -1;
            }
            return result;
       }
    }
}
