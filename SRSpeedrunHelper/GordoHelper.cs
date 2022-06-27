using System.Collections.Generic;
using System.Reflection;
using MonomiPark.SlimeRancher.DataModel;
using UnityEngine;

namespace SRSpeedrunHelper
{
    static class GordoHelper
    {
        #region Gordo IDs
        public static readonly Dictionary<string, string> gordoIdToName = new Dictionary<string, string>()
        {
            { "gordo1686430858", "Pink Gordo (Main)" },
            { "gordo1440735342", "Pink Gordo (Ring Island)" },
            { "gordo0806598363", "Tabby Gordo (Ring Island)" },
            { "gordo0966530436", "Tabby Gordo (Overgrowth)" },
            { "gordo1173217994", "Phosphor Gordo" },
            { "gordo2083992877", "Honey Gordo" },
            { "gordo1294483236", "Hunter Gordo" },
            { "gordo1831887310", "Rock Gordo (Cave)" },
            { "gordo0983207065", "Rock Gordo (Ash Isle)" },
            { "gordo0769818715", "Rad Gordo" },
            { "gordo1457275006", "Crystal Gordo" },
            { "gordo0190962505", "Quantum Gordo" },
            { "gordo0984460099", "Boom Gordo" },
            { "gordo1556645272", "Dervish Gordo" },
            { "gordo1480216957", "Tangle Gordo" },
            { "gordo0359370083", "Mosaic Gordo" }
        };

        public static readonly List<string> gordoIdsOrdered = new List<string>
        {
            "gordo1686430858",
            "gordo1440735342",
            "gordo0806598363",
            "gordo0966530436",
            "gordo1173217994",
            "gordo2083992877",
            "gordo1294483236",
            "gordo1831887310",
            "gordo0983207065",
            "gordo0769818715",
            "gordo1457275006",
            "gordo0190962505",
            "gordo0984460099",
            "gordo1556645272",
            "gordo1480216957",
            "gordo0359370083",
        };
        #endregion

        private static readonly FieldInfo targetCountField = typeof(GordoModel).GetField("targetCount", BindingFlags.Instance | BindingFlags.NonPublic);

        #region Helper Methods
        public static void PopGordo(string gordoId)
        {
            GordoModel gordoModel = SceneContext.Instance.GameModel.GetGordoModel(gordoId);
            if(gordoModel != null)
            {
                // Don't pop already popped gordos
                if(gordoModel.gordoEatenCount == -1)
                {
                    return;
                }
                gordoModel.gordoEatenCount = (int)targetCountField.GetValue(gordoModel);

                GameObject gameObject = (GameObject)typeof(GordoModel).GetField("gameObj", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gordoModel);
                GordoEat gordoEat = gameObject.GetComponent<GordoEat>();
                if(gordoEat != null)
                {
                    MethodInfo immediateReachedTarget = typeof(GordoEat).GetMethod("ImmediateReachedTarget", BindingFlags.Instance | BindingFlags.NonPublic);
                    immediateReachedTarget.Invoke(gordoEat, null);
                }
            }
        }

        public static void ResetGordo(string gordoId)
        {
            GordoModel gordoModel = SRSingleton<SceneContext>.Instance.GameModel.GetGordoModel(gordoId);
            if (gordoModel != null) {
                gordoModel.gordoEatenCount = 0;
                gordoModel.SetGameObjectActive(true);

                // Reset gordo expression
                GameObject gameObject = (GameObject)typeof(GordoModel).GetField("gameObj", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(gordoModel);
                GordoEat gordoEat = gameObject.GetComponent<GordoEat>();
                if(gordoEat == null)
                {
                    SRSpeedrunHelper.Log("Warning: GordoEat from GordoModel.gameObj is null. Cannot reset gordo expression.");
                }
                else
                {
                    gordoEat.OnResetEatenCount();
                }
            }
        }

        public static string GetGordoStatus(string gordoId)
        {
            GordoModel gordoModel = SRSingleton<SceneContext>.Instance.GameModel.GetGordoModel(gordoId);
            if(gordoModel.gordoEatenCount == -1)
            {
                return "Status: Popped";
            }
            else
            {
                return "Status: Fed " + gordoModel.gordoEatenCount + "/" + (int)targetCountField.GetValue(gordoModel);
            }
        }
        #endregion
    }
}
