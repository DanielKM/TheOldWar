using UnityEngine;
using UnityEditor;
namespace GeNa.Core
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GeNaTerrainDecorator))]
    public class GeNaTerrainDecoratorEditor : GeNaDecoratorEditor<GeNaTerrainDecorator>
    {
        [MenuItem("GameObject/GeNa/Decorators/Terrain Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaTerrainDecorator decorator = gameObject.AddComponent<GeNaTerrainDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected GeNaTerrainDecorator[] m_tree;
        protected TerrainTools m_terrainTools;
        protected TerrainEntity m_terrainEntity;
        public TerrainTools TerrainTools
        {
            get
            {
                if (m_terrainTools == null)
                {
                    GeNaManager gm = GeNaManager.GetInstance();
                    m_terrainTools = gm.TerrainTools;
                }
                return m_terrainTools;
            }
        }
        protected void SelectTree(bool isSelected)
        {
            Transform transform = Decorator.transform;
            Transform root = transform.root;
            m_tree = root.GetComponentsInChildren<GeNaTerrainDecorator>();
            foreach (GeNaTerrainDecorator tree in m_tree)
                tree.IsSelected = isSelected;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            if (Decorator == null)
                return;
            SelectTree(true);
        }
        private void OnDisable()
        {
            if (Decorator == null)
                return;
            SelectTree(false);
            TerrainTools.Dispose();
        }
        public override void OnSceneGUI()
        {
            GeNaTerrainDecorator decorator = target as GeNaTerrainDecorator;
            if (decorator == null)
                return;
            Transform transform = decorator.transform;
            TerrainModifier modifier = decorator.TerrainModifier;
            GeNaEditorUtility.RenderTerrainModifier(transform, modifier);
            //TODO : Manny : Come back to this!
            m_terrainEntity = TerrainTools.GetPaintModification(modifier);
            if (m_terrainEntity != null)
            {
                TerrainTools.Visualize(m_terrainEntity);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
            EditorGUI.BeginChangeCheck();
            {
                EditorUtils.TerrainModifier(Decorator.TerrainModifier, helpEnabled);
            }
            if (EditorGUI.EndChangeCheck())
            {
                // EditorUtility.SetDirty(Decorator);
                foreach (Object o in targets)
                {
                    GeNaTerrainDecorator decorator = (GeNaTerrainDecorator) o;
                    decorator.TerrainModifier.CopyFrom(Decorator.TerrainModifier);
                    EditorUtility.SetDirty(decorator);
                }
            }
        }
    }
}