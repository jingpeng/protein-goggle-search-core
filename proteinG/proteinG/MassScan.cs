using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProteinGoggle.Model
{
    [Serializable]
    public class MassScan
    {
        private UInt32 index;

        public UInt32 Index
        {
            get { return index; }
            set { index = value; }
        }

        private UInt32 msLevel;

        public UInt32 MsLevel
        {
            get { return msLevel; }
            set { msLevel = value; }
        }

        private UInt32 peaksCount;

        public UInt32 PeaksCount
        {
            get { return peaksCount; }
            set { peaksCount = value; }
        }

        private bool polarity;

        public bool Polarity
        {
            get { return polarity; }
            set { polarity = value; }
        }

        private String scanType;

        public String ScanType
        {
            get { return scanType; }
            set { scanType = value; }
        }

        private String filterLine;

        public String FilterLine
        {
            get { return filterLine; }
            set { filterLine = value; }
        }

        private String retentionTime;

        public String RetentionTime
        {
            get { return retentionTime; }
            set { retentionTime = value; }
        }

        private double lowMz;

        public double LowMz
        {
            get { return lowMz; }
            set { lowMz = value; }
        }

        private double highMz;

        public double HighMz
        {
            get { return highMz; }
            set { highMz = value; }
        }

        private double basePeakMz;

        public double BasePeakMz
        {
            get { return basePeakMz; }
            set { basePeakMz = value; }
        }

        private double basePeakIntensity;

        public double BasePeakIntensity
        {
            get { return basePeakIntensity; }
            set { basePeakIntensity = value; }
        }

        private UInt32 totIonCurrent;

        public UInt32 TotIonCurrent
        {
            get { return totIonCurrent; }
            set { totIonCurrent = value; }
        }

        private float precursorMz;

        public float PrecursorMz
        {
            get { return precursorMz; }
            set { precursorMz = value; }
        }

        private String peaks;

        public String Peaks
        {
            get { return peaks; }
            set { peaks = value; }
        }

        private bool isPeaks = false;

        public bool IsPeaks
        {
            get { return isPeaks; }
            set { isPeaks = value; }
        }

        private string id;

        public String ID
        {
            get { return id; }
            set { id = value; }
        }

        private string mod_res;

        public String MOD_RES
        {
            get { return mod_res; }
            set { mod_res = value; }
        }

        private int z;

        public int Z
        {
            get { return z; }
            set { z = value; }
        }
    }
}

