using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace FfxivMacroCalculator
{

    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public record ConfigFile
    {
        public const string DefaultConfigPath = "./config.json";
        public const string DefaultConfigDirectory = "./";

        [JsonProperty("lang")]
        public string Language = "zhCN";

        [JsonProperty("outputLang")]
        public string OutputLanguage = "zhCN";


        [JsonProperty("langFolder")]
        public string LanguageFilesDirectory = "./lang/";

        [JsonProperty("uiFile")]
        public string UIFilePath = "./ui.json";

        [JsonProperty("lastUsedConfigFile")]
        public string lastUsedConfig = "./last_config.json";


        [JsonProperty("outputFolder")]
        public string OutputDirectory = "./output/";

        [JsonProperty("cacheFolder")]
        public string CacheDirectory = "./cache/";


        // TODO

        public static ConfigFile Read(string path, out string? errMsg)
        {
            errMsg = null;
            try
            {
                var rawText = File.ReadAllText(path);
                var ret = JsonConvert.DeserializeObject<ConfigFile>(rawText);
                if (ret != null)
                    return ret;
                else
                {
                    errMsg = "JsonConvert.DeserializeObject returned null";
                    return new();
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException)
            {
                errMsg = ex.Message;
                return new();
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return new();
            }
        }

        public static bool Write(ConfigFile f, string path, out string? errMsg)
        {
            errMsg = null;
            try
            {
                var s = JsonConvert.SerializeObject(f, Formatting.Indented);
                if (s != null)
                {
                    File.WriteAllText(path, s);
                    return true;
                }
                else
                {
                    errMsg = "JsonConvert.SerializeObject returned null";
                    return false;
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }
    }
}
