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
            Console.BufferHeight = 20000;
            Console.Out.WriteLine();
            stopWatch.Start();
            Console.Write("dupa");
            
            string[] pdfPages = readPFDPages(pdfFilePath);

            int krsIndeksPage = getKrsIndexPage(pdfPages);

            for (int i = krsIndeksPage -1; i < pdfPages.Length; i++)
                getKrsIndex(KRSIndexFixLines(PageToLines(pdfPages[i])));

            //DataTable dt = getKrsIndex(PageToLines(pdfPages[340]));
            int k = 0; 
            foreach (string page in pdfPages)
            {
                Console.WriteLine("\n********************Page {0}*********************", k);
                ParseTOCPage(pdfPages[k]);
                if (getKrsIndexPage(page) != 0)
                    break;
                k++;
            }
            
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
                currentText = Regex.Replace(currentText, @"^Odbiorca\:(\s+)?\*(\s+)?ID\:(\s+)?\w+", string.Empty);
                currentText = Regex.Replace(currentText, @"[-]\s?\d+\s?[-]", string.Empty);
                currentText = Regex.Replace(currentText, @"[-]\s?\w\s?[-]", string.Empty);
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
            {
                Console.WriteLine("**************************************");
                Console.Out.WriteLine("-->Year Position: {0}\n-->Page: {1}\n-->Header: {2}", Regex.Replace(row["YearPosition"].ToString(), @"\n|\r", string.Empty),
                                                                                        Regex.Replace(row["Page"].ToString(), @"\n|\r", string.Empty),
                                                                                         Regex.Replace(row["Header"].ToString(), @"\n|\r", string.Empty));
            }
            return out_4;
        }

        static public List<string> KRSIndexFilter(List<string> pdfPage)
        {
            List<string> out_page = new List<string>();

            foreach (string line in pdfPage)
                if (!Regex.Match(line, @"MONITOR SĄDOWY I GOSPODARCZY|MSIG\s\d+\/\d+\s\(\d+\)|^INDEKS$|^INDEKS KRS$|\s?[–]\s?\w+\s?[–]\s?|\s?[–]\s?\d+\s?[–]\s?|\d{1,2}\s\w+\s\d{4}\sR\.", RegexOptions.IgnoreCase).Success)
                {
                    out_page.Add(line);
                }
            return out_page;
        }

        static public List<string> KRSIndexFixLines(List<string> pdfPage)
        {
            bool pageVerified = false;
            foreach (string pline in pdfPage)
                if (Regex.Match(pline, @"INDEKS KRS").Success)
                    pageVerified = true;
            if (!pageVerified)
                return null;
            List<string> page = KRSIndexFilter(pdfPage);
            List<string> out_page = new List<string>();
            for(int i = 0; i < page.Count; i++)
            {
                
                string line = string.Empty;
                while(true)
                {
                    line += page[i];
                    i++;
                    if ((i == page.Count || (Regex.Match(rms(line), @"poz\.\d+\.").Success && !Regex.Match(rms(page[i-1]) + rms(page[i]), @"poz\.\d+\.\[\w{2}\.").Success) ))
                    {
                        //Console.WriteLine("Output: {0}", line);
                        out_page.Add(line);
                        i--;
                        break;
                    }
                }
            }

            return out_page;
        }

        static public  DataTable getKrsIndex(List<string> indexItems)
        {
            Dictionary<string, string> cities = new Dictionary<string, string>();

            if (indexItems == null)
                return null;
            Console.WriteLine("\nKRS Indeks parser Start");
            foreach (string item in indexItems)
            {
                string companyName = Regex.Replace(item, @"\[.+", string.Empty);
                string city = Regex.Match(item, @"\[.+\]").ToString();
                city = city[1].ToString() + city[1].ToString();
                string KRSId = Regex.Match(item, @"(\/\d+){3}").ToString();
                uint page = Convert.ToUInt32(Regex.Replace(Regex.Match(rms(item), @"[-]s\.\d+").ToString(), @"[-]s\.", string.Empty).ToString());
                uint position = Convert.ToUInt32(Regex.Replace(Regex.Match(rms(item), @"poz.\d+").ToString(), @"poz\.", string.Empty).ToString());
                Console.WriteLine("-->Company Name: {0}, \n-->Registration City: {1}, \n-->KRS ID: {2}, \n-->Page: {3}, \n-->Position: {4}", companyName, city, KRSId, page, position);
                Console.WriteLine("**************************************");
            }



            return null;
        }

        static string rms(string input)
        {
            return Regex.Replace(input, @"\s+", string.Empty);
        }

        static int getKrsIndexPage(string[] pdfPages)
        {
            int result = 0;
            foreach (string page in pdfPages)
                if ((result = getKrsIndexPage(page)) != 0)
                    return result;
            return 0;
        }
        static int getKrsIndexPage(string page)
        {
            foreach (string line in PageToLines(page))
                if (Regex.Match(line, @"INDEKS KRS(\s+)?(\.\s)+(\s+)?\d+").Success)
                    return Convert.ToInt32(Regex.Replace(line, @"INDEKS KRS\s+(\.\s)+", string.Empty).ToString());
            return 0;
        }

        
    }   
}



