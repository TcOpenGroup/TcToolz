
using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace Libs.Network
{
    public static class XIPAddress
    {
        public static string GetIpAddressFromString(string fullString)
        {
            try
            {
                Regex IPAd = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
                MatchCollection MatchResult = IPAd.Matches(fullString);
                return MatchResult[0].Value;
            }
            catch (Exception ex) { return ""; }
        }
        public static bool PingHost(string ip, int attempt)
        {
            bool pingable = false;
            Ping pinger = null;
            for (int i = 0; i < attempt; i++)
            {
                try
                {
                    pinger = new Ping();
                    PingReply reply = pinger.Send(ip);
                    pingable = reply.Status == IPStatus.Success;
                }
                catch (PingException)
                {
                    pingable = false;
                }
                finally
                {
                    if (pinger != null)
                    {
                        pinger.Dispose();
                    }
                }
            }
            return pingable;
        }
        public static bool PingHost(string ip)
        {
            bool pingable = false;
            Ping pinger = null;

            try
            {
                pinger = new Ping();
                PingReply reply = pinger.Send(ip);
                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                pingable = false;
            }
            finally
            {
                if (pinger != null)
                {
                    pinger.Dispose();
                }
            }

            return pingable;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool CorrectIP(string ip)
        {
            if (String.IsNullOrWhiteSpace(ip))
            {
                return false;
            }
            string[] splitValues = ip.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }
            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static string CutIPAddress(string ip)
        {
            return ip.Substring(0, ip.LastIndexOf(".") + 1);
        }



    }
}
