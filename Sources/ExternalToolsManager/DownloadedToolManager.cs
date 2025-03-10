namespace VeriSolRunner.ExternalTools
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Net;
    using System.Runtime.InteropServices;

    internal class DownloadedToolManager : ToolManager
    {
        private static string OsName
        {
            get
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    return "windows";
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    return "osx";
                }
                else
                {
                    return "linux";
                }
            }
        }

        private string DownloadURL
        {
            get
            {

                return this.settings.DownloadURLs[OsName];
            }
        }

        private string ExePathWithinZip
        {
            get
            {

                return this.settings.ExePathsWithinZip[OsName];
            }
        }        

        private string ZipFileName
        {
            get
            {
                return this.settings.Name + ".zip";
            }
        }

        private string TempDirectory
        {
            get
            {
                return this.settings.CommandPath + "Temp/";
            }
        }

        private string ZipFilePath
        {
            get
            {
                return this.TempDirectory + this.ZipFileName;
            }
        }

        private string ExtractPath
        {
            get
            {
                return this.TempDirectory + this.settings.Name + "/";
            }
        }

        private string ExeTempPath
        {
            get
            {
                return this.ExtractPath + this.ExePathWithinZip;
            }
        }

        internal DownloadedToolManager(ToolSourceSettings settings) : base(settings)
        {
        }

        internal override void EnsureExisted()
        {
            EnsureCommandPathExisted();

            if (!Exists())
            {
                DownloadedAndUnZip();
            }
            else
            {
                ExternalToolsManager.Log($"Skip installing tool {this.settings.Name} as we could find it under {this.settings.CommandPath}.");
            }
        }

        private void DownloadedAndUnZip()
        {
            if (!Directory.Exists(TempDirectory))
            {
                ExternalToolsManager.Log($"Creating temporary directory {TempDirectory}");
                Directory.CreateDirectory(TempDirectory);
            }
            else
            {
                if (File.Exists(ZipFilePath))
                {
                    ExternalToolsManager.Log($"Deleting file {ZipFilePath}");
                    File.Delete(ZipFilePath);
                }

                if (Directory.Exists(ExtractPath))
                {
                    ExternalToolsManager.Log($"Deleting directory {ExtractPath}");
                    Directory.Delete(ExtractPath, true);
                }
            }

            using (var client = new WebClient())
            {
                ExternalToolsManager.Log($"Downloading {ZipFileName} from {DownloadURL} to {TempDirectory}");
                client.DownloadFile(DownloadURL, ZipFilePath);
            }

            ExternalToolsManager.Log($"Extracting {ZipFileName} to {ExtractPath}");
            ZipFile.ExtractToDirectory(ZipFilePath, ExtractPath);

            ExternalToolsManager.Log($"Copying {ExeTempPath} to {Command}");
            File.Copy(ExeTempPath, Command);

            ExternalToolsManager.Log($"Cleaning up");
            File.Delete(ZipFilePath);
            Directory.Delete(ExtractPath, true);
            ExternalToolsManager.Log($"Done");
        }
    }
}