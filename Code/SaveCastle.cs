using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NobleLife
{
    public class SaveCastle
    {
        public static void Awake()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(SaveManager), "clickSaveSlot"); // update
            patch = AccessTools.Method(typeof(SaveCastle), "clickSaveSlot_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(SaveManager), "startLoadSlot"); // update
            patch = AccessTools.Method(typeof(SaveCastle), "startLoadSlot_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
        }
        public static bool startLoadSlot_Prefix()
        {
            Castle.castleList.Clear();
            return true;
        }
        public static void clickSaveSlot_Prefix()
        {
            
        }
    }
}
