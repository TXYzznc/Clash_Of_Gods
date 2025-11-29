using UnityEngine;

namespace ClashOfGods.Gameplay.Map
{
    /// <summary>
    /// 地图事件点 - 宝箱、商人、祭坛等
    /// Map event point - Chests, merchants, altars, etc.
    /// </summary>
    public class MapEventPoint : MonoBehaviour
    {
        [Header("Event Info")]
        public EventType EventType;
        public string EventID;
        public bool IsActive = true;

        [Header("Interaction")]
        public float InteractionRange = 3f;
        public KeyCode InteractionKey = KeyCode.E;

        [Header("Visual")]
        public GameObject VisualIndicator;
        public ParticleSystem EffectParticle;

        private bool playerInRange = false;
        private GameObject player;

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(InteractionKey))
            {
                TriggerEvent();
            }
        }

        /// <summary>
        /// 触发事件
        /// Trigger event
        /// </summary>
        public void TriggerEvent()
        {
            if (!IsActive) return;

            Debug.Log($"[MapEvent] Triggered: {EventType}");

            switch (EventType)
            {
                case EventType.Chest:
                    OpenChest();
                    break;
                case EventType.Merchant:
                    OpenMerchant();
                    break;
                case EventType.Altar:
                    OpenAltar();
                    break;
                case EventType.Fountain:
                    UseFountain();
                    break;
                case EventType.SlotMachine:
                    UseSlotMachine();
                    break;
                case EventType.Enemy:
                    TriggerCombat();
                    break;
            }

            // 播放特效
            if (EffectParticle != null)
            {
                EffectParticle.Play();
            }
        }

        #region Event Implementations

        private void OpenChest()
        {
            Debug.Log("[MapEvent] Opening chest...");
            // TODO: 生成战利品
            DeactivateEvent();
        }

        private void OpenMerchant()
        {
            Debug.Log("[MapEvent] Opening merchant...");
            // TODO: 打开商店UI
        }

        private void OpenAltar()
        {
            Debug.Log("[MapEvent] Opening altar...");
            // TODO: 打开强化UI
        }

        private void UseFountain()
        {
            Debug.Log("[MapEvent] Using fountain...");
            // TODO: 恢复所有单位生命值
            DeactivateEvent();
        }

        private void UseSlotMachine()
        {
            Debug.Log("[MapEvent] Using slot machine...");
            // TODO: 随机抽奖
        }

        private void TriggerCombat()
        {
            Debug.Log("[MapEvent] Triggering combat...");
            // TODO: 开始战斗
            DeactivateEvent();
        }

        #endregion

        /// <summary>
        /// 停用事件
        /// Deactivate event
        /// </summary>
        private void DeactivateEvent()
        {
            IsActive = false;
            
            if (VisualIndicator != null)
            {
                VisualIndicator.SetActive(false);
            }

            MapManager.Instance?.RemoveEvent(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = true;
                player = other.gameObject;
                
                // TODO: 显示交互提示UI
                Debug.Log($"[MapEvent] Player entered range of {EventType}");
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                playerInRange = false;
                player = null;
                
                // TODO: 隐藏交互提示UI
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, InteractionRange);
        }
    }

    /// <summary>
    /// 事件类型
    /// Event type
    /// </summary>
    public enum EventType
    {
        Chest,          // 宝箱
        Merchant,       // 商人
        Altar,          // 强化祭坛
        Fountain,       // 圣泉
        SlotMachine,    // 老虎机
        Enemy,          // 敌人
        Boss            // BOSS
    }
}
