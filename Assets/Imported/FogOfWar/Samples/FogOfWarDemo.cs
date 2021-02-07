using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace FoW
{
    [System.Serializable]
    public class DemoDescription
    {
        public string name;
        public string sceneName;
        [Multiline]
        public string info;
    }

    public class FogOfWarDemo : MonoBehaviour
    {
        public Button previousButton;
        public Button nextButton;
        public Text demoNameText;
        public Text demoInfoText;
        public Button demoInfoButton;
        public GameObject demoInfoPanel;
        public string scenePostfix;
        public DemoDescription[] demoScenes;

        int _currentSceneIndex = 0;
        Scene? _loadedScene = null;

        void Awake()
        {
            previousButton.onClick.AddListener(() => ChangeScene(-1));
            nextButton.onClick.AddListener(() => ChangeScene(1));
            demoInfoButton.onClick.AddListener(() => demoInfoPanel.SetActive(!demoInfoPanel.activeSelf));

            UpdateSceneUI();
        }

        void ChangeScene(int amount)
        {
            _currentSceneIndex = (_currentSceneIndex + amount) % demoScenes.Length;
            if (_currentSceneIndex < 0)
                _currentSceneIndex += demoScenes.Length;
            UpdateSceneUI();
        }

        void UpdateSceneUI()
        {
            if (_loadedScene.HasValue)
            {
                SceneManager.UnloadSceneAsync(_loadedScene.Value);
                _loadedScene = null;
            }

            DemoDescription desc = demoScenes[_currentSceneIndex];
            string scenename = desc.sceneName + scenePostfix;
            SceneManager.LoadSceneAsync(scenename, LoadSceneMode.Additive);
            demoNameText.text = desc.name;
            demoInfoText.text = desc.info;
            _loadedScene = SceneManager.GetSceneByName(scenename);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
        }
    }
}
