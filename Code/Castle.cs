using ai.behaviours.conditions;
using NCMS.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
//using Unity.Mathematics;
using UnityEngine;
//using static UltimateJoystick;

namespace NobleLife
{
    public class Castle //: Building // a castle is a building that contain a group of other buildings as part of it
    {
        public City mainCity = null;
        public WorldTile mainTile = null;
        public WorldTile centerTile = null;
        public WorldTile gateTile = null;
        public TileType oldTileType = null;
        public TopTileType oldTopTile = null;
        public Dictionary<WorldTile, Actor> ArcherPosList = new Dictionary<WorldTile, Actor>();
        public List<WorldTile> ArcherPosTile = new List<WorldTile>();

        //public List<Actor> insideTroops = new List<Actor>();
        public Building gateBottom = null; // 

        public Building leftcorner = null;
        public Building rightcorner = null ;
        public Building horizontalWall = null;

        public bool Alert = false;
        public bool sameRaceWar = true;
        public bool isDemolished = false;

        public int base_health = 10000; // total health
        public int curHealth = 10000;

        public static Dictionary<City, Castle> castleList = new Dictionary<City, Castle>();
        public static ConstructionCost cost = new ConstructionCost(0, 0, 0, 100);

        public static void addNewCastle(WorldTile curTile, City fixCity = null)
        {
            if (curTile == null) return;
            if (curTile.zone.city != null || fixCity != null)
            {
                var pCity = (curTile.zone.city != null) ? curTile.zone.city : fixCity;
                if (!castleList.ContainsKey(pCity) && pCity != null)
                {
                    Castle pCastle = new Castle();
                    castleList.Add(pCity, pCastle);
                    pCastle.create_Castle(curTile, fixCity);
                }
            }
        }
        public static void init()
        {
            BuildingAsset gate = AssetManager.buildings.clone("castle_gate", "!city_building");
            gate.type = "castle_gate";
            gate.base_stats[S.health] = 10000;
            gate.fundament = new BuildingFundament(0, 0, 0, 0);
            gate.cost = new ConstructionCost(0, 0, 0, 0);
            gate.draw_light_area = true;
            gate.draw_light_size = 0.4f;
            gate.draw_light_area_offset_y = 4;
            gate.hasKingdomColor = true;
            gate.canBePlacedOnLiquid = false;
            gate.canBeDamagedByTornado = false;
            gate.cityBuilding = true;
            gate.ignoreBuildings = true;
            gate.checkForCloseBuilding = false;
            gate.canBeLivingHouse = false;
            gate.burnable = false;
            gate.setShadow(0.56f, 0.23f, 0.28f);

            AssetManager.buildings.add(gate);
            AssetManager.buildings.loadSprites(gate);

            gate = AssetManager.buildings.clone("castle_topleftcorner", "!city_building");
            gate.type = "leftcorner";
            gate.base_stats[S.health] = 5000;
            gate.fundament = new BuildingFundament(0, 0, 0, 0);
            gate.cost = new ConstructionCost(0, 0, 0, 0);
            gate.draw_light_area = true;
            gate.draw_light_size = 0.3f;
            gate.draw_light_area_offset_y = 4;
            gate.hasKingdomColor = true;
            gate.canBePlacedOnLiquid = false;
            gate.canBeDamagedByTornado = false;
            gate.cityBuilding = true;
            gate.ignoreBuildings = true;
            gate.checkForCloseBuilding = false;
            gate.canBeLivingHouse = false;
            gate.burnable = false;
            gate.setShadow(0.56f, 0.23f, 0.28f);

            AssetManager.buildings.add(gate);
            AssetManager.buildings.loadSprites(gate);

            gate = AssetManager.buildings.clone("castle_toprightcorner", "!city_building");
            // gate.upgradeTo = "toprightcorner_lvl2";
            gate.type = "rightcorner";
            gate.base_stats[S.health] = 5000;
            gate.fundament = new BuildingFundament(0, 0, 0, 0);
            gate.cost = new ConstructionCost(0, 0, 0, 0);
            gate.draw_light_area = true;
            gate.draw_light_size = 0.3f;
            gate.draw_light_area_offset_y = 4;
            gate.hasKingdomColor = true;
            gate.canBePlacedOnLiquid = false;
            gate.canBeDamagedByTornado = false;
            gate.cityBuilding = true;
            gate.buildingType = BuildingType.None;
            gate.ignoreBuildings = true;
            gate.checkForCloseBuilding = false;
            gate.canBeLivingHouse = false;
            gate.burnable = false;
            gate.setShadow(0.56f, 0.23f, 0.28f);

            AssetManager.buildings.add(gate);
            AssetManager.buildings.loadSprites(gate);

            gate = AssetManager.buildings.clone("castle_topwall", "!city_building");
            gate.type = "wall";
            gate.base_stats[S.health] = 5000;
            gate.fundament = new BuildingFundament(0, 0, 0, 0);
            gate.cost = new ConstructionCost(0, 0, 0, 0);
            gate.draw_light_area = true;
            gate.draw_light_size = 0.3f;
            gate.draw_light_area_offset_y = 1;
            gate.hasKingdomColor = true;
            gate.canBePlacedOnLiquid = false;
            gate.canBeDamagedByTornado = false;
            gate.cityBuilding = true;
            gate.buildingType = BuildingType.None;
            gate.ignoreBuildings = true;
            gate.checkForCloseBuilding = false;
            gate.canBeLivingHouse = false;
            gate.burnable = false;
            gate.setShadow(0.56f, 0.23f, 0.28f);

            AssetManager.buildings.add(gate);
            AssetManager.buildings.loadSprites(gate);

            Debug.Log("Successfully loaded assets for castle construction !");
        }
        public void setKingdom(Kingdom pkingdom)
        {
            if (gateBottom != null && !gateBottom.isRuin()) gateBottom.setKingdom(pkingdom);
            if (leftcorner != null && !leftcorner.isRuin()) leftcorner.setKingdom(pkingdom);
            if (rightcorner != null && !rightcorner.isRuin()) rightcorner.setKingdom(pkingdom);
            if (horizontalWall != null && !horizontalWall.isRuin()) horizontalWall.setKingdom(pkingdom);
        }
        public void repairCastle()
        {
            curHealth = base_health;
            gateBottom.data.health = (int)gateBottom.stats[S.health];
        }
        public static void switchedKingdomCastle_Postfix(City __instance)
        {
            if (castleList.ContainsKey(__instance))
            {
                castleList[__instance].setKingdom(__instance.kingdom);
                castleList[__instance].repairCastle();
            }
        }
        public int ArcherCount() // part of strategy if u have no archers
        {
            int cnt = 0;
            if (mainCity.army != null)
            {
                foreach (var unit in mainCity.army.units)
                    if (unit.data.alive && unit.s_attackType == WeaponType.Range && unit.currentTile.zone.city != null &&
                        unit.currentTile.zone.city == mainCity) 
                        cnt++;
            }
            return cnt;
        }
        public WorldTile getGuardsPos()
        {
            foreach (var pos in ArcherPosTile)
                if (!ArcherPosList.ContainsKey(pos))
                    return pos;
            return null;
        }
        public bool insideCastle(Actor pActor)
        {
            if (pActor == null) return false;
            if (!mainTile.isSameIsland(pActor.currentTile)) return false;
            var curTile = pActor.currentTile;
            if (curTile.pos.x > mainTile.pos.x - 5 && curTile.pos.x < mainTile.pos.x + 5 &&
                curTile.pos.y > mainTile.pos.y + 2 && curTile.pos.y < mainTile.pos.y + 10)
                return true;
            return false;
        }
        public bool FullDefendPos()
        {
            foreach (var pos in ArcherPosTile)
                if (!ArcherPosList.ContainsKey(pos)) return false;
            return true;
        }
        public void BehGuard()
        {
            foreach (var unit in ArcherPosList.ToList())
                if (!unit.Value.data.alive) ArcherPosList.Remove(unit.Key);
            //if (!FullDefendPos() && mainCity.army != null)
            //{
            //    foreach (var troop in mainCity.army.units)
            //    {
            //        if (troop.data.alive && troop.currentTile.zone.city != null && troop.currentTile.zone.city == mainCity)
            //        {
            //            if (!ArcherPosList.ContainsValue(troop))
            //            {
            //                WorldTile position = getGuardsPos();
            //                if (troop.s_attackType == WeaponType.Range && position != null)
            //                {
            //                    ArcherPosList.Add(position, troop);
            //                    Debug.Log(mainCity.data.cityName + " has added 1 archer.Total = " + ArcherPosList.Count());
            //                }
            //            }
            //            else
            //            {
            //                WorldTile pos = ArcherPosList.FirstOrDefault(x => x.Value == troop).Key;
            //                if (/*Toolbox.DistTile(troop.currentTile, pos) >= 1f*/ troop.currentTile != pos)
            //                {
            //                    troop.ai.setTaskBehFinished();
            //                    troop.attackTarget = null;
            //                    troop.goTo(pos);
            //                }
            //                else
            //                {
            //                    troop.ai.setTaskBehFinished();
            //                    troop.stopMovement();
            //                    troop.currentTile = pos;
            //                    troop.zPosition.y = 3;
            //                    Actor enemy = main.ActorNearTile(pos, mainCity.kingdom);
            //                    if (enemy != null && enemy.data.alive) troop.tryToAttack(enemy);
            //                }
            //            }
            //        }
            //        else if (!troop.data.alive && ArcherPosList.ContainsValue(troop))
            //        {
            //            ArcherPosList.Remove(ArcherPosList.FirstOrDefault(x => x.Value == troop).Key);
            //        }
            //    }
            //}

            //foreach(var archer in ArcherPosList.ToList())
            //{
            //    if (archer.Value != null)
            //    {
            //        if (!archer.Value.data.alive)
            //        {
            //            ArcherPosList.Remove(archer.Key);
            //            continue;
            //        }
            //        if (Toolbox.DistTile(archer.Value.currentTile, archer.Key) >= 1f)
            //            archer.Value.goTo(archer.Key);
            //        else
            //        {
            //            archer.Value.currentTile = archer.Key;
            //            archer.Value.zPosition.y = 4;
            //            Actor enemy = main.ActorNearTile(archer.Key, mainCity.kingdom);
            //            if (enemy != null && enemy.data.alive) archer.Value.tryToAttack(enemy);
            //        }
            //    }
            //}

            if (mainCity.army != null)
            {
                List<Actor> defenders = new List<Actor>(); // a list of available defenders to defend the city
                defenders.Add(mainCity.leader);
                foreach (var unit in mainCity.army.units)
                {
                    if (unit.data.alive && unit.currentTile.zone.city != null && unit.currentTile.zone.city == mainCity && unit != main.controlledActor)
                        defenders.Add(unit);
                }
                if (defenders.Count() > 1 && (mainCity.danger_zones.Count() >= defenders.Count() / 6 ||
                    mainCity._capturing_units.Count() > defenders.Count() / 6))
                {
                    //Debug.Log(city.data.cityName + " retreat to castle with " + defenders.Count() + " defenders against ");
                    for (int i = 0; i < defenders.Count(); i++)
                    {
                        Actor unit = defenders[i];
                        if (unit != main.controlledActor)
                        {
                            WorldTile pos = null;
                            if (unit.s_attackType == WeaponType.Range && !FullDefendPos())
                            {
                                if (!ArcherPosList.ContainsValue(unit))
                                {
                                    pos = getGuardsPos();
                                    ArcherPosList.Add(pos, unit);
                                }
                                else pos = ArcherPosList.FirstOrDefault(x => x.Value == unit).Key;
                            }
                            else pos = getDefendingTile(i);
                            if (unit.currentTile != pos && pos != null)
                            {
                                //unit.attackTarget = null; // archers often dont do anything but stand still and wait for death
                                unit.goTo(pos);
                                //unit.data.traits.Contains("It's a boat");
                            }
                            else
                            {
                                unit.stopMovement();
                                //unit.stayInBuilding(fortress.getTower());
                                //unit.setActorVisible(true);
                                //if (townHall.asset.id.Contains("2hall") || townHall.asset.id.Contains("1hall"))
                                //{
                                //    unit.zPosition.y = 4;
                                //}
                                if (unit.s_attackType == WeaponType.Range) unit.zPosition.y = 3;
                                if (unit != null && unit.data.alive && unit.kingdom != null && unit.currentTile != null)
                                {
                                    Actor ptarget = main.ActorNearTile(unit.currentTile, unit.kingdom);
                                    if (ptarget != null && ptarget.data.alive && unit.isInAttackRange(ptarget)) unit.tryToAttack(ptarget);
                                }
                            }
                        }
                    }
                }
            }
        }
        public static bool isCastleTile(WorldTile tile, Castle pcastle)
        {
            if (tile == null) return false;
            if (tile.pos.x >= pcastle.mainTile.pos.x - 7 && tile.pos.x <= pcastle.mainTile.pos.x + 7 &&
                tile.pos.y >= pcastle.mainTile.pos.y + 2 && tile.pos.y <= pcastle.mainTile.pos.y + 10)
                return true;
            return false;
        }
        public void checkZone(Building castlePart)
        {
            if (castlePart == null)
            {
                //Debug.Log("null castlepart");
                if (gateBottom == null) // other castlepart is null except the gate which is weird
                    Debug.Log("null " + mainCity.name);
                return;
            }
            if (!mainCity.hasBuildingType(castlePart.asset.type)) mainCity.addBuilding(castlePart);
            if (castlePart.currentTile.zone.city == null) mainCity.addZone(castlePart.currentTile.zone);
        }
        public int countdown = 300;
        public bool Alive = true;
        public static bool destroyCity_Prefix(City pCity)
        {
            if (castleList.ContainsKey(pCity))
            {
                castleList[pCity].Alive = false;
                castleList[pCity].setRuin_castle();
                castleList.Remove(pCity);
            }
            return true;
        }
        public void Behaviour()
        {
            if (mainCity == null || !mainCity.data.alive || hasNullCastlePart())
            {
                destroy_castle();
                castleList.Remove(mainCity);
                //Debug.Log("remove castle " + mainCity.name);
            }
            else switchColor(mainCity.kingdom);
            if (mainCity.army != null && mainCity.army.groupLeader != null && mainCity.leader != null)
            {
                if (Castle_Patches.isAtOwnCity(mainCity.army.groupLeader) && !insideCastle(mainCity.army.groupLeader) && 
                    GateClosed) 
                    OpenGate();

                if (GateBroken || badCondition(gateBottom)) OpenGate();
                else if (Alert)
                {
                    if (insideCastle(mainCity.army.groupLeader)) CloseGate();
                }
            }
            //BehGuard();
            if (GateBroken) OpenGate();
            if (!isSieged())
            {
                if (Alert) Alert = false;
                if (GateBroken) gateBottom.data.health = (int)gateBottom.stats[S.health];
                //if (curHealth < base_health) curHealth = base_health;
                if (GateClosed) OpenGate();
            }
            if (Alive)
            {
                checkZone(gateBottom);
                checkZone(leftcorner);
                checkZone(rightcorner);
                checkZone(horizontalWall);
                checkDestroyedPart();
            }
            //if (MapBox.instance.wars.Count <= 0)
            //{
            //    if (Alert) Alert = false;
            //    OpenGate();
            //}
            //if (gateBottom != null || !gateBottom.isRuin()) // we need signal from the leader to raise or hold castle gate
            //{
            //    if (Alert && Castle_Patches.isAtOwnCity(mainCity.army.groupLeader) && insideCastle(mainCity.army.groupLeader))
            //    {
            //        Debug.Log(mainCity.name + " is alert");
            //        if (!GateClosed && !GateBroken) CloseGate();
            //        else OpenGate();
            //    }
            //    else
            //    {
            //        if (countdown == 0 && !Alert)
            //        {
            //            countdown = 300;
            //            //Debug.Log(mainCity.name + " is not alert");
            //        }
            //        else countdown--;
            //        OpenGate();
            //    }
            //}
            //else OpenGate();
        }
        public bool isCompleteLyDestroyed()
        {
            return ((gateBottom == null || gateBottom.isRuin()) && (leftcorner == null || leftcorner.isRuin()) && (rightcorner == null || rightcorner.isRuin()) && 
                (horizontalWall == null || horizontalWall.isRuin()));
        }
        public bool isSieged()
        {
            return (mainCity.isInDanger() || mainCity.isGettingCaptured());
        }
        public void switchColor(Kingdom pKingdom)
        {
            if (mainCity != null)
            {
                if (gateBottom != null && !gateBottom.isRuin() && gateBottom.kingdom != mainCity.kingdom) gateBottom.setKingdom(pKingdom);
                if (leftcorner != null && !leftcorner.isRuin() && leftcorner.kingdom != mainCity.kingdom) leftcorner.setKingdom(pKingdom);
                if (rightcorner != null && !rightcorner.isRuin() && rightcorner.kingdom != mainCity.kingdom) rightcorner.setKingdom(pKingdom);
                if (horizontalWall != null && !horizontalWall.isRuin() && horizontalWall.kingdom != mainCity.kingdom) horizontalWall.setKingdom(pKingdom);
            }
        }
        public static WorldTile getInfantryPosRand(Castle pCastle)
        {
            var defendTile = pCastle.mainTile;
            System.Random temp = new System.Random();
            return MapBox.instance.GetTile(defendTile.pos.x + temp.Next(-5, 5), defendTile.pos.y + temp.Next(2, 10)); // or + 6
        }
        public bool badCondition(Building test)
        {
            if (test == null || test.isRuin())
                return true;
            return false;
        }
        public void checkDestroyedPart()
        {
            if (badCondition(gateBottom))
            {
                gateBottom = MapBox.instance.buildings.addBuilding("castle_gate", mainTile);
                mainCity.addBuilding(gateBottom);
                //Debug.Log(mainCity.data.cityName + "gate destroyed");
                if (!isSieged())
                {
                    ////gateBottom.zPosition.z = 0;
                    //for (int i = 1; i < 7; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 2);
                    //    if (!build.Type.damagedWhenWalked) MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                    //for (int i = 1; i < 7; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 2);
                    //    if (!build.Type.damagedWhenWalked) MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                }
                else // place ruin and destroy mountain tile, replace the old top tile
                {
                    // gatebottom
                    //for (int i = 1; i < 7; i++)
                    //{
                    // WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 2);
                    // MapAction.terraformMain(build, oldTileType);
                    // MapAction.terraformTop(build, oldTopTile);
                    //}
                    //for (int i = 1; i < 7; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 2);
                    //    MapAction.terraformMain(build, oldTileType);
                    //    MapAction.terraformTop(build, oldTopTile);
                    //}
                }
                //switchColor(mainCity.kingdom);
            }
            if (badCondition(leftcorner))
            {
                leftcorner = MapBox.instance.buildings.addBuilding("castle_topleftcorner", MapBox.instance.GetTile(mainTile.pos.x - 4, mainTile.pos.y + 5));
                mainCity.addBuilding(leftcorner);
                if (!isSieged())
                {
                    
                    //for (int i = 0; i < 7; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 10);
                    //    if (!build.Type.damagedWhenWalked) MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                    //for (int i = 1; i < 10; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - 5, mainTile.pos.y + i);
                    //    if (!build.Type.damagedWhenWalked) MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                }
                else
                {
                    //top left corner
                    //for (int i = 0; i < 7; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 10);
                    //    MapAction.terraformMain(build, oldTileType);
                    //    MapAction.terraformTop(build, oldTopTile);
                    //}
                    //for (int i = 1; i < 10; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - 5, mainTile.pos.y + i);
                    //    MapAction.terraformMain(build, oldTileType);
                    //    MapAction.terraformTop(build, oldTopTile);
                    //}
                }
                //switchColor(mainCity.kingdom);
            }
            if (badCondition(rightcorner))
            {
                rightcorner = MapBox.instance.buildings.addBuilding("castle_toprightcorner", MapBox.instance.GetTile(mainTile.pos.x + 4, mainTile.pos.y + 5));
                mainCity.addBuilding(rightcorner);
                if (!isSieged())
                {
                    
                    //for (int i = 0; i < 7; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 10);
                    //    if (!build.Type.damagedWhenWalked) MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                    //for (int i = 1; i < 10; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + 5, mainTile.pos.y + i);
                    //    if (!build.Type.damagedWhenWalked) MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                }
                else
                {
                    //top right corner
                    //for (int i = 0; i < 7; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 10);
                    //    MapAction.terraformMain(build, oldTileType);
                    //    MapAction.terraformTop(build, oldTopTile);
                    //}
                    //for (int i = 1; i < 10; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + 5, mainTile.pos.y + i);
                    //    MapAction.terraformMain(build, oldTileType);
                    //    MapAction.terraformTop(build, oldTopTile);
                    //}
                }
                //switchColor(mainCity.kingdom);
            }
            if (badCondition(horizontalWall))
            {
                horizontalWall = MapBox.instance.buildings.addBuilding("castle_topwall", MapBox.instance.GetTile(mainTile.pos.x, mainTile.pos.y + 9));
                mainCity.addBuilding(horizontalWall);
                if (!isSieged())
                {
                    
                    //for (int i = 0; i < 2; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 9);
                    //    if (!build.Type.damagedWhenWalked) MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                }
                else
                {
                    //top horizontal wall
                    //for (int i = 0; i < 2; i++)
                    //{
                    //    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 9);
                    //    //MapAction.terraformMain(build, TileLibrary.mountains);
                    //}
                }
                //switchColor(mainCity.kingdom);
            }
        }
        public Castle loadBase(WorldTile curtile, City fixCity = null)
        {
            mainTile = curtile;
            gateTile = MapBox.instance.GetTile(mainTile.pos.x, mainTile.pos.y + 2);
            centerTile = MapBox.instance.GetTile(gateTile.pos.x, gateTile.pos.y + 4); // for leader to gather garrison
            mainCity = (fixCity != null) ? fixCity : curtile.zone.city;
            var TileTypeFound = getSurroundTileType(mainTile);
            oldTileType = TileTypeFound.Item1;
            oldTopTile = TileTypeFound.Item2;
            return this;
        }
        public Building loadCastlePart(string asset_id, WorldTile curtile, BuildingData pData)
        {
            //var result = MapBox.instance.buildings.addBuilding(asset_id, curtile);
            var result =  World.world.buildings.newObject(PrefabLibrary.instance.building);
            result.create();
            result.setBuilding(curtile, AssetManager.buildings.get(asset_id), pData);
            if (asset_id == "castle_gate") gateBottom = result;
            if (asset_id == "castle_topleftcorner") leftcorner = result;
            if (asset_id == "castle_toprightcorner") rightcorner = result;
            if (asset_id == "castle_topwall") horizontalWall = result;
            mainCity.addBuilding(result);  
            return result;
        }
        public bool hasNullCastlePart()
        {
            if (gateBottom == null) return true;
            if (horizontalWall == null) return true;
            if (leftcorner == null) return true;
            if (rightcorner == null) return true;
            return false;
        }
        public static (TileType, TopTileType) getSurroundTileType(WorldTile mainTile)
        {
            (TileType, TopTileType) result;
            for (int i = -6; i <= 6; i++)
                for (int j = 1; j < 10; j++)
                {
                    WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + j);
                    if (!build.Type.liquid && !build.Type.damagedWhenWalked)
                    {
                        result.Item1 = build.main_type;
                        result.Item2 = build.top_type;
                        return result;
                    }
                }
            return (TileLibrary.soil_low, TopTileLibrary.biomass_low);
        }
        public static bool isGoodForCastleBuild(WorldTile centerTile)
        {
            if (centerTile.Type.damagedWhenWalked || centerTile.Type.liquid)
                return false;
            for (int i = -6; i <= 6; i++)
                for (int j = 2; j <= 10; j++)
                {
                    var build = MapBox.instance.GetTile(centerTile.pos.x + i, centerTile.pos.y + j);
                    if (build == null) return false;
                }
            return true;
        }
        public void create_Castle(WorldTile curtile, City fixCity = null)
        {
            if (curtile == null) return;
            loadBase(curtile, fixCity);

            gateBottom = MapBox.instance.buildings.addBuilding("castle_gate", curtile);
            mainCity.addBuilding(gateBottom);

            leftcorner = MapBox.instance.buildings.addBuilding("castle_topleftcorner", MapBox.instance.GetTile(curtile.pos.x - 4, curtile.pos.y + 5));
            mainCity.addBuilding(leftcorner);

            rightcorner = MapBox.instance.buildings.addBuilding("castle_toprightcorner", MapBox.instance.GetTile(curtile.pos.x + 4, curtile.pos.y + 5));
            mainCity.addBuilding(rightcorner);

            horizontalWall = MapBox.instance.buildings.addBuilding("castle_topwall", MapBox.instance.GetTile(curtile.pos.x, curtile.pos.y + 9));
            mainCity.addBuilding(horizontalWall);

            //create collision and add defending pos for archer
            //top left corner
            for (int i = 0; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(curtile.pos.x - i, curtile.pos.y + 10);
                build.zone.buildings_all.Add(leftcorner);
                MapAction.terraformMain(build, TileLibrary.mountains);
                if (!ArcherPosTile.Contains(build)) ArcherPosTile.Add(build);
            }
            for (int i = 1; i < 10; i++)
            {
                WorldTile build = MapBox.instance.GetTile(curtile.pos.x - 5, curtile.pos.y + i);
                build.zone.buildings_all.Add(leftcorner);
                MapAction.terraformMain(build, TileLibrary.mountains);
                if (!ArcherPosTile.Contains(build)) ArcherPosTile.Add(build);
            }
            //top right corner
            for (int i = 0; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(curtile.pos.x + i, curtile.pos.y + 10);
                build.zone.buildings_all.Add(rightcorner);
                MapAction.terraformMain(build, TileLibrary.mountains);
                if (!ArcherPosTile.Contains(build)) ArcherPosTile.Add(build);
            }
            for (int i = 1; i < 10; i++)
            {
                WorldTile build = MapBox.instance.GetTile(curtile.pos.x + 5, curtile.pos.y + i);
                build.zone.buildings_all.Add(rightcorner);
                MapAction.terraformMain(build, TileLibrary.mountains);
                if (!ArcherPosTile.Contains(build)) ArcherPosTile.Add(build);
            }

            //top horizontal wall
            for (int i = 0; i < 2; i++)
            {
                WorldTile build = MapBox.instance.GetTile(curtile.pos.x - i, curtile.pos.y + 9);
                if (!ArcherPosTile.Contains(build)) ArcherPosTile.Add(build);
                //MapAction.terraformMain(build, TileLibrary.mountains);
            }
            // gatebottom
            for (int i = -6; i <= 6; i++)
            {
                if (i == 0) continue;
                WorldTile build = MapBox.instance.GetTile(curtile.pos.x - i, curtile.pos.y + 2);
                build.zone.buildings_all.Add(gateBottom);
                MapAction.terraformMain(build, TileLibrary.mountains);
                if (!ArcherPosTile.Contains(build)) ArcherPosTile.Add(build);
            }
            // make sure that theres no liquid tile within the castle interior
            for (int i = -6; i <= 6; i++)
                for (int j = 1; j < 10; j++)
                {
                    WorldTile build = MapBox.instance.GetTile(curtile.pos.x + i, curtile.pos.y + j);
                    if (build.Type.liquid || build.Type.damagedWhenWalked)
                    {
                        MapAction.terraformMain(build, oldTileType);
                        MapAction.terraformTop(build, oldTopTile);
                    }
                }
        }
        public bool GateBroken => (gateBottom != null && gateBottom.data.health <= 0);
        public bool GateClosed = false;
        public void OpenGate()
        {
            if (GateClosed)
            { 
                gateTile.setTileTypes(oldTileType, oldTopTile);
                GateClosed = false;
            }
        }
        public void CloseGate()
        {
            if (!GateClosed && !badCondition(gateBottom) && !GateBroken)
            { 
                gateTile.setTileType(TileLibrary.mountains);
                GateClosed = true;
            }
        }
        public static bool canAttackCastle(Building check)
        {
            if (check.asset.id != "castle_gate" && check.asset.id != "castle_topleftcorner" &&
                check.asset.id != "castle_toprightcorner" && check.asset.id != "castle_topwall")
                return false;
            if (check.asset.id == "castle_gate" && check.data.health > 0 && check.city != null
                && castleList.ContainsKey(check.city) && castleList[check.city].GateClosed)
                return false; // continue ramming the gate
            return true;
        }
        public WorldTile getDefendingTile(int elementPos)
        {
            //System.Random temp = new System.Random();
            //return MapBox.instance.GetTile(mainTile.pos.x + temp.Next(-7,7), mainTile.pos.y + temp.Next(1,10)); // or + 6
            if (elementPos < 120)
                return MapBox.instance.GetTile(mainTile.pos.x + Convert.ToInt32(Math.Pow(-1, elementPos) * (elementPos % 6)), mainTile.pos.y + elementPos % 8 + 2);
            else return getDefendingTile(elementPos - 120); // a bit of recursive
        }
        public void setRuinPart(Building __instance)
        {
            __instance.clearCity();
            __instance.setState(BuildingState.Ruins);
            __instance.setAnimationState(BuildingAnimationState.OnRuin);
            __instance.setKingdom(World.world.kingdoms.getKingdomByID(SK.ruins));
        }
        public void setRuin_castle()
        {
            setRuinPart(gateBottom);
            setRuinPart(leftcorner);
            setRuinPart(rightcorner);
            setRuinPart(horizontalWall);
        }
        public static void destroy_VisibleWall(WorldTile mainTile)
        {
            var gate = mainTile.building;//.startRemove();
            var leftcorner = MapBox.instance.GetTile(mainTile.pos.x - 4, mainTile.pos.y + 5).building;//.startRemove();
            var rightcorner = MapBox.instance.GetTile(mainTile.pos.x + 4, mainTile.pos.y + 5).building;//.startRemove();
            var topwall = MapBox.instance.GetTile(mainTile.pos.x, mainTile.pos.y + 9).building;//.startRemove();

            if (gate != null) gate.startRemove();
            if (leftcorner != null) leftcorner.startRemove();
            if (rightcorner != null) rightcorner.startRemove();
            if (topwall != null) topwall.startRemove();
        }
        public static void destroy_physicWall(WorldTile mainTile)
        {
            var TileTypeFound = getSurroundTileType(mainTile);
            var oldTileType = TileTypeFound.Item1;
            var oldTopTile = TileTypeFound.Item2;
            // gatebottom
            for (int i = 1; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 2);
                if (build != null) build.setTileTypes(oldTileType, oldTopTile);
            }
            for (int i = 1; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 2);
                if (build != null) build.setTileTypes(oldTileType, oldTopTile);
            }
            //top left corner
            for (int i = 0; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 10);
                if (build != null) build.setTileTypes(oldTileType, oldTopTile);
            }
            for (int i = 1; i < 10; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - 5, mainTile.pos.y + i);
                if (build != null) build.setTileTypes(oldTileType, oldTopTile);
            }
            //top right corner
            for (int i = 0; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 10);
                if (build != null) build.setTileTypes(oldTileType, oldTopTile);
            }
            for (int i = 1; i < 10; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + 5, mainTile.pos.y + i);
                if (build != null) build.setTileTypes(oldTileType, oldTopTile);
            }
            // top horizontal wall
            for (int i = 0; i < 2; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 9);
                if (build != null) build.setTileTypes(oldTileType, oldTopTile);
            }
        }
        public void destroy_castle()
        {
            Debug.Log("Destroy castle " + mainCity.data.name);
            // gatebottom
            for (int i = 1; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 2);
                build.setTileTypes(oldTileType, oldTopTile);
            }
            for (int i = 1; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 2);
                build.setTileTypes(oldTileType, oldTopTile);
            }
            gateBottom.clearCity();
            gateBottom.startRemove();
            //top left corner
            for (int i = 0; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 10);
                build.setTileTypes(oldTileType, oldTopTile);
            }
            for (int i = 1; i < 10; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - 5, mainTile.pos.y + i);
                build.setTileTypes(oldTileType, oldTopTile);
            }
            leftcorner.clearCity();
            leftcorner.startRemove();
            //top right corner
            for (int i = 0; i < 7; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + i, mainTile.pos.y + 10);
                build.setTileTypes(oldTileType, oldTopTile);
            }
            for (int i = 1; i < 10; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x + 5, mainTile.pos.y + i);
                build.setTileTypes(oldTileType, oldTopTile);
            }
            rightcorner.clearCity();
            rightcorner.startRemove();
            // top horizontal wall
            for (int i = 0; i < 2; i++)
            {
                WorldTile build = MapBox.instance.GetTile(mainTile.pos.x - i, mainTile.pos.y + 9);
                build.setTileTypes(oldTileType, oldTopTile);
            }
            horizontalWall.clearCity();
            horizontalWall.startRemove();

            gateTile.setTileTypes(oldTileType, oldTopTile);
            //mainTile.setTileType(TileLibrary.shallow_waters);

            mainCity = null;
            mainTile = null;
        }
    }
}