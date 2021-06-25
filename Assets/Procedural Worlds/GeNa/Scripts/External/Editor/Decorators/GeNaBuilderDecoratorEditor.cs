using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    [CustomEditor(typeof(GeNaBuilderDecorator))]
    public class GeNaBuilderDecoratorEditor : GeNaDecoratorEditor<GeNaBuilderDecorator>
    {
        /// <summary>
        /// Currently selected builder - snapping only works if this is selected
        /// </summary>
        public GeNaSubSpawnerDecorator[] m_subSpawnerDecorators = null;
        public GeNaTerrainDecorator[] m_terrainDecorators = null;
        public GeNaBuilderDecorator m_selectedDecorator = null;
        // private KeyCode m_keyPrevElement;
        // private KeyCode m_keyPrevElementTAB;
        // private KeyCode m_keyNextElement;
        // private KeyCode m_keyNextElementTAB;
        private void ProcessUnityEditorEvent()
        {
            // Event e = Event.current;
            // if (e == null)
            //     return;
            //
            // //Nothing to do if this is not an origami related structure
            // if (m_selectedDecorator == null)
            //     return;
            // //Grab selected transform
            // Transform selectedTr = m_selectedDecorator.GetSelectedTransform();
            // if (e.type == EventType.KeyDown)
            // {
            //     //Make sure scene window seleted
            //     //EditorApplication.ExecuteMenuItem("Window/Scene");
            //
            //     //Element selection
            //     if (!e.control && !e.shift && e.keyCode == m_keyPrevElement)
            //     {
            //         if (selectedTr.parent == null)
            //         {
            //         }
            //         else
            //         {
            //             int idx = selectedTr.GetSiblingIndex();
            //             if (idx > 0)
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.GetChild(idx - 1).gameObject;
            //             }
            //             else
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.gameObject;
            //             }
            //         }
            //         Event.current.Use();
            //         return;
            //     }
            //     if (!e.control && e.shift && e.keyCode == m_keyPrevElementTAB)
            //     {
            //         if (selectedTr.parent == null)
            //         {
            //         }
            //         else
            //         {
            //             int idx = selectedTr.GetSiblingIndex();
            //             if (idx > 0)
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.GetChild(idx - 1).gameObject;
            //             }
            //             else
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.gameObject;
            //             }
            //         }
            //         Event.current.Use();
            //         return;
            //     }
            //     if (!e.control && !e.shift && e.keyCode == m_keyNextElement)
            //     {
            //         if (selectedTr.parent == null)
            //         {
            //             if (selectedTr.childCount > 0)
            //             {
            //                 Selection.activeGameObject = selectedTr.GetChild(0).gameObject;
            //             }
            //         }
            //         else
            //         {
            //             int idx = selectedTr.GetSiblingIndex();
            //             if (idx < selectedTr.parent.childCount - 1)
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.GetChild(idx + 1).gameObject;
            //             }
            //             else
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.gameObject;
            //             }
            //         }
            //         Event.current.Use();
            //         return;
            //     }
            //     if (!e.control && !e.shift && e.keyCode == m_keyNextElementTAB)
            //     {
            //         if (selectedTr.parent == null)
            //         {
            //             if (selectedTr.childCount > 0)
            //             {
            //                 Selection.activeGameObject = selectedTr.GetChild(0).gameObject;
            //             }
            //         }
            //         else
            //         {
            //             int idx = selectedTr.GetSiblingIndex();
            //             if (idx < selectedTr.parent.childCount - 1)
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.GetChild(idx + 1).gameObject;
            //             }
            //             else
            //             {
            //                 Selection.activeGameObject = selectedTr.parent.gameObject;
            //             }
            //         }
            //         Event.current.Use();
            //         return;
            //     }
            // }
            //
            //     //Increase snap distance
            //     if (e.control && e.keyCode == m_keyIncreaseSnapDistance)
            //     {
            //         SnapXZ *= 2f;
            //         SnapY *= 2f;
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Decrease snap distance
            //     if (e.control && e.keyCode == m_keyDecreaseSnapDistance)
            //     {
            //         SnapXZ /= 2f;
            //         SnapY /= 2f;
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Increase snap rotation
            //     if (e.shift && e.keyCode == m_keyIncreaseSnapRotation)
            //     {
            //         SnapRotation *= 2f;
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Decrease snap rotation
            //     if (e.shift && e.keyCode == m_keyDecreaseSnapRotation)
            //     {
            //         SnapRotation /= 2f;
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Move Left
            //     if (e.control && e.keyCode == m_keyMoveLeft)
            //     {
            //         Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.left;
            //         selectedTr.position += (movement * SnapXZ);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.position = SnapTransformPosition(selectedTr.position);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Move right
            //     if (e.control && e.keyCode == m_keyMoveRight)
            //     {
            //         Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.right;
            //         selectedTr.position += (movement * SnapXZ);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.position = SnapTransformPosition(selectedTr.position);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Move Forward
            //     if (e.control && e.keyCode == m_keyMoveForward)
            //     {
            //         Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.forward;
            //         selectedTr.position += (movement * SnapXZ);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.position = SnapTransformPosition(selectedTr.position);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Move Backward
            //     if (e.control && e.keyCode == m_keyMoveBackward)
            //     {
            //         Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.back;
            //         selectedTr.position += (movement * SnapXZ);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.position = SnapTransformPosition(selectedTr.position);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Move Up
            //     if (e.control && e.keyCode == m_keyMoveUp)
            //     {
            //         Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.up;
            //         selectedTr.position += (movement * SnapY);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.position = SnapTransformPosition(selectedTr.position);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Move Down
            //     if (e.control && e.keyCode == m_keyMoveDown)
            //     {
            //         Vector3 movement = Quaternion.Euler(0F, SceneView.lastActiveSceneView.rotation.eulerAngles.y, 0f) * Vector3.down;
            //         selectedTr.position += (movement * SnapY);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.position = SnapTransformPosition(selectedTr.position);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Rotate X Down
            //     if (e.shift && e.keyCode == m_keyRotateXDown)
            //     {
            //         selectedTr.Rotate(Vector3.right, -SnapRotation);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.localEulerAngles = SnapTransformRotation(selectedTr.localEulerAngles);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Rotate X Up
            //     if (e.shift && e.keyCode == m_keyRotateXUp)
            //     {
            //         selectedTr.Rotate(Vector3.right, SnapRotation);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.localEulerAngles = SnapTransformRotation(selectedTr.localEulerAngles);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Rotate Y Down
            //     if (e.shift && e.keyCode == m_keyRotateYDown)
            //     {
            //         selectedTr.Rotate(Vector3.up, SnapRotation);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.localEulerAngles = SnapTransformRotation(selectedTr.localEulerAngles);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Rotate Y Up
            //     if (e.shift && e.keyCode == m_keyRotateYUp)
            //     {
            //         selectedTr.Rotate(Vector3.up, -SnapRotation);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.localEulerAngles = SnapTransformRotation(selectedTr.localEulerAngles);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Rotate Z Down
            //     if (e.shift &&  e.keyCode == m_keyRotateZDown)
            //     {
            //         selectedTr.Rotate(Vector3.forward, SnapRotation);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.localEulerAngles = SnapTransformRotation(selectedTr.localEulerAngles);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            //
            //     //Rotate Z Up
            //     if (e.shift && e.keyCode == m_keyRotateZUp)
            //     {
            //         selectedTr.Rotate(Vector3.forward, -SnapRotation);
            //         if (SnapEnabled)
            //         {
            //             selectedTr.localEulerAngles = SnapTransformRotation(selectedTr.localEulerAngles);
            //         }
            //         SelectedBuilder.UpdateSelectedBounds();
            //         Event.current.Use();
            //         return;
            //     }
            // }

            // //Snapping - eg mouse movement
            // if (SnapEnabled)
            // {
            //     if (!Utils.Math_ApproximatelyEqual(m_lastSnapPosition, selectedTr.position))
            //     {
            //         selectedTr.position = SnapTransformPosition(selectedTr.position);
            //         m_lastSnapPosition = selectedTr.position;
            //         SelectedBuilder.UpdateSelectedBounds();
            //     }
            //     if (!Utils.Math_ApproximatelyEqual(m_lastSnapRotation, selectedTr.rotation.eulerAngles))
            //     {
            //         selectedTr.rotation = Quaternion.Euler(SnapTransformRotation(selectedTr.rotation.eulerAngles));
            //         m_lastSnapRotation = selectedTr.rotation.eulerAngles;
            //         SelectedBuilder.UpdateSelectedBounds();
            //     }
            // }

            //if (e.type == EventType.MouseDrag || e.type == EventType.MouseUp)
            //{
            //}
        }
        public void OnHierarchyHotkeys(int instanceID, Rect selectionRect)
        {
            ProcessUnityEditorEvent();
        }
        public void HierarchyWindowChanged()
        {
            ProcessUnityEditorEvent();
        }
        /// <summary>
        /// See if this is an origami structure with a valid builder
        /// </summary>
        private void OnSelectionChange()
        {
            m_subSpawnerDecorators = null;
            m_terrainDecorators = null;
            foreach (var go in Selection.gameObjects)
            {
                GeNaBuilderDecorator builder = go.GetComponentInParent<GeNaBuilderDecorator>();
                if (builder != null)
                {
                    m_selectedDecorator = builder;
                    //m_selectedDecorator.DrawGizmos = true;
                    m_selectedDecorator.SetSelectedTransform(go.transform);
                    m_subSpawnerDecorators = m_selectedDecorator.GetComponentsInChildren<GeNaSubSpawnerDecorator>();
                    m_terrainDecorators = m_selectedDecorator.GetComponentsInChildren<GeNaTerrainDecorator>();
                    break;
                }
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            SceneView.duringSceneGui -= OnSceneViewGUI;
            SceneView.duringSceneGui += OnSceneViewGUI;
            
            EditorApplication.hierarchyChanged -= HierarchyWindowChanged;
            EditorApplication.hierarchyChanged += HierarchyWindowChanged;
            
            Selection.selectionChanged -= OnSelectionChange;
            Selection.selectionChanged += OnSelectionChange;
            
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyHotkeys;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyHotkeys;
            
            OnSelectionChange();
        }
        public void OnSceneViewGUI(SceneView sceneView)
        {
            ProcessUnityEditorEvent();
            if (m_subSpawnerDecorators != null)
            {
                foreach (var decorator in m_subSpawnerDecorators)
                {
                    GeNaSpawner spawner = decorator.SubSpawner;
                    if (spawner == null)
                        continue;
                    GeNaSpawnerData spawnerData = spawner.SpawnerData;
                    Transform transform = decorator.transform;
                    Vector3 position = transform.position;
                    GeNaEditorUtility.RenderSpawnRange(spawnerData, position);
                }
            }
            if (m_terrainDecorators != null)
            {
                foreach (var decorator in m_terrainDecorators)
                {
                    Transform transform = decorator.transform;
                    TerrainModifier modifier = decorator.TerrainModifier;
                    GeNaEditorUtility.RenderTerrainModifier(transform, modifier);
                }
            }
        }
        [MenuItem("GameObject/GeNa/Decorators/Builder Decorator")]
        public static void AddDecorator(MenuCommand command)
        {
            GameObject gameObject = command.context as GameObject;
            if (gameObject != null)
            {
                GeNaBuilderDecorator decorator = gameObject.AddComponent<GeNaBuilderDecorator>();
                GeNaDecoratorEditorUtility.RegisterDecorator(gameObject, decorator);
            }
        }
        protected override void RenderPanel(bool helpEnabled)
        {
        }
    }
}