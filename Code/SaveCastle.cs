using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

namespace NobleLife
{
    public class SaveCastle
    {
        public static List<CastleData> castleDataList = new List<CastleData>();
        public static void Awake()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(SaveManager), "saveWorldToDirectory"); // update
            patch = AccessTools.Method(typeof(SaveCastle), "saveWorldToDirectory_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(MapBox), "finishMakingWorld"); // loadWorld
            patch = AccessTools.Method(typeof(SaveCastle), "finishWorld_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(SaveManager), "startLoadSlot"); // update
            patch = AccessTools.Method(typeof(SaveCastle), "startLoadSlot_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(BuildingManager), "loadObject"); //
            patch = AccessTools.Method(typeof(SaveCastle), "loadBuildingObject_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(MapIconLibrary), "checkBuildingLights"); //
            patch = AccessTools.Method(typeof(SaveCastle), "checkBuildingLights_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));
        }
        public static bool checkBuildingLights_Prefix(Building pBuilding, Color pColor)
        {
            if (pBuilding.hasAnyStatusEffect())
            {
                foreach (StatusEffectData statusEffectData in pBuilding.activeStatus_dict.Values)
                {
                    if (statusEffectData.asset.draw_light_area)
                    {
                        MapIconLibrary.showLightAt(pBuilding.currentPosition, pColor, statusEffectData.asset.draw_light_size);
                    }
                }
            }
            if (!pBuilding.asset.draw_light_area)
            {
                return false;
            }
            if (!pBuilding.asset.draw_light_area)
            {
                return false;
            }
            if (!pBuilding.isUsable())
            {
                return false;
            }
            Vector3 v = pBuilding.currentPosition;
            v.x += pBuilding.asset.draw_light_area_offset_x;
            v.y += pBuilding.asset.draw_light_area_offset_y;
            MapIconLibrary.showLightAt(v, pColor, pBuilding.asset.draw_light_size);
            return false;
        }
        public static bool startLoadSlot_Prefix()
        {
            Castle.castleList.Clear();
            SaveCastle.castleDataList.Clear();
            // Debug.Log("CLEAR CASTLE LIST");
            return true;
        }
        public static void prepareSave()
        {
            foreach (var castle in Castle.castleList.Values)
            {
                castle.data.mainCity_id = castle.mainCity.data.id;
                castle.data.gateBottom = castle.gateBottom.data;
                castle.data.leftcorner = castle.leftcorner.data;
                castle.data.rightcorner = castle.rightcorner.data;
                castle.data.horizontalWall = castle.horizontalWall.data;
                // castle.data.curHealth = castle.data.curHealth;
                castleDataList.Add(castle.data);
            }
        }
        public static void saveWorldToDirectory_Postfix(string pFolder, bool pCompress = true, bool pCheckFolder = true)
        {
            try
            {
                prepareSave();
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = File.Create(pFolder + "nobleLife.wbox");
                formatter.Serialize(fileStream, castleDataList);
                fileStream.Close();
            }
            catch(Exception e) 
            {
                Debug.LogError("Error working on saving stuff " + e);
            }
        }
        public static bool loadBuildingObject_Prefix(ref Building __result, BuildingData pData, Building pPrefab = null)
        {
            // soon this function would be obsolete in the future when everyone get their own save file
            // this would be a little patch for now
            string filePath = SaveManager.currentSavePath + "nobleLife.wbox";
            if (File.Exists(filePath))
            {
                // Debug.Log("File save exists: " + filePath);
                // we will try to maybe create one or do something in case of suck things happen
                return true;
            }
            if (!Castle_Patches.isCastlePart(null, 2, pData.asset_id))
                return true;
            WorldTile tileSimple = World.world.GetTileSimple(pData.mainX, pData.mainY);
            if (pData.cityID.Equals("")) // Obviously the worst case ever, 2 options,repair it or remove it and make new one
            {
                //// Debug.Log("we got fucked up no city found");
                //var closest = World.world.cities.list.GetRandom();
                //foreach (var city in World.world.cities.list)
                //{
                //    if (Castle.castleList.ContainsKey(city)) continue;
                //    var v1 = Toolbox.DistVec3(closest.cityCenter, tileSimple.posV);
                //    var v2 = Toolbox.DistVec3(city.cityCenter, tileSimple.posV);
                //    if (v1 > v2) closest = city;
                //}
                //if (tileSimple.zone.city == null)
                //    closest.addZone(tileSimple.zone);
                //pData.cityID = closest.data.id;

                //if (!Castle.castleList.ContainsKey(closest)) Castle.castleList.Add(closest, new Castle().loadBase(tileSimple, closest));
                //var castle = Castle.castleList[closest];
                //if (pData.asset_id == "castle_gate")
                //{
                //    castle.loadBase(tileSimple, closest);
                //    var gateTile = MapBox.instance.GetTile(tileSimple.pos.x, tileSimple.pos.y + 2);
                //    gateTile.setTileTypes(TileLibrary.soil_low, TopTileLibrary.biomass_low);
                //}
                //__result = castle.loadCastlePart(pData.asset_id, tileSimple, pData);
                return false;
            }
            else // __instance is the best way to load and rebuild castle from save
            {
                var pCity = World.world.cities.get(pData.cityID);
                // loadBase on any parts which could be incorrects
                if (!Castle.castleList.ContainsKey(pCity)) Castle.castleList.Add(pCity, new Castle().loadBase(tileSimple, pCity));
                var castle = Castle.castleList[pCity];
                if (pData.asset_id == "castle_gate")
                {
                    //Debug.Log("Open da gate of " + pCity.name);
                    castle.loadBase(tileSimple, pCity); // so we correct it on the right part
                    // nah god
                    var gateTile = MapBox.instance.GetTile(tileSimple.pos.x, tileSimple.pos.y + 2);
                    gateTile.setTileTypes(TileLibrary.soil_low, TopTileLibrary.biomass_low);
                }
                __result = castle.loadCastlePart(pData.asset_id, tileSimple, pData);
                //Debug.Log(pData.asset_id + " of " + World.world.cities.get(pData.cityID).name);
            }
            return false;
            //return true; // use __instance when things broken and house not spawn ok
        }
        public static void finishWorld_Postfix() // we can change the way we name the file using the mod version to avoid conflict and many things else
        {
            string filePath = SaveManager.currentSavePath + "nobleLife.wbox";
            if (!File.Exists(filePath))
            {
                // Debug.Log("File save not exists: " + filePath);
                // we will try to maybe create one or do something in case of suck things happen
                return;
            }
            Debug.Log("Try loading few more stuffs at the moment");
            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                FileStream fileStream = File.OpenRead(filePath);
                castleDataList = (List<CastleData>)formatter.Deserialize(fileStream); // read and cast

                foreach(var data in castleDataList)
                {
                    var pCity = World.world.cities.get(data.mainCity_id);
                    if (pCity == null) continue;
                    if (Castle.castleList.ContainsKey(pCity)) continue;
                    var newCastle = new Castle().loadFromData(data);
                    if (newCastle != null)
                        Castle.castleList.Add(pCity, newCastle);
                    // else Debug.Log("Lost one castle");
                }
                // Debug.Log("We have read like " + castleDataList.Count() + " castle datas");
            }
            catch(Exception e)
            {
                Debug.LogError("Error loading stuff " + e);
                Debug.Log("There's error while loading stuff you should remove this map I'm sorry the mod still in development");
                // return to load object in a systematic way
            }
        }
    }
}