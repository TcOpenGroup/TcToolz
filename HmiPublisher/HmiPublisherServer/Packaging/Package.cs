using HmiPublisherServer;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Libs.Packaging
{
   public class Package
    {
        public static void CreateZipFile(string directoryPath, string outputFilePath)
        {
            //Select Files from given directory
            List<string> directoryFileNames = Directory.GetFiles(directoryPath).ToList();
            using (ZipFile zip = new ZipFile())
            {
                zip.AddDirectory(directoryPath);
                zip.Save(outputFilePath);
            }
        }
        public static void ExtractAll(string zipPath,string destinationPath)
        {
            using (ZipFile zip = new ZipFile(zipPath))
            {
                zip.ExtractAll(destinationPath);
            }
        }

        
    }
}
