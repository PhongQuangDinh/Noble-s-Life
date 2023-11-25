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
using ai;
using ai.behaviours;

namespace NobleLife
{
    public class BehRetreatCastle: BehaviourActionActor
    {
        public override void create()
        {
            base.create();
            this.null_check_actor_target = true;
            this.null_check_building_target = true;
            // Debug.Log("create the Beh");
        }
        public override BehResult execute(Actor pActor)
        {
            Debug.Log("retreat to castle " + pActor.city.name); //its not working why
            Castle castle = Castle.castleList[pActor.city];
            if (castle.insideCastle(pActor))
                return BehResult.Stop;
            pActor.beh_tile_target = Castle.getInfantryPosRand(castle);
            return BehResult.Continue;
        }
    }
}
