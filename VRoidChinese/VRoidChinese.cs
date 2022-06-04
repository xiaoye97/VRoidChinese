using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Text;
using Newtonsoft.Json;
using VRoid.UI.Messages;
using BepInEx.Configuration;
using System.Collections.Generic;
using StandaloneWindowTitleChanger;

namespace VRoidChinese
{
    [BepInPlugin("VRoid.Chinese", "VRoid汉化插件", "1.8")]
    public class VRoidChinese : BaseUnityPlugin
    {
        /// <summary>
        /// 汉化文本存放路径
        /// </summary>
        public DirectoryInfo WorkDir = new DirectoryInfo($"{Paths.GameRootPath}/Chinese");

        /// <summary>
        /// 启动汉化插件时是否Dump原文
        /// </summary>
        public ConfigEntry<bool> OnStartDump;

        /// <summary>
        /// 出现空值时是否Dump合并文本
        /// </summary>
        public ConfigEntry<bool> OnHasNullValueDump;

        /// <summary>
        /// 开发者模式
        /// </summary>
        public ConfigEntry<bool> DevMode;

        /// <summary>
        /// 刷新UI快捷键
        /// </summary>
        public ConfigEntry<KeyCode> RefreshLangKey;

        /// <summary>
        /// 切换中英文快捷键
        /// </summary>
        public ConfigEntry<KeyCode> SwitchLangKey;

        /// <summary>
        /// 是否有空值，有空值则需要Dump
        /// </summary>
        public static bool HasNullValue;

        /// <summary>
        /// 英文原文Messages
        /// </summary>
        public string ENMessage;

        /// <summary>
        /// 英文原文String
        /// </summary>
        public string ENString;

        /// <summary>
        /// 英文原文String的字典形式
        /// </summary>
        public Dictionary<string, string> ENStringDict = new Dictionary<string, string>();

        /// <summary>
        /// 合并文本Messages
        /// </summary>
        public string MergeMessage;

        /// <summary>
        /// 合并文本String
        /// </summary>
        public string MergeString;

        /// <summary>
        /// 是否显示提示
        /// </summary>
        public static bool ShowUpdateTip;

        /// <summary>
        /// 是否进行了回退
        /// </summary>
        public static bool IsFallback;

        /// <summary>
        /// 当前是否为中文
        /// </summary>
        private bool nowCN;

        private void Start()
        {
            try
            {
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                if (!WorkDir.Exists)
                {
                    WorkDir.Create();
                }
                // 读取配置
                OnStartDump = Config.Bind<bool>("config", "OnStartDump", false, "当启动时进行转储(原词条)");
                OnHasNullValueDump = Config.Bind<bool>("config", "OnHasNullValueDump", false, "当缺失词条时进行转储(合并后词条)");
                DevMode = Config.Bind<bool>("config", "DevMode", false, "汉化者开发模式");
                RefreshLangKey = Config.Bind<KeyCode>("config", "RefreshLangKey", KeyCode.F10, "[仅限开发模式]刷新语言快捷键");
                SwitchLangKey = Config.Bind<KeyCode>("config", "SwitchLangKey", KeyCode.F11, "[仅限开发模式]切换语言快捷键");

                // 备份原文
                Backup();
                if (OnStartDump.Value)
                {
                    // Dump原文到硬盘
                    DumpOri();
                }
                // 开始汉化文本
                ToCN();
                Harmony.CreateAndPatchAll(typeof(VRoidChinese));
                StandaloneWindowTitle.Change("VRoid Studio");
                // 切换到中文
                VRoid.UI.EditorOption.EditorOptionManager.Instance.EditorOption.Preference.languageMode = VRoid.UI.EditorOption.LanguageMode.En;
                Messages.CurrentCrowdinLanguageCode = "en";
                sw.Stop();
                Logger.LogInfo($"总耗时 {sw.ElapsedMilliseconds}ms");
            }
            catch (Exception e)
            {
                Logger.LogError(e);
                ShowUpdateTip = true;
            }
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
            GUI.backgroundColor = Color.black;
            if (ShowUpdateTip)
            {
                Rect rect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 150, 400, 300);
                rect = GUILayout.Window(1234, rect, ExceptionTipWindowFunc, "出现异常", GUILayout.ExpandHeight(true));
            }
        }

        public void ExceptionTipWindowFunc(int id)
        {
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.black;
            GUILayout.Label("检查到汉化插件出现了异常，可能是与新版本不兼容导致。");
            GUILayout.Label("可以前往GitHub查看汉化是否有更新。");
            GUILayout.Label("如果GitHub上未更新汉化，可以到VRoid交流群找我反馈。");
            GUILayout.Label("汉化作者:xiaoye97");
            GUILayout.Label("GitHub:xiaoye97");
            GUILayout.Label("QQ:1066666683");
            GUILayout.Label("B站:宵夜97");
            GUILayout.Label("宵夜食堂:528385469");
            GUILayout.Label("VRoid交流群:684544577");
            GUILayout.Label("汉化插件网址:https://github.com/xiaoye97/VRoidChinese");
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
            Logger.LogInfo("开始备份原文...");
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            ENMessage = JsonConvert.SerializeObject(Messages.All["en"], Formatting.Indented);
            var enDict = Messages.s_localeStringDictionary["en"];
            StringBuilder sb = new StringBuilder();
            foreach (var kv in enDict)
            {
                ENStringDict.Add(kv.Key, kv.Value);
                string value = kv.Value.Replace("\r\n", "\\r\\n");
                sb.AppendLine($"{kv.Key}={value}");
            }
            ENString = sb.ToString();
            sw.Stop();
            Logger.LogInfo($"备份耗时{sw.ElapsedMilliseconds}ms");
        }

        /// <summary>
        /// 转储词条
        /// </summary>
        public void DumpOri()
        {
            Debug.Log("开始Dump原文...");
            File.WriteAllText($"{WorkDir.FullName}/DumpMessages_en_{Application.version}.json", ENMessage);
            File.WriteAllText($"{WorkDir.FullName}/DumpString_en_{Application.version}.txt", ENString);
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
            var strDict = Messages.s_localeStringDictionary["en"]; 
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
                IsFallback = true;
                ToEN();
            }
        }

        /// <summary>
        /// 切换到英文原文
        /// </summary>
        public void ToEN()
        {
            Logger.LogInfo("切换到英文...");
            Messages.s_localeDictionary["en"] = JsonConvert.DeserializeObject<Messages>(ENMessage);
            Messages.OnMessagesLanguageChange();
            foreach (var kv in ENStringDict)
            {
                Messages.s_localeStringDictionary["en"][kv.Key] = kv.Value;
            }
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
                    Messages.s_localeDictionary["en"] = cn;
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
                var strDict = Messages.s_localeStringDictionary["en"];
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
            newTitle += $" 汉化作者: 宵夜97 (开源免费)";
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
                    Logger.LogWarning($"检测到缺失的词条 {k}:{baseJson[k]}");
                }
            }
        }
    }
}