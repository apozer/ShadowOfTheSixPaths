using System;
using System.Collections.Generic;
using ThunderRoad;
using UnityEngine;

namespace Jutsu.Rinnegan.DevaPath
{
    public class PathsOfPain : MonoBehaviour
    {
        public static List<Creature> obtainedCreatures = new List<Creature>();
        public Tuple<string, Creature, Material, bool> pathData;
    }
}