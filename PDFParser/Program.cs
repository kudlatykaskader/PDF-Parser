using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace PDFParser
{
    class Program
    {
        static string pdfFilePath = @"C:\Users\EMAZWOK\OneDrive\root\Projekty\PostVertising\Monitory\monitor_2002_01.pdf";
        static string txtFilePath = @"C:\Users\EMAZWOK\Desktop\monitor_2016_221.txt";

        static void Main(string[] args)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            TimeSpan ts = new TimeSpan();

            Console.SetWindowSize(200, 50);
            Console.BufferHeight = 20000;
            Console.Out.WriteLine();

           
            //string pattern1 = @"(?<=-\ss\.\s)\d+";
            //string inputp = "  spsps - s. 123";
            //Console.WriteLine("Match {0}", Regex.Match(inputp, pattern1));
            //Console.In.Read();
            //Reading pdf file -> replace with reading txt file
            //string[] pdfPages = readPFDPages(pdfFilePath);
            string[] pdfPages = txtReader(txtFilePath);
            //Reading KRS Indeks start page number
            int krsIndeksPage = getKrsIndexPage(pdfPages);

            //Parsing KRS Index
            for (int i = krsIndeksPage - 1; i < pdfPages.Length; i++)
                getKrsIndex(KRSIndexFixLines(PageToLines(pdfPages[i])));


            int k = 0;
            foreach (string page in pdfPages)
            {
                TOC_ParsePage(pdfPages[k]);
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
                //Remove every occurence of 'MSIG x/x (x) 
                currentText = Regex.Replace(currentText, @"MSIG\s\d+\/\d{4}\s\(\d+\)", Environment.NewLine);
                currentText = Regex.Replace(currentText, @"^Odbiorca\:(\s+)?\*(\s+)?ID\:(\s+)?\w+", string.Empty);
                //Remove every occurence of ' - <char> - '
                currentText = Regex.Replace(currentText, @"[-]\s?\d+\s?[-]", string.Empty);
                //Remove overy occurence of ' - <digit> - '
                currentText = Regex.Replace(currentText, @"[-]\s?\w\s?[-]", string.Empty);
                pdfPages[page - 1] = currentText;
            }
            return pdfPages;
        }

        static string[] txtReader(string filepath)
        {
            char[] lineDelimeter = { '\n' };
            string text = System.IO.File.ReadAllText(filepath, Encoding.UTF8);
            List<string> lines = new List<string>(text.Split(lineDelimeter));
            Console.WriteLine("{0} lines in file", lines.Count);
            string[] pages = new string[Regex.Matches(text, @"\f").Count];
            Console.WriteLine("{0} pages in file", pages.Length);
            string tmp_page = string.Empty;
            uint tmp_page_iterator = 0;
            foreach (string line in lines)
            {
                string tmp_line = line;
                if (Regex.Match(line, @"\f").Success)
                {
                    pages[tmp_page_iterator] = tmp_page;
                    tmp_page_iterator++;
                    tmp_page = string.Empty;
                }
                else
                    tmp_page += tmp_line;
            }
            return pages;
        }

        static string lineFilter(string i_line)
        {
            string o_line = i_line;
            o_line = Regex.Replace(o_line, @"MSIG\s\d+\/\d{4}\s\(\d+\)", string.Empty);
            o_line = Regex.Replace(o_line, @"^Odbiorca\:(\s+)?\*(\s+)?ID\:(\s+)?\w+", string.Empty);
            o_line = Regex.Replace(o_line, @"^[-]\s?\w\s?[-]", string.Empty);
            return o_line;
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

        static List<string> TOC_FixLineBreaks(List<string> lines)
        {
            List<string> returnLines = new List<string>();
            for (int i = 0; i < lines.Count; i++)
                if (Regex.Match(lines[i], @"^Poz\.\s\d+\.\s.+\s\d+$").Success)
                    returnLines.Add(lines[i]);
                else if (Regex.Match(lines[i], @"^Poz\.\s\d+\.\s.+$").Success && Regex.Match(lines[i + 1], @"\s\d+$").Success)
                    returnLines.Add(lines[i] + lines[i + 1]);
                else if (Regex.Match(lines[i], @"^Poz\.\s\d+\.\s.+$").Success && Regex.Match(lines[i + 2], @"\s\d+$").Success)
                    returnLines.Add(lines[i] + lines[i + 1] + lines[i + 2]);
                //else if (Regex.Match(lines[i], @"^Poz\.\s\d+\.\s.+$").Success && Regex.Match(lines[i + 3], @"\s\d+$").Success)
                //    returnLines.Add(lines[i] + lines[i + 1] + lines[i + 2] + lines[i + 3]);
            return returnLines;
        }

        static DataTable TOC_PageToDataTable(List<string> entryList)
        {
            DataTable TOC_Entries = new DataTable("TOC_EntriesCollection");
            TOC_Entries.Columns.Add("YearPosition", typeof(uint));
            TOC_Entries.Columns.Add("Header", typeof(string));
            TOC_Entries.Columns.Add("Page", typeof(uint));

            foreach (string item in entryList)
            {
                //Replace any occurence of "Poz. " or ". " in "Poz. x. " and converts it to integer;
                uint yearPosition = Convert.ToUInt32(Regex.Match(Regex.Match(item, @"Poz\.\s\d+\.\s").ToString(), @"\d+").ToString());
                //Removes any occurence of "Poz. x. " or "w <city-name> x"
                string header = Regex.Replace(item, @"(Poz\.\s\d+\.\s)|(((w)|(we))\s((\w+)[\s-])+)*\d+$", string.Empty);
                uint page = Convert.ToUInt32(Regex.Match(item, @"\d+$").ToString());
                if (yearPosition == 0 || header.Equals(string.Empty) || page == 0)
                {

                }
                TOC_Entries.Rows.Add(yearPosition, header, page);
            }
            return TOC_Entries;
        }

        static DataTable TOC_ParsePage(string page)
        {
            DataTable out_4 = new DataTable();

            string out_1 = Regex.Replace(page, @"(\s\.){2,}", String.Empty);
            out_1 = Regex.Replace(out_1, @"\s{2,}", " ");
            List<string> out_3 = TOC_FixLineBreaks(PageToLines(out_1));
            return TOC_PageToDataTable(out_3);
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
            //For checking if current page is valid KRS Indeks page;
            foreach (string pline in pdfPage)
                if (Regex.Match(pline, @"INDEKS KRS").Success)
                    pageVerified = true;
            if (!pageVerified)
                return null;
            List<string> page = KRSIndexFilter(pdfPage);
            List<string> out_page = new List<string>();
            for (int i = 0; i < page.Count; i++)
            {

                string line = string.Empty;
                while (true)
                {
                    line += page[i];
                    if (((i + 1) == page.Count || (Regex.Match(rms(line), @"poz\.\d+\.").Success && !Regex.Match(rms(page[i]) + rms(page[i + 1]), @"poz\.\d+\.\[\w{2}\.").Success)))
                    {
                        out_page.Add(line);
                        break;
                    }
                    i++;
                }
            }

            return out_page;
        }

        static public DataTable getKrsIndex(List<string> indexItems)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("city", typeof(string));
            dt.Columns.Add("krs_id", typeof(string));
            dt.Columns.Add("page", typeof(uint));
            dt.Columns.Add("position", typeof(uint));

            if (indexItems == null)
                return null;
            foreach (string item in indexItems)
            {
                if (item.Length == 0)
                    continue;
                try
                {
                    string companyName = Regex.Replace(item, @"\[.+", string.Empty);
                    string city = Regex.Match(item, @"\[.+\]").ToString();
                    city = city[1].ToString() + city[2].ToString();
                    string KRSId = Regex.Match(item, @"(\/\d+){3}").ToString();
                    uint page = Convert.ToUInt32(Regex.Match(item, @"(?<=\ss\.\s)\d+").ToString());
                    uint position = Convert.ToUInt32(Regex.Match(item, @"(?<=poz\.\s)\d+").ToString());

                    dt.Rows.Add(companyName, city, KRSId, page, position);
                }
                catch (Exception e)
                {
                    //Replace this with proper logging function, or call GUI window to require user intevention
                    Console.WriteLine("Unable to parse KRS Item due to: {0}\n ->{1}", e.Message.ToString(), item);
                }
            }
            return dt;
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



