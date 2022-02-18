using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeMadeSkyModel
{
    class AppConfig
    {
        public double userLatitude;
        public double userLongitude;
        public int numberOfPoints;
        public double minAltitude;
        public double exposure;
        public int binning;
        public double focal;
        public int? gain;
        public string pathToASTAP;
        public bool randomDistribution;
        public string telescope;
        public string camera;
    }
}
