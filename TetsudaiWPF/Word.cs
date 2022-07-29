using IronOcr;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TetsudaiWPF
{
    internal class Word
    {
        public string Text { get; set; }

        public Word(OcrResult.Word ocrWord)
        {
            Text = ocrWord.Text;
        }
    }
}
