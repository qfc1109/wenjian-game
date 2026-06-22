using UnityEngine;
using Wenjian.Client.Net;

namespace Wenjian.Client.Presentation
{
    public sealed class PrototypeCombatFeedbackPresenter : MonoBehaviour
    {
        [SerializeField]
        private Transform swordQi;

        [SerializeField]
        private Transform slashWake;

        [SerializeField]
        private Transform hitRangePreview;

        [SerializeField]
        private TextMesh damageText;

        [SerializeField]
        private Transform hitBurst;

        [SerializeField]
        private Transform targetTransform;

        [SerializeField]
        private SpriteRenderer targetRenderer;

        [SerializeField]
        private TextMesh enemyStateText;

        [SerializeField]
        private float skillVisibleSeconds = 0.35f;

        [SerializeField]
        private float damageVisibleSeconds = 0.65f;

        [SerializeField]
        private Color targetFlashColor = new(1f, 0.95f, 0.62f, 1f);

        [SerializeField]
        private float idlePulseAmplitude = 0.025f;

        [SerializeField]
        private float idlePulseSpeed = 4.5f;

        private float skillTimer;
        private float damageTimer;
        private float idleTimer;
        private Color targetBaseColor = Color.white;
        private Vector3 targetRendererBaseScale = Vector3.one;
        private Vector3 targetBaseLocalPosition = Vector3.zero;
        private Vector3 targetBaseLocalScale = Vector3.one;
        private Vector3 damageTextBaseLocalPosition;
        private bool enemyDefeated;

        public Transform SwordQi
        {
            get => swordQi;
            set => swordQi = value;
        }

        public Transform SlashWake
        {
            get => slashWake;
            set => slashWake = value;
        }

        public Transform HitRangePreview
        {
            get => hitRangePreview;
            set => hitRangePreview = value;
        }

        public TextMesh DamageText
        {
            get => damageText;
            set
            {
                damageText = value;
                if (damageText != null)
                {
                    damageTextBaseLocalPosition = damageText.transform.localPosition;
                }
            }
        }

        public Transform HitBurst
        {
            get => hitBurst;
            set => hitBurst = value;
        }

        public Transform TargetTransform
        {
            get => targetTransform;
            set
            {
                targetTransform = value;
                if (targetTransform != null)
                {
                    targetBaseLocalPosition = targetTransform.localPosition;
                    targetBaseLocalScale = targetTransform.localScale;
                }
            }
        }

        public SpriteRenderer TargetRenderer
        {
            get => targetRenderer;
            set
            {
                targetRenderer = value;
                if (targetRenderer != null)
                {
                    targetBaseColor = targetRenderer.color;
                    targetRendererBaseScale = targetRenderer.transform.localScale;
                }
            }
        }

        public TextMesh EnemyStateText
        {
            get => enemyStateText;
            set
            {
                enemyStateText = value;
                SetEnemyState(LastEnemyState);
            }
        }

        public int LastSkillId { get; private set; }

        public Vector2Int LastAimDirection { get; private set; }

        public int LastDamageDelta { get; private set; }

        public string LastEnemyState { get; private set; } = "Idle";

        private void Awake()
        {
            if (targetRenderer != null)
            {
                targetBaseColor = targetRenderer.color;
                targetRendererBaseScale = targetRenderer.transform.localScale;
            }

            if (targetTransform != null)
            {
                targetBaseLocalPosition = targetTransform.localPosition;
                targetBaseLocalScale = targetTransform.localScale;
            }

            SetActive(swordQi, false);
            SetActive(slashWake, false);
            SetActive(hitRangePreview, false);
            SetActive(hitBurst, false);
            if (damageText != null)
            {
                damageTextBaseLocalPosition = damageText.transform.localPosition;
                damageText.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            TickFeedback(Time.deltaTime);
        }

        public bool ApplySkillEvent(FirstChainMessage message)
        {
            if (message == null || message.Type != "SKILL_EVENT")
            {
                return false;
            }

            LastSkillId = message.Has("skillId") ? message.GetInt("skillId") : 0;
            int aimX = message.Has("aimX") ? Mathf.Clamp(message.GetInt("aimX"), -1, 1) : 0;
            int aimY = message.Has("aimY") ? Mathf.Clamp(message.GetInt("aimY"), -1, 1) : -1;
            LastAimDirection = aimX == 0 && aimY == 0 ? new Vector2Int(0, -1) : new Vector2Int(aimX, aimY);

            if (swordQi != null)
            {
                Vector2 direction = new Vector2(LastAimDirection.x, LastAimDirection.y).normalized;
                swordQi.gameObject.SetActive(true);
                swordQi.localPosition = new Vector3(direction.x * 0.9f, direction.y * 0.9f, -0.05f);
                swordQi.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                swordQi.localScale = new Vector3(1.35f, 0.18f, 1f);
            }

            if (slashWake != null)
            {
                Vector2 direction = new Vector2(LastAimDirection.x, LastAimDirection.y).normalized;
                slashWake.gameObject.SetActive(true);
                slashWake.localPosition = new Vector3(direction.x * 0.42f, direction.y * 0.42f, -0.06f);
                slashWake.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                slashWake.localScale = new Vector3(2.15f, 0.34f, 1f);
            }

            if (hitRangePreview != null)
            {
                Vector2 direction = new Vector2(LastAimDirection.x, LastAimDirection.y).normalized;
                hitRangePreview.gameObject.SetActive(true);
                hitRangePreview.localPosition = new Vector3(direction.x * 0.72f, direction.y * 0.72f, -0.07f);
                hitRangePreview.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
                hitRangePreview.localScale = new Vector3(1.45f, 0.82f, 1f);
            }

            skillTimer = skillVisibleSeconds;
            return true;
        }

        public bool ApplyDamageEvent(FirstChainMessage message)
        {
            if (message == null || message.Type != "DAMAGE_EVENT")
            {
                return false;
            }

            if (enemyDefeated)
            {
                return false;
            }

            LastDamageDelta = message.Has("hpDelta") ? message.GetInt("hpDelta") : 0;
            if (damageText != null)
            {
                damageText.text = LastDamageDelta.ToString();
                damageText.transform.localPosition = damageTextBaseLocalPosition;
                damageText.gameObject.SetActive(true);
            }

            if (hitBurst != null)
            {
                hitBurst.localScale = new Vector3(0.55f, 0.55f, 1f);
                hitBurst.gameObject.SetActive(true);
            }

            if (targetRenderer != null)
            {
                targetRenderer.color = targetFlashColor;
                targetRenderer.transform.localScale = targetRendererBaseScale * 1.12f;
            }

            if (targetTransform != null)
            {
                targetTransform.localPosition = targetBaseLocalPosition + new Vector3(0.12f, 0.04f, 0f);
                targetTransform.localScale = targetBaseLocalScale * 1.08f;
            }

            SetEnemyState("Hit");
            damageTimer = damageVisibleSeconds;
            return true;
        }

        public void ApplyEnemyDefeated()
        {
            enemyDefeated = true;
            damageTimer = 0f;
            if (damageText != null)
            {
                damageText.gameObject.SetActive(false);
            }

            if (hitBurst != null)
            {
                hitBurst.localScale = new Vector3(0.72f, 0.72f, 1f);
                hitBurst.gameObject.SetActive(true);
            }

            if (targetRenderer != null)
            {
                targetRenderer.color = new Color(targetBaseColor.r * 0.36f, targetBaseColor.g * 0.36f, targetBaseColor.b * 0.36f, targetBaseColor.a);
                targetRenderer.transform.localScale = targetRendererBaseScale;
            }

            if (targetTransform != null)
            {
                targetTransform.localPosition = targetBaseLocalPosition + new Vector3(0f, -0.18f, 0f);
                targetTransform.localScale = new Vector3(targetBaseLocalScale.x * 1.08f, targetBaseLocalScale.y * 0.42f, targetBaseLocalScale.z);
            }

            SetEnemyState("Dead");
        }

        public void ResetEnemyState()
        {
            enemyDefeated = false;
            damageTimer = 0f;
            if (damageText != null)
            {
                damageText.gameObject.SetActive(false);
                damageText.transform.localPosition = damageTextBaseLocalPosition;
            }

            SetActive(hitBurst, false);

            if (targetRenderer != null)
            {
                targetRenderer.color = targetBaseColor;
                targetRenderer.transform.localScale = targetRendererBaseScale;
            }

            if (targetTransform != null)
            {
                targetTransform.localPosition = targetBaseLocalPosition;
                targetTransform.localScale = targetBaseLocalScale;
            }

            SetEnemyState("Idle");
        }

        public void TickFeedback(float deltaTime)
        {
            float safeDelta = Mathf.Max(0f, deltaTime);
            idleTimer += safeDelta;
            if (skillTimer > 0f)
            {
                skillTimer -= safeDelta;
                if (skillTimer <= 0f)
                {
                    SetActive(swordQi, false);
                    SetActive(slashWake, false);
                    SetActive(hitRangePreview, false);
                }
            }

            if (enemyDefeated)
            {
                SetEnemyState("Dead");
                return;
            }

            if (damageTimer > 0f)
            {
                damageTimer -= safeDelta;
                if (damageText != null && damageText.gameObject.activeSelf)
                {
                    damageText.transform.localPosition += new Vector3(0f, safeDelta * 0.65f, 0f);
                }

                if (hitBurst != null && hitBurst.gameObject.activeSelf)
                {
                    hitBurst.localScale += new Vector3(safeDelta * 0.55f, safeDelta * 0.55f, 0f);
                }

                if (damageTimer <= 0f)
                {
                    if (damageText != null)
                    {
                        damageText.gameObject.SetActive(false);
                    }

                    if (targetRenderer != null)
                    {
                        targetRenderer.color = targetBaseColor;
                        targetRenderer.transform.localScale = targetRendererBaseScale;
                    }

                    if (targetTransform != null)
                    {
                        targetTransform.localPosition = targetBaseLocalPosition;
                        targetTransform.localScale = targetBaseLocalScale;
                    }

                    SetEnemyState("Idle");
                    SetActive(hitBurst, false);
                }
            }

            if (damageTimer <= 0f)
            {
                ApplyIdlePulse();
            }
        }

        private void ApplyIdlePulse()
        {
            if (targetTransform == null)
            {
                return;
            }

            float pulse = 1f + Mathf.Abs(Mathf.Sin(idleTimer * idlePulseSpeed)) * idlePulseAmplitude;
            targetTransform.localPosition = targetBaseLocalPosition;
            targetTransform.localScale = new Vector3(
                targetBaseLocalScale.x,
                targetBaseLocalScale.y * pulse,
                targetBaseLocalScale.z);
            SetEnemyState("Idle");
        }

        private void SetEnemyState(string state)
        {
            LastEnemyState = string.IsNullOrWhiteSpace(state) ? "Idle" : state;
            if (enemyStateText != null)
            {
                enemyStateText.text = LastEnemyState;
            }
        }

        private static void SetActive(Transform target, bool active)
        {
            if (target != null)
            {
                target.gameObject.SetActive(active);
            }
        }
    }
}
