using UnityEngine;
using Mirror;

namespace ClashOfGods.Gameplay.Summoner
{
    /// <summary>
    /// 召唤师控制器 - 挂载在玩家角色Prefab上（支持热重载）
    /// Summoner controller - Attached to player character prefab (Hot reload compatible)
    /// </summary>
    public class SummonerController : NetworkBehaviour
    {
        [Header("Runtime Data")]
        public SummonerRuntimeData RuntimeData;

        [Header("Movement")]
        public float MoveSpeed = 5f;
        public CharacterController CharacterController;

        [Header("References")]
        public Transform CameraTransform;

        [Header("Debug")]
        public bool EnableDebugLogs = true;

        private Vector3 targetPosition;
        private bool isMoving = false;

        private void Start()
        {
            InitializeComponents();
            InitializeSummoner();
        }

        private void InitializeComponents()
        {
            if (CharacterController == null)
                CharacterController = GetComponent<CharacterController>();
        }

        private void InitializeSummoner()
        {
            if (RuntimeData == null)
                RuntimeData = new SummonerRuntimeData();

            LogDebug($"Initialized: {RuntimeData.ClassType}");
        }

        public void Move(Vector3 targetPos)
        {
            targetPosition = targetPos;
            isMoving = true;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            HandleMovement();
            HandleInput();
            UpdateSpiritRegen();
        }

        private void HandleMovement()
        {
            if (!isMoving || CharacterController == null) return;

            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            if (distance > 0.1f)
            {
                CharacterController.Move(direction * MoveSpeed * Time.deltaTime);
            }
            else
            {
                isMoving = false;
            }
        }

        private void HandleInput()
        {
            // TODO: 实现输入处理
        }

        private void UpdateSpiritRegen()
        {
            if (RuntimeData == null) return;
            
            if (RuntimeData.CurrentSpirit < RuntimeData.MaxSpirit)
            {
                RuntimeData.CurrentSpirit = Mathf.Min(
                    RuntimeData.CurrentSpirit + RuntimeData.SpiritRegenRate * Time.deltaTime,
                    RuntimeData.MaxSpirit
                );
            }
        }

        public void Teleport(Vector3 position)
        {
            transform.position = position;
            LogDebug($"Teleported to: {position}");
        }

        public void CastSummonerSkill(int skillID, Vector3 target)
        {
            if (!CanCastSkill(skillID))
            {
                Debug.LogWarning($"[Summoner] Cannot cast skill: {skillID}");
                return;
            }

            LogDebug($"Casting skill {skillID} at {target}");
            // TODO: 实现技能释放逻辑
        }

        private bool CanCastSkill(int skillID)
        {
            return RuntimeData != null && RuntimeData.CurrentSpirit > 0;
        }

        public void TakeDamage(float amount)
        {
            if (RuntimeData == null) return;
            
            RuntimeData.CurrentHP -= amount;
            LogDebug($"Took {amount} damage. HP: {RuntimeData.CurrentHP}/{RuntimeData.MaxHP}");
            
            if (RuntimeData.CurrentHP <= 0)
                OnDeath();
        }

        private void OnDeath()
        {
            LogDebug("Died! Applying penalty...");
            // TODO: 实现死亡惩罚机制
        }

        public void RestoreSpirit(float amount)
        {
            if (RuntimeData == null) return;
            RuntimeData.CurrentSpirit = Mathf.Min(RuntimeData.CurrentSpirit + amount, RuntimeData.MaxSpirit);
        }

        public void RestoreHP(float amount)
        {
            if (RuntimeData == null) return;
            RuntimeData.CurrentHP = Mathf.Min(RuntimeData.CurrentHP + amount, RuntimeData.MaxHP);
        }

        public string GetStatusSummary()
        {
            if (RuntimeData == null) return "No data";
            return $"HP: {RuntimeData.CurrentHP:F0}/{RuntimeData.MaxHP:F0}, Spirit: {RuntimeData.CurrentSpirit:F0}/{RuntimeData.MaxSpirit:F0}";
        }

        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[Summoner] {message}");
        }

        #region Hot Reload Support
        
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] SummonerController reloaded. {GetStatusSummary()}");
            InitializeComponents();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] SummonerController static reload");
        }

        #endregion
    }

    [System.Serializable]
    public class SummonerRuntimeData
    {
        public SummonerClassType ClassType = SummonerClassType.Berserker;
        
        [Header("Health")]
        public float CurrentHP = 100f;
        public float MaxHP = 100f;

        [Header("Spirit (Mana)")]
        public float CurrentSpirit = 100f;
        public float MaxSpirit = 100f;
        public float SpiritRegenRate = 5f;

        [Header("Domination (Population)")]
        public int DominationValue = 2;

        [Header("Skills")]
        public int[] ActiveSkillIDs = new int[0];
        public int[] PassiveSkillIDs = new int[0];
    }

    public enum SummonerClassType
    {
        Berserker,  // 狂战
        Warlock,    // 术士
        Chaos,      // 混沌
        Druid       // 德鲁伊
    }
}
