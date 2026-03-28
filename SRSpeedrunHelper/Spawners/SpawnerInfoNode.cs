using System;
using System.Collections.Generic;
using System.Reflection;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

namespace SRSpeedrunHelper.Spawners
{
    class SpawnerInfoNode : MonoBehaviour
    {
        public enum SlimeSpawnerType
        {
            None,
            Triggered,
            Directed
        }

        private static List<SpawnerInfoNode> allSpawnerInfoNodes;
        private static readonly FieldInfo spawnerTriggerModelField = typeof(SpawnerTrigger).GetField("model", BindingFlags.NonPublic | BindingFlags.Instance);

        private static float SPHERE_SCALE = 2.0f;
        private static float SPHERE_COLOR_ALPHA = 1.0f;
        private static Color SPHERE_INACTIVE_COLOR = new Color(1.0f, 0.0f, 0.0f, SPHERE_COLOR_ALPHA);
        private static Color SPHERE_ACTIVE_COLOR = new Color(0.0f, 1.0f, 0.0f, SPHERE_COLOR_ALPHA);

        public DirectedActorSpawner Spawner { get; private set; }
        public SpawnerTrigger SpawnerTrigger { get; private set; }
        public CellDirector CellDirector { get; private set; }
        public SlimeSpawnerType spawnerType = SlimeSpawnerType.None;

        private static readonly FieldInfo allCellDirectorsFieldInfo = typeof(CellDirector).GetField("allCellDirectors", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo spawnersFieldInfo = typeof(CellDirector).GetField("spawners", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo partFieldInfo = typeof(SpawnerTriggerModel).GetField("part", BindingFlags.NonPublic | BindingFlags.Instance);

        internal const string RAYCAST_LAYER_NAME = "RaycastOnly";
        internal static readonly int raycastOnlyLayer = LayerMask.NameToLayer(RAYCAST_LAYER_NAME);
        internal static readonly int raycastOnlyMask = LayerMask.GetMask(RAYCAST_LAYER_NAME);

        #region Instance Methods
        void Start()
        {
            transform.localScale = new Vector3(SPHERE_SCALE, SPHERE_SCALE, SPHERE_SCALE);
            GetComponent<Renderer>().material.color = SPHERE_INACTIVE_COLOR;
            /*
            SRSpeedrunHelper.Log("Layer: " + gameObject.layer + " (" + LayerMask.LayerToName(gameObject.layer) + ")");
            SRSpeedrunHelper.Log("raycastOnlyLayer: " + raycastOnlyLayer);
            SRSpeedrunHelper.Log("raycastOnlyMask: " + raycastOnlyMask);
            */
        }

        // TODO (OLD): Make this more efficient by only recalculating when necessary. Need to be able to tell when the spawner info display settings change and when a spawn has occurred
        // TODO 2026 UPDATE: ^ Good idea, but probably not the right approach. Future refactor maybe to have Options for each category (Gordo, Spawners, etc) as their own classes. i.e. the options for Spawner display would be like a SpawnerOptions class that can be passed into methods like this. Subclass of SpawnerGUI? not sure on exact implementation
        public string GetInfoText()
        {
            string text = "";
            text += "Spawner type: " + spawnerType.ToString() + "\n\n";

            //temp, whole method needs refactor that will happen as part of above TODO ^^^
            SpawnerTrigger st = SpawnerTrigger;
            CellDirector cd = CellDirector;

            foreach (DirectedActorSpawner.SpawnConstraint constraint in Spawner.constraints)
            {
                string t = "";

                DirectedActorSpawner.TimeMode timeMode = constraint.window.timeMode;
                switch (timeMode)
                {
                    case DirectedActorSpawner.TimeMode.ANY:
                        t = "Any Time:";
                        break;
                    case DirectedActorSpawner.TimeMode.DAY:
                        t = "Day:";
                        break;
                    case DirectedActorSpawner.TimeMode.NIGHT:
                        t = "Night:";
                        break;
                    default:
                        t = "Custom Time: " + constraint.window.startHour + "-" + constraint.window.endHour;
                        break;
                }
                text += t + "\n";

                float weightsSum = 0.0f;

                if (SpawnerGUI.spawnerConvertToPercentage)
                {
                    foreach (SlimeSet.Member slimeSet in constraint.slimeset.members)
                    {
                        weightsSum += slimeSet.weight;
                    }
                }

                foreach (SlimeSet.Member slimeSet in constraint.slimeset.members)
                {
                    string tmp = slimeSet.prefab.ToString();
                    text += tmp.Substring(0, tmp.IndexOf(" "));

                    if (SpawnerGUI.spawnerConvertToPercentage)
                    {
                        double percentage = (double)(slimeSet.weight / weightsSum * 100);
                        if(SpawnerGUI.spawnerRoundPercentage)
                        {
                            percentage = Math.Round(percentage, 2);
                        }
                        text += ": " + percentage + "%\n";
                    }
                    else
                    {
                        text += ": " + slimeSet.weight + "\n";
                    }

                }

                text += "\n";
            }

            if(spawnerType == SlimeSpawnerType.Directed)
            { 
                if (SpawnerGUI.spawnerShowCountRange)
                {
                    text += "Spawn amount: " + cd.minPerSpawn + " - " + cd.maxPerSpawn + "\n\n";
                }

                text += "Cell info:\n";
                text += "Name: " + cd.gameObject.name + "\n";
                text += "Target Slime count: " + cd.targetSlimeCount + "\n";
                if(cd.cullSlimesLimit == int.MaxValue)
                {
                    text += "Max # of Slimes before culling: No Limit\n";
                }
                else
                {
                    text += "Max # of Slimes before culling: " + cd.cullSlimesLimit + "\n";
                }

                text += "avgSpawnTimeGameHours: " + cd.avgSpawnTimeGameHours + "\n";
            }

            if(spawnerType == SlimeSpawnerType.Triggered)
            {
                if (SpawnerGUI.spawnerShowCountRange)
                {
                    text += "Spawn amount: " + st.minSpawn + " - " + st.maxSpawn + "\n";
                }

                if (SpawnerGUI.spawnerShowTriggerRate)
                {
                    if (SpawnerGUI.spawnerConvertToPercentage)
                    {
                        text += "Spawn chance: " + st.chanceOfTrigger * 100 + "%\n";
                    }
                    else
                    {
                        text += "Spawn chance: " + st.chanceOfTrigger + "\n";
                    }
                }
                if (SpawnerGUI.spawnerShowAvgNextSpawn)
                {
                    text += "Avg. hours until next spawn chance: " + st.avgGameHoursBetweenTrigger + "\n";
                }

                if (SpawnerGUI.spawnerShowNextSpawnTime)
                {
                    SpawnerTriggerModel model = (SpawnerTriggerModel)spawnerTriggerModelField.GetValue(st);
                    if (model != null)
                    {
                        int nextTriggerTime = (int)model.nextTriggerTime;

                        int day = nextTriggerTime / 3600 / 24 + 1;
                        int hour = nextTriggerTime / 3600 % 24;
                        int minute = nextTriggerTime % 60;

                        text += "Next possible spawn time: \n";
                        text += $"Day {day}, {hour:00}:{minute:00}";
                    }
                    else
                    {
                        text += "Could not determine next spawn time";
                    }
                }
            }

            return text;
        }

        public void SetIsBeingLookedAt(bool isBeingLookedAt)
        {
            GetComponent<Renderer>().material.color = isBeingLookedAt ? SPHERE_ACTIVE_COLOR : SPHERE_INACTIVE_COLOR;
        }

        // Spawn a single Slime
        // TODO: doesn't work lol. removed for now, look into fix later. this is not important.
        public void ForceSpawn()
        {
            Spawner.Spawn(1, Randoms.SHARED);

            /* Old version that respects the settings of the SpawnerTrigge
            if(SpawnerTrigger == null)
            {
                SRSpeedrunHelper.Log("ForceSpawn: The SpawnerTrigger we're trying to force a spawn on is null!");
                return;
            }

            // Spawn logic copied directly from SpawnerTrigger.OnTriggerEnter
            float num = SpawnerTrigger.spawner is DirectedSlimeSpawner ? SRSingleton<SceneContext>.Instance.ModDirector.SlimeCountFactor() : 1f;
            SpawnerTrigger.StartCoroutine(SpawnerTrigger.spawner.Spawn(Mathf.RoundToInt(Randoms.SHARED.GetInRange(SpawnerTrigger.minSpawn, SpawnerTrigger.maxSpawn + 1) * num), Randoms.SHARED));
            */
        }

        private void SetSpawner(DirectedActorSpawner spawner, SpawnerTrigger trigger = null, CellDirector cellDirector = null)
        {
            if (Spawner != null)
            {
                SRSpeedrunHelper.Log("Error: Trying to set a SpawnerTrigger of a SpawnerInfoNode that already has one.");
                return;
            }

            Spawner = spawner;
            SpawnerTrigger = trigger;
            CellDirector = cellDirector;
            transform.position = spawner.transform.position;
        }
        #endregion

        #region Static Methods
        public static void ActivateNodes()
        {
            if(allSpawnerInfoNodes == null)
            {
                CreateNodes();
            }

            foreach(SpawnerInfoNode node in allSpawnerInfoNodes)
            {
                node.gameObject.SetActive(true);
            }
        }

        public static void DeactivateNodes()
        {
            if(allSpawnerInfoNodes != null)
            {
                foreach(SpawnerInfoNode node in allSpawnerInfoNodes)
                {
                    node.gameObject.SetActive(false);
                }
            }
        }

        public static void ClearNodes()
        {
            allSpawnerInfoNodes = null;
        }

        private static void CreateNodes()
        {
            if(allSpawnerInfoNodes != null)
            {
                SRSpeedrunHelper.Log("Warning: Tried to create new spawner info nodes while they are already active.");
                return;
            }
            allSpawnerInfoNodes = new List<SpawnerInfoNode>();

            // Find all Slime spawners that are directed by a CellDirector
            List<CellDirector> allCellDirs = (List<CellDirector>)allCellDirectorsFieldInfo.GetValue(null);
            foreach(CellDirector cellDir in allCellDirs)
            {
                List<DirectedSlimeSpawner> spawners = (List<DirectedSlimeSpawner>)spawnersFieldInfo.GetValue(cellDir);
                foreach(DirectedActorSpawner spawner in spawners)
                {
                    GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    tmp.layer = raycastOnlyLayer;
                    SpawnerInfoNode infoNodeTmp = tmp.AddComponent<SpawnerInfoNode>();

                    infoNodeTmp.spawnerType = SlimeSpawnerType.Directed;
                    infoNodeTmp.SetSpawner(spawner, null, cellDir);
                    allSpawnerInfoNodes.Add(infoNodeTmp);
                }
            }

            // Find all Slime spawners that are triggered by a SpawnerTrigger
            List<SpawnerTriggerModel> spawnerTriggers = new List<SpawnerTriggerModel>(SRSingleton<SceneContext>.Instance.GameModel.AllSpawnerTriggers());
            foreach (SpawnerTriggerModel model in spawnerTriggers)
            {
                GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                tmp.layer = raycastOnlyLayer;
                SpawnerInfoNode infoNodeTmp = tmp.AddComponent<SpawnerInfoNode>();
                SpawnerTrigger triggerTmp = (SpawnerTrigger)partFieldInfo.GetValue(model);

                infoNodeTmp.spawnerType = SlimeSpawnerType.Triggered;
                infoNodeTmp.SetSpawner(triggerTmp.spawner, triggerTmp);
                allSpawnerInfoNodes.Add(infoNodeTmp);
            }
    }

        /*
        private static void DestroyNodes()
        {
            foreach(SpawnerInfoNode node in allSpawnerInfoNodes)
            {
                GameObject.Destroy(node.gameObject);
            }

            allSpawnerInfoNodes = null;
        }
        */
        #endregion
    }
}
