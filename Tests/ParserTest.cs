using System;
using GostConnector.Models;
using GostConnector.Services;

namespace GostConnector.Tests
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine("==== 开始 GOST CLI 解析单元测试 ====");
            int passed = 0;
            int total = 0;

            // 测试用例 1: 用户给定的典型命令
            total++;
            string cmd1 = "gost -L tcp://:1433/192.168.18.8:1433 -F relay+grpc://110.0.0.1:10808?auth=xxxxxx";
            var p1 = GostCliParser.Parse(cmd1);

            bool ok1 = p1.ListenProtocol == "tcp" &&
                       p1.ListenAddress == ":1433" &&
                       p1.TargetAddress == "192.168.18.8:1433" &&
                       p1.ForwardProtocol == "relay+grpc" &&
                       p1.ForwardServer == "110.0.0.1:10808" &&
                       p1.AuthParam == "xxxxxx";

            if (ok1)
            {
                Console.WriteLine("[PASS] 测试用例 1 (典型命令解析成功)");
                passed++;
            }
            else
            {
                Console.WriteLine(string.Format("[FAIL] 测试用例 1 失败: Listen={0}://{1}/{2}, Forward={3}://{4}?auth={5}",
                    p1.ListenProtocol, p1.ListenAddress, p1.TargetAddress,
                    p1.ForwardProtocol, p1.ForwardServer, p1.AuthParam));
            }

            // 测试用例 2: 逆向组装
            total++;
            string built1 = GostCliParser.BuildFullCommandLine(p1);
            if (built1 == cmd1)
            {
                Console.WriteLine("[PASS] 测试用例 2 (逆向组装与原始命令行完全一致)");
                passed++;
            }
            else
            {
                Console.WriteLine("[FAIL] 测试用例 2 失败:\n预期: " + cmd1 + "\n实际: " + built1);
            }

            // 测试用例 3: 仅参数且带引号与附加选项
            total++;
            string cmd3 = "-L tcp://127.0.0.1:3389/10.0.0.5:3389 -F \"relay+tls://2.2.2.2:443?auth=mytoken\" -D";
            var p3 = GostCliParser.Parse(cmd3);
            bool ok3 = p3.ListenAddress == "127.0.0.1:3389" &&
                       p3.TargetAddress == "10.0.0.5:3389" &&
                       p3.ForwardProtocol == "relay+tls" &&
                       p3.ForwardServer == "2.2.2.2:443" &&
                       p3.AuthParam == "mytoken" &&
                       p3.ExtraArgs == "-D";
            if (ok3)
            {
                Console.WriteLine("[PASS] 测试用例 3 (带引号、自定义IP及附加 -D 标志解析成功)");
                passed++;
            }
            else
            {
                Console.WriteLine(string.Format("[FAIL] 测试用例 3 失败: Listen={0}/{1}, Forward={2}://{3}?auth={4}, Extra={5}",
                    p3.ListenAddress, p3.TargetAddress, p3.ForwardProtocol, p3.ForwardServer, p3.AuthParam, p3.ExtraArgs));
            }

            Console.WriteLine(string.Format("\n测试汇总: 通过 {0}/{1}", passed, total));
            return passed == total ? 0 : 1;
        }
    }
}
