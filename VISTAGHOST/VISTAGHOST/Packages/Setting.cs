﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Vistaghost.VISTAGHOST.Helper;
using Vistaghost.VISTAGHOST.Lib;
using EnvDTE;

namespace Vistaghost.VISTAGHOST
{
    public static class VGSettingConstants
    {
        public const string Version = "1.0.0";

        public const string Author = "ThuanPV3";

        public const string SettingFile = "Setting.xml"; //old file is data.xml
        public const string RegisterFile = "Register.xml";

        public const string ExportFolder = "Vistaghost";
        public const string FileExtenstion = ".vgconfig";

        public const string HistoryFile = "History.xml";

        /*for delete comments*/
        public const string MainCode = @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')";
        public const string BlockCode = @"/\*(?s:.*?)\*/";
        public const string LineCode = @"//.*";

        public const string ErrorLogFile = "ErrorLog.txt";
        public static class SettingsKey
        {
        }
    }

    class VGSetting
    {
        static VGSetting _instance;
        public static VGSetting Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VGSetting();
                }

                return _instance;
            }
        }

        public bool IsFirstInstall { get; set; }

        public string CurrentDate { get; set; }

        public string Title { get; set; }

        public VGSetting()
        {
            Title = "========== " + DateTime.Today.DayOfWeek.ToString() + ", " +
                                    DateTime.Now.ToShortDateString() + ", " +
                                    DateTime.Now.ToShortTimeString() + ", Computer Name: " +
                                    Environment.MachineName + " ==========\n";

            SettingData = LoadSettings();
            RegisterData = LoadRegisterInfo();
        }

        public static string DefaultFileName(ExImType eiType)
        {
            string _defaultName = String.Empty;
            switch (eiType)
            {
                case ExImType.vgEXPORT:
                    _defaultName = "Exported-" + VGOperations.GetDateString(0);
                    break;
                case ExImType.vgIMPORT:
                    _defaultName = "CurSettings-" + VGOperations.GetDateString(0);
                    break;
                default:
                    break;
            }
            return _defaultName;
        }

        public static Settings SettingData { get; set; }

        public static RegisterData RegisterData { get; set; }

        #region Data methods
        public static Settings LoadSettings()
        {
            Settings settings = new Settings
            {
                CommentInfo = new CommentInfo(),
                HeaderInfo = new HeaderInfo(),
                DataInfo = new DataInfo()
            };

            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                if (isoStore.GetFileNames(VGSettingConstants.SettingFile).Length == 0)
                    return settings;

                var isoStream = new IsolatedStorageFileStream(VGSettingConstants.SettingFile, FileMode.Open, isoStore);

                if (!isoStream.CanRead)
                    return settings;

                XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                settings = (Settings)serializer.Deserialize(isoStream);

                isoStream.Close();
                isoStore.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return settings;
        }

        public static bool SaveSettings()
        {
            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                var isoStream = new IsolatedStorageFileStream(VGSettingConstants.SettingFile, FileMode.Create, isoStore);

                if (!isoStream.CanWrite)
                    return false;

                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                serializer.Serialize(isoStream, SettingData);

                isoStream.Close();
                isoStore.Close();

            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }

            return true;
        }

        public static bool SaveRegisterInfo()
        {
            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                var isoStream = new IsolatedStorageFileStream(VGSettingConstants.RegisterFile, FileMode.Create, isoStore);

                if (!isoStream.CanWrite)
                    return false;

                XmlSerializer serializer = new XmlSerializer(typeof(RegisterData));

                serializer.Serialize(isoStream, RegisterData);

                isoStream.Close();
                isoStore.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                return false;
            }

            return true;
        }

        public static RegisterData LoadRegisterInfo()
        {
            RegisterData regData = new RegisterData { Registered = false };

            try
            {
                var isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);
                if (isoStore.GetFileNames(VGSettingConstants.RegisterFile).Length == 0)
                    return regData;

                var isoStream = new IsolatedStorageFileStream(VGSettingConstants.RegisterFile, FileMode.Open, isoStore);

                if (!isoStream.CanRead)
                    return regData;

                XmlSerializer serializer = new XmlSerializer(typeof(RegisterData));
                regData = (RegisterData)serializer.Deserialize(isoStream);

                isoStream.Close();
                isoStore.Close();
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            return regData;
        }

        #endregion

        public static List<FileNameInfo> GetAllSettingFile(string path)
        {
            List<FileNameInfo> listfile = new List<FileNameInfo>();
            if (Directory.Exists(path))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] Files = dir.GetFiles("*.vgconfig", SearchOption.TopDirectoryOnly);

                foreach (var file in Files)
                {
                    listfile.Add(new FileNameInfo { Name = file.Name, FullPath = file.FullName });
                }
            }
            return listfile;
        }

        public static void GetUniqueFileName(string path, string filename, out string new_filename)
        {
            string fullpath = Path.Combine(path, filename + VGSettingConstants.FileExtenstion);

            int num = 1;
            int i = 0;

            if (File.Exists(fullpath))
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] Files = dir.GetFiles(filename + "*.vgconfig", SearchOption.TopDirectoryOnly);

                while (i < Files.Count())
                {
                    string temp = Path.GetFileNameWithoutExtension(Files[i].Name);
                    if (temp.Length == filename.Length)
                    {
                        i++;
                        continue;
                    }
                    temp = temp.Substring(filename.Length + 1);

                    int tempNum = 0;
                    if (int.TryParse(temp, out tempNum))
                    {
                        if (tempNum >= num)
                            num = tempNum + 1;
                    }

                    i++;
                }
                new_filename = filename + "-" + num.ToString();
                return;
            }

            new_filename = filename;
        }

        public static bool SaveCurrentSetting(string fullPath, out string message, out bool success)
        {
            try
            {
                FileStream fileStream = File.Open(fullPath, FileMode.Create, FileAccess.Write);
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                serializer.Serialize(fileStream, SettingData);

                fileStream.Close();

                message = Properties.Resources.ExportSuccessMessage;
                success = true;
            }
            catch (Exception ex)
            {
                message = "Export failed.";
                Logger.LogMessage(ex.Message);
                success = false;
            }

            return success;
        }

        public static bool LoadNewSetting(string path, out string message, out bool success)
        {
            if (!File.Exists(path))
            {
                message = "File not found.";
                success = false;
                return false;
            }

            try
            {
                FileStream fileStream = File.Open(path, FileMode.Create, FileAccess.Write);
                XmlSerializer serializer = new XmlSerializer(typeof(Settings));

                SettingData = (Settings)serializer.Deserialize(fileStream);

                fileStream.Close();

                message = Properties.Resources.ImportSettingsSuccess;
                success = true;
            }
            catch (Exception ex)
            {
                message = "Import failed.";
                Logger.LogMessage(ex.Message);
                success = false;
            }

            return success;
        }
    }
}
