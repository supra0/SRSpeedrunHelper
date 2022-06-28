using System.Collections.Generic;
using System.Reflection;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

namespace SRSpeedrunHelper
{
    class SpawnerInfoNode : MonoBehaviour
    {
        private static List<SpawnerInfoNode> allSpawnerInfoNodes;
        private static readonly FieldInfo spawnerTriggerModelField = typeof(SpawnerTrigger).GetField("model", BindingFlags.NonPublic | BindingFlags.Instance);

        private static float SPHERE_SCALE = 3.0f;
        private static float SPHERE_COLOR_ALPHA = 0.7f;
        private static Color SPHERE_INACTIVE_COLOR = new Color(1.0f, 0.0f, 0.0f, SPHERE_COLOR_ALPHA);
        private static Color SPHERE_ACTIVE_COLOR = new Color(0.0f, 1.0f, 0.0f, SPHERE_COLOR_ALPHA);

        public SpawnerTrigger SpawnerTrigger { get; private set; }
        
        //private string infoText = null;

        #region Instance Methods
        void Start()
        {
            transform.localScale = new Vector3(SPHERE_SCALE, SPHERE_SCALE, SPHERE_SCALE);
            //GetComponent<Collider>().isTrigger = true; // Would like to have this be a trigger so that nothing collides with it, but then the Raycast only hits it when the player is also standing inside of the object?
            GetComponent<Renderer>().material.color = SPHERE_INACTIVE_COLOR;
        }

        // TODO: Make this more efficient by only recalculating when necessary. Need to be able to tell when the spawner info display settings change and when a spawn has occurred
        public string GetInfoText()
        {
            // Show slimes and times
            string text = "";
            SpawnerTrigger st = SpawnerTrigger; //temp, whole method needs refactor that will happen as part of above TODO ^^^

            foreach (DirectedActorSpawner.SpawnConstraint constraint in st.spawner.constraints)
            {
                string t = "";
                DirectedActorSpawner.TimeMode timeMode = constraint.window.timeMode;
                switch (timeMode)
                {
                    case DirectedActorSpawner.TimeMode.ANY:
                        t = "Any Time:";
                        break;
                    case DirectedActorSpawner.TimeMode.DAY:
                        t = "Daytime:";
                        break;
                    case DirectedActorSpawner.TimeMode.NIGHT:
                        t = "Nighttime:";
                        break;
                    default:
                        t = "Custom Time: " + constraint.window.startHour + "-" + constraint.window.endHour;
                        break;
                }
                text += t + "\n";

                float weightsSum = 0.0f;

                if (SRSpeedrunHelper.spawnerConvertToPercentage)
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

                    if (SRSpeedrunHelper.spawnerConvertToPercentage)
                    {
                        text += ": " + (slimeSet.weight / weightsSum) * 100 + "%\n";
                    }
                    else
                    {
                        text += ": " + slimeSet.weight + "\n";
                    }

                }

                text += "\n";
            }

            // Add more data based on settings
            if (SRSpeedrunHelper.spawnerShowCountRange)
            {
                text += "Spawn amount: " + st.minSpawn + " - " + st.maxSpawn + "\n";
            }

            if (SRSpeedrunHelper.spawnerShowTriggerRate)
            {
                if (SRSpeedrunHelper.spawnerConvertToPercentage)
                {
                    text += "Spawn chance: " + st.chanceOfTrigger * 100 + "%\n";
                }
                else
                {
                    text += "Spawn chance: " + st.chanceOfTrigger + "\n";
                }
            }
            if (SRSpeedrunHelper.spawnerShowAvgNextSpawn)
            {
                text += "Avg. hours until next spawn chance: " + st.avgGameHoursBetweenTrigger + "\n";
            }

            if (SRSpeedrunHelper.spawnerShowNextSpawnTime)
            {
                SpawnerTriggerModel model = (SpawnerTriggerModel)spawnerTriggerModelField.GetValue(st);
                if (model != null)
                {
                    int nextTriggerTime = (int)model.nextTriggerTime;

                    int day = (nextTriggerTime / 3600 / 24) + 1;
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

            return text;
        }

        public void SetIsBeingLookedAt(bool isBeingLookedAt)
        {
            GetComponent<Renderer>().material.color = isBeingLookedAt ? SPHERE_ACTIVE_COLOR : SPHERE_INACTIVE_COLOR;
        }

        public void ForceSpawn()
        {
            if(SpawnerTrigger == null)
            {
                SRSpeedrunHelper.Log("ForceSpawn: The SpawnerTrigger we're trying to force a spawn on is null!");
                return;
            }

            // Spawn logic copied directly from SpawnerTrigger.OnTriggerEnter
            float num = (SpawnerTrigger.spawner is DirectedSlimeSpawner) ? SRSingleton<SceneContext>.Instance.ModDirector.SlimeCountFactor() : 1f;
            SpawnerTrigger.StartCoroutine(SpawnerTrigger.spawner.Spawn(Mathf.RoundToInt((float)Randoms.SHARED.GetInRange(SpawnerTrigger.minSpawn, SpawnerTrigger.maxSpawn + 1) * num), Randoms.SHARED));
        }

        private void SetSpawnerTrigger(SpawnerTrigger trigger)
        {
            if (SpawnerTrigger != null)
            {
                SRSpeedrunHelper.Log("Error: Trying to set a SpawnerTrigger of a SpawnerInfoNode that already has one.");
                return;
            }

            SpawnerTrigger = trigger;
            transform.position = trigger.spawner.transform.position;
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
                SRSpeedrunHelper.Log("Warning: Tried to create new spawner info nodes while they are already active. Call SpawnerInfoNode.DestroyNodes() first.");
                return;
            }

            allSpawnerInfoNodes = new List<SpawnerInfoNode>();
            List<SpawnerTriggerModel> list = new List<SpawnerTriggerModel>(SRSingleton<SceneContext>.Instance.GameModel.AllSpawnerTriggers());

            FieldInfo part = typeof(SpawnerTriggerModel).GetField("part", BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (SpawnerTriggerModel model in list)
            {
                GameObject tmp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                SpawnerInfoNode infoNodeTmp = tmp.AddComponent<SpawnerInfoNode>();
                
                SpawnerTrigger triggerTmp = (SpawnerTrigger)part.GetValue(model);
                infoNodeTmp.SetSpawnerTrigger(triggerTmp);

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
