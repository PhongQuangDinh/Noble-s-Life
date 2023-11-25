using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using UnityEngine;

namespace NobleLife
{
    [Serializable]
    public class CastleData
    {
        public string mainCity_id = null;
        //public TileType oldTileType = null; 
        //public TopTileType oldTopTile = null;
        //public Dictionary<WorldTileData, Actor> ArcherPosList = new Dictionary<WorldTileData, Actor>();
        //public List<WorldTileData> ArcherPosTile = new List<WorldTileData>();

        //public List<Actor> insideTroops = new List<Actor>();

        public BuildingData gateBottom = null;
        public BuildingData leftcorner = null;
        public BuildingData rightcorner = null;
        public BuildingData horizontalWall = null;

        public bool Alert = false;
        public bool sameRaceWar = true;
        // public bool gateClosed = false;
        public int curHealth = 10000;
        public int base_health = 10000; // total health
    }
}
