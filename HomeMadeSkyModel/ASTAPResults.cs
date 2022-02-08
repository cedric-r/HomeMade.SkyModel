using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeMadeSkyModel
{
    public class ASTAPResults
    {
        private string FileName = "";
        string[] contents = null;

        public ASTAPResults(string fileName)
        {
            FileName = fileName;
        }

        private void Load()
        {
            if (String.IsNullOrEmpty(FileName)) throw new IOException("Invalid file name");
            if (!File.Exists(FileName)) throw new IOException("Invalid/missing file");
            if (contents == null) contents = File.ReadAllLines(FileName);
        }

        private string SearchKey(string key)
        {
            if (contents == null) Load();
            string temp = null;
            foreach(string line in contents)
            {
                if (!String.IsNullOrEmpty(line))
                {
                    string[] parts = line.Split('=');
                    if (parts[0] == key) temp = parts[1];
                }
            }

            return temp;
        }

        public double Ra()
        {
            double temp = 0;
            string val = SearchKey("CRVAL1");
            try
            {
                temp = Double.Parse(val);
            }
            catch(Exception e)
            {
                throw new InvalidOperationException("Error converting RA: " + e.Message);
            }
            return temp;
        }

        public double Dec()
        {
            double temp = 0;
            string val = SearchKey("CRVAL2");
            try
            {
                temp = Double.Parse(val);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error converting DEC: " + e.Message);
            }
            return temp;
        }

        public bool IsSuccess()
        {
            string temp = SearchKey("PLTSOLVD");
            if (temp == null | temp == "F")
                return false;
            return true;
        }
    }
}
