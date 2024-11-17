using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
//using UnityEngine.EventSystems;
//using UnityEngine.UI;
using ai;

using HarmonyLib;
using UnityEngine.UIElements;
using ai.behaviours;
using System.Configuration.Internal;
using System.Reflection;
//using Amazon.Auth.AccessControlPolicy;
using static UnityEngine.GraphicsBuffer;

using ReflectionUtility;
using ModDeclaration;
using System.IO;
using UnityEngine.Assertions.Must;
using System.IO.Ports;
using static UnityEngine.TouchScreenKeyboard;
using System.Runtime.Remoting.Messaging;
using ai.behaviours.conditions;
//using static UnityEngine.UI.CanvasScaler;
using NeoModLoader.api;

namespace NobleLife
{
    // [ModEntry]
    public class main : BasicMod<main> // MonoBehaviour // old NCMS way
    {
        public static string pluginName = "ModdingCastle";
        public static string pluginGuid = "phong.worldbox.NobleLife";
        public static string pluginVersion = "0.0.0.2";
        public enum MovingDir { up, down, left, right, up_left, up_right, down_left, down_right, stop };
        public WorldTile mouseTile => MapBox.instance.getMouseTilePos();
        public static Actor controlledActor = null;
        public WorldTile curTile = null;
        public MovingDir direction = MovingDir.stop;

        public static string errorLog = "";
        // public void Awake() // first frame, NCMS way
        protected override void OnModLoad()
        {
            SaveCastle.Awake();
            Warband.Awake();

            Harmony harmony;
            MethodInfo original;
            MethodInfo patch;

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(City), "removeZone"); // 
            patch = AccessTools.Method(typeof(Castle_Patches), "removeZone_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(MapBox), "generateNewMap"); // 
            patch = AccessTools.Method(typeof(Castle_Patches), "generateNewMap_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(City), "tryToMakeWarrior"); // 
            patch = AccessTools.Method(typeof(main), "tryToMakeWarrior_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(MouseCursor), "renderCursor"); //
            patch = AccessTools.Method(typeof(main), "renderCursor_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(UnitSelectionEffect), "update"); //
            patch = AccessTools.Method(typeof(main), "removeSelectionEffect_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(MapBox), "canInspectUnitWithCurrentPower"); //
            patch = AccessTools.Method(typeof(main), "canInspectUnitWithCurrentPower_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Actor), "tryToAttack"); //
            patch = AccessTools.Method(typeof(main), "tryToAttack_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(City), "switchedKingdom"); //
            patch = AccessTools.Method(typeof(Castle), "switchedKingdomCastle_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(CityBehBuild), "buildTick"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "buildTick_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(City), "update"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "updateCity_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BehFindRandomTile), "execute"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "FindRandomTile_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BehFindTileNearbyGroupLeader), "execute"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "TileNearLeader_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BehGoToTileTarget), "execute"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "GoToTileTarget_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BehCityGetRandomDangerZone), "execute"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "GetDangerZone_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(SimObjectsZones), "addUnit"); // for Alarm raise
            patch = AccessTools.Method(typeof(Castle_Patches), "addUnit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Building), "getHit"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "BuildingGetHit_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(City), "abandonBuilding"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "abandonBuilding_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(PowerLibrary), "drawDemolish");
            patch = AccessTools.Method(typeof(Castle_Patches), "drawDemolish_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(Building), "startDestroyBuilding");
            patch = AccessTools.Method(typeof(Castle_Patches), "startDestroyBuilding_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            // this is how I gonna prevent the Earthquake from shutting down my castle
            //harmony = new Harmony(pluginGuid);
            //original = AccessTools.Method(typeof(Building), "startRemove");
            //patch = AccessTools.Method(typeof(Castle_Patches), "startRemove_Prefix");
            //harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(CitiesManager), "destroyCity"); //
            patch = AccessTools.Method(typeof(Castle), "destroyCity_Prefix");
            harmony.Patch(original, new HarmonyMethod(patch));

            harmony = new Harmony(pluginGuid);
            original = AccessTools.Method(typeof(BaseSimObject), "canAttackTarget"); //
            patch = AccessTools.Method(typeof(Castle_Patches), "canAttackTarget_Postfix");
            harmony.Patch(original, null, new HarmonyMethod(patch));

            Castle.init();

            //var cachedSprites = Reflection.GetField(typeof(SpriteTextureLoader), null, "cached_sprites") as Dictionary<string, Sprite>;
            //cachedSprites.Clear();
            //var cachedSpritesList = Reflection.GetField(typeof(SpriteTextureLoader), null, "cached_sprite_list") as Dictionary<string, Sprite[]>;
            //cachedSpritesList.Clear();

            //In __instance list put all ids of changed buildings sprites to reload the sprites
            //var listChanged = new List<string>() {
            //    "hall_human",
            //    "1hall_human",
            //    "2hall_human"
            //};
            //foreach (var item in listChanged)
            //{
            //    var building = AssetManager.buildings.get(item);
            //    AssetManager.buildings.loadSprites(building);
            //}

        }
        public static bool tryToMakeWarrior_Prefix(ref bool __result, City __instance, Actor pActor)
        {
            if (__instance.isArmyFull())
            {
                __result = false;
                return false;
            }
            if (__instance.blacksmith == null)
            {
                __result = false;
                return false;
            }
            if (__instance.getEquipmentList(EquipmentType.Weapon).Count <= 0)
            {
                __result = false;
                return false;
            }
            pActor.setProfession(UnitProfession.Warrior, true);
            if (pActor.equipment.weapon.isEmpty())
            {
                City.giveItem(pActor, __instance.getEquipmentList(EquipmentType.Weapon), __instance);
            }
            __instance.status.warriors_current++;
            __instance._timer_warrior = 15f;
            if (__instance.leader != null)
            {
                float num = __instance.leader.stats[S.warfare] / 2f;
                __instance._timer_warrior -= num;
                if (__instance._timer_warrior < 1f)
                {
                    __instance._timer_warrior = 1f;
                }
            }
            if (__instance.hasBuildingType(SB.type_barracks, true))
            {
                __instance._timer_warrior /= 2f;
            }
            __result = true;
            return false;
        }
        public static bool tryToAttack_Prefix(ref bool __result, BaseSimObject pTarget, Actor __instance, bool pDoChecks = true)
        {
            if (controlledActor != null && controlledActor == __instance)
            {
                Actor ptarget = null;
                WorldTile currentTile = MapBox.instance.getMouseTilePos();
                if (currentTile != null)
                    foreach (var unit in currentTile._units)
                        if (unit != pTarget)
                        {
                            ptarget = unit;
                            break;
                        }
                if (pTarget == null) return false;
                if (pTarget.kingdom != null && !pTarget.kingdom.isEnemy(controlledActor.kingdom)) 
                    return false;
                if (Input.GetMouseButtonDown(0) && pTarget != controlledActor)
                {
                    if (pDoChecks)
                    {
                        if (__instance.s_attackType == WeaponType.Melee && pTarget.zPosition.y > 0f)
                        {
                            __result = false;
                            return false;
                        }
                        if (__instance.isInLiquid() && !__instance.asset.oceanCreature)
                        {
                            __result = false;
                            return false;
                        }
                        if (!__instance.isAttackReady())
                        {
                            __result = false;
                            return false;
                        }
                        if (!__instance.isInAttackRange(pTarget))
                        {
                            __result = false;
                            return false;
                        }
                    }
                    __instance.timer_action = __instance.s_attackSpeed_seconds;
                    __instance.attackTimer = __instance.s_attackSpeed_seconds;
                    __instance.punchTargetAnimation(pTarget.currentPosition, true, __instance.s_attackType == WeaponType.Range, 40f);
                    float num = __instance.stats[S.size];
                    float num2 = pTarget.stats[S.size];
                    Vector3 vector = new Vector3(pTarget.currentPosition.x, pTarget.currentPosition.y);
                    if (pTarget.isActor() && pTarget.a.is_moving && pTarget.isFlying())
                    {
                        vector = Vector3.MoveTowards(vector, pTarget.a.nextStepPosition, num2 * 3f);
                    }
                    float num3 = Vector2.Distance(__instance.currentPosition, pTarget.currentPosition) + pTarget.getZ();
                    Vector3 newPoint = Toolbox.getNewPoint(__instance.currentPosition.x, __instance.currentPosition.y, vector.x, vector.y, num3 - num2, true);
                    AttackData pData = new AttackData(__instance, pTarget.currentTile, newPoint, pTarget, AttackType.Weapon, __instance.haveMetallicWeapon(), false, true);
                    bool result;
                    using (ListPool<CombatActionAsset> listPool = new ListPool<CombatActionAsset>())
                    {
                        List<string> attack_spells = __instance.asset.attack_spells;
                        if (attack_spells != null && attack_spells.Count > 0)
                        {
                            __instance.addToAttackPool(CombatActionLibrary.combat_cast_spell, listPool);
                        }
                        CombatActionAsset combatActionAsset;
                        if (listPool.Count > 0)
                        {
                            if (__instance.s_attackType == WeaponType.Melee)
                            {
                                __instance.addToAttackPool(CombatActionLibrary.combat_attack_melee, listPool);
                            }
                            else
                            {
                                __instance.addToAttackPool(CombatActionLibrary.combat_attack_range, listPool);
                            }
                            combatActionAsset = listPool.GetRandom<CombatActionAsset>();
                            if (!combatActionAsset.action(pData) && !combatActionAsset.basic)
                            {
                                if (__instance.s_attackType == WeaponType.Melee)
                                {
                                    CombatActionLibrary.combat_attack_melee.action(pData);
                                }
                                else
                                {
                                    CombatActionLibrary.combat_attack_range.action(pData);
                                }
                            }
                        }
                        else
                        {
                            if (__instance.s_attackType == WeaponType.Melee)
                            {
                                combatActionAsset = CombatActionLibrary.combat_attack_melee;
                            }
                            else
                            {
                                combatActionAsset = CombatActionLibrary.combat_attack_range;
                            }
                            combatActionAsset.action(pData);
                        }
                        if (combatActionAsset.play_unit_attack_sounds && !string.IsNullOrEmpty(__instance.asset.fmod_attack))
                        {
                            MusicBox.playSound(__instance.asset.fmod_attack, (float)__instance.currentTile.x, (float)__instance.currentTile.y, false, false);
                        }
                        if (__instance.asset.needFood && Toolbox.randomBool())
                        {
                            __instance.decreaseHunger(-1);
                        }
                        result = true;
                    }
                    __result = result;
                    return false;
                }
                return false;
            }
            return true;
        }
        public static bool canInspectUnitWithCurrentPower_Prefix()
        {
            if (controlledActor != null) return false;
            return true;
        }
        public static bool removeSelectionEffect_Prefix(float pElapsed, UnitSelectionEffect __instance)
        {
            if (controlledActor != null)
            {
                UnitSelectionEffect.last_actor = null;
                __instance.gameObject.SetActive(false);
                return false;
            }
            return true;
        }
        public static bool renderCursor_Prefix(MouseCursor __instance)
        {
            if (controlledActor != null)
            {
                //if (cursorOverride != string.Empty)
                //{
                //    UnityEngine.Cursor.SetCursor(cursors[cursorOverride], Vector2.zero, CursorMode.ForceSoftware);
                //    __instance._lastTextureID = cursorOverride;
                //    return false;
                //}
                return false;
            }
            return true;
        }
        public static void makeMove(WorldTile nextTile, Actor dude)
        {
            if (nextTile == null)
            {
                WorldTip.instance.show("Border reached !", false, "top", 1);
                return;
            }
            if (nextTile.Type.damagedWhenWalked || nextTile.Type.edge_mountains) return;
            dude.moveTo(nextTile);
        }
        //[Obsolete]
        public void ControlCharacterUpdate()
        {
            curTile = MapBox.instance.GetTileSimple((int)controlledActor.currentPosition.x, (int)controlledActor.currentPosition.y);
            if (Input.GetKey(KeyCode.W))
            {
                if (Input.GetKey(KeyCode.A))
                    direction = MovingDir.up_left;
                else if (Input.GetKey(KeyCode.D))
                    direction = MovingDir.up_right;
                else
                    direction = MovingDir.up;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                if (Input.GetKey(KeyCode.A))
                    direction = MovingDir.down_left;
                else if (Input.GetKey(KeyCode.D))
                    direction = MovingDir.down_right;
                else
                    direction = MovingDir.down;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                direction = MovingDir.left;
                if (Input.GetKey(KeyCode.W))
                    direction = MovingDir.up_left;
                else if (Input.GetKey(KeyCode.S))
                    direction = MovingDir.down_left;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                direction = MovingDir.right;
                if (Input.GetKey(KeyCode.W))
                    direction = MovingDir.up_right;
                else if (Input.GetKey(KeyCode.S))
                    direction = MovingDir.down_right;
            }
            else
            {
                direction = MovingDir.stop;
                controlledActor.cancelAllBeh(controlledActor);
            }
            if (direction != MovingDir.stop)
            {
                WorldTile moveTile = null;
                if (direction == MovingDir.up)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x, curTile.pos.y + 1);
                else if (direction == MovingDir.right)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x + 1, curTile.pos.y);
                else if (direction == MovingDir.left)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x - 1, curTile.pos.y);
                else if (direction == MovingDir.down)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x, curTile.pos.y - 1);
                else if (direction == MovingDir.up_right)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x + 1, curTile.pos.y + 1);
                else if (direction == MovingDir.up_left)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x - 1, curTile.pos.y + 1);
                else if (direction == MovingDir.down_right)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x + 1, curTile.pos.y - 1);
                else if (direction == MovingDir.down_left)
                    moveTile = MapBox.instance.GetTile(curTile.pos.x - 1, curTile.pos.y - 1);
                makeMove(moveTile, controlledActor);
            }
            else
            {
                controlledActor.cancelAllBeh();
                controlledActor.stopMovement();
            }
            // troop behaviour while marching and maybe sieging, fighting bla bla 
            //if (controlledActor.is_group_leader && controlledActor.unit_group != null)
            //{
            //    foreach (var troops in controlledActor.unit_group.units.getSimpleList()) // __instance is slow and stutter as hell
            //    {
            //        //if (Toolbox.DistVec2Float(troops.currentPosition, controlledActor.currentPosition) > 10f)
            //        //{
            //        //    //troops.cancelAllBeh();
            //        //    troops.goTo(controlledActor.currentTile);
            //        //}
            //        if (troops.ai != null && troops.ai.task != null)
            //        {
            //            if (troops.ai.task.id == "check_warrior_transport" ||
            //                troops.ai.task.id == "wait" ||
            //                troops.ai.task.id == "try_to_return_home" ||
            //                troops.ai.task.id == "check_if_stuck_on_small_land")
            //            {
            //                troops.ai.task = null;
            //                troops.ai.setTaskBehFinished();
            //            }
            //        }
            //    }
            //}
        }
        public static Actor ActorNearTile(WorldTile tile, Kingdom pkingdom = null)
        {
            if (tile == null) return null;
            Actor result = null;
            float minDist = Toolbox.DistVec2Float(tile.pos, MapBox.instance.units.ToList()[0].currentPosition);
            if (pkingdom != null)
            {
                foreach (var dudes in MapBox.instance.units)
                    if (dudes.kingdom.isEnemy(pkingdom) && dudes.isProfession(UnitProfession.Warrior | UnitProfession.Leader | UnitProfession.King))
                    {
                        minDist = Toolbox.DistVec2Float(tile.pos, dudes.currentPosition);
                        break;
                    }
            }
            foreach (var dudes in MapBox.instance.units)
            {
                if (pkingdom != null)
                {
                    if (Toolbox.DistVec2Float(tile.pos, dudes.currentPosition) <= minDist && dudes.kingdom.isEnemy(pkingdom)
                        && dudes.isProfession(UnitProfession.Warrior | UnitProfession.Leader | UnitProfession.King))
                    {
                        minDist = Toolbox.DistVec2Float(tile.pos, dudes.currentPosition);
                        result = dudes;
                    }
                }
                else if (Toolbox.DistVec2Float(tile.pos, dudes.currentPosition) <= minDist)
                {
                    minDist = Toolbox.DistVec2Float(tile.pos, dudes.currentPosition);
                    result = dudes;
                }
            }
            return result;
        }
        public void followActor(Actor target, float speed = 5f)
        {
            Vector3 posV = target.currentPosition;
            Camera.main.transform.position = target.currentPosition; //Vector3.Lerp(Camera.main.transform.position, posV, speed * Time.deltaTime);
        }
        public void Update()
        {
            // debug 
            //if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.K))
            //    Castle.addNewCastle(mouseTile, mouseTile.zone.city);

            //if (Input.GetKey(KeyCode.K))
            //{
            //    String text = (Castle.castleList.Count > 0) ? Castle.castleList.First().Value.mainCity.name + " + health: " + Castle.castleList.First().Value.data.curHealth : " Empty "; //;
            //    WorldTip.instance.show("Castle list " + Castle.castleList.Count + " data list: " + text, false, "top", 1);
            //}
            if (MapBox.instance.getActorNearCursor() != null && Input.GetKey(KeyCode.R) && controlledActor == null)
            {
                Actor temp = MapBox.instance.getActorNearCursor();
                WorldTip.instance.show("U are controlling " + temp.getName() + "\nW,A,S,D to control\nLeft click on Enemy Kingdom to attack", false, "top", 1);
                controlledActor = temp;
            }
            else if (controlledActor != null && Input.GetKey(KeyCode.Q)) controlledActor = null;
            if (controlledActor != null)
            {
                if (controlledActor != null)
                {
                    // set cursor override or shut down the event
                }
                ControlCharacterUpdate();
                //CameraFollow();
                followActor(controlledActor);

                if (controlledActor.currentTile.zone != null && controlledActor.currentTile.zone.city != null)
                {
                    var city = controlledActor.currentTile.zone.city;
                    //WorldTip.instance.show("Total castles: " + Castle.castleList.Count, false, "top", 1);

                    if (Castle.castleList.ContainsKey(city))
                    {
                        var pCastle = Castle.castleList[city];
                        //if (Input.GetKey(KeyCode.P))
                        //    pCastle.leftcorner.getHit(10000);
                        //float distance = 1f;
                        //if (Toolbox.DistTile(controlledActor.currentTile, pCastle.mainTile) < distance)
                        //    WorldTip.instance.show("U are within the radius of " + distance, false, "top", 1f);
                        //if (pCastle.insideCastle(controlledActor) && pCastle.gateBottom != null)
                        //    WorldTip.instance.show("U are inside " + city.name + " castle courtyard\n" + "Gate health: " + pCastle.gateBottom.data.health, false, "top", 1);
                        //else if (pCastle.gateBottom == null && pCastle.insideCastle(controlledActor))
                        //    WorldTip.instance.show("The gate is null idk", false, "top", 1);
                        //else
                        //    WorldTip.instance.show("U are outside of the castle " + city.name, false, "top", 1);

                        //if (Input.GetKey(KeyCode.L))
                        //{
                        //    //Castle.castleList[city].gateTile.setTileType(TileLibrary.mountains);
                        //    pCastle.OpenGate();

                        //    if (pCastle.GateClosed) Debug.Log(" The fucking gate is closed ");
                        //    else Debug.Log("The gate is not close");
                        //}
                        //if (Input.GetKey(KeyCode.LeftControl))
                        //{
                        //    //Castle.castleList[city].gateTile.setTileType(TileLibrary.mountains);
                        //    pCastle.CloseGate();
                        //    if (pCastle.GateClosed) Debug.Log(" The fucking gate is closed ");
                        //    else Debug.Log("The gate is not close");
                        //}
                    }

                    //if (Castle.castleList[city].Alert)
                    //    WorldTip.instance.show("The city is on alert", false, "top",1);
                    

                    //else WorldTip.instance.show("No castle here", false, "top", 1);
                }
                //else if (controlledActor.currentTile.building != null && Castle_Patches.isCastlePart(controlledActor.currentTile.building))
                //{
                //    WorldTip.instance.show("__instance __instance is the castle part of ", false, "top", 1);
                //}
                //else if (controlledActor.currentTile.zone.city == null)
                //    WorldTip.instance.show("__instance tile/zone not belong to any city", false, "top", 1);

            }
            //CityMod();
        }
    }
}
