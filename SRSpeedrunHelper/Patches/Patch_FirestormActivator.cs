using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using MonomiPark.SlimeRancher.DataModel;

namespace SRSpeedrunHelper.Patches
{
    [HarmonyPatch(typeof(FirestormActivator))]
    [HarmonyPatch("Update")]
    static class Patch_FirestormActivator
    {
        static void Prefix(FirestormActivator __instance)
        {
            if(SRSpeedrunHelper.disableFirestorms)
            {
                WorldModel worldModel = SceneContext.Instance.GameModel.GetWorldModel();
                worldModel.nextFirecolumnTime = double.PositiveInfinity;
                worldModel.nextFirestormTime = double.PositiveInfinity;
            }
        }
    }
}
