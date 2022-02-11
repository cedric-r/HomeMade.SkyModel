﻿using ASCOM.DriverAccess;
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
        internal static ConcurrentQueue<string> Queue = new ConcurrentQueue<string>();

        private static string InputFile = "";
        private static string OutputFile = "";

        private static void StopSearch()
        {
            Stop = false;
            Action = "";
            Queue.Enqueue("Stopping search");
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

        internal static void Search(double userLatitude, double userLongitude, int numberOfPoints, double minAltitude, double exposure, int binning, int? gain, string pathToASTAP, Telescope telescope, Camera camera)
        {
            int nbPoint = numberOfPoints;
            double minAlt = minAltitude;

            Queue.Enqueue("Generating points");
            List<AltAzCoordinates> points = Sphere.GenerateRandomAltAzPointingCoordinates(nbPoint, minAlt);

            Queue.Enqueue("Starting search");
            Astro astro = new Astro();
            int frame = 1;
            foreach (AltAzCoordinates point in points)
            {
                Queue.Enqueue("Switching off tracking");
                telescope.Tracking = false;

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
                try
                {
                    if (telescope.CanSlewAltAz)
                    {
                        Queue.Enqueue("Slewing AltAz to " + point.Alt + "," + point.Az);
                        telescope.SlewToAltAz(point.Az, point.Alt);
                    }
                    else
                    {
                        Queue.Enqueue("Slewing RaDec to " + coord["RA"].ToString() + "," + coord["DEC"].ToString());
                        telescope.SlewToCoordinates(coord["RA"], coord["DEC"]);
                    }
                }
                catch(Exception e)
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

                if (binning > camera.MaxBinX || binning > camera.MaxBinY)
                {
                    Queue.Enqueue("Invalind binning: " + binning);
                    return;
                }
                if (exposure > camera.ExposureMax || exposure < camera.ExposureMin)
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
                    Queue.Enqueue("Imaging, attempt " + (plateSolveTries+1));
                    camera.BinX = (short)binning;
                    camera.BinY = (short)binning;
                    if (gain != null)
                    {
                        if (gain.Value > camera.GainMax || gain.Value < camera.GainMin)
                        {
                            Queue.Enqueue("Invalid gain:" + gain.Value);
                            return;
                        }

                        camera.Gain = (short)gain.Value;
                    }
                    camera.StartExposure(exposure, true);
                    while (!camera.ImageReady)
                    {
                        Thread.Sleep(100);
                        if (Stop)
                        {
                            StopSearch();
                            if (camera.CanAbortExposure) camera.AbortExposure();
                            return;
                        }
                    }
                    Action = "";
                    Action = "Getting image";
                    int[,] image = (int[,])camera.ImageArray;
                    Action = "";
                    Action = "Saving image";
                    InputFile = Path.Combine(Path.GetTempPath(), "skymodelframe.fit");
                    Dictionary<string, Tuple<string, string>> additionalParameters = new Dictionary<string, Tuple<string, string>>();
                    additionalParameters.Add("XBINNING", new Tuple<string, string>(binning.ToString(), null));
                    additionalParameters.Add("YBINNING", new Tuple<string, string>(binning.ToString(), null));
                    additionalParameters.Add("XPIXSZ", new Tuple<string, string>(camera.PixelSizeX.ToString(), null));
                    additionalParameters.Add("YPIXSZ", new Tuple<string, string>(camera.PixelSizeY.ToString(), null));
                    additionalParameters.Add("RA", new Tuple<string, string>(coord["RA"].ToString(), null));
                    additionalParameters.Add("DEC", new Tuple<string, string>(coord["DEC"].ToString(), null));
                    additionalParameters.Add("CENTALT", new Tuple<string, string>(point.Alt.ToString(), null));
                    additionalParameters.Add("CENTAZ", new Tuple<string, string>(point.Az.ToString(), null));
                    additionalParameters.Add("SITELAT", new Tuple<string, string>(userLatitude.ToString(), null));
                    additionalParameters.Add("SITELONG", new Tuple<string, string>(userLongitude.ToString(), null));
                    FitsImage.SaveFitsFrame(frame, InputFile, camera.NumX, camera.NumY, FitsImage.To1DArray(image), DateTime.Now, (int)exposure, DateTime.Now.ToLongTimeString(), additionalParameters);
                    Action = "";

                    if (Stop)
                    {
                        StopSearch();
                        if (camera.CanAbortExposure) camera.AbortExposure();
                        return;
                    }

                    Action = "Plate solving";
                    Queue.Enqueue("Plate solving");
                    OutputFile = Path.Combine(Path.GetTempPath(), "astap.ini");
                    string arguments = "-f " + InputFile + " -r 90 -fov 0 -o " + OutputFile;
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
                        if (camera.CanAbortExposure) camera.AbortExposure();
                        return;
                    }

                    ASTAPResults plateSolve = new ASTAPResults(OutputFile);
                    if (!plateSolve.IsSuccess())
                    {
                        Queue.Enqueue("Plate solve failed");
                    }
                    else
                    {
                        Action = "Syncing telescope";
                        Queue.Enqueue("Syncing telescope");
                        plateSolveOk = true;
                        double actualRa = plateSolve.Ra();
                        double actualDec = plateSolve.Dec();
                        Queue.Enqueue("Syncing " + coord["RA"] + "/" + coord["DEC"] + " to " + actualRa + "/" + actualDec);
                        telescope.SyncToCoordinates(astro.ToHours(actualRa), actualDec);
                    }
                    if (Stop)
                    {
                        StopSearch();
                        if (camera.CanAbortExposure) camera.AbortExposure();
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
            }
        }

    }
}