using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BadNorth3D.UI
{
    /// <summary>
    /// 升级面板UI - 显示和执行单位升级
    /// AI可以调整升级参数和UI显示
    /// </summary>
    public class UpgradePanel : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject upgradePanel;
        public Button upgradeButton;
        public TextMeshProUGUI unitNameText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI upgradeCostText;
        public TextMeshProUGUI statsPreviewText;

        [Header("Display Settings")]
        public Color canUpgradeColor = Color.green;
        public Color cannotUpgradeColor = Color.red;

        private UnitUpgrade currentUpgrade;

        void Start()
        {
            // 默认隐藏升级面板
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
            }

            // 设置升级按钮事件
            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }
        }

        void Update()
        {
            // 按U键打开/关闭升级面板
            if (Input.GetKeyDown(KeyCode.U))
            {
                ToggleUpgradePanel();
            }

            // 每帧更新UI（因为金币可能会变化）
            if (upgradePanel != null && upgradePanel.activeSelf)
            {
                UpdateUpgradeUI();
            }
        }

        public void ToggleUpgradePanel()
        {
            if (upgradePanel != null)
            {
                bool newState = !upgradePanel.activeSelf;
                upgradePanel.SetActive(newState);

                if (newState)
                {
                    // 打开时自动选择第一个可升级的单位
                    SelectUnitToUpgrade();
                }
                else
                {
                    currentUpgrade = null;
                }

                // 播放音效
                if (AudioSynthesizer.Instance != null)
                {
                    AudioSynthesizer.Instance.PlaySelectSound();
                }
            }
        }

        void SelectUnitToUpgrade()
        {
            // 查找所有选择中的单位
            var selectedUnits = FindObjectsOfType<SquadUnitAdvanced>();
            foreach (var unit in selectedUnits)
            {
                if (unit.isSelected)
                {
                    var upgrade = unit.GetComponent<UnitUpgrade>();
                    if (upgrade != null)
                    {
                        ShowUnitUpgrade(upgrade);
                        return;
                    }
                }
            }

            // 如果没有选中的单位，查找第一个可升级的单位
            var allUnits = FindObjectsOfType<UnitUpgrade>();
            foreach (var upgrade in allUnits)
            {
                if (upgrade.CanUpgrade())
                {
                    ShowUnitUpgrade(upgrade);
                    return;
                }
            }

            // 如果没有可升级的单位，显示提示
            ShowNoUpgradeableUnits();
        }

        void ShowUnitUpgrade(UnitUpgrade upgrade)
        {
            currentUpgrade = upgrade;
            UpdateUpgradeUI();
        }

        void ShowNoUpgradeableUnits()
        {
            if (unitNameText != null)
            {
                unitNameText.text = "No units selected";
            }

            if (levelText != null)
            {
                levelText.text = "Select a unit first";
            }

            if (upgradeCostText != null)
            {
                upgradeCostText.text = "";
            }

            if (statsPreviewText != null)
            {
                statsPreviewText.text = "Click on a unit to see upgrade options";
            }

            if (upgradeButton != null)
            {
                upgradeButton.interactable = false;
            }
        }

        void UpdateUpgradeUI()
        {
            if (currentUpgrade == null)
            {
                ShowNoUpgradeableUnits();
                return;
            }

            var upgradeInfo = currentUpgrade.GetUpgradeInfo();

            // 更新单位名称
            if (unitNameText != null)
            {
                var squadUnit = currentUpgrade.GetComponent<SquadUnitAdvanced>();
                if (squadUnit != null)
                {
                    unitNameText.text = $"{squadUnit.unitType} (Level {upgradeInfo.CurrentLevel})";
                }
                else
                {
                    unitNameText.text = $"Unit (Level {upgradeInfo.CurrentLevel})";
                }
            }

            // 更新等级文本
            if (levelText != null)
            {
                if (upgradeInfo.CurrentLevel >= upgradeInfo.MaxLevel)
                {
                    levelText.text = "MAX LEVEL";
                    levelText.color = Color.yellow;
                }
                else
                {
                    levelText.text = $"Level {upgradeInfo.CurrentLevel}/{upgradeInfo.MaxLevel}";
                    levelText.color = Color.white;
                }
            }

            // 更新升级费用
            if (upgradeCostText != null)
            {
                if (upgradeInfo.CurrentLevel >= upgradeInfo.MaxLevel)
                {
                    upgradeCostText.text = "MAX LEVEL REACHED";
                    upgradeCostText.color = Color.yellow;
                }
                else
                {
                    upgradeCostText.text = $"Cost: {Mathf.Floor(upgradeInfo.UpgradeCost)}G";
                    upgradeCostText.color = upgradeInfo.CanUpgrade ? canUpgradeColor : cannotUpgradeColor;
                }
            }

            // 更新属性预览
            if (statsPreviewText != null)
            {
                if (upgradeInfo.CurrentLevel >= upgradeInfo.MaxLevel)
                {
                    statsPreviewText.text = "Unit is at maximum level";
                }
                else
                {
                    statsPreviewText.text = $"Next upgrade:\n" +
                                          $"+{upgradeInfo.HealthIncrease} Health\n" +
                                          $"+{upgradeInfo.DamageIncrease} Damage\n" +
                                          $"+{upgradeInfo.SpeedIncrease} Speed";
                }
            }

            // 更新升级按钮状态
            if (upgradeButton != null)
            {
                upgradeButton.interactable = upgradeInfo.CanUpgrade;
            }
        }

        void OnUpgradeButtonClicked()
        {
            if (currentUpgrade == null)
                return;

            if (currentUpgrade.Upgrade())
            {
                Debug.Log("Unit upgraded successfully!");
                UpdateUpgradeUI();

                // 通知UIManager更新金币显示
                if (EconomyManager.Instance != null && UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateGoldUI(EconomyManager.Instance.GetCurrentGold());
                }
            }
            else
            {
                Debug.Log("Upgrade failed! Not enough gold or max level reached.");
            }
        }

        public void ShowUpgradePanel()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(true);
                SelectUnitToUpgrade();
            }
        }

        public void HideUpgradePanel()
        {
            if (upgradePanel != null)
            {
                upgradePanel.SetActive(false);
                currentUpgrade = null;
            }
        }
    }
}