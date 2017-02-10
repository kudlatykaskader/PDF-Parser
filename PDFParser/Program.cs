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
using System.Threading;

namespace PDFParser
{
    class Program
    {
        StringBuilder text = new StringBuilder();


        static string pdfFilePath = @"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Monitory\monitor_2016_221.pdf";


        static void Main(string[] args)
        {
            Console.SetWindowSize(200, 60);
            Console.BufferHeight = 3000;
            Console.Out.WriteLine();

            string[] pdfPages = readPFDPages(pdfFilePath);
            PrintPage(pdfPages, 3);

            string out_1 = Regex.Replace(pdfPages[2], @"\s\.", String.Empty);
            string out_2 = Regex.Replace(out_1, @"[ ]{2,}", " ");
            string out_3 = LinesToPage(TOC_Filter(PageToLines(out_2)));
            Console.Out.WriteLine("\n\nResult: \n\n");
            Console.Out.Write(out_3);
            while (true) ;
            
        }

        static string[] readPFDPages(string filepath)
        {
            PdfReader pdfReader = new PdfReader(filepath);
            string[] pdfPages = new string[pdfReader.NumberOfPages]; 
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, new SimpleTextExtractionStrategy());
                currentText = Encoding.Default.GetString(Encoding.Default.GetBytes(currentText));
                currentText = currentText.Replace("MSIG 221/2016 (5106)", System.Environment.NewLine);
                pdfPages[page-1] = currentText;
            }
            return pdfPages;
        }

        static void PrintPage(string[] pdfPages, uint page)
        {
            Console.Out.Write(pdfPages[page - 1]);
        }

        static List<string> PageToLines(string Page)
        {
            char[] lineDelimeter = { '\n' };
            List<string> outPage = new List<string>(Page.Split(lineDelimeter));
            for (int i = 0; i < outPage.Count; i++)
                outPage[i] += System.Environment.NewLine;
            return outPage;
        }

        static string LinesToPage(List<string> lines)
        {
            string page = string.Empty;
            foreach (string line in lines)
                page += line;
            return page;
        }

        static List<string> TOC_Filter(List<string> lines)
        {
            List<string> returnLines = new List<string>();
            for (int i = 0; i < lines.Count; i++)
                if((i == lines.Count - 1) && Regex.Match(lines[i], @"Poz.").Success)
                    returnLines.Add(lines[i]);
                else if (Regex.Match(lines[i], @"Poz.").Success && !(Regex.Match(lines[i + 1], @"Poz.").Success && Regex.Match(lines[i + 1], @"[0-9]{1,3}").Success))
                {
                    returnLines.Add(Regex.Replace(lines[i], @"\n|\r", "") + Regex.Replace(lines[i + 1], @"\r", " "));
                    i++;
                }
                else if (Regex.Match(lines[i], @"Poz.").Success)
                    returnLines.Add(lines[i]);
            return returnLines;
        }

    }   
}



