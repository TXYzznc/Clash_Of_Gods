using UnityEngine;

namespace ClashOfGods.Gameplay.Units
{
    /// <summary>
    /// 单位动画控制器 - 统一管理Animator（支持热重载）
    /// Unit animation controller - Unified Animator management (Hot reload compatible)
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class UnitAnimationController : MonoBehaviour
    {
        [Header("References")]
        public Animator Animator;

        [Header("Animation Parameters")]
        private readonly int speedHash = Animator.StringToHash("Speed");
        private readonly int attackHash = Animator.StringToHash("Attack");
        private readonly int hitHash = Animator.StringToHash("Hit");
        private readonly int deathHash = Animator.StringToHash("Death");
        private readonly int skillHash = Animator.StringToHash("Skill");

        private void Awake()
        {
            if (Animator == null)
            {
                Animator = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// 设置移动速度
        /// Set movement speed
        /// </summary>
        public void SetSpeed(float speed)
        {
            if (Animator != null)
            {
                Animator.SetFloat(speedHash, speed);
            }
        }

        /// <summary>
        /// 触发攻击动画
        /// Trigger attack animation
        /// </summary>
        public void TriggerAttack(float attackSpeedMultiplier = 1f)
        {
            if (Animator != null)
            {
                Animator.SetTrigger(attackHash);
                Animator.speed = attackSpeedMultiplier;
            }
        }

        /// <summary>
        /// 触发受击动画
        /// Trigger hit animation
        /// </summary>
        public void TriggerHit()
        {
            if (Animator != null)
            {
                Animator.SetTrigger(hitHash);
            }
        }

        /// <summary>
        /// 触发死亡动画
        /// Trigger death animation
        /// </summary>
        public void TriggerDeath()
        {
            if (Animator != null)
            {
                Animator.SetTrigger(deathHash);
            }
        }

        /// <summary>
        /// 触发技能动画
        /// Trigger skill animation
        /// </summary>
        public void TriggerSkill()
        {
            if (Animator != null)
            {
                Animator.SetTrigger(skillHash);
            }
        }

        /// <summary>
        /// 触发自定义动画
        /// Trigger custom animation
        /// </summary>
        public void TriggerAnimation(string triggerName)
        {
            if (Animator != null)
            {
                Animator.SetTrigger(triggerName);
            }
        }

        /// <summary>
        /// 设置动画速度
        /// Set animation speed
        /// </summary>
        public void SetAnimationSpeed(float speed)
        {
            if (Animator != null)
            {
                Animator.speed = speed;
            }
        }

        #region Hot Reload Support
        
        void OnScriptHotReload()
        {
            Debug.Log("[HotReload] UnitAnimationController reloaded");
            if (Animator == null)
                Animator = GetComponent<Animator>();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] UnitAnimationController static reload");
        }

        #endregion
    }
}
