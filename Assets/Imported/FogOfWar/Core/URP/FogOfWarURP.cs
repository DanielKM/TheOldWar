using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FoW
{
    [System.Serializable, VolumeComponentMenu("FogOfWarURP")]
    public sealed class FogOfWarURP : VolumeComponent, IPostProcessComponent
    {
        public IntParameter team = new IntParameter(0);
        public BoolParameter fogFarPlane = new BoolParameter(true);
        public FloatParameter outsideFogStrength = new ClampedFloatParameter(1f, 0f, 1f);
        public BoolParameter pointFiltering = new BoolParameter(false);

        [Header("Color")]
        public ColorParameter fogColor = new ColorParameter(Color.clear);
        public TextureParameter fogColorTexture = new TextureParameter(null);
        public BoolParameter fogTextureScreenSpace = new BoolParameter(false);
        public FloatParameter fogColorTextureScale = new FloatParameter(1);
        public FloatParameter fogColorTextureHeight = new FloatParameter(0);
    
        public bool IsActive() => Application.isPlaying && fogColor.value.a > 0.001f;

        public bool IsTileCompatible() => false;
    }
}
