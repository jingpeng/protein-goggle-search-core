using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using ProteinGoggle.Model;

namespace ProteinGoggle.Model
{
    public class Base64Convert
    {
        public static List<MassPoint> CalcMz(String base64Code, UInt32 peaksCount)
        {
            byte[] bytes = Convert.FromBase64String(base64Code);
            List<MassPoint> lstMassPoint = new List<MassPoint>();

            for (int n = 0; n < peaksCount; n++)
            {
                byte[] massByteArray = new byte[4] { bytes[8 * n], bytes[8 * n + 1], bytes[8 * n + 2], bytes[8 * n + 3] };
                double mass = GetFloatFromByte(massByteArray);
                byte[] intensityByteArray = new byte[4] { bytes[8 * n + 4], bytes[8 * n + 5], bytes[8 * n + 6], bytes[8 * n + 7] };
                double intensity = GetFloatFromByte(intensityByteArray);

                MassPoint massPt = new MassPoint();
                massPt.Index = n;
                massPt.Mass = mass;
                massPt.Intensity = intensity;
                lstMassPoint.Add(massPt);
            }

            return lstMassPoint;
        }

        private static SByte[] DecodeBase64(string code)
        {
            byte[] bytes = Convert.FromBase64String(code);
            SByte[] res = bytes.Cast<SByte>().ToArray();
            return res;
        }

        private static double GetFloatFromByte(byte[] bytes)
        {
            if (bytes == null || bytes.Length != 4)
            {
                return -1;
            }
            else
            {
                // change the byte array to int value
                int tempRes = BitConverter.ToInt32(bytes, 0);

                // change the int value from network byte to host byte order
                int hostOrder = IPAddress.NetworkToHostOrder(tempRes);

                // change the host order value to float
                double res = BitConverter.ToSingle(BitConverter.GetBytes(hostOrder), 0);

                return res;
            }
        }
    }


}
