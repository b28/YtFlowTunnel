﻿using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using Windows.Storage;
using YtFlow.Tunnel.Adapter.Factory;

namespace YtFlow.Tunnel.Config
{
    [DataContract]
    public sealed class AdapterConfig : IAdapterConfig
    {
        private const string DEFAULT_CONFIG_PATH_KEY = "ytflow.tunnel.config.default_path";
        private static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AdapterConfig));
        private static Exception adapterTypeNotFoundException = new ArgumentOutOfRangeException("AdapterType", "AdapterType not recognized.");
        public static IAdapterConfig GetConfigFromFilePath (string filePath)
        {
            AdapterConfig baseConfig = null;
            using (var stream = new FileStream(filePath, FileMode.Open))
            {
                baseConfig = serializer.ReadObject(stream) as AdapterConfig;
                if (baseConfig != null)
                {
                    baseConfig.Path = filePath;
                }
            }
            switch (baseConfig.AdapterType)
            {
                case "shadowsocks":
                    return ShadowsocksConfig.GetConfigFromFilePath(filePath);
                case "http":
                    return HttpConfig.GetConfigFromFilePath(filePath);
                case "trojan":
                    return TrojanConfig.GetConfigFromFilePath(filePath);
                default:
                    throw adapterTypeNotFoundException;
            }
        }
        internal static IRemoteAdapterFactory GetAdapterFactoryFromDefaultFile ()
        {
            var config = GetConfigFromFilePath(GetDefaultConfigFilePath());
            switch (config)
            {
                case ShadowsocksConfig ssConfig:
                    return new ShadowsocksFactory(ssConfig);
                case HttpConfig htConfig:
                    return new HttpFactory(htConfig);
                case TrojanConfig tjConfig:
                    return new TrojanFactory(tjConfig);
                default:
                    throw adapterTypeNotFoundException;
            }
        }
        public static string GetDefaultConfigFilePath ()
        {
            ApplicationData.Current.LocalSettings.Values.TryGetValue(DEFAULT_CONFIG_PATH_KEY, out var ret);
            return ret as string;
        }
        public static void SetDefaultConfigFilePath (string configFilePath)
        {
            ApplicationData.Current.LocalSettings.Values[DEFAULT_CONFIG_PATH_KEY] = configFilePath;
        }
        public static void ClearDefaultConfigFilePath ()
        {
            ApplicationData.Current.LocalSettings.Values.Remove(DEFAULT_CONFIG_PATH_KEY);
        }

        public void SaveToFile (string filePath)
        {
            throw new NotImplementedException();
        }

        [DataMember]
        public string AdapterType { get; set; }

        [DataMember]
        public string Name { get; set; }

        [IgnoreDataMember]
        public string Path { get; set; }
    }
}
