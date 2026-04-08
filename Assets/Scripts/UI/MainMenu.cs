using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BadNorth3D
{
    /// <summary>
    /// 主菜单控制器
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [Header("菜单UI")]
        public GameObject mainMenuPanel;
        public GameObject settingsPanel;
        public GameObject creditsPanel;

        [Header("按钮")]
        public Button playButton;
        public Button settingsButton;
        public Button creditsButton;
        public Button quitButton;
        public Button backButton;

        void Start()
        {
            SetupButtons();
            ShowMainMenu();
        }

        void SetupButtons()
        {
            if (playButton != null)
                playButton.onClick.AddListener(OnPlayClicked);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);

            if (creditsButton != null)
                creditsButton.onClick.AddListener(OnCreditsClicked);

            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);

            if (backButton != null)
                backButton.onClick.AddListener(OnBackClicked);
        }

        void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        void OnPlayClicked()
        {
            SceneManager.LoadScene("GameScene");
        }

        void OnSettingsClicked()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        void OnCreditsClicked()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(true);
        }

        void OnBackClicked()
        {
            if (settingsPanel != null && settingsPanel.activeSelf)
            {
                settingsPanel.SetActive(false);
                mainMenuPanel.SetActive(true);
            }
            else if (creditsPanel != null && creditsPanel.activeSelf)
            {
                creditsPanel.SetActive(false);
                mainMenuPanel.SetActive(true);
            }
        }

        void OnQuitClicked()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
