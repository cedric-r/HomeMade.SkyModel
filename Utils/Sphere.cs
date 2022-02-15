using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public class SphereCoordinates
    {
        public double x;
        public double y;
        public double z;
    }

    public class AltAzCoordinates
    {
        public double Alt;
        public double Az;
    }

    public static class Sphere
    {
        public static List<AltAzCoordinates> GenerateRandomAltAzPointingCoordinates(int sample, double minAlt)
        {
            List<AltAzCoordinates> temp = new List<AltAzCoordinates>();
            Random rnd = new Random();

            // Generate the coordinates in 2 stages to separate the hemispheres into quadrants and avoid a bit of travel
            for (int i = 0; i < sample/4; i++)
            {
                temp.Add(new AltAzCoordinates() { Alt = rnd.Next((int)Math.Ceiling(minAlt), 90), Az = rnd.Next(0, 90) });
            }

            for (int i = 0; i < sample / 4; i++)
            {
                temp.Add(new AltAzCoordinates() { Alt = rnd.Next((int)Math.Ceiling(minAlt), 90), Az = rnd.Next(91, 180) });
            }

            for (int i = 0; i < sample/4; i++)
            {
                temp.Add(new AltAzCoordinates() { Alt = rnd.Next((int)Math.Ceiling(minAlt), 90), Az = rnd.Next(181, 270) });
            }

            for (int i = 0; i < sample / 4; i++)
            {
                temp.Add(new AltAzCoordinates() { Alt = rnd.Next((int)Math.Ceiling(minAlt), 90), Az = rnd.Next(271, 360) });
            }

            return temp;
        }

        public static List<AltAzCoordinates> GenerateAltAzPointingCoordinates(int sample, double minAlt)
        {
            List<AltAzCoordinates> temp = new List<AltAzCoordinates>();
            int slices = (int)Math.Ceiling(Math.Sqrt(sample));
            int degreesPerSlice = (int)(360.0 / ((double)slices));
            int pointsPerSlice = (int)Math.Floor(((double)sample) / slices);
            int step = (int)Math.Floor((90-minAlt)/pointsPerSlice);

            // Generate the coordinates in 2 stages to separate the hemispheres into quadrants and avoid a bit of travel
            for (int i = 0; i < slices; i++)
            {
                for (int j = 0; j < pointsPerSlice; j++)
                {
                    temp.Add(new AltAzCoordinates() { Alt = minAlt+(j*step), Az = i*degreesPerSlice+(degreesPerSlice/2) });
                }
            }

            return temp;
        }

        public static List<SphereCoordinates> FibonacciSphere(double samples = 10)
        {
            List<SphereCoordinates> points = new List<SphereCoordinates>();

            double phi = Math.PI * (3.0 - Math.Sqrt(5.0)); // golden angle in radians

            for (int i =0; i< samples; i++)
            {
                double y = 1 - (i / (samples - 1)) * 2;  // y goes from 1 to -1
                double radius = Math.Sqrt(1 - y * y); // radius at y

                double theta = phi * i; // golden angle increment


                double x = Math.Cos(theta) * radius;
                double z = Math.Sin(theta) * radius;

                points.Add(new SphereCoordinates() { x = x, y = y, z = z });
            }

            return points;
        }
    }
}
