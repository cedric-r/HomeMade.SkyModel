using ASCOM.DriverAccess;
using Newtonsoft.Json;
using SGPClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Utils;

namespace HomeMadeSkyModel
{
    public partial class Form1 : Form
    {
        private Telescope telescope = null;
        private Camera camera = null;
        private bool Stop = false;
        private AppConfig appConfig;
        internal Task BW = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numberOfPointTextBox.Text = "20";
            minAltitudeTextBox.Text = "30";

            string strPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            strPath = Path.Combine(strPath, "HomeMade.SkyModel");
            try
            {
                appConfig = JsonConvert.DeserializeObject<AppConfig>(File.ReadAllText(Path.Combine(strPath, "HomeMade.SkyModel.Config")));
                binningTextBox.Text = appConfig.binning.ToString();
                exposureTextBox.Text = appConfig.exposure.ToString();
                focalTextBox.Text = appConfig.focal.ToString();
                gainTextBox.Text = appConfig.gain.ToString();
                minAltitudeTextBox.Text = appConfig.minAltitude.ToString();
                numberOfPointTextBox.Text = appConfig.numberOfPoints.ToString();
                astapPathTextBox.Text = appConfig.pathToASTAP.ToString();
                RandomCheckBox.Checked = appConfig.randomDistribution;
                userLatitudeTextBox.Text = appConfig.userLatitude.ToString();
                userLongitudeTextBox.Text = appConfig.userLongitude.ToString();

                if (!String.IsNullOrEmpty(appConfig.telescope)) ConnectTelescope(appConfig.telescope);
                if (!String.IsNullOrEmpty(appConfig.camera)) ConnectCamera(appConfig.camera);
            }
            catch (Exception)
            {
                // There might be no config.
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(userLatitudeTextBox.Text))
            {
                MessageBox.Show("User latitude can't be empty");
                return;
            }
            if (String.IsNullOrEmpty(userLongitudeTextBox.Text))
            {
                MessageBox.Show("User longitude can't be empty");
                return;
            }
            if (String.IsNullOrEmpty(numberOfPointTextBox.Text))
            {
                MessageBox.Show("Number of points can't be empty");
                return;
            }
            if (telescope == null)
            {
                MessageBox.Show("Telescope not selected");
                return;
            }
            if (!telescope.Connected)
            {
                MessageBox.Show("Telescope not connected");
                return;
            }
            if (camera == null)
            {
                MessageBox.Show("Camera not selected");
                return;
            }
            if (!camera.Connected)
            {
                MessageBox.Show("Camera not connected");
                return;
            }

            try
            {
                Double.Parse(userLatitudeTextBox.Text);
            }
            catch (Exception e1)
            {
                MessageBox.Show("Invalid user latitude");
                return;
            }
            try
            {
                Double.Parse(userLongitudeTextBox.Text);
            }
            catch (Exception e2)
            {
                MessageBox.Show("Invalid user longitude");
                return;
            }
            try
            {
                int a = Int32.Parse(numberOfPointTextBox.Text);
                if (a <4)
                {
                    MessageBox.Show("Invalid number of points. Minimum is 4.");
                    return;
                }
            }
            catch (Exception e3)
            {
                MessageBox.Show("Invalid number of points.");
                return;
            }
            try
            {
                Double.Parse(minAltitudeTextBox.Text);
            }
            catch (Exception e4)
            {
                MessageBox.Show("Invalid minimum altitude");
                return;
            }
            try
            {
                Double.Parse(exposureTextBox.Text);
            }
            catch (Exception e5)
            {
                MessageBox.Show("Invalid exposure time");
                return;
            }
            try
            {
                Int32.Parse(binningTextBox.Text);
            }
            catch (Exception e6)
            {
                MessageBox.Show("Invalid binning");
                return;
            }
            int? gain = null;
            try
            {
                if (!String.IsNullOrEmpty(gainTextBox.Text))
                    gain = Int32.Parse(gainTextBox.Text);
            }
            catch (Exception e6)
            {
                MessageBox.Show("Invalid gain");
                return;
            }
            if (String.IsNullOrEmpty(astapPathTextBox.Text))
            {
                MessageBox.Show("Invalid path to ASTAP");
                return;
            }
            if (String.IsNullOrEmpty(focalTextBox.Text))
            {
                MessageBox.Show("Please provide focal length");
                return;
            }
            SearchProcess.Stop = false;
            appConfig = new AppConfig();
            appConfig.binning = Int32.Parse(binningTextBox.Text);
            appConfig.camera = cameraName.Text;
            appConfig.exposure = Double.Parse(exposureTextBox.Text);
            appConfig.focal = Double.Parse(focalTextBox.Text);
            appConfig.gain = gain;
            appConfig.minAltitude = Double.Parse(minAltitudeTextBox.Text);
            appConfig.numberOfPoints = Int32.Parse(numberOfPointTextBox.Text);
            appConfig.pathToASTAP = astapPathTextBox.Text;
            appConfig.randomDistribution = RandomCheckBox.Checked;
            appConfig.telescope = telescopeName.Text;
            appConfig.userLatitude = Double.Parse(userLatitudeTextBox.Text);
            appConfig.userLongitude = Double.Parse(userLongitudeTextBox.Text);
            string strPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            strPath = Path.Combine(strPath, "HomeMade.SkyModel");
            try
            {
                System.IO.Directory.CreateDirectory(strPath);
                File.WriteAllText(Path.Combine(strPath, "HomeMade.SkyModel.Config"), JsonConvert.SerializeObject(appConfig));
            }
            catch (Exception) {
                MessageBox.Show("Error saving config");
            }

            BW = Task.Run(() => { SearchProcess.Search(appConfig, telescope, camera); });
        }

        private void ConnectTelescope(string telescopeId)
        {
            telescope = new Telescope(telescopeId);
            telescopeName.Text = telescopeId;
            telescopeName.Visible = true;
            try
            {
                telescope.Connected = true;
            }
            catch (Exception eTelescope)
            {
                MessageBox.Show("Error connecting the telescope: " + eTelescope.Message);
                return;
            }

            raTextBox.Text = telescope.RightAscension.ToString();
            decTextBox.Text = telescope.Declination.ToString();

            userLatitudeTextBox.Text = telescope.SiteLatitude.ToString();
            userLongitudeTextBox.Text = telescope.SiteLongitude.ToString();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string telescopeId = Telescope.Choose("");
            ConnectTelescope(telescopeId);
        }

        private void ConnectCamera(string cameraId)
        {
            camera = new Camera(cameraId);
            cameraName.Text = cameraId;
            cameraName.Visible = true;
            try
            {
                camera.Connected = true;
            }
            catch (Exception eCamera)
            {
                MessageBox.Show("Error connecting the camera: " + eCamera.Message);
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string cameraId = Camera.Choose("");
            ConnectCamera(cameraId);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            SearchProcess.Stop = true;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (BW != null)
            {
                raTextBox.Text = SearchProcess.Ra.ToString();
                decTextBox.Text = SearchProcess.Dec.ToString();
                if (!String.IsNullOrEmpty(SearchProcess.Action))
                {
                    actionLabel.Text = SearchProcess.Action;
                    actionLabel.Visible = true;
                }
                else actionLabel.Visible = false;
                string message;
                if (SearchProcess.Queue.TryDequeue(out message))
                {
                    logTextBox.AppendText(message + "\r\n");
                }
                progressBar.Value = (int)SearchProcess.Progress;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (telescope != null)
            {
                try
                {
                    telescope.Connected = false;
                }
                catch (Exception eTelescope)
                {

                }
            }
            if (camera != null)
            {
                try
                {
                    camera.Connected = false;
                }
                catch (Exception eCamera)
                {

                }
            }
        }
    }
}
