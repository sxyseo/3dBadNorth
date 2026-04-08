using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BadNorth3D.UI
{
    /// <summary>
    /// 技能冷却UI - 显示单位技能的冷却状态
    /// AI可以调整技能参数和UI显示
    /// </summary>
    public class SkillCooldownUI : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject skillPanel;
        public Transform skillButtonsContainer;
        public GameObject skillButtonPrefab;

        [Header("Display Settings")]
        public Color readyColor = Color.green;
        public Color cooldownColor = Color.red;
        public Color activeColor = Color.yellow;

        private SkillButton[] skillButtons;
        private SquadUnitAdvanced trackedUnit;

        void Start()
        {
            // 默认隐藏技能面板
            if (skillPanel != null)
            {
                skillPanel.SetActive(false);
            }
        }

        void Update()
        {
            // 更新技能冷却显示
            if (trackedUnit != null && skillButtons != null)
            {
                UpdateSkillCooldowns();
            }

            // 检查是否有选中的单位
            UpdateTrackedUnit();
        }

        void UpdateTrackedUnit()
        {
            // 查找选中的单位
            var selectedUnits = FindObjectsOfType<SquadUnitAdvanced>();
            SquadUnitAdvanced newTrackedUnit = null;

            foreach (var unit in selectedUnits)
            {
                if (unit.isSelected)
                {
                    newTrackedUnit = unit;
                    break;
                }
            }

            // 如果追踪的单位改变了，重新创建技能按钮
            if (newTrackedUnit != trackedUnit)
            {
                trackedUnit = newTrackedUnit;
                CreateSkillButtons();
            }
        }

        void CreateSkillButtons()
        {
            // 清除旧按钮
            if (skillButtons != null)
            {
                foreach (var button in skillButtons)
                {
                    if (button.ButtonObj != null)
                    {
                        Destroy(button.ButtonObj);
                    }
                }
            }

            // 如果没有追踪的单位，隐藏面板
            if (trackedUnit == null)
            {
                if (skillPanel != null)
                {
                    skillPanel.SetActive(false);
                }
                return;
            }

            // 显示面板
            if (skillPanel != null)
            {
                skillPanel.SetActive(true);
            }

            // 获取单位的技能
            var abilities = GetUnitAbilities(trackedUnit);
            if (abilities == null || abilities.Length == 0)
                return;

            skillButtons = new SkillButton[abilities.Length];

            for (int i = 0; i < abilities.Length; i++)
            {
                var ability = abilities[i];
                if (ability == null) continue;

                // 只显示主动技能
                if (!(ability is ActiveAbility activeAbility))
                    continue;

                // 创建按钮
                GameObject buttonObj = Instantiate(skillButtonPrefab, skillButtonsContainer);
                buttonObj.name = $"Skill_{i}";

                // 设置按钮文本
                TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = $"{activeAbility.Name}\n(Cooldown: {activeAbility.Cooldown}s)";
                }

                // 设置按钮颜色
                Image buttonImage = buttonObj.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.color = readyColor;
                }

                // 添加点击事件
                Button button = buttonObj.GetComponent<Button>();
                int index = i; // 闭包捕获
                if (button != null)
                {
                    button.onClick.AddListener(() => UseSkill(index));
                }

                skillButtons[i] = new SkillButton
                {
                    ButtonObj = buttonObj,
                    Button = button,
                    Text = buttonText,
                    Image = buttonImage,
                    Ability = activeAbility,
                    Index = i
                };
            }
        }

        Ability[] GetUnitAbilities(SquadUnitAdvanced unit)
        {
            // 这里需要从SquadUnitAdvanced获取技能
            // 暂时返回空数组，需要SquadUnitAdvanced暴露技能数组
            return new Ability[0];
        }

        void UpdateSkillCooldowns()
        {
            if (skillButtons == null)
                return;

            foreach (var skillButton in skillButtons)
            {
                if (skillButton?.Button == null || skillButton.Ability == null)
                    continue;

                // 这里需要检查技能的冷却状态
                // 暂时简化处理
                bool isOnCooldown = false; // TODO: 从单位获取冷却状态

                if (skillButton.Button != null)
                {
                    skillButton.Button.interactable = !isOnCooldown;
                }

                if (skillButton.Image != null)
                {
                    skillButton.Image.color = isOnCooldown ? cooldownColor : readyColor;
                }
            }
        }

        void UseSkill(int skillIndex)
        {
            if (trackedUnit == null)
                return;

            // 使用技能
            trackedUnit.UseAbility(skillIndex);

            Debug.Log($"Used skill {skillIndex} on {trackedUnit.name}");

            // 播放音效
            if (AudioSynthesizer.Instance != null)
            {
                AudioSynthesizer.Instance.PlayAttackSound(); // 临时使用攻击音效
            }
        }

        public void ShowSkillPanel()
        {
            if (skillPanel != null)
            {
                skillPanel.SetActive(true);
            }
        }

        public void HideSkillPanel()
        {
            if (skillPanel != null)
            {
                skillPanel.SetActive(false);
            }
        }

        // 技能按钮数据结构
        private class SkillButton
        {
            public GameObject ButtonObj;
            public Button Button;
            public TextMeshProUGUI Text;
            public Image Image;
            public ActiveAbility Ability;
            public int Index;
        }
    }
}