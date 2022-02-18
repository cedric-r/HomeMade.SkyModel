using ASCOM.DriverAccess;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Utils;

namespace HomeMadeSkyModel
{
    internal static class SearchProcess
    {
        internal static bool Stop = false;
        internal static double Alt = 0;
        internal static double Az = 0;
        internal static double Ra = 0;
        internal static double Dec = 0;
        internal static string Action = "";
        internal static double Progress = 0;
        internal static ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();

        private static string InputFile = "";
        private static string OutputFile = "";

        private static Telescope Telescope;
        private static Camera Camera;

        private static void StopSearch()
        {
            Queue.Enqueue("Stopping search");
            Telescope.AbortSlew();
            if (Camera.CanAbortExposure) Camera.AbortExposure();
            Stop = false;
            Action = "";
            try
            {
                File.Delete(InputFile);
            }
            catch (Exception) { }
            try
            {
                File.Delete(OutputFile);
            }
            catch (Exception) { }
        }

        internal static void Search(double userLatitude, double userLongitude, int numberOfPoints, double minAltitude, double exposure, int binning, double focal, int? gain, string pathToASTAP, bool randomDistribution, Telescope telescope, Camera camera)
        {
            try
            {
                Camera = camera;
                Telescope = telescope;
                Progress = 0;

                int nbPoint = numberOfPoints;
                double minAlt = minAltitude;

                Queue.Enqueue("Generating points");
                List<AltAzCoordinates> points;
                if (randomDistribution) points = Sphere.GenerateRandomAltAzPointingCoordinates(nbPoint, minAlt);
                else points = Sphere.GenerateAltAzPointingCoordinates(nbPoint, minAlt);

                Queue.Enqueue("Starting search");
                Astro astro = new Astro();
                int frame = 1;
                int successfulSync = 0;
                double pointNumber = 1;
                foreach (AltAzCoordinates point in points)
                {
                    Queue.Enqueue("Switching off tracking");
                    Telescope.Tracking = false;

                    if (Stop)
                    {
                        StopSearch();
                        return;
                    }
                    Alt = point.Alt;
                    Az = point.Az;

                    double LMST = astro.GMST(DateTime.UtcNow) + userLongitude;

                    Dictionary<string, double> coord = astro.AZALT2RADEC(LMST, point.Az, point.Alt, userLatitude);
                    Ra = astro.ToHours(coord["RA"]);
                    Dec = coord["DEC"];

                    Action = "Moving";

                    Queue.Enqueue("Point " + pointNumber + "/" + points.Count);
                    Progress = pointNumber / (double)points.Count * 100;
                    try
                    {
                        if (telescope.CanSlewAltAz)
                        {
                            Queue.Enqueue("Slewing AltAz to " + point.Alt + "," + point.Az);
                            telescope.SlewToAltAz(point.Az, point.Alt);
                        }
                        else
                        {
                            Queue.Enqueue("Slewing RaDec to " + astro.ToHours(coord["RA"]).ToString() + "," + coord["DEC"].ToString());
                            telescope.SlewToCoordinates(astro.ToHours(coord["RA"]), coord["DEC"]);
                        }
                    }
                    catch (Exception e)
                    {
                        Queue.Enqueue("Error during slew: " + e.Message);
                        return;
                    }

                    while (telescope.Slewing && !Stop)
                    {
                        if (Stop)
                        {
                            StopSearch();
                            return;
                        }
                        Thread.Sleep(500);
                    }

                    Queue.Enqueue("Switching on tracking");
                    telescope.Tracking = true;
                    Thread.Sleep(2000);
                    Action = "";

                    if (Stop)
                    {
                        StopSearch();
                        return;
                    }
                    Ra = telescope.RightAscension;
                    Dec = telescope.Declination;

                    if (Stop)
                    {
                        StopSearch();
                        return;
                    }

                    if (binning > Camera.MaxBinX || binning > Camera.MaxBinY)
                    {
                        Queue.Enqueue("Invalind binning: " + binning);
                        return;
                    }
                    if (exposure > Camera.ExposureMax || exposure < Camera.ExposureMin)
                    {
                        Queue.Enqueue("Invalid exposure time: " + exposure);
                        return;
                    }

                    bool plateSolveOk = false;
                    int plateSolveTries = 0;
                    while (!plateSolveOk && plateSolveTries < 3)
                    {
                        // Take image
                        Action = "Imaging";
                        Queue.Enqueue("Imaging, attempt " + (plateSolveTries + 1));
                        Camera.BinX = (short)binning;
                        Camera.NumX = (int)Camera.CameraXSize / binning;
                        Camera.BinY = (short)binning;
                        Camera.NumY = (int)Camera.CameraYSize / binning;
                        if (gain != null)
                        {
                            if (gain.Value > Camera.GainMax || gain.Value < Camera.GainMin)
                            {
                                Queue.Enqueue("Invalid gain:" + gain.Value);
                                return;
                            }

                            Camera.Gain = (short)gain.Value;
                        }
                        Camera.StartExposure(exposure, true);
                        while (!Camera.ImageReady)
                        {
                            Thread.Sleep(100);
                            if (Stop)
                            {
                                StopSearch();
                                return;
                            }
                        }
                        Action = "";
                        Action = "Getting image";
                        int[,] image = (int[,])Camera.ImageArray;
                        Action = "";
                        Action = "Saving image";
                        InputFile = Path.Combine(Path.GetTempPath(), "skymodelframe.fit");
                        Dictionary<string, Tuple<string, string>> additionalParameters = new Dictionary<string, Tuple<string, string>>();
                        Queue.Enqueue("Saving to " + InputFile);
                        FitsImage.SaveFitsFrame(frame, InputFile, userLatitude, userLongitude, telescope.SiteElevation, focal, Camera.NumX, Camera.NumY, Camera.PixelSizeX, Camera.PixelSizeY, binning, FitsImage.To1DArray(image), point.Alt, point.Az, coord["RA"], coord["DEC"], DateTime.Now, (int)exposure, additionalParameters);
                        Action = "";

                        if (Stop)
                        {
                            StopSearch();
                            return;
                        }

                        Action = "Plate solving";
                        Queue.Enqueue("Plate solving");
                        OutputFile = Path.Combine(Path.GetTempPath(), "astap.ini");
                        double resolution = 206.2648 * Camera.PixelSizeX / focal; // Assumes square pixels
                        string arguments = "-f " + InputFile + " -r 90 -fov 0 -m " + (resolution * 3) + " -z 0 -o " + OutputFile;
                        using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
                        {
                            pProcess.StartInfo.FileName = pathToASTAP;
                            pProcess.StartInfo.Arguments = arguments; //argument
                            pProcess.StartInfo.UseShellExecute = false;
                            pProcess.StartInfo.RedirectStandardOutput = true;
                            pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                            pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                            pProcess.Start();
                            string output = pProcess.StandardOutput.ReadToEnd(); //The output result
                            pProcess.WaitForExit();
                        }
                        Action = "";
                        plateSolveTries++;

                        if (Stop)
                        {
                            StopSearch();
                            return;
                        }

                        ASTAPResults plateSolve = new ASTAPResults(OutputFile);
                        if (!plateSolve.IsSuccess())
                        {
                            Queue.Enqueue("Plate solve failed");
                        }
                        else
                        {
                            successfulSync++;
                            Action = "Syncing telescope";
                            Queue.Enqueue("Syncing telescope");
                            plateSolveOk = true;
                            double actualRa = plateSolve.Ra();
                            double actualDec = plateSolve.Dec();
                            Queue.Enqueue("Syncing " + coord["RA"] + "/" + coord["DEC"] + " to " + actualRa + "/" + actualDec);
                            try
                            {
                                telescope.SyncToCoordinates(astro.ToHours(actualRa), actualDec);
                            }
                            catch(Exception e2)
                            {
                                Queue.Enqueue("Telescope sync refused, ignoring.");
                            }
                        }
                        if (Stop)
                        {
                            StopSearch();
                            return;
                        }

                        Action = "";
                        frame++;
                    }
                    if (plateSolveTries == 5)
                    {
                        Queue.Enqueue("Unable to plate solve");
                        StopSearch();
                        return;
                    }
                    pointNumber++;
                }
                StopSearch();
                Queue.Enqueue(successfulSync+" successful syncs out of "+points.Count());
            }
            catch(Exception e)
            {
                Queue.Enqueue("Error: "+e.Message);
            }
        }

    }
}
