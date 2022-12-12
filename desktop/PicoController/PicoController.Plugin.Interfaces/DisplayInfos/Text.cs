using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PicoController.Plugin.DisplayInfos
{
    public class Text
    {
        public Text(string text, double fontSize = 14, int fontWeight = 400)
        {
            Content = text;
            FontSize = fontSize;
            FontWeight = fontWeight;
        }

        public string Content { get; }
        public double FontSize { get; }
        public int FontWeight { get; }

        public void Deconstruct(out string content, out double fontSize, out int fontWeight)
        {
            content = Content;
            fontSize = FontSize;
            fontWeight = FontWeight;
        }
    }
}
