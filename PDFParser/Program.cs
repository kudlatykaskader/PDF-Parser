using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.IO;
using System.Text;
using Microsoft.OneDrive.Sdk.Authentication;
using Microsoft.OneDrive.Sdk;
using Microsoft.Graph;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Text.RegularExpressions;

namespace PDFParser
{
    class Program
    {
        StringBuilder text = new StringBuilder();


        static string pdfFilePath = @"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Monitory\monitor_2016_237.pdf";


        static void Main(string[] args)
        {
            //Console.SetWindowSize(120, 40);
            //Console.BufferHeight = Int16.MaxValue-1;
            //Console.Out.WriteLine();

            //KRSIndex indexContent = new KRSIndex();
            //List<List<string>>
            //indexContent.ParseItem(indexContent.getKrsIndex(readPFDPages(pdfFilePath), (uint)KRSIndexPage)[9]);

            //if (indexContent.getKrsIndex(readPFDPages(pdfFilePath)c.Count == 0)
            //    Console.Out.WriteLine("KRS indexer returned empty list");
            string pattern = "(Mr\\.? |Mrs\\.? |Miss |Ms\\.? )";
            string[] names = { "Mr. Henry Hunt", "Ms. Sara Samuels",
                         "Abraham Adams", "Ms. Nicole Norris" };
            foreach (string name in names)
                Console.WriteLine(Regex.Replace(name, pattern, String.Empty));
            while (true) ;
            
        }

        static string readPFD(string filepath)
        {
            //LogManager loger = new LogManager();
            //loger.SetLogPath(@"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Logi\", "pdfRead");
            string pdf = string.Empty;
            PdfReader pdfReader = new PdfReader(filepath);

            for(int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, new SimpleTextExtractionStrategy());
                currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                currentText = currentText.Replace("MSIG 221/2016 (5106)", System.Environment.NewLine);
                pdf += currentText + System.Environment.NewLine;
                
            }
            return pdf;
        }

        static string[] readPFDPages(string filepath)
        {
            //LogManager loger = new LogManager();
            //loger.SetLogPath(@"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Logi\", "pdfRead");
            PdfReader pdfReader = new PdfReader(filepath);
            string[] pdfPages = new string[pdfReader.NumberOfPages]; 
            
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, new SimpleTextExtractionStrategy());
                //currentText = Encoding.Default.GetString(Encoding.Default.GetBytes(currentText));
                //currentText = currentText.Replace("MSIG 221/2016 (5106)", System.Environment.NewLine);
                pdfPages[page-1] = currentText;
            }
            return pdfPages;
        }

    }   
}



