using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BadNorth3D.UI
{
    /// <summary>
    /// 招募面板UI - 玩家可以招募不同类型的战斗单位
    /// AI可以通过修改UnitTypesConfig来添加新的可招募单位
    /// </summary>
    public class RecruitmentPanel : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject recruitmentPanel;
        public TextMeshProUGUI goldText;
        public Transform unitButtonsContainer;
        public GameObject unitButtonPrefab;

        [Header("Display Settings")]
        public Color canAffordColor = Color.white;
        public Color cannotAffordColor = Color.red;

        private float currentGold;
        private UnitButton[] unitButtons;

        void Start()
        {
            currentGold = GameConfig.Gameplay.START_GOLD;
            CreateUnitButtons();
            UpdateGoldDisplay();

            // 默认隐藏招募面板
            if (recruitmentPanel != null)
            {
                recruitmentPanel.SetActive(false);
            }
        }

        void CreateUnitButtons()
        {
            // 为每种单位类型创建按钮 - 现在支持7种单位
            var unitConfigs = ExtendedUnitTypesConfig.ALL_UNITS; // 使用扩展配置
            unitButtons = new UnitButton[unitConfigs.Length];

            for (int i = 0; i < unitConfigs.Length; i++)
            {
                var config = unitConfigs[i];

                // 创建按钮
                GameObject buttonObj = Instantiate(unitButtonPrefab, unitButtonsContainer);
                buttonObj.name = $"{config.Type}_Button";

                // 设置按钮文本
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"{config.Name}\n{config.Cost}G\n{config.Description}";
                }

                // 设置按钮颜色
                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = config.Color;
                }

                // 添加点击事件
                Button button = buttonObj.GetComponent<Button>();
                int index = i; // 闭包捕获
                if (button != null)
                {
                    button.onClick.AddListener(() => TryRecruitUnit(index));
                }

                unitButtons[i] = new UnitButton
                {
                    Button = button,
                    Text = buttonText,
                    Image = buttonImage,
                    Config = config
                };
            }
        }

        void Update()
        {
            // 按R键打开/关闭招募面板
            if (Input.GetKeyDown(KeyCode.R))
            {
                ToggleRecruitmentPanel();
            }

            // 更新按钮状态
            UpdateUnitButtons();
        }

        public void ToggleRecruitmentPanel()
        {
            if (recruitmentPanel != null)
            {
                bool newState = !recruitmentPanel.activeSelf;
                recruitmentPanel.SetActive(newState);

                // 播放音效
                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlaySelectSound();
                }
            }
        }

        void UpdateUnitButtons()
        {
            foreach (var unitButton in unitButtons)
            {
                if (unitButton.Button != null)
                {
                    bool canAfford = currentGold >= unitButton.Config.Cost;
                    unitButton.Button.interactable = canAfford;

                    // 更新文本颜色
                    if (unitButton.Text != null)
                    {
                        unitButton.Text.color = canAfford ? canAffordColor : cannotAffordColor;
                    }
                }
            }
        }

        void TryRecruitUnit(int unitIndex)
        {
            if (unitIndex < 0 || unitIndex >= unitButtons.Length)
                return;

            var unitButton = unitButtons[unitIndex];

            if (currentGold >= unitButton.Config.Cost)
            {
                // 扣除金币
                currentGold -= unitButton.Config.Cost;
                UpdateGoldDisplay();

                // 通知GameManager生成单位
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.RecruitUnit(unitButton.Config.Type);
                }

                // 播放招募音效
                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlayAttackSound(); // 临时使用攻击音效
                }

                Debug.Log($"Recruited {unitButton.Config.Name} for {unitButton.Config.Cost} gold");
            }
            else
            {
                Debug.Log($"Not enough gold to recruit {unitButton.Config.Name}");
            }
        }

        public void AddGold(float amount)
        {
            currentGold += amount;
            UpdateGoldDisplay();
        }

        void UpdateGoldDisplay()
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: { Mathf.Floor(currentGold)}";
            }
        }

        public float GetCurrentGold()
        {
            return currentGold;
        }

        public void ShowRecruitmentPanel()
        {
            if (recruitmentPanel != null)
            {
                recruitmentPanel.SetActive(true);
            }
        }

        public void HideRecruitmentPanel()
        {
            if (recruitmentPanel != null)
            {
                recruitmentPanel.SetActive(false);
            }
        }

        // 单元按钮数据结构
        private class UnitButton
        {
            public Button Button;
            public TextMeshProUGUI Text;
            public Image Image;
            public UnitTypesConfig.SquadUnitTypeConfig Config;
        }
    }
}