using System.Collections.Generic;
using System.Reflection;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

namespace SRSpeedrunHelper
{
    class SpawnerInfoNode : MonoBehaviour
    {
        private static List<SpawnerInfoNode> allSpawnerInfoNodes;

        private static float SPHERE_SCALE = 3.0f;
        private static float SPHERE_COLOR_ALPHA = 0.7f;
        private static Color SPHERE_INACTIVE_COLOR = new Color(1.0f, 0.0f, 0.0f, SPHERE_COLOR_ALPHA);
        private static Color SPHERE_ACTIVE_COLOR = new Color(0.0f, 1.0f, 0.0f, SPHERE_COLOR_ALPHA);

        public SpawnerTrigger SpawnerTrigger { get; private set; }

        #region Instance Methods
        void Start()
        {
            transform.localScale = new Vector3(SPHERE_SCALE, SPHERE_SCALE, SPHERE_SCALE);
            //GetComponent<Collider>().isTrigger = true; // Would like to have this be a trigger so that nothing collides with it, but then the Raycast only hits it when the player is also standing inside of the object?
            GetComponent<Renderer>().material.color = SPHERE_INACTIVE_COLOR;
        }

        public void SetIsBeingLookedAt(bool isBeingLookedAt)
        {
            GetComponent<Renderer>().material.color = isBeingLookedAt ? SPHERE_ACTIVE_COLOR : SPHERE_INACTIVE_COLOR;
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
