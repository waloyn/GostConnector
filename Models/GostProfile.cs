using System;

namespace GostConnector.Models
{
    [Serializable]
    public class GostProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }

        // 完整命令行缓存或用户直接自定义命令
        public string CommandLine { get; set; }

        // -L 监听与目标配置
        public string ListenProtocol { get; set; }
        public string ListenAddress { get; set; }
        public string TargetAddress { get; set; }

        // -F 转发中继节点配置
        public string ForwardProtocol { get; set; }
        public string ForwardServer { get; set; }
        public string AuthParam { get; set; }

        // 附加选项
        public string ExtraArgs { get; set; }

        public GostProfile()
        {
            Id = Guid.NewGuid().ToString("N");
            Name = "新配置";
            ListenProtocol = "tcp";
            ListenAddress = ":1433";
            TargetAddress = "192.168.18.8:1433";
            ForwardProtocol = "relay+grpc";
            ForwardServer = "110.0.0.1:10808";
            AuthParam = "";
            ExtraArgs = "";
            CommandLine = "";
        }

        public GostProfile Clone()
        {
            return new GostProfile
            {
                Id = Guid.NewGuid().ToString("N"),
                Name = this.Name + " (副本)",
                CommandLine = this.CommandLine,
                ListenProtocol = this.ListenProtocol,
                ListenAddress = this.ListenAddress,
                TargetAddress = this.TargetAddress,
                ForwardProtocol = this.ForwardProtocol,
                ForwardServer = this.ForwardServer,
                AuthParam = this.AuthParam,
                ExtraArgs = this.ExtraArgs
            };
        }
    }
}
