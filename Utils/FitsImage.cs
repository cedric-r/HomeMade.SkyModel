using nom.tam.fits;
using nom.tam.util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class FitsImage
    {
        private static int[][] SaveImageData(int width, int height, int[] data)
        {
            int[][] bimg = new int[height][];

            for (int y = 0; y < height; y++)
            {
                bimg[y] = new int[width];

                for (int x = 0; x < width; x++)
                {
                    bimg[y][x] = (int)Math.Max((int)0, data[x + (height - y - 1) * width]);
                }
            }

            return bimg;
        }

        public static void SaveFitsFrame(int frameNo, string fileName, int width, int height, double pixsizex, double pixsizey, int[] framePixels, DateTime timeStamp, float exposureSeconds)
        {
            SaveFitsFrame(frameNo, fileName, null, null, null, null, width, height, pixsizex, pixsizey, null, framePixels, null, null, null, null, timeStamp, exposureSeconds, new Dictionary<string, Tuple<string, string>>());
        }

        public static void SaveFitsFrame(
            int frameNo,
            string fileName,
            double? lat, 
            double? lon, 
            double? elev, 
            double? focal,
            int width,
            int height,
            double pixsizex,
            double pixsizey,
            int? binning,
            int[] framePixels,
            double? alt,
            double? az,
            double? ra,
            double? dec,
            DateTime timeStamp,
            float exposureSeconds,
            Dictionary<string, Tuple<string, string>> additionalFileHeaders)
        {
            Fits f = new Fits();

            object data = (object)SaveImageData(width, height, framePixels);

            BasicHDU imageHDU = Fits.MakeHDU(data);

            nom.tam.fits.Header hdr = imageHDU.Header;
            hdr.AddValue("SIMPLE", "T", null);

            hdr.AddValue("BITPIX", 32, null);

            hdr.AddValue("BZERO", 32768, null);
            hdr.AddValue("BSCALE", 1, null);

            if (lat != null) hdr.AddValue("SITELAT", lat.Value, "[deg] Observation site latitude");
            if (lon != null) hdr.AddValue("SITELONG", lon.Value, "[deg] Observation site longitude");
            if (elev != null) hdr.AddValue("SITEELEV", lon.Value, "[m] Observation site elevation");
            if (focal != null) hdr.AddValue("FOCALLEN", focal.Value, "[mm] Focal length");

            if (binning != null)
            {
                hdr.AddValue("XBINNING", binning.Value, "X axis binning factor");
                hdr.AddValue("YBINNING", binning.Value, "Y axis binning factor");
            }

            if (ra != null) hdr.AddValue("RA", ra.Value, "[deg] RA of telescope");
            if (dec != null) hdr.AddValue("DEC", dec.Value, "[deg] Declination of telescope");

            if (alt != null) hdr.AddValue("CENTALT", alt.Value, "[deg] Altitude of telescope");
            if (az != null) hdr.AddValue("CENTAZ", az.Value, "[deg] Azimuth of telescope");

            hdr.AddValue("XPIXSZ", pixsizex, "[um] Pixel X axis size");
            hdr.AddValue("YPIXSZ", pixsizey, "[um] Pixel Y axis size");

            hdr.AddValue("NAXIS", 2, null);
            hdr.AddValue("NAXIS1", width, null);
            hdr.AddValue("NAXIS2", height, null);

            hdr.AddValue("EXPOSURE", exposureSeconds.ToString("0.000", CultureInfo.InvariantCulture), "Exposure, seconds");

            hdr.AddValue("DATE-LOC", timeStamp.ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture), "Time of observation (local)");
            hdr.AddValue("DATE-OBS", timeStamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture), "Time of observation (UTC)");

            hdr.AddValue("FRAMENO", frameNo, null);

            foreach (var kvp in additionalFileHeaders)
            {
                hdr.AddValue(kvp.Key, kvp.Value.Item1, kvp.Value.Item2);
            }

            hdr.AddValue("END", null, null);

            f.AddHDU(imageHDU);

            // Write a FITS file.
            using (BufferedFile bf = new BufferedFile(fileName, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                f.Write(bf);
                bf.Flush();
            }
        }

        public static int[] To1DArray(int[,] input)
        {
            // Step 1: get total size of 2D array, and allocate 1D array.
            int size = input.Length;
            int[] result = new int[size];

            // Step 2: copy 2D array elements into a 1D array.
            int write = 0;
            for (int z = input.GetUpperBound(1); z >= 0; z--)
            {
                for (int i = 0; i <= input.GetUpperBound(0); i++)
                {
                    result[write++] = input[i, z];
                }
            }
            // Step 3: return the new array.
            return result;
        }
    }
}
