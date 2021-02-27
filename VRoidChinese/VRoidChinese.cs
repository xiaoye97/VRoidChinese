using System;
using BepInEx;
using System.IO;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace VRoidChinese
{
    [BepInPlugin("VRoid.Chinese", "VRoid汉化插件", "1.4")]
    public class VRoidChinese : BaseUnityPlugin
    {
        //中文配置
        public static string zhPath;
        private static bool needSave;
        public static Dictionary<string, string> zhConfig = new Dictionary<string, string>();
        public static Harmony harmony;
        public static Type TranslatorType, WelcomeControllerType;

        void Start()
        {
            zhPath = $"{Paths.ConfigPath}/VRoid.Chinese.cfg";
            //初始化
            TranslatorType = AccessTools.TypeByName("Translator");
            WelcomeControllerType = AccessTools.TypeByName("WelcomeController");
            harmony = new Harmony("VRoid.Chinese");
            //与配置文件进行配对
            var enDict = Traverse.Create(TranslatorType).Field("enDictionary").GetValue<Dictionary<string, string>>();
            enDict.Add("GUI.Welcome.Preferences.Language.Chinese", "中文");
            LoadConfig();
            foreach (var k in enDict.Keys)
            {
                if (!zhConfig.ContainsKey(k))
                {
                    zhConfig.Add(k, enDict[k]);
                    needSave = true;
                }
            }
            if (needSave)
            {
                SaveConfig();
            }
            //添加中文选项
            harmony.Patch(WelcomeControllerType.GetProperty("LanguagePreferencesDropdownItemList").GetGetMethod(), prefix: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WelcomeController_LanguagePreferencesDropdownItemList_Get_Prefix")));
            //修正语言选项刷新索引上限
            harmony.Patch(AccessTools.Method(WelcomeControllerType, "UpdateLanguagePreferencesWithIndex"), transpiler: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WelcomeController_UpdateLanguagePreferencesWithIndex_Transpiler")));
            //设置标题
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("WindowTitle"), "Update"), prefix: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WindowTitle_Update_Prefix")));
            //添加语言数据
            AddLanguageDictToVRoidStudio();
            //设定当前语言为中文
            Traverse.Create(GameObject.FindObjectOfType(WelcomeControllerType)).Method("OnLanguagePreferencesDropdownValueChanged", 2).GetValue();
        }

        /// <summary>
        /// 加载配置
        /// </summary>
        void LoadConfig()
        {
            if (File.Exists(zhPath))
            {
                var lines = File.ReadAllLines(zhPath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (!string.IsNullOrWhiteSpace(lines[i]))
                    {
                        var strs = lines[i].Split('=');
                        if (strs.Length == 2)
                        {
                            if (strs[1].Contains("\\n"))
                            {
                                string str = strs[1].Replace("\\n", "\n");
                                zhConfig.Add(strs[0], str);
                            }
                            else
                            {
                                zhConfig.Add(strs[0], strs[1]);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        void SaveConfig()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kv in zhConfig)
            {
                if (kv.Value.Contains("\n"))
                {
                    string str = kv.Value.Replace("\n", "\\n");
                    sb.AppendLine($"{kv.Key}={str}");
                }
                else
                {
                    sb.AppendLine($"{kv.Key}={kv.Value}");
                }
            }
            File.WriteAllText($"{Paths.ConfigPath}/VRoidChinese.cfg", sb.ToString());
        }

        /// <summary>
        /// 添加中文选项补丁
        /// </summary>
        public static bool WelcomeController_LanguagePreferencesDropdownItemList_Get_Prefix(ref KeyValuePair<string, string>[] __result)
        {
            __result = new KeyValuePair<string, string>[]
            {
                new KeyValuePair<string, string>("en", "GUI.Welcome.Preferences.Language.English"),
                new KeyValuePair<string, string>("ja", "GUI.Welcome.Preferences.Language.Japanese"),
                new KeyValuePair<string, string>("zh", "GUI.Welcome.Preferences.Language.Chinese")
            };
            return false;
        }

        /// <summary>
        /// 中文选项刷新索引补丁
        /// </summary>
        public static IEnumerable<CodeInstruction> WelcomeController_UpdateLanguagePreferencesWithIndex_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);
            codes[8].opcode = OpCodes.Ldc_I4_3;
            return codes.AsEnumerable();
        }

        /// <summary>
        /// 添加语言数据
        /// </summary>
        private void AddLanguageDictToVRoidStudio()
        {
            var dict = Traverse.Create(TranslatorType).Field("dictionaries").GetValue<Dictionary<string, Dictionary<string, string>>>();
            Dictionary<string, string> zh = new Dictionary<string, string>();
            foreach (var kv in zhConfig)
            {
                zh.Add(kv.Key, kv.Value);
            }
            dict.Add("zh", zh);
        }

        /// <summary>
        /// 修改标题
        /// </summary>
        public static bool WindowTitle_Update_Prefix(ref string title)
        {
            title += " 中文汉化 By xiaoye97";
            return true;
        }
    }
}
