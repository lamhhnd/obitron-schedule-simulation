using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SGPdotNET;
using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.Util;

namespace get_list_sat
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string open;
        GroundStation groundStation = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(20.9798), Angle.FromDegrees(105.5239), 0));
        List<Satellite> lstSat = new List<Satellite>();
        List<Phien> lstPhien = new List<Phien>();
        double MinElevation = 15;
        double limitHours = 24;
        class Phien
        {
            GroundStation groundStation_ = new GroundStation(new GeodeticCoordinate(Angle.FromDegrees(20.9798), Angle.FromDegrees(105.5239), 0));
            private string line1;
            private string line2;
            private string line3;
            public string fullCombo;
            public string startTime;
            private string stopTime;
            private string MaxElevationTime;
            private double AzmStart;
            private double AzmMaxElevation;
            private double StartElevation;
            private double MaxElevation;
            private double StopElevation;
            private double AzmStop;
            private double Range = 1900;
            private double SAzm= 142.2;
            private double SElv = 44.7;
            private string Mag = "ecl";
            public Satellite sat { get; set; }
            public double MinElevation { get; set; }
            public SatelliteVisibilityPeriod observations { get; set; }
            private string fill(string a, int x)
            {
                string temp = a;
                while (temp.Length < x)
                {
                    temp = " " + temp;
                }
                return temp;
            }
            public void setPara()
            {
                TimeSpan timeSpan = TimeSpan.FromHours(7);
                startTime = (observations.Start + timeSpan).ToString("yyyy-MM-dd HH:mm:ss");
                AzmStart = Math.Round(groundStation_.Observe(sat, observations.Start).Azimuth.Degrees, 1);
                MaxElevationTime = (observations.MaxElevationTime + timeSpan).ToString("yyyy-MM-dd HH:mm:ss");
                AzmMaxElevation = Math.Round(groundStation_.Observe(sat, observations.MaxElevationTime).Azimuth.Degrees, 1);
                MaxElevation = Math.Round(observations.MaxElevation.Degrees,1);
                stopTime = (observations.End + timeSpan).ToString("yyyy-MM-dd HH:mm:ss");
                AzmStop = Math.Round(groundStation_.Observe(sat, observations.End).Azimuth.Degrees, 1);
                StartElevation = MinElevation;
                StopElevation = MinElevation;
                line1 = startTime + " " + sat.Name + string.Join("", Enumerable.Repeat(" ",21- sat.Name.Length)) + fill(AzmStart.ToString("0.0"), 5) + " " + fill(StartElevation.ToString("0.0"), 4) + "  " + Mag.ToString() + "  " + Range.ToString() + " " + SAzm.ToString() + "  " + SElv.ToString();
                line2 = MaxElevationTime + " " + sat.Name + string.Join("", Enumerable.Repeat(" ", 21 - sat.Name.Length)) + fill(AzmMaxElevation.ToString("0.0"), 5) + " " + fill(MaxElevation.ToString("0.0"), 4) + "  " + Mag.ToString() + "  " + Range.ToString() + " " + SAzm.ToString() + "  " + SElv.ToString();
                line3 = stopTime + " " + sat.Name + string.Join("", Enumerable.Repeat(" ", 21 - sat.Name.Length)) + fill(AzmStop.ToString("0.0"), 5) + " " + fill(StopElevation.ToString("0.0"), 4) + "  " + Mag.ToString() + "  " + Range.ToString() + " " + SAzm.ToString() + "  " + SElv.ToString();
                fullCombo = line1 + "\n" + line2 + "\n" + line3 + "\n" + "\n";
            }
           
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog fd = new OpenFileDialog();
            if (fd.ShowDialog() == DialogResult.OK)
            {
                open = fd.FileName;
            }
            lstSat.Clear();
            lstPhien.Clear();
            string[] listLine = File.ReadAllLines(open);
            for (int i = 0; i < listLine.Length; i = i + 3)
            {
                lstSat.Add(new Satellite(listLine[i], listLine[i + 1], listLine[i + 2]));
            }
            string a = "";
            for (int i = 0; i < lstSat.Count; i++)
            {
                var sat = lstSat[i];
                var observations = groundStation.Observe(sat, DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromHours(limitHours), TimeSpan.FromSeconds(5), minElevation: Angle.FromDegrees(MinElevation));
                for (int j = 0; j < observations.Count; j++)
                {
                    Phien phien = new Phien();
                    phien.observations = observations[j];
                    phien.sat = sat;
                    phien.MinElevation = MinElevation;
                    phien.setPara();
                    lstPhien.Add(phien);
                }
            }
            lstPhien.Sort((left, right) => left.startTime.CompareTo(right.startTime));
            for(int i = 0;i < lstPhien.Count; i++)
            {
                a = a + lstPhien[i].fullCombo;
            }
            string initstring = @"Satellite passes / Orbitron 3.71 / www.stoff.pl

Location      : HN (105.5239° E, 20.9798° N)
Time zone     : UTC +7:00
Search period : 2023-11-06 08:18:48 - 1 days
                2023-11-07 08:18:48
Conditions    : Maximum sun elevation = None
                Minimum sat elevation = 15 deg
                Illumination NOT required

Time                Satellite              Azm  Elv  Mag Range S.Azm S.Elv
--------------------------------------------------------------------------" + "\n";
            File.WriteAllText("2.txt", initstring + a);
        }
    }
}
