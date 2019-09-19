using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindGhC
{
    public class TextFile
    {
        //properties
        private string textFile = "";
        private string textFileName = "";

        //constructors (in this case 2 overloads) 
        public TextFile()
        {
        }

        public TextFile(string textFile, string textFileName)
        {
            this.textFile = textFile;
            this.textFileName = textFileName;
        }


        //methods
        public string GetName()
        {
            return textFileName;
        }

        void SetName(string newName)
        {
            textFileName = newName;
        }

        public string GetFileText()
        {
            return textFile;
        }


    }
}
