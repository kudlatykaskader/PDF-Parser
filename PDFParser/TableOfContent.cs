using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDFParser
{
    class TableOfContent
    {
        public TableOfContent()
        {

        }
        public int getWDRSIndexPage(List<string> pages)
        {
            /*
            foreach (string in  )
            foreach (string s in Program.PageToLines()
            {
                if (s.Contains("INDEKS KRS"))
                {
                    string[] words = s.Split(wordDelimeter);
                    foreach(string item in words)
                    {
                        int n;
                        if( int.TryParse(item, out n) )
                        {
                            loger.write("IDEKS KRS fount at page:" + n);
                            return n;
                        }
                    }
                }
            }
            */
            return 0;
        }

        


    }
}