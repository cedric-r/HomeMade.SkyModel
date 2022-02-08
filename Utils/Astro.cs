using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class Astro
    {
        public Dictionary<string, double> RADEC2AZALT(double LMST, double RA, double DEC, double LAT)
        {
            Dictionary<string, double> temp = new Dictionary<string, double>();

            double HA;
            double AZtemp;
            double ALT;
            double AZ;

            // Hour Angle (degrees)
            HA = LMST - RA;

            //Altitude (degrees)
            ALT = Math.Asin(Math.Sin(DEC / 180 * Math.PI) * Math.Sin(LAT / 180 * Math.PI) + Math.Cos(DEC / 180 * Math.PI) * Math.Cos(LAT / 180 * Math.PI) * Math.Cos(HA / 180 * Math.PI)) / Math.PI * 180;
            //Azimuth (degrees)
            AZtemp = Math.Acos((Math.Sin(DEC / 180 * Math.PI) - Math.Sin(ALT / 180 * Math.PI) * Math.Sin(LAT / 180 * Math.PI)) / (Math.Cos(ALT / 180 * Math.PI) * Math.Cos(LAT / 180 * Math.PI))) / Math.PI * 180;

            if (Math.Sin(HA / 180 * Math.PI) > 0)
            {
                AZ = 360 - AZtemp;
            }
            else {
                AZ = AZtemp;
            }

            temp.Add("ALT", ALT);
            temp.Add("AZ", AZ);

            return temp;
        }

        public Dictionary<string, double> AZALT2RADEC(double LMST, double AZ, double ALT, double LAT)
        {
            Dictionary<string, double> temp = new Dictionary<string, double>();

            double RA;
            double DEC;
            double HA;

            //DEC (degrees)
            DEC = Math.Asin(Math.Sin(ALT / 180 * Math.PI) * Math.Sin(LAT / 180 * Math.PI) + Math.Cos(ALT / 180 * Math.PI) * Math.Cos(LAT / 180 * Math.PI) * Math.Cos(AZ / 180 * Math.PI)) / Math.PI * 180;

            //Hour angle (degrees)
            HA = Math.Acos((Math.Sin(ALT / 180 * Math.PI) - Math.Sin(DEC / 180 * Math.PI) * Math.Sin(LAT / 180 * Math.PI)) / (Math.Cos(DEC / 180 * Math.PI) * Math.Cos(LAT / 180 * Math.PI))) / Math.PI * 180;

            //RA (degrees)
            if (AZ > 180)             //if approaching meridian
            {
                RA = 360 + LMST - HA;
                if (RA > 360)
                    RA = RA - 360;
            }
            else                        //if moving away from meridian
                RA = LMST + HA;

            temp.Add("RA", RA);
            temp.Add("DEC", DEC);

            return temp;
        }

        public double ToHours(double RaDeg)
        {
            return RaDeg * 24 / 360;
        }

        public double GMST(DateTime DT)
        {
            int HRS, MINS, SECS;
            double JD, TU, FunReturn;

            HRS = DT.Hour;
            MINS = DT.Minute;
            SECS = DT.Second;

            //julian date 
            JD = JulianDate(DT);

            //universal time
            TU = (JD - 2451545.0) / 36525;

            //Greenwich Mean Sidereal Time
            FunReturn = 24110.54841 + 8640184.812866 * TU + 0.093104 * Math.Pow(TU, 2) - 0.0000062 * Math.Pow(TU, 3);
            FunReturn = FunReturn + HRS * 3600 + MINS * 60 + SECS;

            //convert to degrees
            FunReturn = (FunReturn % 86400) / 86400 * 360;

            return FunReturn;
        }

        public double JulianDate(DateTime DT)
        {
            int A, B, C, E, F;
            int Y, M, D;
            int HRS, MINS, SECS;
            double JD;

            Y = DT.Year;
            M = DT.Month;
            D = DT.Day;

            HRS = DT.Hour;
            MINS = DT.Minute;
            SECS = DT.Second;

            A = (int)Math.Floor((double)Y / 100);
            B = (int)Math.Floor((double)A / 4);
            C = 2 - A + B;
            E = (int)Math.Floor(365.25 * (Y + 4716));
            F = (int)Math.Floor(30.6001 * (M + 1));

            JD = C + D + E + F - 1524;   //integer julian day

            JD = JD + (HRS - 12) / 24 + MINS / 1440 + SECS / 86400;  //add fraction of a day

            return JD;
        }
    }
}
