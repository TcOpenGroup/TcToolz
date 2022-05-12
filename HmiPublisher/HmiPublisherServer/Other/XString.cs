/*********************************************************
   Libs\Other\XString.cs
   
   Copyright (©) 2018 Marek Gvora
*********************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Libs.Other
{
    public static class XString
    {
        public static bool IsSomeLetter(string content)
        {
            // @".*?[a-zA-Z].*?"   //only letters
            //@"^([a-zA-Z0-9]+)$" //only letters and alphanumeric

            if (Regex.IsMatch(content, @"\S+"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public static string RemoveEmptyLine(string content)
        {
            string filter = Regex.Replace(content, @"^\s*$(\n|\r|\r\n)", "", RegexOptions.Multiline);
            if (!string.IsNullOrEmpty(filter))
            {
                return filter;
            }
            else
            {
                return "";
            }
        }

        public static SecureString StringToSecureString(string plainText)
        {
            if (plainText == null)
                throw new ArgumentNullException("password");

            var securePassword = new SecureString();

            foreach (char c in plainText)
                securePassword.AppendChar(c);

            securePassword.MakeReadOnly();
            return securePassword;
        }
        public static String SecureStringToString(SecureString value)
        {
            IntPtr valuePtr = IntPtr.Zero;
            try
            {
                valuePtr = Marshal.SecureStringToGlobalAllocUnicode(value);
                return Marshal.PtrToStringUni(valuePtr);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePtr);
            }
        }
    }
}
