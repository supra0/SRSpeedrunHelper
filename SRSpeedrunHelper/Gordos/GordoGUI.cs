using MonomiPark.SlimeRancher.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SRSpeedrunHelper.Gordos
{
    internal static class GordoGUI
    {
        private static Vector2 gordoScrollPosition = Vector2.zero;

        internal static void DoGUI()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Pop all Gordos"))
            {
                foreach (string gordoId in GordoUtil.gordoIdsOrdered)
                {
                    GordoUtil.PopGordo(gordoId);
                }
            }
            if (GUILayout.Button("Reset all Gordos"))
            {
                foreach (string gordoId in GordoUtil.gordoIdsOrdered)
                {
                    GordoUtil.ResetGordo(gordoId);
                }
            }
            GUILayout.EndHorizontal();

            gordoScrollPosition = GUILayout.BeginScrollView(gordoScrollPosition);

            GameModel gameModel = SRSingleton<SceneContext>.Instance.GameModel;

            // Present them in the order defined in GordoHelper.gordoIdsOrdered
            // Excludes Party Gordos, Gold Gordos (Rush Mode), and snared Gordos
            foreach (string gordoId in GordoUtil.gordoIdsOrdered)
            {
                //GordoModel gordoModel = gameModel.GetGordoModel(gordoId);

                if (GordoUtil.gordoIdToName.TryGetValue(gordoId, out string gordoName))
                {
                    GUILayout.Label(gordoName, SRSpeedrunHelper.LABEL_STYLE_BOLD);
                }

                GUILayout.Label(GordoUtil.GetGordoStatus(gordoId), SRSpeedrunHelper.LABEL_STYLE_DEFAULT);
                if (GUILayout.Button("Pop Gordo"))
                {
                    GordoUtil.PopGordo(gordoId);
                }

                if (GUILayout.Button("Reset Gordo"))
                {
                    GordoUtil.ResetGordo(gordoId);
                }
            }

            GUILayout.EndScrollView();
        }
    }
}
