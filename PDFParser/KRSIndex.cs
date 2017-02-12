using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PDFParser
{
    
    class KRSIndex
    {
        
        public KRSIndex()
        {
            indexDataTable.Columns.Add("Poz", typeof(string));
            indexDataTable.Columns.Add("Name", typeof(int));
            indexDataTable.Columns.Add("Page", typeof(int));
            indexDataTable.Columns.Add("NRKRS", typeof(int));
        }

        DataTable indexDataTable = new DataTable();
        


       


        public DataTable ParseItem(List<string> IndexItem)
        {
            List<string> currentItemWords = new List<string> { };
            char[] wordDelimeter = { ' ' };
            Console.Out.WriteLine("\n\nStarting parsing item");

            string name = string.Empty;
            string krs = string.Empty;
            string page = string.Empty;
            string poz = string.Empty;

            foreach (string item in IndexItem)
            {
                string[] words = item.Split(wordDelimeter);
                foreach (string word in words)
                {
                    currentItemWords.Add(word);
                }      
            }
            foreach (string item in IndexItem)
            {
                Console.Out.WriteLine(item);
            }
            int spolkaIndex = 0, krsindex = 0, pageindex = 0, pozindex = 0;
            foreach (string item in currentItemWords)
            {
                if(item.Contains("SPÓŁKA"))
                    spolkaIndex = currentItemWords.IndexOf(item);
                if (item.Contains("KRS"))
                    krsindex = currentItemWords.IndexOf(item);
                if (item.Contains("s."))
                    pageindex = currentItemWords.IndexOf(item);
                if (item.Contains("poz."))
                    pozindex = currentItemWords.IndexOf(item);
            }
            if (spolkaIndex != 0)
                for (int i = 0; i < spolkaIndex; i++)
                    name += currentItemWords[i] + " "; 
            if ( krsindex != 0)
            {
                krs = currentItemWords[krsindex];
            }
            if (pageindex != 0)
            {
                page = currentItemWords[pageindex + 1].Remove(currentItemWords[pageindex + 1].IndexOf(","), 1);
            }
            if (pozindex != 0)
            {
                poz = currentItemWords[pozindex + 1].Remove(currentItemWords[pozindex + 1].IndexOf("."), 1);
            }


            Console.Out.WriteLine("**********\nResults\n\n\nNazwa spółki: {0}", name);
            Console.Out.WriteLine("\nNumer KRS: {0}", krs);
            Console.Out.WriteLine("\nWystępuje na stronie: {0}", page);
            Console.Out.WriteLine("\nPozycja roczna: {0}\n\n", poz);
                






            return indexDataTable = new DataTable();
        }
        private string[] removePageElements(string[] pdfPage, string[] Elements)
        {
            var cleanPdfPage = new List<string>(pdfPage);
            foreach(string toRemoveElement in pdfPage)
                cleanPdfPage.RemoveAt(Array.IndexOf(pdfPage, toRemoveElement));
            return cleanPdfPage.ToArray();
        }
    }
}
