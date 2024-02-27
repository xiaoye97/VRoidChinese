using BepInEx;
using BepInEx.Unity.IL2CPP;
using System.Collections.Generic;
using System.IO;
using System;
using BepInEx.Configuration;
using VRoid.UI.Messages;
using System.Text;
using HarmonyLib;
using Newtonsoft.Json;
using UnityEngine;

namespace vroid_i18n
{
    class PluginMeta
    {
        public const string PLUGIN_GUID = "VRoid.Chinese";
        public const string PLUGIN_NAME = "VRoid Studio i18n plugin";
        public const string PLUGIN_VERSION = "1.2.0";
        public const string EXE_NAME = "VRoidStudio.exe";
        /// <summary>
        /// 汉化文本存放路径
        /// </summary>
        public static readonly DirectoryInfo I18nFilePath = new($"{Paths.GameRootPath}\\locates\\zh_CN\\");
    }

    [BepInPlugin(PluginMeta.PLUGIN_GUID, PluginMeta.PLUGIN_NAME, PluginMeta.PLUGIN_VERSION)]
    [BepInProcess(PluginMeta.EXE_NAME)]
    public class Plugin : BasePlugin
    {
        /// <summary>
        /// 启动汉化插件时是否 Dump 原文
        /// </summary>
        public ConfigEntry<bool> OnStartDump;

        /// <summary>
        /// 出现空值时是否 Dump 合并文本
        /// </summary>
        public ConfigEntry<bool> OnHasNullValueDump;
       
        /// <summary>
        /// 开发者模式
        /// </summary>
        public ConfigEntry<bool> DebugMode;
        
        /// <summary>
        /// 刷新UI快捷键
        /// </summary>
        public ConfigEntry<BepInEx.Unity.IL2CPP.UnityEngine.KeyCode> UiRefreshShortcut;
        
        /// <summary>
        /// 切换中英文快捷键
        /// </summary>
        public ConfigEntry<BepInEx.Unity.IL2CPP.UnityEngine.KeyCode> LanguageSwitchShortcut;
        
        /// <summary>
        /// 是否有空值, 有空值则需要 Dump
        /// </summary>
        public bool HasNullValue;

        /// <summary>
        /// 英文原文Messages
        /// </summary>
        public string RawMessage;

        /// <summary>
        /// 英文原文String
        /// </summary>
        public string RawString;

        /// <summary>
        /// 英文原文String的字典形式
        /// </summary>
        public Dictionary<string, string> RawStringDict = new();

        /// <summary>
        /// 合并文本Messages
        /// </summary>
        public string MergeMessage;

        /// <summary>
        /// 合并文本String
        /// </summary>
        public string MergeString;

        /// <summary>
        /// 显示更新提示
        /// </summary>
        public static bool ShowUpdateTip;

        /// <summary>
        /// 是否进行了回退
        /// </summary>
        public bool IsFallback;

        /// <summary>
        /// 当前是否为中文
        /// </summary>
        public bool IsLanguageOverride;

        public override void Load()
        {
            Log.LogMessage($"Plugin {PluginMeta.PLUGIN_GUID} {PluginMeta.PLUGIN_VERSION} is loaded for {PluginMeta.EXE_NAME}!");
            try
            {
                InitPluginConfig();
                InitPlugin();
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
        }

        private void InitPlugin()
        {
            System.Diagnostics.Stopwatch sw = new();
            sw.Start();

            if (!PluginMeta.I18nFilePath.Exists)
            {
                PluginMeta.I18nFilePath.Create();
            }

            OverrideLanguageToChinese();

            Harmony.CreateAndPatchAll(typeof(Plugin));
            VRoid.UI.EditorOption.EditorOptionManager.Instance.EditorOption.Preference.languageMode = VRoid.UI.EditorOption.LanguageMode.En;
            Messages.CurrentCrowdinLanguageCode = "en";
            sw.Stop();

            Log.LogDebug($"i18n complete in {sw.ElapsedMilliseconds}ms");
        }

        private void InitPluginConfig()
        {
            OnStartDump = Config.Bind<bool>("config", "OnStartDump", false, "当启动时进行转储 (原词条)");
            OnHasNullValueDump = Config.Bind<bool>("config", "OnHasNullValueDump", false, "当缺失词条时进行转储 (合并后词条)");
            DebugMode = Config.Bind<bool>("config", "DebugMode", false, "调试模式");
            UiRefreshShortcut = Config.Bind("config", "UiRefreshShortcut", BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.F10, "[仅限开发模式] 刷新语言快捷键");
            LanguageSwitchShortcut = Config.Bind("config", "LanguageSwitchShortcut", BepInEx.Unity.IL2CPP.UnityEngine.KeyCode.F11, "[仅限开发模式] 切换语言快捷键");

            CreateBackup();
            if (OnStartDump.Value)
            {
                DumpRawLanguage();
            }
        }

        public void OverrideLanguageToChinese()
        {
            HasNullValue = false;
            OverrideStrings();
            OverrideMessages();
            try
            {
                Messages.OnMessagesLanguageChange?.Invoke();
                IsLanguageOverride = true;
            }
            catch (Exception e)
            {
                Log.LogError($"Error on refresh UI: {e.Message}\n{e.StackTrace}");
                IsFallback = true;
                OverrideLanguageToEnglish();
            }
        }

        public void OverrideLanguageToEnglish()
        {
            try
            {
            Messages.s_localeDictionary["en"] = JsonConvert.DeserializeObject<Messages>(RawMessage);
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
            try
            {
                Messages.OnMessagesLanguageChange?.Invoke();
                foreach (var kv in RawStringDict)
                {
                    Messages.s_localeStringDictionary["en"][kv.Key] = kv.Value;
                }
            }
            catch (Exception e)
            {
                Log.LogError(e);
            }
            IsLanguageOverride = false;
        }

        public void OverrideMessages()
        {
            Log.LogDebug("bug on InitPluginConfig");
            string messageFilePath = $"{PluginMeta.I18nFilePath}\\messages.json";
            if (!File.Exists(messageFilePath))
            {
                Log.LogError($"No {messageFilePath} founded");
                return;
            }
            try
            {
                string json = File.ReadAllText(messageFilePath);
                JSONObject ori = new(RawMessage);
                JSONObject cnJson = new(json);

                MergeJson(ori, cnJson);

                JSONObject sortJson = JsonSorter(ori);
                MergeMessage = sortJson.ToString();
                Messages cn = JsonConvert.DeserializeObject<Messages>(MergeMessage);

                Messages.s_localeDictionary["en"] = cn;

                if (HasNullValue)
                {
                    if (OnHasNullValueDump.Value)
                    {
                        MergeDumpedFiles();
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError($"Error on load i18n: {e.Message}\n{e.StackTrace}");
            }
        }

        public void OverrideStrings()
        {
            string stringFilePath = $"{PluginMeta.I18nFilePath}\\string";
            if (!File.Exists(stringFilePath))
            {
                Log.LogError($"No {stringFilePath} founded");
                return;
            }
            try
            {
                string[] lines = File.ReadAllLines(stringFilePath);
                var strDict = Messages.s_localeStringDictionary["en"];
                foreach (var line in lines)
                {
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        var kv = line.Split(['='], 2);
                        if (kv.Length == 2)
                        {
                            strDict[kv[0]] = kv[1].Replace("\\r\\n", "\r\n");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogError($"Unable to decode: {e.Message}\n{e.StackTrace}");
            }
        }

        public void MergeDumpedFiles()
        {
            Messages messages = JsonConvert.DeserializeObject<Messages>(MergeMessage);
            string messagesStr = JsonConvert.SerializeObject(messages);
            File.WriteAllText($"{PluginMeta.I18nFilePath.FullName}\\DumpMergeMessages.json", messagesStr);
            var strDict = Messages.s_localeStringDictionary["en"];
            StringBuilder sb = new();
            foreach (var kv in strDict)
            {
                string value = kv.Value.Replace("\r\n", "\\r\\n");
                sb.AppendLine($"{kv.Key}={value}");
            }
            File.WriteAllText($"{PluginMeta.I18nFilePath.FullName}\\DumpMergeString.txt", sb.ToString());
        }

        public void CreateBackup()
        {
            System.Diagnostics.Stopwatch sw = new();
            sw.Start();
            RawMessage = JsonConvert.SerializeObject(Messages.All["en"]);
            var enDict = Messages.s_localeStringDictionary["en"];
            StringBuilder sb = new();
            foreach (var kv in enDict)
            {
                RawStringDict.Add(kv.Key, kv.Value);
                string value = kv.Value.Replace("\r\n", "\\r\\n");
                sb.AppendLine($"{kv.Key}={value}");
            }
            RawString = sb.ToString();
            sw.Stop();
        }

        public void DumpRawLanguage()
        {
            File.WriteAllText($"{PluginMeta.I18nFilePath.FullName}\\DumpMessages_en_{PluginMeta.PLUGIN_VERSION}.json", RawMessage);
            File.WriteAllText($"{PluginMeta.I18nFilePath.FullName}\\DumpString_en_{PluginMeta.PLUGIN_VERSION}.txt", RawString);
        }

        public JSONObject JsonSorter(JSONObject baseJson)
        {
            if (baseJson.type == JSONObject.Type.OBJECT)
            {
                List<string> keys = new(baseJson.keys);
                keys.Sort();
                JSONObject obj = new(JSONObject.Type.OBJECT);
                foreach (var key in keys)
                {
                    obj.SetField(key, baseJson[key]);
                }
                return obj;
            }
            else
            {
                return baseJson;
            }
        }

        public void MergeJson(JSONObject baseJson, JSONObject modJson)
        {
            List<string> baseKeys = new(baseJson.keys);
            foreach (var key in baseKeys)
            {
                if (modJson.HasField(key))
                {
                    if (baseJson[key].IsString)
                    {
                        baseJson.SetField(key, modJson[key]);
                    }
                    else if (baseJson[key].IsObject)
                    {
                        MergeJson(baseJson[key], modJson[key]);
                    }
                }
                else
                {
                    HasNullValue = true;
                    Log.LogWarning($"Detected missing value: {key}:{baseJson[key]}");
                }
            }
        }

        // unused
/*
        private void Update()
        {
            if (DebugMode.Value)
            {
                if (Input.GetKeyDown(RefreshLangKey.Value))
                {
                    ToCN();
                }
                if (Input.GetKeyDown(SwitchLangKey.Value))
                {
                    if (nowCN)
                    {
                        ToEN();
                    }
                    else
                    {
                        ToCN();
                    }
                }
            }
        }

        private void OnGUI()
        {
            GUI.backgroundColor = Color.black;
            if (ShowUpdateTip)
            {
                Rect rect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 150, 400, 300);
                rect = GUILayout.Window(1234, rect, (GUI.WindowFunction)ExceptionTipWindowFunc, "出现异常", GUILayout.ExpandHeight(true));
            }
        }

        public void ExceptionTipWindowFunc(int id)
        {
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.black;
            GUILayout.Label("检查到汉化插件出现了异常, 可能是与新版本不兼容导致.");
            GUILayout.Label("可以前往 GitHub 查看汉化是否有更新.");
            GUILayout.Label("如果 GitHub 上未更新汉化, 可以到VRoid交流群找我反馈.");
            GUILayout.Label("汉化作者: xiaoye97");
            GUILayout.Label("GitHub: xiaoye97");
            GUILayout.Label("QQ: 1066666683");
            GUILayout.Label("B站: 宵夜97");
            GUILayout.Label("宵夜食堂: 528385469");
            GUILayout.Label("VRoid交流群: 684544577");
            GUILayout.Label("汉化插件官网: https://github.com/xiaoye97/VRoidChinese");
            GUILayout.Label(" ");
            if (IsFallback)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label("由于缺失词条导致异常, 已回退到英文.");
            }
            GUI.contentColor = Color.black;
            if (GUILayout.Button("确定"))
            {
                ShowUpdateTip = false;
            }
        }*/
    }
}
