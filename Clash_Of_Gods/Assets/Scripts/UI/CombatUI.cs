using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClashOfGods.UI
{
    /// <summary>
    /// 战斗UI - 显示灵力条、手牌栏、召唤师技能
    /// Combat UI - Displays mana bar, hand cards, summoner skills
    /// </summary>
    public class CombatUI : MonoBehaviour
    {
        [Header("Summoner Info")]
        public Slider HPBar;
        public Slider ManaBar;
        public TextMeshProUGUI HPText;
        public TextMeshProUGUI ManaText;

        [Header("Hand Cards")]
        public Transform HandCardContainer;
        public GameObject CardUIPrefab;

        [Header("Skills")]
        public Button[] SkillButtons;
        public Image[] SkillCooldownImages;

        [Header("Unit Status")]
        public Transform UnitStatusContainer;
        public GameObject UnitStatusPrefab;

        private void Start()
        {
            // TODO: 初始化UI
        }

        /// <summary>
        /// 更新召唤师HP
        /// Update summoner HP
        /// </summary>
        public void UpdateHP(float current, float max)
        {
            if (HPBar != null)
            {
                HPBar.value = current / max;
            }

            if (HPText != null)
            {
                HPText.text = $"{current:F0}/{max:F0}";
            }
        }

        /// <summary>
        /// 更新召唤师灵力
        /// Update summoner mana
        /// </summary>
        public void UpdateMana(float current, float max)
        {
            if (ManaBar != null)
            {
                ManaBar.value = current / max;
            }

            if (ManaText != null)
            {
                ManaText.text = $"{current:F0}/{max:F0}";
            }
        }

        /// <summary>
        /// 更新手牌显示
        /// Update hand cards display
        /// </summary>
        public void UpdateHandCards()
        {
            // TODO: 清空并重新生成手牌UI
        }

        /// <summary>
        /// 更新技能冷却
        /// Update skill cooldowns
        /// </summary>
        public void UpdateSkillCooldown(int skillIndex, float cooldownPercent)
        {
            if (skillIndex >= 0 && skillIndex < SkillCooldownImages.Length)
            {
                SkillCooldownImages[skillIndex].fillAmount = cooldownPercent;
            }
        }
    }
}
