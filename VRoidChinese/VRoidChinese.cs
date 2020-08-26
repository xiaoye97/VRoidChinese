using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Linq;
using BepInEx.Configuration;
using System.Reflection.Emit;
using System.Collections.Generic;

namespace VRoidChinese
{
    [BepInPlugin("VRoid.Chinese", "VRoid汉化插件", "1.2")]
    public class VRoidChinese : BaseUnityPlugin
    {
        //中文配置
        public static Dictionary<string, ConfigEntry<string>> zhDict = new Dictionary<string, ConfigEntry<string>>();
        public static Harmony harmony;
        public static Type TranslatorType, WelcomeControllerType;

        void Start()
        {
            //初始化
            TranslatorType = AccessTools.TypeByName("Translator");
            WelcomeControllerType = AccessTools.TypeByName("WelcomeController");
            harmony = new Harmony("VRoid.Chinese");
            //与配置文件进行配对
            var enDict = Traverse.Create(TranslatorType).Field("enDictionary").GetValue<Dictionary<string, string>>();
            enDict.Add("GUI.Welcome.Preferences.Language.Chinese", "中文");
            foreach (var k in enDict.Keys)
            {
                var config = Config.Bind<string>("翻译", k, enDict[k]);
                zhDict.Add(k, config);
            }
            //添加中文选项
            harmony.Patch(WelcomeControllerType.GetProperty("LanguagePreferencesDropdownItemList").GetGetMethod(), prefix: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WelcomeController_LanguagePreferencesDropdownItemList_Get_Prefix")));
            //修正语言选项刷新索引上限
            harmony.Patch(AccessTools.Method(WelcomeControllerType, "UpdateLanguagePreferencesWithIndex"), transpiler: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WelcomeController_UpdateLanguagePreferencesWithIndex_Transpiler")));
            //设置标题
            harmony.Patch(AccessTools.Method(AccessTools.TypeByName("WindowTitle"), "Update"), prefix: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WindowTitle_Update_Prefix")));
            //刷新数据
            RefreshLanguage();
            //设定当前语言为中文
            Traverse.Create(GameObject.FindObjectOfType(WelcomeControllerType)).Method("OnLanguagePreferencesDropdownValueChanged", 2).GetValue();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                Logger.LogInfo("刷新语言数据");
                RefreshLanguage();
            }
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
        /// 刷新语言数据
        /// </summary>
        private void RefreshLanguage()
        {
            var dict = Traverse.Create(TranslatorType).Field("dictionaries").GetValue<Dictionary<string, Dictionary<string, string>>>();
            Dictionary<string, string> editCache = new Dictionary<string, string>();
            if (dict.ContainsKey("zh"))
            {
                foreach (var kv in dict["zh"])
                {
                    if (zhDict.ContainsKey(kv.Key) && kv.Value != zhDict[kv.Key].Value)
                    {
                        editCache.Add(kv.Key, zhDict[kv.Key].Value);
                    }
                }
                foreach (var kv in editCache)
                {
                    dict["zh"][kv.Key] = kv.Value;
                }
            }
            else
            {
                Dictionary<string, string> zh = new Dictionary<string, string>();
                foreach (var kv in zhDict)
                {
                    zh.Add(kv.Key, kv.Value.Value);
                }
                dict.Add("zh", zh);
            }
        }

        public static bool WindowTitle_Update_Prefix(ref string title)
        {
            title += " 中文汉化 By xiaoye97";
            return true;
        }
    }
}
