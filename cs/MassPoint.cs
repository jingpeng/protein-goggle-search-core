using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProteinGoggle.Model
{
    [Serializable]
    public class MassPoint
    {
        private Int32 index;

        public Int32 Index
        {
            get { return index; }
            set { index = value; }
        }
        private double intensityPercentage;

        public double IntensityPercentage
        {
            get { return intensityPercentage; }
            set { intensityPercentage = value; }
        }

        private double intensity;
        public double Intensity
        {
            get { return intensity; }
            set { intensity = value; }
        }
        private double mass;

        public double Mass
        {
            get { return mass; }
            set { mass = value; }
        }

        private bool isSearch = true;

        public bool IsSearch
        {
            get { return isSearch; }
            set { isSearch = value; }
        }

        private Int32 matchIndex = 0;

        public Int32 MatchIndex
        {
            get { return matchIndex; }
            set { matchIndex = value; }
        }

        private Int32 comPared = 0;
        public Int32 ComPared
        {
            get { return comPared; }
            set { comPared = value; }
        }

        private double deviation;

        public double Deviation
        {
            get { return deviation; }
            set { deviation = value; }
        }

        private double ipmd_r;
        public double IPMD_R
        {
            get { return ipmd_r; }
            set { ipmd_r = value; }
        }

        private double ipad_r;
        public double IPAD_R
        {
            get { return ipad_r; }
            set { ipad_r = value; }
        }

        private double ipadom_r;
        public double IPADOM_R
        {
            get { return ipadom_r; }
            set { ipadom_r = value; }
        }

        private double ipmdom_r;
        public double IPMDOM_R
        {
            get { return ipmdom_r; }
            set { ipmdom_r = value; }
        }
    }
}
