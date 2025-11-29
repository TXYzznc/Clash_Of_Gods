using System.Collections.Generic;
using UnityEngine;
using ClashOfGods.Core;
using ClashOfGods.Gameplay.Units;


namespace ClashOfGods.Gameplay.Combat
{
    /// <summary>
    /// 卡牌管理器 - 管理牌库、手牌、抽卡和灵力消耗（支持热重载）
    /// Card manager - Manages deck, hand, drawing, and mana cost (Hot reload compatible)
    /// </summary>
    public class CardManager : MonoBehaviour
    {
        private static CardManager _instance;
        
        public static CardManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<CardManager>();
                return _instance;
            }
        }

        [Header("Deck Configuration")]
        public List<int> DeckCardIDs = new List<int>(); // 玩家配置的卡组（8张）
        
        [Header("Hand")]
        public List<CardData> Hand = new List<CardData>();
        public int MaxHandSize = 8;

        [Header("Refresh")]
        public float AutoRefreshInterval = 30f;
        public float RefreshTimer = 0f;
        public int ManualRefreshCost = 1; // 金币消耗

        [Header("References")]
        public Transform HandUIContainer;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = (CardManager)(object)this;
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(_instance, this))
                _instance = null;
        }

        private void Start()
        {
            DrawCards();
        }

        private void Update()
        {
            // 自动刷新计时
            RefreshTimer += Time.deltaTime;
            if (RefreshTimer >= AutoRefreshInterval)
            {
                DrawCards();
                RefreshTimer = 0f;
            }
        }

        /// <summary>
        /// 抽卡
        /// Draw cards
        /// </summary>
        public void DrawCards()
        {
            Hand.Clear();

            if (ConfigManager.Instance == null)
            {
                Debug.LogError("[CardManager] ConfigManager not initialized!");
                return;
            }

            if (DeckCardIDs == null || DeckCardIDs.Count == 0)
            {
                Debug.LogWarning("[CardManager] Deck is empty!");
                return;
            }

            // 从卡组中随机抽取卡牌填充手牌
            List<int> availableCards = new List<int>(DeckCardIDs);
            
            for (int i = 0; i < MaxHandSize && availableCards.Count > 0; i++)
            {
                int randomIndex = Random.Range(0, availableCards.Count);
                int cardID = availableCards[randomIndex];
                
                CardConfig config = ConfigManager.Instance.GetCardConfig(cardID);
                if (config != null)
                {
                    Hand.Add(new CardData
                    {
                        Config = config,
                        IsUsable = true
                    });
                }
                
                availableCards.RemoveAt(randomIndex);
            }

            Debug.Log($"[CardManager] Drew {Hand.Count} cards");
            
            // TODO: 更新UI显示
            UpdateHandUI();
        }

        /// <summary>
        /// 使用卡牌
        /// Use card
        /// </summary>
        public bool UseCard(int cardIndex, Vector3 targetPos)
        {
            if (cardIndex < 0 || cardIndex >= Hand.Count)
            {
                Debug.LogWarning("[CardManager] Invalid card index");
                return false;
            }

            CardData card = Hand[cardIndex];
            
            if (!card.IsUsable)
            {
                Debug.LogWarning("[CardManager] Card is not usable");
                return false;
            }

            // 检查灵力是否足够
            // TODO: 从SummonerController获取当前灵力
            float currentMana = 100f; // 临时值
            
            if (currentMana < card.Config.ManaCost)
            {
                Debug.LogWarning("[CardManager] Not enough mana");
                return false;
            }

            // 执行卡牌效果
            ExecuteCardEffect(card.Config, targetPos);
            
            // 消耗灵力
            // TODO: 扣除召唤师灵力值
            
            // 从手牌移除
            Hand.RemoveAt(cardIndex);
            
            // 更新UI
            UpdateHandUI();
            
            Debug.Log($"[CardManager] Used card: {card.Config.CardName}");
            return true;
        }

        /// <summary>
        /// 手动刷新手牌
        /// Manual refresh hand
        /// </summary>
        public void ManualRefresh()
        {
            // TODO: 检查并消耗金币
            Debug.Log("[CardManager] Manual refresh");
            DrawCards();
            RefreshTimer = 0f;
        }

        /// <summary>
        /// 执行卡牌效果
        /// Execute card effect
        /// </summary>
        private void ExecuteCardEffect(CardConfig config, Vector3 targetPos)
        {
            // 生成特效
            if (config.EffectPrefab != null)
            {
                Instantiate(config.EffectPrefab, targetPos, Quaternion.identity);
            }

            // 播放音效
            if (config.CastSound != null)
            {
                AudioSource.PlayClipAtPoint(config.CastSound, targetPos);
            }

            // 根据效果类型执行逻辑
            switch (config.EffectType)
            {
                case CardEffectType.Damage:
                    ApplyDamageEffect(targetPos, config);
                    break;
                case CardEffectType.Heal:
                    ApplyHealEffect(targetPos, config);
                    break;
                case CardEffectType.Buff:
                    ApplyBuffEffect(targetPos, config);
                    break;
                case CardEffectType.Debuff:
                    ApplyDebuffEffect(targetPos, config);
                    break;
                case CardEffectType.Control:
                    ApplyControlEffect(targetPos, config);
                    break;
            }

            Debug.Log($"[CardManager] Executed {config.CardName} effect at {targetPos}");
        }

        #region Card Effect Implementations

        private void ApplyDamageEffect(Vector3 position, CardConfig config)
        {
            Collider[] colliders = Physics.OverlapSphere(position, config.EffectRadius);
            
            foreach (Collider col in colliders)
            {
                UnitEntity unit = col.GetComponent<UnitEntity>();
                if (unit != null && !unit.IsPlayer)
                {
                    unit.TakeDamage(config.EffectValue);
                }
            }
        }

        private void ApplyHealEffect(Vector3 position, CardConfig config)
        {
            Collider[] colliders = Physics.OverlapSphere(position, config.EffectRadius);
            
            foreach (Collider col in colliders)
            {
                UnitEntity unit = col.GetComponent<UnitEntity>();
                if (unit != null && unit.IsPlayer)
                {
                    unit.ApplyHealing(config.EffectValue);
                }
            }
        }

        private void ApplyBuffEffect(Vector3 position, CardConfig config)
        {
            // TODO: 实现Buff应用逻辑
            Debug.Log($"[CardManager] Applied buff at {position}");
        }

        private void ApplyDebuffEffect(Vector3 position, CardConfig config)
        {
            // TODO: 实现Debuff应用逻辑
            Debug.Log($"[CardManager] Applied debuff at {position}");
        }

        private void ApplyControlEffect(Vector3 position, CardConfig config)
        {
            // TODO: 实现控制效果逻辑
            Debug.Log($"[CardManager] Applied control at {position}");
        }

        #endregion

        /// <summary>
        /// 更新手牌UI
        /// Update hand UI
        /// </summary>
        private void UpdateHandUI()
        {
            // TODO: 实现UI更新逻辑
        }

        #region Hot Reload Support
        
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] CardManager reloaded. Hand: {Hand.Count} cards");
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] CardManager static reload");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        #endregion
    }

    /// <summary>
    /// 卡牌数据（运行时）
    /// Card data (runtime)
    /// </summary>
    [System.Serializable]
    public class CardData
    {
        public CardConfig Config;
        public bool IsUsable = true;
    }
}
