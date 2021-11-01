using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Collections.Generic;
using VRoid.UI.Messages;
using Newtonsoft.Json;
using System.Collections;
using System.Reflection;
using BepInEx.Configuration;
using System.Globalization;
using StandaloneWindowTitleChanger;

namespace VRoidChinese
{
    [BepInPlugin("VRoid.Chinese", "VRoid汉化插件", "1.6")]
    public class VRoidChinese : BaseUnityPlugin
    {
        public DirectoryInfo WorkDir = new DirectoryInfo($"{Paths.GameRootPath}/Chinese");
        public ConfigEntry<bool> OnStartDump;
        public ConfigEntry<bool> OnHasNullValueDump;
        public ConfigEntry<bool> DevMode;
        public ConfigEntry<KeyCode> RefreshLangKey;
        public ConfigEntry<KeyCode> SwitchLangKey;

        public static List<string> MessagesNullItems = new List<string>();

        /// <summary>
        /// 是否有空值，有空值则需要Dump
        /// </summary>
        public static bool HasNullValue;

        public string ENMessage;
        public string ENString;
        public Dictionary<string, string> ENStringDict = new Dictionary<string, string>();
        public string MergeMessage;
        public string MergeString;

        public static bool ShowUpdateTip;

        // 是否进行了回退
        public static bool IsFallback;

        public Vector2 tipV2;

        private bool nowCN;

        private void Start()
        {
            if (!WorkDir.Exists)
            {
                WorkDir.Create();
            }
            Backup();
            OnStartDump = Config.Bind<bool>("config", "OnStartDump", false, "当启动时进行转储(原词条)");
            OnHasNullValueDump = Config.Bind<bool>("config", "OnHasNullValueDump", false, "当缺失词条时进行转储(合并后词条)");
            DevMode = Config.Bind<bool>("config", "DevMode", false, "汉化者开发模式");
            RefreshLangKey = Config.Bind<KeyCode>("config", "RefreshLangKey", KeyCode.F10, "[仅限开发模式]刷新语言快捷键");
            SwitchLangKey = Config.Bind<KeyCode>("config", "SwitchLangKey", KeyCode.F11, "[仅限开发模式]切换语言快捷键");
            if (OnStartDump.Value)
            {
                DumpOri();
            }
            ToCN();
            Harmony.CreateAndPatchAll(typeof(VRoidChinese));
            StandaloneWindowTitle.Change("VRoid Studio");
        }

        private void Update()
        {
            if (DevMode.Value)
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
            if (ShowUpdateTip)
            {
                Rect rect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200);
                rect = GUILayout.Window(1234, rect, TipWindowFunc, "出现异常", GUILayout.ExpandHeight(true));
            }
        }

        public void TipWindowFunc(int id)
        {
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.black;
            GUILayout.Label("检查到有缺失的词条并引发了异常，可能是新版本新加入的词条。");
            GUILayout.Label("可以前往Github查看汉化是否有更新。");
            GUILayout.Label("如果Github上未更新汉化，可以到VRoid交流群找我反馈。");
            GUILayout.Label("汉化作者:xiaoye97");
            GUILayout.Label("QQ:1066666683");
            GUILayout.Label("B站:宵夜97");
            GUILayout.Label("VRoid交流群:684544577");
            GUILayout.Label(" ");
            if (IsFallback)
            {
                GUI.contentColor = Color.red;
                GUILayout.Label("由于缺失词条导致异常，已回退到英文。");
            }
            GUI.contentColor = Color.black;
            if (GUILayout.Button("确定"))
            {
                ShowUpdateTip = false;
            }
        }

        /// <summary>
        /// 备份原文
        /// </summary>
        public void Backup()
        {
            Debug.Log("开始备份原文...");
            ENMessage = JsonConvert.SerializeObject(Messages.All["en"], Formatting.Indented);
            var s_localeStringDictionary = Traverse.Create(typeof(Messages)).Field("s_localeStringDictionary").GetValue<Dictionary<string, Dictionary<string, string>>>();
            var enDict = s_localeStringDictionary["en"];
            StringBuilder sb = new StringBuilder();
            foreach (var kv in enDict)
            {
                ENStringDict.Add(kv.Key, kv.Value);
                string value = kv.Value.Replace("\r\n", "\\r\\n");
                sb.AppendLine($"{kv.Key}={value}");
            }
            ENString = sb.ToString();
        }

        /// <summary>
        /// 转储词条
        /// </summary>
        public void DumpOri()
        {
            Debug.Log("开始Dump原文...");
            File.WriteAllText($"{WorkDir.FullName}/DumpMessages_en.json", ENMessage);
            File.WriteAllText($"{WorkDir.FullName}/DumpString_en.txt", ENString);
        }

        /// <summary>
        /// Dump合并后的文本
        /// </summary>
        public void DumpMerge()
        {
            Debug.Log("开始Dump Merge Messages...");
            Messages messages = JsonConvert.DeserializeObject<Messages>(MergeMessage);
            string messagesStr = JsonConvert.SerializeObject(messages, Formatting.Indented);
            File.WriteAllText($"{WorkDir.FullName}/DumpMergeMessages.json", messagesStr);
            Debug.Log("开始Dump Merge String...");
            var s_localeStringDictionary = Traverse.Create(typeof(Messages)).Field("s_localeStringDictionary").GetValue<Dictionary<string, Dictionary<string, string>>>();
            var strDict = s_localeStringDictionary["en"];
            StringBuilder sb = new StringBuilder();
            foreach (var kv in strDict)
            {
                string value = kv.Value.Replace("\r\n", "\\r\\n");
                sb.AppendLine($"{kv.Key}={value}");
            }
            File.WriteAllText($"{WorkDir.FullName}/DumpMergeString.txt", sb.ToString());
        }

        /// <summary>
        /// 开始汉化
        /// </summary>
        public void ToCN()
        {
            MessagesNullItems.Clear();
            HasNullValue = false;
            Logger.LogInfo("----------开始汉化----------");
            FixString();
            FixMessages();
            Logger.LogInfo("刷新界面...");
            try
            {
                Messages.OnMessagesLanguageChange();
                nowCN = true;
                Logger.LogInfo("----------汉化完成----------");
            }
            catch (Exception e)
            {
                Logger.LogError($"刷新界面出现异常:{e.Message}\n{e.StackTrace}");
                ToEN();
            }
        }

        /// <summary>
        /// 切换到英文原文
        /// </summary>
        public void ToEN()
        {
            Logger.LogInfo("切换到英文...");
            IsFallback = true;
            var ori = Traverse.Create(typeof(Messages)).Field("s_localeDictionary").GetValue<Dictionary<string, Messages>>();
            ori["en"] = JsonConvert.DeserializeObject<Messages>(ENMessage);
            Traverse.Create(typeof(Messages)).Field("s_localeDictionary").SetValue(ori);
            Messages.OnMessagesLanguageChange();

            var s_localeStringDictionary = Traverse.Create(typeof(Messages)).Field("s_localeStringDictionary").GetValue<Dictionary<string, Dictionary<string, string>>>();
            var strDict = s_localeStringDictionary["en"];
            foreach (var kv in ENStringDict)
            {
                strDict[kv.Key] = kv.Value;
            }
            Traverse.Create(typeof(Messages)).Field("s_localeStringDictionary").SetValue(s_localeStringDictionary);
            nowCN = false;
        }

        /// <summary>
        /// 汉化Messages
        /// </summary>
        public void FixMessages()
        {
            Logger.LogInfo("开始汉化Messages...");
            if (File.Exists($"{WorkDir}/MessagesChinese.json"))
            {
                Logger.LogInfo("检测到Messages汉化文件,开始读取文件...");
                string json;
                try
                {
                    json = File.ReadAllText($"{WorkDir}/MessagesChinese.json");
                }
                catch (Exception e)
                {
                    Logger.LogError($"读取Messages汉化文件出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                Logger.LogInfo("合并软件原有英文和Messages汉化文件...");
                try
                {
                    JSONObject ori = new JSONObject(ENMessage);
                    JSONObject cnJson = new JSONObject(json);
                    MergeJson(ori, cnJson);
                    MergeMessage = ori.ToString();
                }
                catch (Exception e)
                {
                    Logger.LogError($"合并软件原有英文和Messages汉化文件出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                Logger.LogInfo("开始解析合并后文件...");
                Messages cn;
                try
                {
                    cn = JsonConvert.DeserializeObject<Messages>(MergeMessage);
                }
                catch (Exception e)
                {
                    Logger.LogError($"解析合并后文件出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                if (HasNullValue)
                {
                    Logger.LogWarning("有缺失的词条,需要通知汉化作者进行更新.");
                    if (OnHasNullValueDump.Value)
                    {
                        DumpMerge();
                    }
                }
                Logger.LogInfo("开始将中文Messages对象替换到英文对象...");
                try
                {
                    var ori = Traverse.Create(typeof(Messages)).Field("s_localeDictionary").GetValue<Dictionary<string, Messages>>();
                    ori["en"] = cn;
                    Traverse.Create(typeof(Messages)).Field("s_localeDictionary").SetValue(ori);
                }
                catch (Exception e)
                {
                    Logger.LogError($"将中文Messages对象替换到英文对象出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                Logger.LogInfo("Messages汉化完毕.");
            }
            else
            {
                Logger.LogError($"未检测到Messages汉化文件{WorkDir}/MessagesChinese.json,请检查安装.");
            }
        }

        /// <summary>
        /// 汉化常规文本
        /// </summary>
        public void FixString()
        {
            Logger.LogInfo("开始汉化常规文本...");
            if (File.Exists($"{WorkDir}/StringChinese.txt"))
            {
                Logger.LogInfo("检测到String汉化文件,开始读取文件...");
                string[] lines;
                try
                {
                    lines = File.ReadAllLines($"{WorkDir}/StringChinese.txt");
                }
                catch (Exception e)
                {
                    Logger.LogError($"读取String汉化文件出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                Logger.LogInfo("开始解析String汉化文件...");
                var s_localeStringDictionary = Traverse.Create(typeof(Messages)).Field("s_localeStringDictionary").GetValue<Dictionary<string, Dictionary<string, string>>>();
                var strDict = s_localeStringDictionary["en"];
                try
                {
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            var kv = line.Split(new char[] { '=' }, 2);
                            if (kv.Length == 2)
                            {
                                strDict[kv[0]] = kv[1].Replace("\\r\\n", "\r\n");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.LogError($"解析String汉化文件出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                Logger.LogInfo("开始将中文String替换到英文...");
                try
                {
                    Traverse.Create(typeof(Messages)).Field("s_localeStringDictionary").SetValue(s_localeStringDictionary);
                }
                catch (Exception e)
                {
                    Logger.LogError($"将中文String替换到英文出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                Logger.LogInfo("String汉化完毕.");
            }
            else
            {
                Logger.LogError($"未检测到String汉化文件{WorkDir}/StringChinese.txt,请检查安装.");
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(StandaloneWindowTitle), "Change")]
        public static bool WindowTitlePatch(ref string newTitle)
        {
            newTitle += " 中文汉化 By 宵夜97";
            return true;
        }

        /// <summary>
        /// 合并游戏英文数据和从文本读取的中文数据
        /// </summary>
        public void MergeJson(JSONObject baseJson, JSONObject modJson)
        {
            List<string> keys = new List<string>();
            foreach (var k in baseJson.keys)
            {
                keys.Add(k);
            }
            foreach (var k in keys)
            {
                if (modJson.HasField(k))
                {
                    if (baseJson[k].IsString)
                    {
                        baseJson.SetField(k, modJson[k]);
                    }
                    else if (baseJson[k].IsObject)
                    {
                        MergeJson(baseJson[k], modJson[k]);
                    }
                }
                else
                {
                    // 没有字段，添加到通知
                    HasNullValue = true;
                    MessagesNullItems.Add($"{k}:{baseJson[k]}");
                    Logger.LogWarning($"检测到缺失的词条 {k}:{baseJson[k]}");
                }
            }
        }
    }
}