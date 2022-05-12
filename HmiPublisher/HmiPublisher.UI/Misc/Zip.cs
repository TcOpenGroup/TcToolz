using HmiPublisher.Model;
using Ionic.Zip;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Libs.Packaging
{
    public class Package
    {
        public static void CreateZipFile(string directoryPath, string outputFilePath, Compression compression)
        {
            List<string> directoryFileNames = Directory.GetFiles(directoryPath).ToList();
            using (ZipFile zip = new ZipFile())
            {
                switch (compression)
                {
                    case Compression.BestCompression:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                        break;

                    case Compression.BestSpeed:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestSpeed;
                        break;

                    case Compression.Default:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                        break;
                    case Compression.Level0:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level0;
                        break;

                    case Compression.Level1:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level1;
                        break;
                    case Compression.Level2:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level2;
                        break;

                    case Compression.Level3:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level3;
                        break;
                    case Compression.Level4:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level4;
                        break;

                    case Compression.Level5:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level5;
                        break;
                    case Compression.Level6:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level6;
                        break;

                    case Compression.Level7:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level7;
                        break;
                    case Compression.Level8:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level8;
                        break;
                    //case Compression.Level9:
                    //    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Level9;
                    //    break;

                    case Compression.None:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.None;
                        break;
                    default:
                        zip.CompressionLevel = Ionic.Zlib.CompressionLevel.Default;
                        break;
                }
                zip.AddDirectory(directoryPath);
                zip.Save(outputFilePath);
            }
        }
        public static void ExtractAll(string zipPath, string destinationPath)
        {
            using (ZipFile zip = new ZipFile(zipPath))
            {
                zip.ExtractAll(destinationPath);
            }
        }
    }
}
