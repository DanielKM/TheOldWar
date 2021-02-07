using UnityEngine;

namespace FoW
{
    public class FogOfWar2DSimpleAnimator : MonoBehaviour
    {
        public SpriteRenderer spriteRenderer;
        public float cycleDuration = 1;
        public Sprite[] up;
        public Sprite[] down;
        public Sprite[] left;
        public Sprite[] right;
        public Vector2 movement { get; set; }
        float _cycleTime = 0;
        Sprite[] _lastSpriteSet = null;

        void LateUpdate()
        {
            if (movement.sqrMagnitude < 0.01f)
                _cycleTime = 0;
            else
                _cycleTime += Time.deltaTime;

            if (movement.sqrMagnitude > 0.0001f)
            {
                if (Mathf.Abs(movement.y) >= Mathf.Abs(movement.x))
                    _lastSpriteSet = movement.y > 0 ? up : down;
                else
                    _lastSpriteSet = movement.x > 0 ? right : left;
            }
            else if (_lastSpriteSet == null)
                _lastSpriteSet = down;

            float t = _cycleTime / cycleDuration;
            int frame = Mathf.FloorToInt(t * _lastSpriteSet.Length) % _lastSpriteSet.Length;
            spriteRenderer.sprite = _lastSpriteSet[frame];
        }
    }
}
