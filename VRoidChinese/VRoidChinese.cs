using System;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;

namespace VRoidChinese
{
    [BepInPlugin("me.xiaoye97.plugin.VRoid.Chinese", "VRoid汉化插件", "1.0")]
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
            harmony = new Harmony("me.xiaoye97.plugin.VRoid.Chinese");
            //与配置文件进行配对
            var enDict = Traverse.Create(TranslatorType).Field("enDictionary").GetValue<Dictionary<string, string>>();
            enDict.Add("GUI.Welcome.Preferences.Language.Chinese", "简体中文");
            foreach (var k in enDict.Keys)
            {
                var config = Config.Bind<string>("翻译", k, enDict[k]);
                zhDict.Add(k, config);
            }
            //添加中文选项
            harmony.Patch(WelcomeControllerType.GetProperty("LanguagePreferencesDropdownItemList").GetGetMethod(), prefix: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WelcomeController_LanguagePreferencesDropdownItemList_Get_Prefix")));
            //修正语言选项刷新索引上限
            harmony.Patch(AccessTools.Method(WelcomeControllerType, "UpdateLanguagePreferencesWithIndex"), prefix: new HarmonyMethod(typeof(VRoidChinese).GetMethod("WelcomeController_UpdateLanguagePreferencesWithIndex_Prefix")));
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
            //Debug.Log("触发WelcomeController_LanguagePreferencesDropdownItemList_Get_Prefix");
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
        public static bool WelcomeController_UpdateLanguagePreferencesWithIndex_Prefix(object __instance, int index)
        {
            //Debug.Log("触发WelcomeController_UpdateLanguagePreferencesWithIndex_Prefix");
            if (index == Traverse.Create(__instance).Field("languageDropdownIndex").GetValue<int>() || index < 0 || index >= 3)
            {
                return false;
            }
            Traverse.Create(__instance).Field("languageDropdownIndex").SetValue(index);
            Type UserPreferencesManagerType = AccessTools.TypeByName("UserPreferencesManager");
            object ins = Traverse.Create(UserPreferencesManagerType).Property("Instance").GetValue();
            object loc = Traverse.Create(ins).Field("Application").Property("Localization").GetValue();
            Traverse.Create(loc).Property("Language").SetValue(Traverse.Create(WelcomeControllerType).Property("LanguagePreferencesDropdownItemList").GetValue<KeyValuePair<string, string>[]>()[index].Key);
            Traverse.Create(TranslatorType).Method("UpdateCurrentUICulture").GetValue();
            Traverse.Create(__instance).Field("welcomeTranslation").Method("ApplyTranslation").GetValue();
            Traverse.Create(__instance).Method("UpdateSampleList").GetValue();
            Traverse.Create(__instance).Method("UpdateOpenList").GetValue();
            return false;
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
