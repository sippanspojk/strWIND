using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindGhC
{
    public class TextFile
    {
        //properties / fields
        private string File = "";
        private string FileName = "";

        //constructors (in this case 2 overloads) 
        public TextFile()
        {
        }

        public TextFile(string file, string fileName)
            : this()            //this means the empty constructur will be called first and then this constructor
        {
            this.File = file;
            this.FileName = fileName;
        }


        //methods
        public string GetName()
        {
            return FileName;
        }

        void SetName(string newFileName)
        {
            FileName = newFileName;
        }

        public string GetFileText()
        {
            return File;
        }


    }
}
