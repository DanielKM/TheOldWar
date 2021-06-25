// Copyright © 2018 Procedural Worlds Pty Limited.  All Rights Reserved.
using UnityEngine;
using UnityEditor;
using PWCommon5;

namespace Gaia.Internal
{
    public class GaiaStdMenu : Editor
    {
        /// <summary>
        /// Show tutorials
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Gaia/Show Gaia Tutorials...", false, 60)]
        public static void ShowTutorial()
        {
            Application.OpenURL(PWApp.CONF.TutorialsLink);
        }

        /// <summary>
        /// Show support page
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Gaia/Show Gaia Support, Lodge a Ticket...", false, 61)]
        public static void ShowSupport()
        {
            Application.OpenURL(PWApp.CONF.SupportLink);
        }

        /// <summary>
        /// Show review option
        /// </summary>
        [MenuItem("Window/" + PWConst.COMMON_MENU + "/Gaia/Please Review Gaia...", false, 62)]
        public static void ShowProductAssetStore()
        {
            Application.OpenURL(PWApp.CONF.ASLink);
        }

    }
}
