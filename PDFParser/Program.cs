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
using System.Linq;

namespace PDFParser
{
    class Program
    {
        static string pdfFilePath = @"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Monitory\monitor_2016_221.pdf";

        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            TimeSpan ts = new TimeSpan();
            string elapsedTime = string.Empty;

           
            Console.SetWindowSize(200, 60);
            Console.BufferHeight = 3000;
            Console.Out.WriteLine();
            stopWatch.Start();
            Console.Write("dupa");
            string[] pdfPages = readPFDPages(pdfFilePath);
            for (int i = 340; i < 340 + 21; i++)
                KRSIndexFixLines(PageToLines(pdfPages[i]));

            //DataTable dt = getKrsIndex(PageToLines(pdfPages[340]));
            /*
            
            Console.WriteLine("\n********************Page 1*********************");
            ParseTOCPage(pdfPages[0]);
            Console.WriteLine("\n********************Page 2*********************");
            ParseTOCPage(pdfPages[1]);
            Console.WriteLine("\n********************Page 3*********************");
            ParseTOCPage(pdfPages[2]);
            Console.WriteLine("\n********************Page 4*********************");
            ParseTOCPage(pdfPages[3]);
            Console.WriteLine("\n********************Page 5*********************");
            ParseTOCPage(pdfPages[4]);
            Console.WriteLine("\n********************Page 6*********************");
            ParseTOCPage(pdfPages[5]);
            Console.WriteLine("\n********************Page 7*********************");
            ParseTOCPage(pdfPages[6]);
            Console.WriteLine("\n********************Page 8*********************");
            ParseTOCPage(pdfPages[7]);
            */
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
                //Replace every occurence of 'MSIG x/x (x)
                currentText = Regex.Replace(currentText, @"MSIG\s\d+\/\d{4}\s\(\d+\)", Environment.NewLine);
                //Replacy any line with format of " - x - "
                currentText = Regex.Replace(currentText, @".+[-]\s\d+\s[-]", Environment.NewLine);
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
            { 
                if (Regex.Match(lines[i], @"^Poz\.\s\d+\.\s.+\s\d+$").Success)
                {
                    returnLines.Add(lines[i]);
                }
                //If line contains 'Poz. xxxx where x is string of digits at beggining, 
                //no string of digits at end, and next line contains string of digits at end, entry is complete
                else if (Regex.Match(lines[i], @"^Poz\.\s\d+\.\s.+$").Success && Regex.Match(lines[i+1], @"\s\d+$").Success)
                {
                    returnLines.Add(lines[i] + lines[i+1]);
                }
                else if(Regex.Match(lines[i], @"^Poz\.\s\d+\.\s.+$").Success && Regex.Match(lines[i + 2], @"\s\d+$").Success)
                {
                    returnLines.Add(lines[i] + lines[i+1] + lines[i+2]);
                }
            }


            return returnLines;
        }

        static DataTable TOC_PageToDataTable(List<string> entryList)
        {
            //DataTable for storing TOC entries
            DataTable TOC_Entries = new DataTable("TOC_EntriesCollection");
            TOC_Entries.Columns.Add("YearPosition", typeof(uint));
            TOC_Entries.Columns.Add("Header", typeof(string));
            TOC_Entries.Columns.Add("Page", typeof(uint));

            foreach(string item in entryList)
            {
                /*
                 *    To Do: Add Try Catch? 
                 *           After catching an exception log to file details about input string ang exception type;
                 */ 

                //Replace any occurence of "Poz. " or ". " in "Poz. x. " and converts it to integer;
                uint yearPosition = Convert.ToUInt32( Regex.Match( Regex.Match(item, @"Poz\.\s\d+\.\s").ToString() , @"\d+").ToString());
                //Removes any occurence of "Poz. x. " or "w <city-name> x"
                string header = Regex.Replace(item, @"(Poz\.\s\d+\.\s)|(((w)|(we))\s((\w+)[\s-])+)*\d+$", string.Empty);
                uint page = Convert.ToUInt32(Regex.Match(item, @"\d+$").ToString());
                if (yearPosition == 0 || header.Equals(string.Empty) || page == 0)
                {
                    Console.Out.WriteLine("Error parsing line: {0}", item);
                    return null;
                }
                TOC_Entries.Rows.Add(yearPosition, header, page);       
            }
            return TOC_Entries;
        }

        static DataTable ParseTOCPage(string page)
        {
            string out_1 = Regex.Replace(page, @"(\s\.){2,}", String.Empty);
            string out_2 = Regex.Replace(out_1, @"\s{2,}", " ");
            List<string> out_3 = TOC_Filter(PageToLines(out_2));
            DataTable out_4 = new DataTable();
            out_4 = TOC_PageToDataTable(out_3);
            string out_5 = LinesToPage(out_3);
            Console.Out.WriteLine("\n\nResult: \n\n");
            foreach (DataRow row in out_4.Rows)
                Console.Out.WriteLine("Year Position: {0}, Page: {1}, Header: {2}", Regex.Replace(row["YearPosition"].ToString(), @"\n|\r", string.Empty),
                                                                                    Regex.Replace(row["Page"].ToString(), @"\n|\r", string.Empty),
                                                                                    Regex.Replace(row["Header"].ToString(), @"\n|\r", string.Empty));
            return out_4;
        }

        static public List<string> KRSIndexFilter(List<string> pdfPage)
        {
            List<string> out_page = new List<string>();

            foreach (string line in pdfPage)
                if (!Regex.Match(line, @"MONITOR SĄDOWY I GOSPODARCZY|MSIG\s\d+\/\d+\s\(\d+\)|^INDEKS$|^INDEKS KRS$|\s?[-]\s?\w+\s?[-]\s?|\d{1,2}\s\w+\s\d{4}\sR\.", RegexOptions.IgnoreCase).Success)
                {
                    out_page.Add(line);
                    Console.WriteLine("Adding: {0}", line);
                }
            return out_page;
        }

        static public List<string> KRSIndexFixLines(List<string> pdfPage)
        {
            List<string> page = KRSIndexFilter(pdfPage);
            List<string> out_page = new List<string>();
            string pos = @"poz\.\s\d+";
            RegexOptions opt = RegexOptions.IgnoreCase;

            for (int i = 0; i < page.Count; i++)
            {
                if (!Regex.Match(page[i], pos, opt).Success && Regex.Match(page[i + 1], pos, opt).Success)
                {
                    out_page.Add(page[i] + page[i + 1]);
                    Console.WriteLine("Adding: {0}", page[i] + page[i + 1]);
                    i++;

                }
                else if (!Regex.Match(page[i], pos, opt).Success && Regex.Match(page[i + 2], pos, opt).Success)
                {
                    out_page.Add(page[i] + page[i + 1] + page[i + 2]);
                    Console.WriteLine("Adding: {0}", page[i] + page[i + 1] + page[i + 2]);
                    i++;
                    i++;
                }
                else if (!Regex.Match(page[i], pos, opt).Success && Regex.Match(page[i + 3], pos, opt).Success)
                {
                    out_page.Add(page[i] + page[i + 1] + page[i + 2] + page[i + 3]);
                    Console.WriteLine("Adding: {0}", page[i] + page[i + 1] + page[i + 2] + page[i + 3]);
                    i++;
                    i++;
                    i++;
                }
                else
                    Console.WriteLine("Omitting line: {0}", page[i]);
            }
            return out_page;
        }

        static public  DataTable getKrsIndex(List<string> pdfPage)
        {
            Console.WriteLine("\nKRS Indeks parser Start");




            return null;
        }
    }   
}



