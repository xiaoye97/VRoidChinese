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
        public ConfigEntry<bool> OnFallbackDump;
        public ConfigEntry<bool> DevMode;
        public ConfigEntry<KeyCode> RefreshLangKey;

        public static List<string> MessagesNullItems = new List<string>();

        /// <summary>
        /// 是否有空值，有空值则需要Dump
        /// </summary>
        public static bool HasNullValue;

        public string ENMessage;

        public static bool ShowUpdateTip;
        // 是否进行了回退
        public static bool IsFallback;
        public Vector2 tipV2;

        private void Start()
        {
            if (!WorkDir.Exists)
            {
                WorkDir.Create();
            }
            ENMessage = JsonConvert.SerializeObject(Messages.All["en"]);
            OnStartDump = Config.Bind<bool>("config", "OnStartDump", false, "当启动时进行转储");
            OnFallbackDump = Config.Bind<bool>("config", "OnFallbackDump", false, "当触发fallback时进行转储");
            DevMode = Config.Bind<bool>("config", "DevMode", false, "汉化者开发模式");
            RefreshLangKey = Config.Bind<KeyCode>("config", "RefreshLangKey", KeyCode.F10, "[仅限开发模式]刷新语言快捷键");
            if (OnStartDump.Value)
            {
                Dump();
            }
            StartToChinese();
            Harmony.CreateAndPatchAll(typeof(VRoidChinese));
            StandaloneWindowTitle.Change("VRoid Studio");
        }

        private void Update()
        {
            if (DevMode.Value)
            {
                if (Input.GetKeyDown(RefreshLangKey.Value))
                {
                    StartToChinese();
                }
            }
        }

        private void OnGUI()
        {
            if (ShowUpdateTip)
            {
                Rect rect = new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200);
                rect = GUILayout.Window(1234, rect, TipWindowFunc, "有缺失的汉化", GUILayout.ExpandHeight(true));
            }
        }

        public void TipWindowFunc(int id)
        {
            GUI.backgroundColor = Color.white;
            GUI.contentColor = Color.black;
            GUILayout.Label("检查到有缺失的词条，可以前往Github查看汉化是否有更新。");
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
        /// 转储词条
        /// </summary>
        public void Dump()
        {
            Debug.Log("开始Dump Messages...");
            var en = JsonConvert.SerializeObject(Messages.All["en"], Formatting.Indented);
            File.WriteAllText($"{WorkDir.FullName}/DumpMessages_en.json", en);
            Debug.Log("开始Dump String...");
            var s_localeStringDictionary = Traverse.Create(typeof(Messages)).Field("s_localeStringDictionary").GetValue<Dictionary<string, Dictionary<string, string>>>();
            var enDict = s_localeStringDictionary["en"];
            StringBuilder sb = new StringBuilder();
            foreach (var kv in enDict)
            {
                string value = kv.Value.Replace("\r\n", "\\r\\n");
                sb.AppendLine($"{kv.Key}={value}");
            }
            File.WriteAllText($"{WorkDir.FullName}/DumpString_en.txt", sb.ToString());
        }

        /// <summary>
        /// 开始汉化
        /// </summary>
        public void StartToChinese()
        {
            MessagesNullItems.Clear();
            HasNullValue = false;
            Logger.LogInfo("开始汉化...");
            FixMessages();
            FixString();
            Logger.LogInfo("刷新界面...");
            try
            {
                Messages.OnMessagesLanguageChange();
            }
            catch (Exception e)
            {
                Logger.LogError($"刷新界面出现异常:{e.Message}\n{e.StackTrace}");
                Logger.LogInfo("回退汉化...");
                IsFallback = true;
                var ori = Traverse.Create(typeof(Messages)).Field("s_localeDictionary").GetValue<Dictionary<string, Messages>>();
                ori["en"] = JsonConvert.DeserializeObject<Messages>(ENMessage);
                Traverse.Create(typeof(Messages)).Field("s_localeDictionary").SetValue(ori);
                Messages.OnMessagesLanguageChange();
            }
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
                Logger.LogInfo("开始解析Messages汉化文件...");
                Messages cn;
                try
                {
                    cn = JsonConvert.DeserializeObject<Messages>(json);
                    File.WriteAllText($"{WorkDir}/test1.json", JsonConvert.SerializeObject(cn, Formatting.Indented));
                }
                catch (Exception e)
                {
                    Logger.LogError($"解析Messages汉化文件出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                Logger.LogInfo("检查缺失的词条...");
                Messages cnCheck;
                try
                {
                    cnCheck = FallbackCopy(cn, Messages.All["en"]);
                }
                catch (Exception e)
                {
                    Logger.LogError($"检查缺失的词条出现异常:{e.Message}\n{e.StackTrace}");
                    return;
                }
                if (HasNullValue)
                {
                    Logger.LogWarning("触发fallback,需要通知汉化作者进行更新.");
                    if (OnFallbackDump.Value)
                    {
                        Dump();
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
        /// 带退路的拷贝(对VRoidStudio的汉化无用，VRoidStudio使用只读属性，这里只用来检查空值)
        /// </summary>
        public static T FallbackCopy<T>(T target, T fallback)
        {
            // 检查是否为空，为空则使用后备
            if (target == null)
            {
                HasNullValue = true;
                ShowUpdateTip = true;
                Debug.LogWarning($"Messages缺失汉化:{fallback}");
                if (target is string)
                {
                    MessagesNullItems.Add(fallback as string);
                }
                return fallback;
            }
            // 如果是string或者值类型则直接返回
            if (target is string || target.GetType().IsValueType)
            {
                return target;
            }

            object retval;
            try
            {
                retval = Activator.CreateInstance(target.GetType());
            }
            catch
            {
                var json = JsonConvert.SerializeObject(fallback);
                retval = JsonConvert.DeserializeObject<T>(json);
            }
            PropertyInfo[] pros = target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var pro in pros)
            {
                try
                {
                    object pt = pro.GetValue(target);
                    object pf = pro.GetValue(fallback);
                    object o = FallbackCopy(pt, pf);
                    pro.SetValue(retval, o);
                }
                catch { }
            }
            return (T)retval;
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
    }
}