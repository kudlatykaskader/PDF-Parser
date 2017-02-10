using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFParser
{
    class TableOfContent
    {
        private DataTable SpólkiKomandytowoAkcyjne;
        private DataTable SpólkiZoo;
        private DataTable SpólkiAkcyjne;

        private uint FirstLine { get; set; }


        public TableOfContent()
        {
            SpólkiKomandytowoAkcyjne = new DataTable();
            SpólkiKomandytowoAkcyjne.Columns.Add("Pos", typeof(int));
            SpólkiKomandytowoAkcyjne.Columns.Add("Content", typeof(string));
            SpólkiKomandytowoAkcyjne.Columns.Add("Page", typeof(int));
        }

        public uint addEntry(string entry)
        {
            string buffer = string.Empty;
            char[] delimiterChars = { ' ', '.'};
            string[] entryElements = entry.Split(delimiterChars);
            if(!entryElements.Contains("Poz"))
            {
                //Entry invalid, Option "Poz" not found
                return 1;
            }
            foreach (string element in entryElements)
            {
                //entryElements.Contains
            }
            return 0;
        }

        public void TOC_test(string dirName)
        {
            LogManager loger = new LogManager();
            loger.SetLogPath(dirName, "minor_major_TOC_Delimeters_search");

            string[] TOC_minorCategoryDelimeters = {
                                                    "1. Spółki komandytowo-akcyjne",
                                                    "2. Spółki z ograniczoną odpowiedzialnością",
                                                    "3. Spółki akcyjne",
                                                    "1. Postanowienie o ogłoszeniu upadłości",
                                                    "3. Ogłoszenie o sporządzeniu i przekazaniu sędziemu komisarzowi listy wierzytelności",
                                                    "4. Ogłoszenie o możliwości przeglądania planu podziału",
                                                    "6. Postanowienie o zakończeniu postępowania upadłościowego",
                                                    "9. Inne"
                                                    };
            string[] TOC_majorCategorydelimeters = {
                                                    "I. OGŁOSZENIA WYMAGANE PRZEZ KODEKS SPÓŁEK HANDLOWYCH",
                                                    "II. WPISY DO REJESTRU HANDLOWEGO",
                                                    "III. OGŁOSZENIA WYMAGANE PRZEZ PRAWO UPADŁOŚCIOWE",
                                                    "IV. OGŁOSZENIA WYMAGANE PRZEZ USTAWĘ O KRAJOWYM REJESTRZE SĄDOWYM",
                                                    "V. OGŁOSZENIA WYMAGANE PRZEZ KODEKS POSTĘPOWANIA CYWILNEGO",
                                                    "VI. OGŁOSZENIA O ZAREJESTROWANIU I WYKREŚLENIU PODATKOWYCH GRUP KAPITAŁOWYCH",
                                                    "IX. OGŁOSZENIA WYMAGANE PRZEZ PRAWO RESTRUKTURYZACYJNE",
                                                    "X. OGŁOSZENIA WYMAGANE PRZEZ USTAWĘ O RACHUNKOWOŚCI",
                                                    "XV. WPISY DO KRAJOWEGO REJESTRU SĄDOWEGO",
                                                    };


            DataTable positions = new DataTable();
            positions.Columns.Add("Pos", typeof(string));
            char[] lineDelimeter = { '\n' };
            DirectoryInfo d = new DirectoryInfo(dirName);
            foreach (var file in d.GetFiles("monitor*.pdf"))
            {
                loger.write("#######################################");
                PdfReader pdfReader = new PdfReader(file.FullName);
                loger.write("File: " + file.Name);
                foreach (string tocDelimeter in TOC_majorCategorydelimeters)
                {
                    bool isFound = false;
                    loger.write("Looking for: \'" + tocDelimeter+ "\'" );
                    for (int page = 1; page <= 15; page++)
                    {
                        string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, new SimpleTextExtractionStrategy());
                        currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                        string[] lines = currentText.Split(lineDelimeter);
                        foreach (string s in lines)
                            if (s.Equals(tocDelimeter))
                                isFound = true;
                    }
                    if (isFound)
                        loger.write("Success");
                    else
                        loger.write("Failure");
                    loger.write("-------");
                    isFound = false;
                }
                pdfReader.Close();
            }
        }

        public int getWDRSIndexPage(string filePath, string filename)
        {
            LogManager loger = new LogManager();
            char[] lineDelimeter = { '\n' };
            char[] wordDelimeter = { ' ', '.' };
            if (!loger.SetLogPath(filePath, "Index"))
                Console.Out.Write("Failed to start logging");
            PdfReader pdfReader = new PdfReader(filePath+"\\"+filename);
           
            loger.write("File: " + filePath + filename);
            string pdfFile = string.Empty;
            string[] pdfFileLines;
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                string tempraryContainer = PdfTextExtractor.GetTextFromPage(pdfReader, page, new SimpleTextExtractionStrategy());
                tempraryContainer = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(tempraryContainer)));
                pdfFile += tempraryContainer;
            }
            pdfFile = pdfFile.Replace("MSIG 237/2016 (5122)", System.Environment.NewLine);
            pdfFileLines = pdfFile.Split(lineDelimeter);
            Console.Out.WriteLine("Lines id PDF: {0}", pdfFileLines.Length);
            foreach (string s in pdfFileLines)
            {
                //Console.Out.WriteLine(s);
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
            return 0;
        }

        


    }
}