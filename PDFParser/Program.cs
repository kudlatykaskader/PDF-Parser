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
using System.Diagnostics;

namespace PDFParser
{
    class Program
    {
        StringBuilder text = new StringBuilder();


        static string pdfFilePath = @"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Monitory\monitor_2016_221.pdf";


        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            TimeSpan ts = new TimeSpan();
            string elapsedTime = string.Empty;

           
            Console.SetWindowSize(200, 60);
            Console.BufferHeight = 3000;
            Console.Out.WriteLine();

            string[] pdfPages = readPFDPages(pdfFilePath);
            //ParseTOCPage(pdfPages[2]);
            //ParseTOCPage(pdfPages[3]);
            ParseTOCPage(pdfPages[4]);
            //ParseTOCPage(pdfPages[5]);

            stopWatch.Stop();

            ts = stopWatch.Elapsed;
            Console.WriteLine("RunTime " + String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
            
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
            char[] lineDelimeter = { '\n', '\r' };
            List<string> outPage = new List<string>(Page.Split(lineDelimeter));
            for (int i = 0; i < outPage.Count; i++)
                outPage[i] = Regex.Replace(outPage[i], @"\n|\r", String.Empty);
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
                else if (Regex.Match(lines[i], @"Poz.").Success && !(Regex.Match(lines[i + 1], @"Poz\.[ ]{1}").Success) && (Regex.Match(lines[i + 1], @"[ ]{1,}[0-9]{1,3}").Success) && !(Regex.Match(lines[i + 1], @"^[0-9]{1,}").Success))
                {
                    returnLines.Add(lines[i] + lines[i + 1] + System.Environment.NewLine);
                    i++;
                }
                else if (Regex.Match(lines[i], @"Poz\.[ ]{1}").Success)
                    returnLines.Add(lines[i] + System.Environment.NewLine);
            return returnLines;
        }

        static DataTable TOC_PageToDataTable(List<string> entryList)
        {
            DataTable TOC_Entries = new DataTable("TOC_EntriesCollection");
            TOC_Entries.Columns.Add("YearPosition", typeof(uint));
            TOC_Entries.Columns.Add("Header", typeof(string));
            TOC_Entries.Columns.Add("Page", typeof(uint));

            foreach(string item in entryList)
            {
                uint yearPosition = Convert.ToUInt32(Regex.Match(Regex.Match(item, @"Poz. [0-9]{1,6}").ToString(), @"[0-9]{1,6}").ToString());
                string header = Regex.Replace(item, @"Poz\.[ ]{1,}[0-9]{1,6}|[ ]{1,}[0-9]{1,4}[\n]{1,}", String.Empty);
                uint page = Convert.ToUInt32(Regex.Match(item, @"[ ]{1,}[0-9]{1,4}", RegexOptions.RightToLeft).ToString());
                if (yearPosition == 0 || header.Equals(String.Empty) || page == 0)
                {
                    Console.Out.WriteLine("Error parsing line: {0}", item);
                    return null;
                }
                TOC_Entries.Rows.Add(yearPosition, header, page);       
            }
            return TOC_Entries;
        }

        static void ParseTOCPage(string page)
        {
            string out_1 = Regex.Replace(page, @"\s\.", String.Empty);
            string out_2 = Regex.Replace(out_1, @"[ ]{2,}", " ");
            List<string> out_3 = TOC_Filter(PageToLines(out_2));
            DataTable out_4 = new DataTable();
            out_4 = TOC_PageToDataTable(out_3);
            string out_5 = LinesToPage(out_3);
            Console.Out.WriteLine("\n\nResult: \n\n");
            Console.Out.Write(out_3);
            foreach (DataRow row in out_4.Rows)
                Console.Out.WriteLine("Year Position: {0}, Page: {1}, Header: {2}", Regex.Replace(row["YearPosition"].ToString(), @"\n|\r", string.Empty),
                                                                                    Regex.Replace(row["Page"].ToString(), @"\n|\r", string.Empty),
                                                                                    Regex.Replace(row["Header"].ToString(), @"\n|\r", string.Empty));
        }
    }   
}



