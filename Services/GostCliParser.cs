using System;
using System.Collections.Generic;
using System.Text;
using GostConnector.Models;

namespace GostConnector.Services
{
    public static class GostCliParser
    {
        /// <summary>
        /// 将 CLI 命令行文本解析为 GostProfile 结构化数据
        /// </summary>
        public static GostProfile Parse(string commandLine, GostProfile profile = null)
        {
            if (profile == null)
            {
                profile = new GostProfile();
            }

            if (string.IsNullOrWhiteSpace(commandLine))
            {
                return profile;
            }

            string trimmed = commandLine.Trim();
            profile.CommandLine = trimmed;

            List<string> tokens = Tokenize(trimmed);
            if (tokens.Count == 0) return profile;

            // 移除前置 gost 或 gost.exe
            int startIndex = 0;
            string first = tokens[0].ToLowerInvariant();
            if (first == "gost" || first == "gost.exe" || first.EndsWith("\\gost.exe") || first.EndsWith("/gost.exe"))
            {
                startIndex = 1;
            }

            List<string> extras = new List<string>();

            for (int i = startIndex; i < tokens.Count; i++)
            {
                string token = tokens[i];
                if (token.Equals("-L", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Count)
                {
                    ParseListenArg(tokens[++i], profile);
                }
                else if (token.Equals("-F", StringComparison.OrdinalIgnoreCase) && i + 1 < tokens.Count)
                {
                    ParseForwardArg(tokens[++i], profile);
                }
                else
                {
                    extras.Add(token);
                }
            }

            profile.ExtraArgs = string.Join(" ", extras.ToArray());
            return profile;
        }

        /// <summary>
        /// 解析 -L 参数，例如: tcp://:1433/192.168.18.8:1433 或 :1433/192.168.18.8:1433
        /// </summary>
        private static void ParseListenArg(string raw, GostProfile profile)
        {
            if (string.IsNullOrEmpty(raw)) return;

            string proto = "tcp";
            string rest = raw;

            int protoIndex = raw.IndexOf("://", StringComparison.Ordinal);
            if (protoIndex >= 0)
            {
                proto = raw.Substring(0, protoIndex);
                rest = raw.Substring(protoIndex + 3);
            }

            profile.ListenProtocol = proto;

            int slashIndex = rest.IndexOf('/');
            if (slashIndex >= 0)
            {
                profile.ListenAddress = rest.Substring(0, slashIndex);
                profile.TargetAddress = rest.Substring(slashIndex + 1);
            }
            else
            {
                profile.ListenAddress = rest;
                profile.TargetAddress = string.Empty;
            }
        }

        /// <summary>
        /// 解析 -F 参数，例如: relay+grpc://110.0.0.1:10808?auth=xxxxxx
        /// </summary>
        private static void ParseForwardArg(string raw, GostProfile profile)
        {
            if (string.IsNullOrEmpty(raw)) return;

            string proto = "relay+grpc";
            string rest = raw;

            int protoIndex = raw.IndexOf("://", StringComparison.Ordinal);
            if (protoIndex >= 0)
            {
                proto = raw.Substring(0, protoIndex);
                rest = raw.Substring(protoIndex + 3);
            }

            profile.ForwardProtocol = proto;

            int qIndex = rest.IndexOf('?');
            if (qIndex >= 0)
            {
                profile.ForwardServer = rest.Substring(0, qIndex);
                string query = rest.Substring(qIndex + 1);

                // 解析 auth 参数
                string authVal = "";
                string[] parts = query.Split('&');
                List<string> remainQuery = new List<string>();
                foreach (var part in parts)
                {
                    if (part.StartsWith("auth=", StringComparison.OrdinalIgnoreCase))
                    {
                        authVal = part.Substring(5);
                    }
                    else
                    {
                        remainQuery.Add(part);
                    }
                }

                profile.AuthParam = authVal;
            }
            else
            {
                profile.ForwardServer = rest;
                profile.AuthParam = string.Empty;
            }
        }

        /// <summary>
        /// 根据结构化字段组装完整的命令行参数（不含 gost 前缀）
        /// </summary>
        public static string BuildArguments(GostProfile profile)
        {
            if (profile == null) return string.Empty;

            StringBuilder sb = new StringBuilder();

            // 监听部分 -L
            string listenPart = BuildListenPart(profile);
            if (!string.IsNullOrEmpty(listenPart))
            {
                sb.Append("-L ").Append(listenPart);
            }

            // 转发部分 -F
            string forwardPart = BuildForwardPart(profile);
            if (!string.IsNullOrEmpty(forwardPart))
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append("-F ").Append(forwardPart);
            }

            // 附加参数
            if (!string.IsNullOrWhiteSpace(profile.ExtraArgs))
            {
                if (sb.Length > 0) sb.Append(" ");
                sb.Append(profile.ExtraArgs.Trim());
            }

            return sb.ToString();
        }

        /// <summary>
        /// 构建完整命令行（含 gost 前缀）
        /// </summary>
        public static string BuildFullCommandLine(GostProfile profile)
        {
            string args = BuildArguments(profile);
            return string.IsNullOrEmpty(args) ? "gost" : "gost " + args;
        }

        private static string BuildListenPart(GostProfile profile)
        {
            string listenAddr = string.IsNullOrWhiteSpace(profile.ListenAddress) ? ":1433" : profile.ListenAddress.Trim();
            if (!listenAddr.StartsWith(":") && !listenAddr.Contains(":"))
            {
                listenAddr = ":" + listenAddr;
            }

            string proto = string.IsNullOrWhiteSpace(profile.ListenProtocol) ? "tcp" : profile.ListenProtocol.Trim();

            if (!string.IsNullOrWhiteSpace(profile.TargetAddress))
            {
                return string.Format("{0}://{1}/{2}", proto, listenAddr, profile.TargetAddress.Trim());
            }
            else
            {
                return string.Format("{0}://{1}", proto, listenAddr);
            }
        }

        private static string BuildForwardPart(GostProfile profile)
        {
            if (string.IsNullOrWhiteSpace(profile.ForwardServer)) return string.Empty;

            string proto = string.IsNullOrWhiteSpace(profile.ForwardProtocol) ? "relay+grpc" : profile.ForwardProtocol.Trim();
            string server = profile.ForwardServer.Trim();

            StringBuilder sb = new StringBuilder();
            sb.Append(proto).Append("://").Append(server);

            if (!string.IsNullOrWhiteSpace(profile.AuthParam))
            {
                string auth = profile.AuthParam.Trim();
                if (auth.StartsWith("?")) auth = auth.Substring(1);
                if (auth.StartsWith("auth=", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append("?").Append(auth);
                }
                else
                {
                    sb.Append("?auth=").Append(auth);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// 分词算法：支持空格分割及引号包裹参数
        /// </summary>
        public static List<string> Tokenize(string text)
        {
            List<string> tokens = new List<string>();
            if (string.IsNullOrEmpty(text)) return tokens;

            StringBuilder current = new StringBuilder();
            bool inQuotes = false;
            char quoteChar = '\0';

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (inQuotes)
                {
                    if (c == quoteChar)
                    {
                        inQuotes = false;
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
                else
                {
                    if (c == '"' || c == '\'')
                    {
                        inQuotes = true;
                        quoteChar = c;
                    }
                    else if (char.IsWhiteSpace(c))
                    {
                        if (current.Length > 0)
                        {
                            tokens.Add(current.ToString());
                            current.Clear();
                        }
                    }
                    else
                    {
                        current.Append(c);
                    }
                }
            }

            if (current.Length > 0)
            {
                tokens.Add(current.ToString());
            }

            return tokens;
        }
    }
}
