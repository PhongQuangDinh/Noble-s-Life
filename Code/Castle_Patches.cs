using ai;
using ai.behaviours;
using HarmonyLib;
using pathfinding;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.Diagnostics;

namespace NobleLife
{
    public class Castle_Patches
    {
        public static bool generateNewMap_Prefix(bool pClear = false)
        {
            Castle.castleList.Clear();
            return true;
        }
        public static bool removeZone_Prefix(TileZone pZone, City __instance)
        {
            if (!Castle.castleList.ContainsKey(__instance)) return true;
            var castle = Castle.castleList[__instance];
            if (pZone.buildings.Contains(castle.gateBottom) || pZone.buildings.Contains(castle.leftcorner) ||
                pZone.buildings.Contains(castle.horizontalWall) || pZone.buildings.Contains(castle.rightcorner))
                return false;
            return true;
        }
        public static void canAttackTarget_Postfix(BaseSimObject pTarget, ref bool __result, BaseSimObject __instance)
        {
            if (pTarget.isActor())
            {
                if (pTarget.city != null && Castle.castleList.ContainsKey(pTarget.city))
                {
                    var castle = Castle.castleList[pTarget.city];
                    if (castle.insideCastle(pTarget.a) && castle.GateClosed) // break through the gate to kill defenders
                    {
                        __result = false;
                        return;
                    }
                    else return;
                }
            }
            if (pTarget.isBuilding() && Castle.canAttackCastle(pTarget.b))
                __result = false;
            else if (pTarget.isBuilding() && !Castle.canAttackCastle(pTarget.b) && pTarget.b.asset.id == "castle_gate")
                if (__instance.a != null && __instance.a.s_attackType == WeaponType.Range)
                    __result = false;
        }
        public static bool isCastlePart(Building check, int mode = 1, string id = null)
        {
            if (mode == 1)
            {
                if (check.asset.id != "castle_gate" && check.asset.id != "castle_topleftcorner" &&
                    check.asset.id != "castle_toprightcorner" && check.asset.id != "castle_topwall")
                    return false;
                return true;
            }
            else if (id != null)
            {
                if (id != "castle_gate" && id != "castle_topleftcorner" &&
                    id != "castle_toprightcorner" && id != "castle_topwall")
                    return false;
                return true;
            }
            return false;
        }
        public static bool abandonBuilding_Prefix(Building pBuilding, City __instance)
        {
            if (!isCastlePart(pBuilding)) return true;
            return false;
        }
        public static bool drawDemolish_Prefix(WorldTile pTile, string pPowerID)
        {
            if (pTile.building != null && !pTile.building.asset.ignoreDemolish && pTile.building.asset.buildingType == BuildingType.None &&
                isCastlePart(pTile.building) && pTile.building.isAnimationState(BuildingAnimationState.Normal) && pTile.zone.city != null &&
                Castle.castleList.ContainsKey(pTile.zone.city))
            {
                var castle = Castle.castleList[pTile.zone.city];
                var city = pTile.zone.city;
                castle.destroy_castle();
                Castle.castleList.Remove(city);
                return false;
            }
            return true;
        }
        public static bool startRemove_Prefix(Building __instance) // hmm
        {
            if (!isCastlePart(__instance)) return true;
            return false;
        }
        public static bool startDestroyBuilding_Prefix(Building __instance)
        {
            if (!isCastlePart(__instance))
                return true;
            if (__instance.isAnimationState(BuildingAnimationState.OnRemove))
                return false;
            if ( __instance.city != null && Castle.castleList.ContainsKey(__instance.city)) // destroy when castle is alive
            {
                var castle = Castle.castleList[__instance.city];
                Castle.destroy_physicWall(castle.mainTile);
                Castle.destroy_VisibleWall(castle.mainTile);
                Castle.castleList.Remove(__instance.city);
            }
            if (__instance.isState(BuildingState.Ruins) && __instance.city == null)
            {
                var mainTile = FindMainTileCastle(__instance);
                Castle.destroy_physicWall(mainTile);
                Castle.destroy_VisibleWall(mainTile);
                // hmmm
                return false;
            }
            if (__instance.city == null || !Castle.castleList.ContainsKey(__instance.city)) 
                return true;
            //if (!Castle.castleList[__instance.city].Alive)
            //{
            //    __instance.clearCity();
            //    __instance.setState(BuildingState.Ruins);
            //    __instance.setAnimationState(BuildingAnimationState.OnRuin);
            //    __instance.setKingdom(World.world.kingdoms.getKingdomByID(SK.ruins));
            //    return false;
            //}
            return false;
        }
        public static WorldTile FindMainTileCastle(Building __instance)
        {
            var curtile = __instance.currentTile;
            WorldTile mainTile = null;
            if (__instance.asset.type == "castle_gate") mainTile = curtile;
            if (__instance.asset.type == "leftcorner") mainTile = MapBox.instance.GetTile(curtile.pos.x + 4, curtile.pos.y - 5);
            if (__instance.asset.type == "rightcorner") mainTile = MapBox.instance.GetTile(curtile.pos.x - 4, curtile.pos.y - 5);
            if (__instance.asset.type == "wall") mainTile = MapBox.instance.GetTile(curtile.pos.x, curtile.pos.y - 9);
            return mainTile;
        }
        public static bool BuildingGetHit_Prefix(Building __instance, float pDamage, bool pFlash = true, AttackType pType = AttackType.Other, 
            BaseSimObject pAttacker = null, bool pSkipIfShake = true, bool pMetallicWeapon = false)
        {
            if (!isCastlePart(__instance))
                return true;
            if (__instance.isAnimationState(BuildingAnimationState.OnRuin))
            {
                var mainTile = FindMainTileCastle(__instance);
                Castle.destroy_physicWall(mainTile);
                Castle.destroy_VisibleWall(mainTile);
                return false;
            }
            //if (__instance.city != null && __instance.city.data.alive)
            //    Debug.Log(__instance.city.data.name + " " + __instance.asset.id + " : " + __instance.data.health);
            if (__instance.asset.id == "castle_gate")
            {
                if (__instance.data.health > 0)
                {
                    __instance.data.health += -(int)pDamage;//(__instance.city != null && __instance.city.army != null) ? (__instance.city.army.countUnits() - (int)pDamage) : (-(int)pDamage);
                    if (__instance.data.health > __instance.stats[S.health]) __instance.data.health = (int)__instance.stats[S.health];
                    if (pType == AttackType.Weapon && __instance.asset.sound_hit != string.Empty)
                        MusicBox.playSound(__instance.asset.sound_hit, __instance.currentTile, false, true);
                }
                else Debug.Log(__instance.city.name + " gate is breached through by attackers");
            }
            else if (__instance.city != null) // this is where siege engine show their power
            {
                var castle = Castle.castleList[__instance.city];
                //WorldTip.instance.show("Castle " + __instance.city.name + " health: " + castle.curHealth, false, "top",1f);
                if (castle.data.curHealth > 0)
                    castle.data.curHealth -= (int)pDamage;
                else
                {
                    var city = __instance.city;
                    castle.setRuin_castle();
                    Castle.castleList.Remove(city);
                }
                // inflict damage to the main wall or something like that for further update
            }
            return false;
        }
        public static bool containPartOfCastle(City pCity)
        {
            if (pCity.buildings_dict_id.ContainsKey("castle_gate") || pCity.buildings_dict_id.ContainsKey("castle_topwall") ||
                pCity.buildings_dict_id.ContainsKey("castle_topleftcorner") || pCity.buildings_dict_id.ContainsKey("castle_toprightcorner"))
                return true;
            return false;
        }
        public static void updateCity_Postfix(City __instance, float pElapsed)
        {
            if (__instance == null) return;
            if (Castle.castleList.ContainsKey(__instance))
            {
                var castle = Castle.castleList[__instance];
                if (castle == null) return;
                Castle.castleList[__instance].Behaviour();
            }
            //if (containPartOfCastle(__instance) && !Castle.castleList.ContainsKey(__instance))
            //    Debug.Log("We got bug here in __instance castle " + __instance.name);
        }
        public static void buildTick_Postfix(City pCity) // always make void function if we do postfix
        {
            // make sure theres only one castle per city
            if (pCity.hasBuildingType(SB.type_hall) && !containPartOfCastle(pCity))
            {
                //Debug.Log("Castle " + pCity.getCityName() + " is built");
                var build = MapBox.instance.GetTile((int)pCity.cityCenter.x, (int)pCity.cityCenter.y);
                if (Castle.isGoodForCastleBuild(build) && pCity.getPopulationTotal() >= 60
                    && pCity.hasEnoughResourcesFor(Castle.cost))
                {
                    pCity.spendResourcesFor(Castle.cost);
                    Castle.addNewCastle(build);
                }
            }
        }
        public static void FindRandomTile_Postfix(ref BehResult __result, Actor pActor)
        {
            if (!pActor.is_group_leader || pActor.city == null)
                return;
            MapRegion mapRegion = pActor.currentTile.region;
            if (pActor.city != null && !Castle.castleList.ContainsKey(pActor.city))
                return;
            var castle = Castle.castleList[pActor.city];
            if (castle.Alert)
            {
                //Debug.Log("Captain " + pActor.city.data.name + " command to get inside castle and defend");
                //pActor.beh_tile_target = castle.centerTile;
                if (castle.insideCastle(pActor)) // ensure that the commander is inside the castle
                {
                    if (!castle.GateClosed) // by somehow that it takes a while before the commander can close the gate
                    {
                        //Debug.Log("Captain " + pActor.city.data.name + " order to close gate");
                        castle.CloseGate();
                    }
                }
                else pActor.beh_tile_target = castle.centerTile;
                __result = BehResult.Continue;
                return;
            }
            if (Toolbox.randomChance(1f) && mapRegion.tiles.Count > 0) // 100% that leader will defend at the castle on free time
            {
                if (isAtOwnCity(pActor)) // for defenders on free time
                {
                    pActor.beh_tile_target = castle.centerTile;
                    castle.OpenGate();
                    __result = BehResult.Continue;
                    return;
                }
                else if (pActor.currentTile.zone.city != null && 
                    Castle.castleList.ContainsKey(pActor.currentTile.zone.city)) // for attackers on enemy territory
                {
                    if (pActor.kingdom.isEnemy(pActor.currentTile.zone.city.kingdom))
                    {
                        var target_castle = Castle.castleList[pActor.currentTile.zone.city];
                        var targetTile = (target_castle.GateClosed) ? target_castle.mainTile : target_castle.centerTile;
                        pActor.beh_tile_target = targetTile;
                    }
                    else if (pActor.currentTile.zone.city.kingdom == pActor.kingdom &&
                        pActor.currentTile.zone.city.isInDanger()) // save other castle as well
                    {
                        var target_castle = Castle.castleList[pActor.currentTile.zone.city];
                        var targetTile = (target_castle.GateClosed) ? target_castle.mainTile : target_castle.centerTile;
                        pActor.beh_tile_target = targetTile;
                    }
                    __result = BehResult.Continue;
                    return;
                }
            }
            // the rest will start patrolling area
            if (mapRegion.neighbours.Count > 0 && Toolbox.randomBool())
                mapRegion = mapRegion.neighbours.GetRandom<MapRegion>();
            if (mapRegion.tiles.Count <= 0)
            {
               __result = BehResult.Stop;
                return;
            }
            pActor.beh_tile_target = mapRegion.tiles.GetRandom<WorldTile>();
            __result = BehResult.Continue;
        }
        public static void TileNearLeader_Postfix(ref BehResult __result, Actor pActor)
        {
            // we may give command to attack castle gate maybe
            if (pActor.unit_group == null || !pActor.unit_group.isGroupLeaderAlive())
            {
                __result = BehResult.Stop;
                return;
            }
            List<WorldTile> currentPath = pActor.unit_group.groupLeader.current_path;
            Actor leader = pActor.unit_group.groupLeader;
            WorldTile random;
            if (currentPath != null && currentPath.Count > 0) // __instance mean the leader is moving with path
            {
                //Debug.Log("Leader has path " + pActor.city.data.name);
                random = currentPath[(int)(currentPath.Count * 0.75)].region.tiles.GetRandom<WorldTile>(); // origin
                // random = leader.currentTile.region.tiles.GetRandom<WorldTile>(); // marching army with leader
            }
            else
            {
                //Debug.Log("Leader stopping " + pActor.city.data.name);
                if (pActor.city != null && Castle.castleList.ContainsKey(pActor.city))
                {
                    var castle = Castle.castleList[pActor.city];
                    if (castle.Alert)
                    {
                        random = Castle.getInfantryPosRand(castle);
                    }
                    else if (castle.insideCastle(leader)) // leader inside castle
                    {
                        random = Castle.getInfantryPosRand(castle);
                    }
                    else
                    {
                        MapRegion mapRegion = pActor.unit_group.groupLeader.currentTile.region;
                        if (mapRegion.tiles.Count < 20 && mapRegion.neighbours.Count > 0)
                            mapRegion = mapRegion.neighbours.GetRandom<MapRegion>();
                        random = mapRegion.tiles.GetRandom<WorldTile>();
                    }
                }
                else
                {
                    MapRegion mapRegion = pActor.unit_group.groupLeader.currentTile.region;
                    if (mapRegion.tiles.Count < 20 && mapRegion.neighbours.Count > 0)
                        mapRegion = mapRegion.neighbours.GetRandom<MapRegion>();
                    random = mapRegion.tiles.GetRandom<WorldTile>();
                }
            }
            pActor.beh_tile_target = random;
            __result = BehResult.Continue;
        }
        public static bool isAtOwnCity(Actor pActor)
        {
            if (pActor == null) return false;
            if (pActor.city == null || !pActor.city.data.alive) return false;
            if (pActor.currentTile.zone == null) return false;
            if (pActor.currentTile.zone.city == null) return false;
            if (pActor.currentTile.zone.city != pActor.city) return false;
            return true;
        }
        public static void GoToTileTarget_Postfix(ref BehResult __result, Actor pActor)
        {
            // && (pActor.isProfession(UnitProfession.King) || pActor.isProfession(UnitProfession.Leader) || pActor.isProfession(UnitProfession.Warrior))
            if (!isAtOwnCity(pActor)) return;
            if (pActor.city != null && Castle.castleList.ContainsKey(pActor.city) && Castle.castleList[pActor.city].Alert
                )
            {
                var castle = Castle.castleList[pActor.city];
                // if the actor is already inside the castle we then let the game working on the rest until we are sure 
                // that the castle gate is closed, they may come out but soon they will have to return to the interior
                if (castle.insideCastle(pActor))
                    return;
                if (castle.GateClosed) // too late to come
                    return;
                // avoid useless civilian walk through the war zone
                if (castle.data.sameRaceWar && !pActor.isProfession(UnitProfession.Warrior))
                    return;
                // temporary removement
                pActor.beh_actor_target = null;
                pActor.clearAttackTarget();

                //Debug.Log( pActor.city.data.name + " decided to stay inside fortress");
                pActor.beh_tile_target = Castle.getInfantryPosRand(Castle.castleList[pActor.city]);
                __result = (pActor.goTo(pActor.beh_tile_target) == ExecuteEvent.False) ? BehResult.Stop : BehResult.Continue;
            }
        }
        public static bool addUnit_Prefix(Actor pActor, WorldTile pTile, SimObjectsZones __instance)
        {
            if (!pTile.hasUnits())
                __instance._tiles.Add(pTile);
            pTile.addUnit(pActor);
            TileZone zone = pTile.zone;
            City city = pTile.zone.city;
            if (city != null && !pActor.isInsideSomething())
            {
                if (pActor.professionAsset.can_capture)
                    city.updateConquest(pActor);
                if (pActor.kingdom.asset.mobs && World.world.worldLaws.world_law_peaceful_monsters.boolVal)
                    return false;
                if (pActor.kingdom.asset.count_as_danger && pActor.kingdom != city.kingdom && pActor.kingdom.isEnemy(city.kingdom))
                {
                    city.danger_zones.Add(zone);
                    if (city.army != null && pActor.currentTile.zone.city != null &&
                        pActor.currentTile.zone.city == city) // Alert the city for incoming raiders that they cant defeat
                    {
                        if (isAtOwnCity(city.army.groupLeader) && Castle.castleList.ContainsKey(city)) // if captain is guarding at city
                        {
                            if (pActor.race != city.race)
                            {
                                if (Castle.castleList[city].data.sameRaceWar)
                                    Castle.castleList[city].data.sameRaceWar = false;
                                else Castle.castleList[city].data.sameRaceWar = true;
                            }
                            if (pActor.isProfession(UnitProfession.Warrior) && pActor.unit_group != null)
                            {
                                if (pActor.unit_group.countUnits() / 0.5f > city.army.countUnits())
                                {
                                    //Debug.Log("Alert of massive invaders for " + city.data.name);
                                    //Debug.Log(city.name + " raise alert for defenders " + pActor.name);
                                    if (!Castle.castleList[city].Alert) Castle.castleList[city].data.Alert = true;
                                }
                                else if (Castle.castleList[city].Alert)
                                {
                                    Castle.castleList[city].data.Alert = false;
                                    if (Castle.castleList[city].GateClosed)
                                        Castle.castleList[city].OpenGate();
                                }
                            }
                        }
                        else if (Castle.castleList.ContainsKey(city)) // main army is not at the city, must defend at all cost
                        {
                            Castle.castleList[city].data.Alert = true;
                            //Debug.Log(city.name + " alert without army at town " + pActor.name);
                        }
                    }
                }
            }
            return false;
        }
        public static void GetDangerZone_Postfix(ref BehResult __result, Actor pActor)
        {
            City city = pActor.city;
            if (Castle.castleList.ContainsKey(city) && Castle.castleList[city].Alert) // Alert on being outnumbered
            {
                if (Castle.castleList[city].insideCastle(pActor)) // you may do something for defenders inside castle
                {
                    //pActor.ai.setTask("defend the fucking castle maybe");
                    __result = BehResult.Stop;
                    return;
                }
                else if (!Castle.castleList[city].GateClosed)
                {
                    // Debug.Log("retreat to castle " + city.name);
                    if (pActor.ai.task.id != "retreat_to_Castle")
                    {
                        pActor.clearTasks();
                        pActor.ai.setTaskBehFinished();
                        pActor.ai.setTask("retreat_to_Castle", true, true); // this one is good now but idk why da hell
                    }
                    // pActor.beh_tile_target = Castle.getInfantryPosRand(Castle.castleList[city]);

                    // temporary removement
                    //pActor.beh_actor_target = null;
                    //pActor.clearAttackTarget();
                    pActor.goTo(Castle.getInfantryPosRand(Castle.castleList[city]));
                    __result = BehResult.Continue;
                    return;
                }
            }
        }
    }
}
