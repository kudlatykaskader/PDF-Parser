using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDFParser
{
    class LogManager
    {
        System.IO.StreamWriter file;

        public bool SetLogPath(string path, string filename)
        {
            file = new System.IO.StreamWriter(path +"\\" + 
                                              filename + 
                                              "_log_" + 
                                              DateTime.Now.ToString().Replace('/', '_').Replace(':', '_') + ".txt"
                                              );
            if (file != null)
                return true;
            else
                return false;
        }

        public void write(string logMessage)
        {
            this.file.WriteLine(logMessage);
        }
    }
}
