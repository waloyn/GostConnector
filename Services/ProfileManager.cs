using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using GostConnector.Models;

namespace GostConnector.Services
{
    public class ProfileStorageData
    {
        public string ActiveProfileId { get; set; }
        public List<GostProfile> Profiles { get; set; }

        public ProfileStorageData()
        {
            Profiles = new List<GostProfile>();
        }
    }

    public class ProfileManager
    {
        private readonly string _filePath;
        private readonly JavaScriptSerializer _serializer;

        public ProfileStorageData Data { get; private set; }

        public ProfileManager(string customPath = null)
        {
            if (string.IsNullOrEmpty(customPath))
            {
                string dir = AppDomain.CurrentDomain.BaseDirectory;
                _filePath = Path.Combine(dir, "profiles.json");
            }
            else
            {
                _filePath = customPath;
            }

            _serializer = new JavaScriptSerializer();
            Data = new ProfileStorageData();
            Load();
        }

        public void Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    Data = _serializer.Deserialize<ProfileStorageData>(json);
                }
            }
            catch
            {
                Data = null;
            }

            if (Data == null || Data.Profiles == null || Data.Profiles.Count == 0)
            {
                Data = new ProfileStorageData();
                // 初始化用户日常使用的默认配置
                var defaultProfile = new GostProfile
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "SQL Server 转发 (1433)",
                    ListenProtocol = "tcp",
                    ListenAddress = ":1433",
                    TargetAddress = "192.168.18.8:1433",
                    ForwardProtocol = "relay+grpc",
                    ForwardServer = "110.0.0.1:10808",
                    AuthParam = "xxxxxx",
                    ExtraArgs = ""
                };
                defaultProfile.CommandLine = GostCliParser.BuildFullCommandLine(defaultProfile);
                Data.Profiles.Add(defaultProfile);
                Data.ActiveProfileId = defaultProfile.Id;
                Save();
            }
        }

        public void Save()
        {
            try
            {
                string json = _serializer.Serialize(Data);
                File.WriteAllText(_filePath, json);
            }
            catch
            {
                // 保存失败静默或记录
            }
        }

        public GostProfile GetActiveProfile()
        {
            if (Data.Profiles.Count == 0) return null;
            var found = Data.Profiles.Find(p => p.Id == Data.ActiveProfileId);
            return found ?? Data.Profiles[0];
        }
    }
}
