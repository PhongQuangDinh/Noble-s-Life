using ai.behaviours;
using ai;
using ai.behaviours.conditions;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace NobleLife
{
    public class Warband
    {
        public static void Awake()
        {
            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(City), "finishCapture"); //
            patch = AccessTools.Method(typeof(Warband), "finishCapture_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            //harmony = new Harmony(pluginGuid); // within warband
            //original = AccessTools.Method(typeof(Actor), "checkEnemyTargets"); // consider fixing
            //patch = AccessTools.Method(typeof(Warband), "checkEnemyTargets_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid); // within warband
            original = AccessTools.Method(typeof(Actor), "b5_checkPathMovement"); // update
            patch = AccessTools.Method(typeof(Warband), "b5_checkPathMovement_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            //harmony = new Harmony(pluginGuid); // within warband
            //original = AccessTools.Method(typeof(Actor), "behaviourActorTargetCheck"); // still under fixing
            //patch = AccessTools.Method(typeof(Warband), "behaviourActorTargetCheck_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));

            //harmony = new Harmony(main.pluginGuid); // within warband
            //original = AccessTools.Method(typeof(BehFightCheckEnemyIsOk), "execute"); //
            //patch = AccessTools.Method(typeof(Warband), "BehFightCheckEnemyIsOk_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(BehGoToActorTarget), "execute"); //
            patch = AccessTools.Method(typeof(Warband), "GoToActorTarget_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(BaseSimObject), "findEnemyObjectTarget"); //
            patch = AccessTools.Method(typeof(Warband), "findEnemyObjectTarget_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(BuildingRenderer), "drawBuilding"); //
            patch = AccessTools.Method(typeof(Warband), "drawBuilding_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(Building), "isUnderConstruction"); //
            patch = AccessTools.Method(typeof(Warband), "isUnderConstruction_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(MapIconLibrary), "drawShadowsBuildings"); //
            patch = AccessTools.Method(typeof(Warband), "drawShadowsBuildings_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(main.pluginGuid);
            original = AccessTools.Method(typeof(MapIconLibrary), "drawBuildingsLightWindows"); //
            patch = AccessTools.Method(typeof(Warband), "drawBuildingsLightWindows_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            //harmony = new Harmony(main.pluginGuid);
            //original = AccessTools.Method(typeof(BehaviourTaskActorLibrary), "init"); // add some more tasks
            //patch = AccessTools.Method(typeof(Warband), "BehaviourTaskActorLibrary_Postfix");
            //harmony.Patch(original, null, new HarmonyMethod(patch));

            //harmony = new Harmony(main.pluginGuid);
            //original = AccessTools.Method(typeof(City), "addNewUnit"); //
            //patch = AccessTools.Method(typeof(Warband), "addNewUnit_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));

            BehaviourTaskActorLibrary_Postfix();
        }

        //public static bool addNewUnit_Prefix(City __instance, Actor pActor, bool pSetKingdom = true)
        //{
        //    pActor.setCity(__instance);
        //    if (pActor.asset.isBoat)
        //    {
        //        __instance.boats.Add(pActor);
        //    }
        //    else
        //    {
        //        __instance.units.Add(pActor);
        //        __instance._dirty_units = true;
        //    }
        //    __instance.setStatusDirty();
        //    AchievementLibrary.achievementMegapolis.check(__instance);
        //    if (!pSetKingdom)
        //        return false;
        //    pActor.setKingdom(__instance.kingdom);
        //    return false;
        //}
        public static void BehaviourTaskActorLibrary_Postfix(/*BehaviourTaskActorLibrary __instance*/)
        {
            BehaviourTaskActor behaviourTaskActor = new BehaviourTaskActor();
            behaviourTaskActor.id = "retreat_to_Castle";
            behaviourTaskActor.ignoreFightCheck = true;
            BehaviourTaskActor pAsset = behaviourTaskActor;
            AssetManager.tasks_actor.t = behaviourTaskActor;
            AssetManager.tasks_actor.add(pAsset);
            AssetManager.tasks_actor.t.addBeh((BehaviourActionActor)new BehRetreatCastle());
            AssetManager.tasks_actor.t.addBeh((BehaviourActionActor)new BehGoToTileTarget());
            AssetManager.tasks_actor.t.addBeh((BehaviourActionActor)new BehRestartTask()); // idk if __instance is a good idea
            // Debug.Log("Successfully added a new task");
        }
        public static bool finishCapture_Prefix(City __instance, Kingdom pKingdom)
        {
            __instance.clearCapture();
            __instance.recalculateNeighbourCities();
            using (ListPool<War> wars = pKingdom.getWars())
            {
                Kingdom kingdom = __instance.findKingdomToJoinAfterCapture(pKingdom, wars);
                if (!__instance.checkRebelWar(kingdom, wars))
                {
                    kingdom.data.timestamp_new_conquest = World.world.getCurWorldTime();
                }
                __instance.joinAnotherKingdom(kingdom);
            }
            // make them share half or one third of the army maybe, still under development
            float min = 1000000f;
            Actor invader = null;
            foreach (var city in pKingdom.cities)
            {
                if (city.army == null) continue;
                Actor general = city.army.groupLeader;
                if (general != null && general.data.alive)
                {
                    float dist = Toolbox.DistVec3(general.currentPosition, __instance.cityCenter);
                    if (dist < min)
                    {
                        min = dist;
                        invader = general;
                    }
                }
            }
            if (__instance.army == null) return false; // I should have created but I refuse for better future
            int amount = invader.unit_group.countUnits() / 3;
            foreach (var men in invader.unit_group.units)
            {
                if (men == null || !men.data.alive) continue;
                if (amount > 0) // bruh problem around here
                {
                    invader.unit_group.removeUnit(men);
                    men.removeFromCity();
                    //invader.city.removeCitizen(men);
                    men.joinCity(__instance);
                    //men.setCity(__instance);
                    //__instance.addNewUnit(men);

                    //men.setCulture(__instance.getCulture());
                    //__instance.professionsDict[UnitProfession.Warrior].Add(men);
                    __instance.army.addUnit(men);
                    amount--;
                }
                else break;
            }
            return false;
        }
        // this one cause pausing the people somehow
        public static void GoToActorTarget_Postfix(Actor pActor, ref BehResult __result, BehGoToActorTarget __instance)
        {
            if (!Castle_Patches.isAtOwnCity(pActor)) return ;
            if (!Castle.castleList.ContainsKey(pActor.city)) return ;
            var castle = Castle.castleList[pActor.city];
            if (castle.Alert && !castle.GateClosed)
            {
                if (!pActor.isProfession(UnitProfession.Warrior) && castle.data.sameRaceWar)
                    return;
                pActor.goTo(Castle.getInfantryPosRand(castle), __instance.pathOnWater);
                //if (pActor.goTo(Castle.getInfantryPosRand(castle), __instance.pathOnWater, false) == ExecuteEvent.True)
                //{
                //    pActor.ignoreTarget(pActor.beh_actor_target);
                //    __result = BehResult.Stop;
                //}
                pActor.ignoreTarget(pActor.beh_actor_target);
                __result = BehResult.Stop;
            }
        }
        public static bool BehFightCheckEnemyIsOk_Prefix(ref BehResult __result, Actor pActor) // the key point
        {
            if (pActor == null)
                return false;
            if (pActor.city == null)
                return true;
            if (!Castle.castleList.ContainsKey(pActor.city))
                return true;
            var castle = Castle.castleList[pActor.city];
            if (castle.Alert && !castle.GateClosed)
            {
                string txt = castle.insideCastle(pActor) ? "inside" : "outside";
                Debug.Log("Stop attack, " + txt);
                pActor.clearAttackTarget();
                __result = BehResult.Stop;
                return false;
            }
            return true;
        }
        public static bool checkEnemyTargets_Prefix(Actor __instance, ref bool __result)
        {
            if (__instance.city != null && !Castle.castleList.ContainsKey(__instance.city)) return true;
            if (!Castle_Patches.isAtOwnCity(__instance))
                return true; // or do the attack castle here for attackers
            var castle = Castle.castleList[__instance.city];
            if (!castle.Alert) return true; // for defender
            if (!castle.insideCastle(__instance) && !castle.GateClosed && !castle.GateBroken)
            {
                /*__instance._timeout_targets = 0.0f;
                __instance.attackTarget = (BaseSimObject)null;*/
                if (!Castle.isCastleTile(__instance.tileTarget, castle))
                {
                    __instance.current_path.Clear();
                    __instance.setTileTarget(Castle.getInfantryPosRand(castle));
                }
                __result = false;
                return false;
            }
            if (!castle.GateClosed && !castle.GateBroken)
            {
                /*__instance._timeout_targets = 0.0f;
                __instance.attackTarget = (BaseSimObject)null;*/
                __result = false;
                return false;
            }
            __result = false;
            return true;
        }
        public static bool behaviourActorTargetCheck_Prefix(Actor __instance, ref bool __result)
        {
            if (__instance.city != null && !Castle.castleList.ContainsKey(__instance.city)) return true;
            if (!Castle_Patches.isAtOwnCity(__instance))
                return true; // or do the attack castle here for attackers
            var castle = Castle.castleList[__instance.city];
            if (!castle.Alert) return true; // for defender
            if (!castle.insideCastle(__instance) && !castle.GateClosed && !castle.GateBroken)
            {
                __instance._timeout_targets = 0.0f;
                __instance.attackTarget = (BaseSimObject)null;
                if (!Castle.isCastleTile(__instance.tileTarget, castle))
                {
                    __instance.current_path.Clear();
                    __instance.setTileTarget(Castle.getInfantryPosRand(castle));
                }
                __result = false;
                return false;
            }
            if (!castle.GateClosed && !castle.GateBroken)
            {
                __instance._timeout_targets = 0.0f;
                __instance.attackTarget = (BaseSimObject)null;
                __result = false;
                return false;
            }
            __result = false;
            return true;
        }
        public static bool b5_checkPathMovement_Prefix(float pElapsed, Actor __instance)
        {
            if (main.controlledActor != null && main.controlledActor == __instance)
            {
                return false;
            }
            if (__instance.update_done)
            {
                return false;
            }
            if (__instance.beh_skip)
            {
                return false;
            }
            if (Castle_Patches.isAtOwnCity(__instance) && Castle.castleList.ContainsKey(__instance.city))
            {
                var castle = Castle.castleList[__instance.city];
                if (castle.data.sameRaceWar && !__instance.isProfession(UnitProfession.Warrior))
                    goto keepmoving;
                if (castle.Alert && !castle.GateClosed && !castle.GateBroken) //if I move the castle.GateClosed to somewhere inside,
                                                                              //could cause slow gate close
                {
                    if (!castle.insideCastle(__instance))
                    {
                        if (!Castle.isCastleTile(__instance.tileTarget, castle))
                        {
                            __instance.current_path.Clear();
                            __instance.setTileTarget(Castle.getInfantryPosRand(castle));
                        }
                        //__instance.goTo(Castle.getInfantryPosRand(castle)); // it takes more time to close gate if I do __instance
                        //if (!castle.GateClosed)
                        //{
                               
                        //}
                        //else
                        //{
                        //    if (Toolbox.DistTile(__instance.currentTile, castle.mainTile) < 1f)
                        //        __instance.currentTile = Castle.getInfantryPosRand(castle);
                        //    if (!Castle.isCastleTile(__instance.tileTarget, castle))
                        //    {
                        //        __instance.current_path.Clear();
                        //        __instance.setTileTarget(castle.mainTile);
                        //    }
                        //}
                    }
                    else
                    {
                        if (__instance.is_group_leader)
                        {
                            if (!castle.GateClosed && !castle.GateBroken) castle.CloseGate();
                            else if (castle.GateClosed) castle.OpenGate();
                        }
                        else if (__instance.isProfession(UnitProfession.Leader) && __instance.city.army != null && castle.insideCastle(__instance.city.army.groupLeader))
                        {
                            if (!Castle_Patches.isAtOwnCity(__instance.city.army.groupLeader))
                            {
                                if (!castle.GateClosed && !castle.GateBroken) castle.CloseGate();
                                else if (castle.GateClosed) castle.OpenGate();
                            }
                            else if (castle.insideCastle(__instance.city.army.groupLeader))
                            {
                                if (!castle.GateClosed && !castle.GateBroken) castle.CloseGate();
                                else if (castle.GateClosed) castle.OpenGate();
                            }
                        }
                        else if (__instance.isProfession(UnitProfession.Warrior))
                        {
                            // do something to defend for example standing on the tile wall
                            //__instance.currentTile = castle.getDefendingTile(2);
                            //__instance.zPosition.y = 3;
                        }
                    }
                }
            }
        keepmoving:
            if (__instance.isUsingPath())
            {
                __instance.updatePathMovement();
                __instance.beh_skip = true;
            }
            return false;
        }
        // IMPORTANT FUNCTION FOR THE SIEGE
        public static void findEnemyObjectTarget_Postfix(BaseSimObject __instance, ref BaseSimObject __result)
        {
            // first we indicate attacker and defender here, different purpose different target
            if (Castle_Patches.isAtOwnCity(__instance.a))
            {
                // for defenders
                if (!Castle.castleList.ContainsKey(__instance.city))
                    return;
                var castle_def = Castle.castleList[__instance.city];
                if (!castle_def.insideCastle(__instance.a))
                {
                    // __result = null; // I still havent fixed the god damn chase to enemy position and he stop for nothing
                    return;
                }
                // else choose the target under the wall and bla bla bla
                // __result = castle_def.gateTile._units.GetRandom();
                return;
            }
            if (__instance.currentTile.zone.city == null)
                return;
            // for attackers
            if (!Castle.castleList.ContainsKey(__instance.currentTile.zone.city))
                return;
            var city = __instance.currentTile.zone.city;
            var castle = Castle.castleList[city];
            if (castle.GateClosed && !castle.GateBroken)
                __result = castle.gateBottom;
        }
        public static bool drawBuilding_Prefix(Building pBuilding)
        {
            if (pBuilding.asset == null || pBuilding.kingdom == null || pBuilding.data == null)
                return false;
            return true;
        }
        public static bool isUnderConstruction_Prefix(ref bool __result, Building __instance)
        {
            if (__instance.asset == null)
            {
                __result = false;
                return false;
            }
            __result = !(__instance.asset.sprites.construction == null) && __instance.data.hasFlag(S.under_construction);
            return false;
        }
        public static bool drawShadowsBuildings_Prefix(MapIconAsset pAsset)
        {
            if (!Config.shadowsActive)
            {
                return false;
            }
            bool flag = World.world.qualityChanger.renderBuildings;
            if (World.world.camera.orthographicSize > World.world.qualityChanger.getZoomRateShadows())
            {
                flag = false;
            }
            if (!flag)
            {
                return false;
            }
            List<Building> visible_buildings = World.world.buildings.visible_buildings;
            if (visible_buildings.Count == 0)
            {
                return false;
            }
            for (int i = 0; i < visible_buildings.Count; i++)
            {
                Building building = visible_buildings[i];
                if (building.asset == null) continue;
                if (building.asset.shadow && !building.chopped && building.data != null)
                {
                    Sprite spriteBuildingShadow = UnitSpriteConstructor.getSpriteBuildingShadow(building, building.data, building.last_main_sprite);
                    MapMark next = pAsset.group_system.getNext();
                    next.set(ref building.currentPosition, ref building.currentScale);
                    next.setSprite(spriteBuildingShadow);
                }
            }
            UnitSpriteConstructor.checkDirty();
            return false;
        }
        public static bool drawBuildingsLightWindows_Prefix(MapIconAsset pAsset)
        {
            if (!World.world.qualityChanger.renderBuildings)
            {
                return false;
            }
            if (!World.world.eraManager.shouldShowLights())
            {
                return false;
            }
            if (!PlayerConfig.optionBoolEnabled("night_lights"))
            {
                return false;
            }
            Color white = Color.white;
            if (Toolbox.randomBool())
            {
                white.a = 0.95f;
            }
            else
            {
                white.a = 1f;
            }
            List<Building> visible_buildings = World.world.buildings.visible_buildings;
            for (int i = 0; i < visible_buildings.Count; i++)
            {
                Building building = visible_buildings[i];
                if (building.asset != null && building.asset.cityBuilding && building.isUsable())
                {
                    Sprite spriteBuildingLight = UnitSpriteConstructor.getSpriteBuildingLight(building);
                    if (!(spriteBuildingLight == null))
                    {
                        Vector3 curTransformPosition = building.curTransformPosition;
                        curTransformPosition.z = -0.59000003f;
                        MapIconLibrary.drawMark(pAsset, curTransformPosition, null, null, null, null, 1f, false, building.last_scale_y).setSprite(spriteBuildingLight);
                    }
                }
            }
            return false;
        }
    }
}