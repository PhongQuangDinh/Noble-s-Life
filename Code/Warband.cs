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

namespace NobleLife
{
    public class Warband
    {
        public static void GoToActorTarget_Postfix(Actor pActor, ref BehResult __result, BehGoToActorTarget __instance)
        {
            if (!Castle_Patches.isAtOwnCity(pActor)) return ;
            if (!Castle.castleList.ContainsKey(pActor.city)) return ;
            var castle = Castle.castleList[pActor.city];
            if (castle.Alert && !castle.GateClosed)
            {
                if (pActor.goTo(Castle.getInfantryPosRand(castle), __instance.pathOnWater, false) == ExecuteEvent.True)
                {
                    __result = BehResult.Continue;
                }
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
                        //__instance.goTo(Castle.getInfantryPosRand(castle)); // it takes more time to close gate if I do this
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
            if (__instance.isUsingPath())
            {
                __instance.updatePathMovement();
                __instance.beh_skip = true;
            }
            return false;
        }
        public static void findEnemyObjectTarget_Postfix(BaseSimObject __instance, ref BaseSimObject __result)
        {
            if (__instance.currentTile.zone.city == null || __instance.currentTile.zone.city == __instance.city)
                return;
            if (!Castle.castleList.ContainsKey(__instance.currentTile.zone.city))
                return;
            var city = __instance.currentTile.zone.city;
            var castle = Castle.castleList[city];
            if (castle.GateClosed && !castle.GateBroken && __result == null)
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