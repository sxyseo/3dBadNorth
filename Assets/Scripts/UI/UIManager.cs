using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BadNorth3D
{
    /// <summary>
    /// UI管理器 - 处理所有用户界面
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("资源UI")]
        public TextMeshProUGUI goldText;
        public TextMeshProUGUI dayText;
        public TextMeshProUGUI waveText;
        public TextMeshProUGUI waveTimerText;

        [Header("单位UI")]
        public TextMeshProUGUI selectedCountText;
        public GameObject recruitButton;

        [Header("Panel References")]
        public GameObject recruitmentPanel;
        public GameObject upgradePanel;

        [Header("消息面板")]
        public GameObject messagePanel;
        public TextMeshProUGUI messageText;
        public GameObject messagePanelBackground;

        [Header("游戏结束UI")]
        public GameObject gameOverPanel;
        public GameObject dayCompletePanel;

        [Header("波次信息")]
        public GameObject waveStartPanel;
        public TextMeshProUGUI waveStartText;
        public GameObject waveCompletePanel;
        public TextMeshProUGUI waveCompleteText;

        // 波次计时器
        private float waveTimer;
        private bool waveTimerActive;

        // 波次奖励动画
        private Coroutine rewardAnimationCoroutine;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            // 初始化UI
            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (dayCompletePanel != null) dayCompletePanel.SetActive(false);
            if (messagePanel != null) messagePanel.SetActive(false);
        }

        void Update()
        {
            UpdateSelectedUnitCount();
            UpdateWaveTimer();
            HandleKeyboardShortcuts();
        }

        void HandleKeyboardShortcuts()
        {
            // R键 - 招募面板
            if (Input.GetKeyDown(KeyCode.R))
            {
                ToggleRecruitmentPanel();
            }

            // U键 - 升级面板
            if (Input.GetKeyDown(KeyCode.U))
            {
                ToggleUpgradePanel();
            }

            // ESC键 - 关闭所有面板
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseAllPanels();
            }
        }

        public void ToggleRecruitmentPanel()
        {
            if (recruitmentPanel != null)
            {
                bool newState = !recruitmentPanel.activeSelf;
                recruitmentPanel.SetActive(newState);

                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlayUIClickSound();
                }
            }
        }

        public void ToggleUpgradePanel()
        {
            if (upgradePanel != null)
            {
                bool newState = !upgradePanel.activeSelf;
                upgradePanel.SetActive(newState);

                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlayUIClickSound();
                }
            }
        }

        public void CloseAllPanels()
        {
            if (recruitmentPanel != null)
            {
                recruitmentPanel.SetActive(false);
            }

            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }
        }

        void UpdateWaveTimer()
        {
            if (waveTimerActive && waveTimerText != null)
            {
                waveTimer -= Time.deltaTime;
                waveTimerText.text = $"下一波: {Mathf.Max(0, Mathf.CeilToInt(waveTimer))}秒";

                if (waveTimer <= 0)
                {
                    waveTimerActive = false;
                    waveTimerText.gameObject.SetActive(false);
                }
            }
        }

        void UpdateSelectedUnitCount()
        {
            if (selectedCountText != null)
            {
                int count = UnitSelectionManager.Instance.GetSelectedUnits().Count;
                selectedCountText.text = $"选中: {count}";
            }
        }

        public void UpdateGoldUI(float gold)
        {
            if (goldText != null)
            {
                goldText.text = $"金币: {Mathf.FloorToInt(gold)}";
            }

            // 更新招募按钮状态
            if (recruitButton != null)
            {
                Button button = recruitButton.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = gold >= GameConfig.Units.RECRUIT_COST;
                }
            }
        }

        public void ShowControlsHelp()
        {
            string helpText = "Controls:\n" +
                            "Left Click - Select unit\n" +
                            "Left Click + Drag - Select multiple units\n" +
                            "Right Click - Move selected units\n" +
                            "R - Recruitment panel\n" +
                            "U - Upgrade panel\n" +
                            "ESC - Close all panels\n" +
                            "Space - Start next wave";

            ShowMessage(helpText, 5f);
        }

        public void UpdateDayUI(int day)
        {
            if (dayText != null)
            {
                dayText.text = $"第 {day} 天";
            }
        }

        public void UpdateWaveUI(int current, int total)
        {
            if (waveText != null)
            {
                waveText.text = $"波次: {current}/{total}";
            }

            // 显示波次开始提示
            ShowWaveStart(current, total);
        }

        public void SetWaveTimer(float duration)
        {
            waveTimer = duration;
            waveTimerActive = true;
            if (waveTimerText != null)
            {
                waveTimerText.gameObject.SetActive(true);
            }
        }

        void ShowWaveStart(int waveNum, int totalWaves)
        {
            if (waveStartPanel != null && waveStartText != null)
            {
                waveStartText.text = $"第 {waveNum} 波开始！";
                waveStartPanel.SetActive(true);

                // 3秒后自动隐藏
                StartCoroutine(HidePanelAfterDelay(waveStartPanel, 3f));
            }

            // 播放波次音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayWaveCompleteSound();
            }
        }

        public void ShowWaveComplete()
        {
            if (waveCompletePanel != null && waveCompleteText != null)
            {
                waveCompleteText.text = "波次完成！";
                waveCompletePanel.SetActive(true);

                StartCoroutine(HidePanelAfterDelay(waveCompletePanel, 2f));
            }

            ShowMessage("波次完成！休息时间", 3f);

            // 播放完成音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayWaveCompleteSound();
            }
        }

        public void ShowDayComplete(int nextDay)
        {
            if (dayCompletePanel != null)
            {
                dayCompletePanel.SetActive(true);
                TextMeshProUGUI[] texts = dayCompletePanel.GetComponentsInChildren<TextMeshProUGUI>();
                foreach (var text in texts)
                {
                    if (text.name == "DayText")
                    {
                        text.text = $"第 {nextDay} 天";
                    }
                }
            }
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            // 播放游戏结束音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayGameOverSound();
            }
        }

        public void ShowMessage(string message, float duration = 2f)
        {
            if (messagePanel != null && messageText != null)
            {
                messageText.text = message;
                messagePanel.SetActive(true);

                StartCoroutine(HideMessageAfterDelay(duration));
            }
        }

        System.Collections.IEnumerator HideMessageAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (messagePanel != null)
            {
                messagePanel.SetActive(false);
            }
        }

        System.Collections.IEnumerator HidePanelAfterDelay(GameObject panel, float delay)
        {
            yield return new WaitForSeconds(delay);
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        // 按钮回调
        public void OnRecruitButtonClicked()
        {
            // 播放UI音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayUIClickSound();
            }

            GameManager.Instance.SpawnSquadUnit();
        }

        public void OnRestartButtonClicked()
        {
            // 播放UI音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayUIClickSound();
            }

            GameManager.Instance.RestartGame();
        }

        public void OnNextDayButtonClicked()
        {
            // 播放UI音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayUIClickSound();
            }

            if (dayCompletePanel != null)
            {
                dayCompletePanel.SetActive(false);
            }
        }
    }
}
