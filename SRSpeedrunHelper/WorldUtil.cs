using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SRSpeedrunHelper
{
    internal static class WorldUtil
    {
        private static readonly FieldInfo isLoadingFieldInfo = typeof(AutoSaveDirector).GetField("loadingGame", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static bool IsGameLoading()
        {
            return (bool)isLoadingFieldInfo.GetValue(SRSingleton<GameContext>.Instance.AutoSaveDirector);
        }

    }
}
