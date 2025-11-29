using UnityEngine;
using UnityEngine.AI;

namespace ClashOfGods.Gameplay.Map
{
    /// <summary>
    /// 敌人大地图控制器 - 巡逻、警戒、追击、战斗、休息五种状态
    /// Enemy overworld controller - Patrol, Alert, Chase, Combat, Rest states
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyOverworldController : MonoBehaviour
    {
        [Header("State")]
        public EnemyState CurrentState = EnemyState.Patrol;

        [Header("Detection")]
        public float AlertRange = 10f;
        public float AlertValue = 0f;
        public float AlertIncreaseRate = 20f;
        public float AlertDecreaseRate = 10f;
        public float MaxAlertValue = 100f;

        [Header("Patrol")]
        public float PatrolRange = 20f;
        public Vector3 PatrolCenter;
        public float PatrolWaitTime = 3f;

        [Header("Chase")]
        public float ChaseSpeed = 5f;
        public float CombatTriggerDistance = 2f;
        public float BroadcastRange = 15f;

        [Header("Rest")]
        public float RestDuration = 10f;
        public float RestTimer = 0f;

        [Header("References")]
        public Transform Player;
        public NavMeshAgent NavAgent;

        private IEnemyState currentStateHandler;
        private float patrolTimer = 0f;

        private void Awake()
        {
            NavAgent = GetComponent<NavMeshAgent>();
            PatrolCenter = transform.position;
        }

        private void Start()
        {
            ChangeState(EnemyState.Patrol);
        }

        private void Update()
        {
            if (Player == null)
            {
                Player = GameObject.FindGameObjectWithTag("Player")?.transform;
            }

            UpdateState();
        }

        /// <summary>
        /// 更新状态
        /// Update state
        /// </summary>
        private void UpdateState()
        {
            currentStateHandler?.Execute();

            // 根据当前状态执行逻辑
            switch (CurrentState)
            {
                case EnemyState.Patrol:
                    HandlePatrolState();
                    break;
                case EnemyState.Alert:
                    HandleAlertState();
                    break;
                case EnemyState.Chase:
                    HandleChaseState();
                    break;
                case EnemyState.Combat:
                    HandleCombatState();
                    break;
                case EnemyState.Rest:
                    HandleRestState();
                    break;
            }
        }

        #region State Handlers

        private void HandlePatrolState()
        {
            if (NavAgent == null) return;

            // 检测玩家
            if (Player != null && IsPlayerInAlertRange())
            {
                ChangeState(EnemyState.Alert);
                return;
            }

            // 巡逻移动
            if (!NavAgent.hasPath || NavAgent.remainingDistance < 0.5f)
            {
                patrolTimer += Time.deltaTime;
                
                if (patrolTimer >= PatrolWaitTime)
                {
                    Vector3 randomPoint = PatrolCenter + Random.insideUnitSphere * PatrolRange;
                    randomPoint.y = transform.position.y; // 保持Y轴高度
                    NavAgent.SetDestination(randomPoint);
                    patrolTimer = 0f;
                }
            }
        }

        private void HandleAlertState()
        {
            if (NavAgent == null) return;

            // 停止移动
            NavAgent.isStopped = true;

            if (Player != null && IsPlayerInAlertRange())
            {
                // 增加警戒值
                float distance = Vector3.Distance(transform.position, Player.position);
                float increaseRate = AlertIncreaseRate * (1f - distance / AlertRange);
                AlertValue += increaseRate * Time.deltaTime;

                // 面向玩家
                Vector3 direction = (Player.position - transform.position).normalized;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
                }

                // 警戒值满，进入追击
                if (AlertValue >= MaxAlertValue)
                {
                    ChangeState(EnemyState.Chase);
                }
            }
            else
            {
                // 玩家离开范围，警戒值下降
                AlertValue -= AlertDecreaseRate * Time.deltaTime;
                
                if (AlertValue <= 0)
                {
                    AlertValue = 0;
                    ChangeState(EnemyState.Patrol);
                }
            }
        }

        private void HandleChaseState()
        {
            if (NavAgent == null) return;

            NavAgent.isStopped = false;
            NavAgent.speed = ChaseSpeed;

            if (Player == null)
            {
                ChangeState(EnemyState.Patrol);
                return;
            }

            // 追击玩家
            float distance = Vector3.Distance(transform.position, Player.position);
            
            if (distance <= CombatTriggerDistance)
            {
                // 触发战斗
                ChangeState(EnemyState.Combat);
            }
            else if (!IsInActivityArea())
            {
                // 离开活动区域，返回
                NavAgent.SetDestination(PatrolCenter);
                ChangeState(EnemyState.Patrol);
            }
            else
            {
                NavAgent.SetDestination(Player.position);
                
                // 广播玩家位置
                BroadcastPosition();
            }
        }

        private void HandleCombatState()
        {
            // 触发战斗场景切换
            Debug.Log("[Enemy] Triggering combat!");
            
            // TODO: 切换到战斗场景
            // TODO: 检测周围R2范围内的敌人，一起进入战斗
            
            // 临时：战斗后返回巡逻
            ChangeState(EnemyState.Patrol);
        }

        private void HandleRestState()
        {
            if (NavAgent == null) return;

            NavAgent.isStopped = true;
            
            RestTimer += Time.deltaTime;
            
            if (RestTimer >= RestDuration)
            {
                RestTimer = 0f;
                ChangeState(EnemyState.Patrol);
            }
        }

        #endregion

        /// <summary>
        /// 改变状态
        /// Change state
        /// </summary>
        private void ChangeState(EnemyState newState)
        {
            Debug.Log($"[Enemy] State changed: {CurrentState} -> {newState}");
            
            CurrentState = newState;
            AlertValue = 0f;
            
            // TODO: 实现状态模式
        }

        /// <summary>
        /// 检查玩家是否在警戒范围内
        /// Check if player is in alert range
        /// </summary>
        private bool IsPlayerInAlertRange()
        {
            if (Player == null) return false;
            
            float distance = Vector3.Distance(transform.position, Player.position);
            return distance <= AlertRange;
        }

        /// <summary>
        /// 检查是否在活动区域内
        /// Check if in activity area
        /// </summary>
        private bool IsInActivityArea()
        {
            float distance = Vector3.Distance(transform.position, PatrolCenter);
            return distance <= PatrolRange * 2f;
        }

        /// <summary>
        /// 广播玩家位置
        /// Broadcast player position
        /// </summary>
        public void BroadcastPosition()
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, BroadcastRange);
            
            foreach (Collider col in colliders)
            {
                EnemyOverworldController otherEnemy = col.GetComponent<EnemyOverworldController>();
                
                if (otherEnemy != null && otherEnemy != this)
                {
                    otherEnemy.ReceiveBroadcast(Player);
                }
            }
        }

        /// <summary>
        /// 接收广播
        /// Receive broadcast
        /// </summary>
        public void ReceiveBroadcast(Transform playerTransform)
        {
            Player = playerTransform;
            ChangeState(EnemyState.Chase);
        }

        private void OnDrawGizmosSelected()
        {
            // 警戒范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, AlertRange);
            
            // 巡逻范围
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(PatrolCenter, PatrolRange);
            
            // 广播范围
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, BroadcastRange);
        }
    }

    /// <summary>
    /// 敌人状态
    /// Enemy state
    /// </summary>
    public enum EnemyState
    {
        Patrol,     // 巡逻
        Alert,      // 警戒
        Chase,      // 追击
        Combat,     // 战斗
        Rest        // 休息
    }

    /// <summary>
    /// 敌人状态接口（状态模式）
    /// Enemy state interface (State pattern)
    /// </summary>
    public interface IEnemyState
    {
        void Enter();
        void Execute();
        void Exit();
    }
}
