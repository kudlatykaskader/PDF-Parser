using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
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
        

        public List<List<string>> getKrsIndex(string[] pdfPages, uint indexPage)
        {
//Line, word delimeters and other helpfull data
            char[] lineDelimeter = { '\n' };
            char[] wordDelimeter = { ' ', '.' };
            char[] letters = { 'A', 'Ą', 'B', 'C', 'Ć', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'R', 'S', 'Ś', 'T', 'U', 'W', 'X', 'Y', 'Z', 'Ź', 'Ż', };
            uint currentLine = 0;
//Containers
            List<List<string>> IndexItems = new List<List<string>>();
            string[] pdfPageLines = new string[1000];
//Logger
            LogManager loger = new LogManager();
            loger.SetLogPath(@"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Logi\", "minor_major_TOC_Delimeters_search");
            loger.write("KRS Index starts at: " + indexPage);
            //Parse begin           
            for (uint page = indexPage + 1; page < pdfPages.GetLength(0); page++)
            {
                pdfPageLines = pdfPages[page].Split(lineDelimeter);
                //Looking for page header
                for (uint i = 0; i > 10; i++)
                    if (pdfPageLines[i].Equals("INDEKS KRS"))
                        for (uint j = i; j > 10; j++)
                            if (pdfPageLines[j].Equals("INDEKS"))
                                currentLine = j;
                //If KRS INDEKS page header was not found on first page, return empty list;
                if (currentLine == 0 && page == indexPage)
                    return new List<List<string>> { };
                List<string> krsIndeksEntry = new List<string>{ };
                foreach (string line in pdfPageLines)
                {
                    string newline = line;
                    if (((newline.IndexOfAny(letters) - 2) == newline.IndexOf('-')) && ((newline.IndexOfAny(letters) + 2) == newline.LastIndexOf('-')))
                        continue;
                    if (newline.StartsWith(" "))
                        newline = newline.Remove(0, 1);
                    krsIndeksEntry.Add(newline);
                    for (int i = 0; i < krsIndeksEntry.Count; i++)
                        if(krsIndeksEntry[i].EndsWith("-"))
                            krsIndeksEntry[i] = krsIndeksEntry[i].Remove(krsIndeksEntry.LastIndexOf("-"), 1);
                    if (newline.Contains("poz."))
                    { 
                        IndexItems.Add(new List<string>(krsIndeksEntry));
                        krsIndeksEntry.Clear();                    
                    }     
                }
            }
            Console.Out.WriteLine("Got KRS Indeks Content!");
            return IndexItems;
        }


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
