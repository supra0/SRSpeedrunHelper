using UnityEngine;

namespace SRSpeedrunHelper
{
    /*
     * This was my first attempt at implementing the spawner info popups.
     * It didn't work out, for various reasons.
     * Keeping this here just in case I want to reuse something from it later.
     */

    class SpawnerTriggerAddon : MonoBehaviour
    {
        private static float SPHERE_SCALE = 3.0f;
        private static float SPHERE_COLOR_ALPHA = 0.7f;
        private static Color SPHERE_INACTIVE_COLOR = new Color(1.0f, 0.0f, 0.0f, SPHERE_COLOR_ALPHA);
        private static Color SPHERE_ACTIVE_COLOR = new Color(0.0f, 1.0f, 0.0f, SPHERE_COLOR_ALPHA);

        private GameObject spawnerSphere;
        private Collider spawnerSphereCollider;
        private Renderer spawnerSphereRenderer;

        public SpawnerTrigger spawnerTrigger;

        public SpawnerTriggerAddon()
        {

        }

        void Awake()
        {
            spawnerTrigger = GetComponentInParent<SpawnerTrigger>();
            if(spawnerTrigger == null)
            {
                SRSpeedrunHelper.Log("Warning: SpawnerTrigger not found in SpawnerTriggerAddon.Awake");
            }
        }

        void Start()
        {
            // Set up sphere that indicates where the spawner is
            spawnerSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spawnerSphere.transform.position = spawnerTrigger.spawner.transform.position; // set to position of spawner, not spawner trigger
            spawnerSphere.transform.localScale = new Vector3(SPHERE_SCALE, SPHERE_SCALE, SPHERE_SCALE);
            spawnerSphere.transform.SetParent(this.transform);

            spawnerSphereCollider = spawnerSphere.GetComponent<Collider>();
            if(spawnerSphereCollider == null)
            {
                SRSpeedrunHelper.Log("Error (SpawnerTriggerAddon.Start): spawnerSphereCollider is null!");
            }
            spawnerSphereCollider.isTrigger = false;

            spawnerSphereRenderer = spawnerSphere.GetComponent<Renderer>();
            if (spawnerSphereCollider == null)
            {
                SRSpeedrunHelper.Log("Error (SpawnerTriggerAddon.Start): spawnerSphereCollider is null!");
            }
            spawnerSphereRenderer.material.color = SPHERE_INACTIVE_COLOR;
        }

        public void SetIsBeingLookedAt(bool isBeingLookedAt)
        {
            spawnerSphereRenderer.material.color = isBeingLookedAt ? SPHERE_ACTIVE_COLOR : SPHERE_INACTIVE_COLOR;
        }

        public void HideSpheres()
        {
            if(spawnerSphere == null)
            {
                SRSpeedrunHelper.Log("Error (SpawnerTriggerAddon.HideSpheres): spawnerSphere is null!");
                return;
            }
            spawnerSphere.SetActive(false);
        }

        public void ShowSpheres()
        {
            if (spawnerSphere == null)
            {
                SRSpeedrunHelper.Log("Error (SpawnerTriggerAddon.ShowSpheres): spawnerSphere is null!");
                return;
            }
            spawnerSphere.SetActive(true);
        }
    }
}
