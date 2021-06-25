using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Gaia
{
    /// <summary>
    /// Simple Example Loading screen - you can build your own loading screen in a similar fashion by following this example
    /// In a nutshell there are 3 events from the Terrain Loader Manager you can subscribe to - when progress tracking has started, 
    /// when it updates and when it ends. What you do in those events is up to you, but normally you would initialize your loading screen on start
    /// update the progress bar (or a loading %, etc.) while it updates, and then close / shut down the loading screen when it ends.
    /// </summary>
    public class GaiaLoadingScreen : MonoBehaviour
    {
        public Slider m_progressBar;
        public Canvas m_canvas;
        public Image m_image;
        public Text m_text;
        public bool m_initialized;
        public float m_fadeOutSpeed;
        bool m_fadeout;

#if GAIA_PRO_PRESENT
        void Start()
        {
            //Subscribe to these events to update your loading screen according to what is happening in the Load Tracking in the Terrain Loader Manager.
            TerrainLoaderManager.Instance.OnLoadProgressStarted += OnLoadProgressStarted;
            TerrainLoaderManager.Instance.OnLoadProgressUpdated += OnLoadProgressUpdated;
            TerrainLoaderManager.Instance.OnLoadProgressStarted += OnLoadProgressEnded;
            TerrainLoaderManager.Instance.OnLoadProgressTimeOut += OnLoadProgressTimeOut;
        }

        private void OnDestroy()
        {
            //Unsubscribe when the loading screen is destroyed.
            TerrainLoaderManager.Instance.OnLoadProgressStarted -= OnLoadProgressStarted;
            TerrainLoaderManager.Instance.OnLoadProgressUpdated -= OnLoadProgressUpdated;
            TerrainLoaderManager.Instance.OnLoadProgressStarted -= OnLoadProgressEnded;
            TerrainLoaderManager.Instance.OnLoadProgressTimeOut -= OnLoadProgressTimeOut;
        }
#endif

        /// <summary>
        /// Gets called when a load progress tracking starts in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressStarted()
        {
            //Load Progress Tracking started, let's reset the loading screen back into its initial state
            m_canvas.enabled = true;
            //make sure the black background image is not faded out
            m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, 1f);
            //reset progress on the loading bar
            m_progressBar.value = 0;
        }

        /// <summary>
        /// Gets called when the load progress tracking updates in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressUpdated(float progress)
        {
            //make sure the canvas is still visible
            m_canvas.enabled = true;
            //update the progress bar position with the progress from the Terrain Loader Manager
            m_progressBar.value = progress;

            //failsafe in case the load progress end process is not being called
            if (progress >= 1)
            {
                OnLoadProgressEnded();
            }
        }

        /// <summary>
        /// Gets called when the load progress tracking ends in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressEnded()
        {
            //begin fading out the screen
            m_fadeout = true;

            //deactivate the progress bar and text
            m_progressBar.gameObject.SetActive(false);
            m_text.enabled = false;
        }

        /// <summary>
        /// Gets called when the load progress tracking times out in the Terrain Loader Manager
        /// </summary>
        private void OnLoadProgressTimeOut(List<TerrainScene> missingScenes)
        {
            Debug.Log("##########################################");
            Debug.Log("Loading Progress Timed Out! Missing Terrain Scenes:");
            foreach (TerrainScene terrainScene in missingScenes)
            {
                string regularReferences = "";
                string impostorReferences = "";

                foreach (GameObject regularGO in terrainScene.RegularReferences)
                {
                    regularReferences += regularGO.name + ", ";
                }

                foreach (GameObject impostorGO in terrainScene.ImpostorReferences)
                {
                    impostorReferences += impostorGO.name + ", ";
                }

                Debug.Log($"Regular Load State: {terrainScene.m_regularLoadState} \r\n" +
                            $"Regular Path: {terrainScene.m_scenePath} \r\n" +
                            $"Regular References: " + regularReferences + "\r\n\r\n" +
                            $"Impostor Load State: { terrainScene.m_regularLoadState} \r\n" +
                            $"Impostor Path: {terrainScene.m_impostorScenePath} \r\n" +
                            $"References: " + impostorReferences + "\r\n\r\n");
                


            }
            Debug.Log("##########################################");
            OnLoadProgressEnded();
        }
            

        void Update()
        {
            //fade out the loading screen over time, then disable the canvas completely.
            if (m_fadeout)
            {
                m_image.color = new Color(m_image.color.r, m_image.color.g, m_image.color.b, m_image.color.a - Time.deltaTime * m_fadeOutSpeed);
                if (m_image.color.a <= 0)
                {
                    //fully faded out, disable canvas and stop the fade out
                    m_canvas.enabled = false;
                    m_fadeout = false;
                }
            }
        }
    }
}