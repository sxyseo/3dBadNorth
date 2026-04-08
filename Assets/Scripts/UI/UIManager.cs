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

        [Header("单位UI")]
        public TextMeshProUGUI selectedCountText;
        public GameObject recruitButton;

        [Header("消息面板")]
        public GameObject messagePanel;
        public TextMeshProUGUI messageText;
        public GameObject messagePanelBackground;

        [Header("游戏结束UI")]
        public GameObject gameOverPanel;
        public GameObject dayCompletePanel;

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
                    button.interactable = gold >= 20f;
                }
            }
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
        }

        public void ShowWaveComplete()
        {
            ShowMessage("波次完成！按空格键继续", 3f);
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

        // 按钮回调
        public void OnRecruitButtonClicked()
        {
            GameManager.Instance.SpawnSquadUnit();
        }

        public void OnRestartButtonClicked()
        {
            GameManager.Instance.RestartGame();
        }

        public void OnNextDayButtonClicked()
        {
            if (dayCompletePanel != null)
            {
                dayCompletePanel.SetActive(false);
            }
        }
    }
}
