using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Logger
    {
        private string _File = "";
        public bool Output = true;

        public Logger(string logFile)
        {
            _File = logFile;
        }

        public void Log(string message)
        {
            Log(_File, message);
        }

        public void Log(string file, string message)
        {
            if (Output)
            {
                try
                {
                    File.AppendAllText(file, message);
                }
                catch (Exception e) { }
            }
        }
    }
}
