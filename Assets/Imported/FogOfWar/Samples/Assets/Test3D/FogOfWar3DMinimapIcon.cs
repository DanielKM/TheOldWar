using UnityEngine;

namespace FoW
{
    public class FogOfWar3DMinimapIcon : MonoBehaviour
    {
        Transform _transform;

        void Awake()
        {
            _transform = transform;
        }

        void LateUpdate()
        {
            _transform.rotation = Quaternion.identity;
        }
    }
}
