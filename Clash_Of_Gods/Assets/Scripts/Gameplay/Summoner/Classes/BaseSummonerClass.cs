using UnityEngine;

namespace ClashOfGods.Gameplay.Summoner.Classes
{
    /// <summary>
    /// 召唤师职业基类 - 使用策略模式实现不同职业逻辑
    /// Base summoner class - Strategy pattern for different class logic
    /// </summary>
    public abstract class BaseSummonerClass : MonoBehaviour
    {
        protected SummonerController summonerController;
        protected SummonerSkillSystem skillSystem;

        protected virtual void Awake()
        {
            summonerController = GetComponent<SummonerController>();
            skillSystem = GetComponent<SummonerSkillSystem>();
        }

        /// <summary>
        /// 职业初始化
        /// Class initialization
        /// </summary>
        public virtual void OnClassInitialized()
        {
            Debug.Log($"[{GetType().Name}] Class initialized");
        }

        /// <summary>
        /// 灵力值更新时调用
        /// Called when spirit is updated
        /// </summary>
        public virtual void OnSpiritUpdated(float oldValue, float newValue)
        {
            // 子类可重写
        }

        /// <summary>
        /// HP更新时调用
        /// Called when HP is updated
        /// </summary>
        public virtual void OnHPUpdated(float oldValue, float newValue)
        {
            // 子类可重写
        }

        /// <summary>
        /// 单位死亡时调用
        /// Called when a unit dies
        /// </summary>
        public virtual void OnUnitDeath(GameObject unit, bool isAlly)
        {
            // 子类可重写
        }

        /// <summary>
        /// 造成伤害时调用
        /// Called when dealing damage
        /// </summary>
        public virtual void OnDealDamage(GameObject target, float damage)
        {
            // 子类可重写
        }

        /// <summary>
        /// 受到伤害时调用
        /// Called when taking damage
        /// </summary>
        public virtual void OnTakeDamage(GameObject source, float damage)
        {
            // 子类可重写
        }

        /// <summary>
        /// 每帧更新
        /// Update per frame
        /// </summary>
        protected virtual void Update()
        {
            // 子类可重写
        }
    }
}
