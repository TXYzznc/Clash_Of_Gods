using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClashOfGods.UI
{
    /// <summary>
    /// 探索UI - 小地图、任务追踪
    /// Exploration UI - Minimap, quest tracking
    /// </summary>
    public class ExplorationUI : MonoBehaviour
    {
        [Header("Minimap")]
        public RawImage MinimapImage;
        public Transform MinimapIconContainer;
        public GameObject MinimapIconPrefab;

        [Header("Quest Tracking")]
        public Transform QuestContainer;
        public TextMeshProUGUI QuestText;

        [Header("Interaction Prompt")]
        public GameObject InteractionPrompt;
        public TextMeshProUGUI InteractionText;

        /// <summary>
        /// 更新小地图图标
        /// Update minimap icons
        /// </summary>
        public void UpdateMinimapIcons()
        {
            // TODO: 清空并重新生成小地图图标
        }

        /// <summary>
        /// 添加小地图标记
        /// Add minimap marker
        /// </summary>
        public void AddMinimapMarker(Vector3 worldPosition, Sprite icon)
        {
            // TODO: 在小地图上添加标记
        }

        /// <summary>
        /// 显示交互提示
        /// Show interaction prompt
        /// </summary>
        public void ShowInteractionPrompt(string text)
        {
            if (InteractionPrompt != null)
            {
                InteractionPrompt.SetActive(true);
            }

            if (InteractionText != null)
            {
                InteractionText.text = text;
            }
        }

        /// <summary>
        /// 隐藏交互提示
        /// Hide interaction prompt
        /// </summary>
        public void HideInteractionPrompt()
        {
            if (InteractionPrompt != null)
            {
                InteractionPrompt.SetActive(false);
            }
        }

        /// <summary>
        /// 更新任务追踪
        /// Update quest tracking
        /// </summary>
        public void UpdateQuestTracking(string questInfo)
        {
            if (QuestText != null)
            {
                QuestText.text = questInfo;
            }
        }
    }
}
