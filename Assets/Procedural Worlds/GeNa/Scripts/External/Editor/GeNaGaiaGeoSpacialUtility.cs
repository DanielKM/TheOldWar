using UnityEditor;
using UnityEngine;

namespace GeNa.Core
{
    [InitializeOnLoad]
    public class GeNaGaiaGeoSpacialUtility
    {
        static GeNaGaiaGeoSpacialUtility() => GeNaEvents.BakeSpline = GeoSpacialBakeSpline;
        /// <summary>
        /// Function used to execute all the extensions and then bake the mesh which returns the baked gameobject
        /// </summary>
        /// <param name="splineParent"></param>
        /// <param name="splineObject"></param>
        /// <param name="spline"></param>
        /// <param name="executeExtensions"></param>
        /// <returns></returns>
        private static GameObject GeoSpacialBakeSpline(GameObject splineParent, GeNaSpline spline) => GeNaUtility.BakeSpline(splineParent, spline);
    }
}