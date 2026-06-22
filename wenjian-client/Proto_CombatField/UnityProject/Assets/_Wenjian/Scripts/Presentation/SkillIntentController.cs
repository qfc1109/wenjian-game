using UnityEngine;
using Wenjian.Client.Net;

namespace Wenjian.Client.Presentation
{
    public sealed class SkillIntentController : MonoBehaviour
    {
        [SerializeField]
        private FirstChainHudController hudController;

        [SerializeField]
        private LocalPlayerMotor playerMotor;

        [SerializeField]
        private int skillId = 2001;

        [SerializeField]
        private float cooldownSeconds = 0.45f;

        [SerializeField]
        private float cooldownRemainingSeconds;

        [SerializeField]
        private int prototypeLocalSkillDamage = 50;

        [SerializeField]
        private TextMesh cooldownText;

        public FirstChainHudController HudController
        {
            get => hudController;
            set => hudController = value;
        }

        public LocalPlayerMotor PlayerMotor
        {
            get => playerMotor;
            set => playerMotor = value;
        }

        public int SkillId
        {
            get => skillId;
            set => skillId = Mathf.Max(1, value);
        }

        public float CooldownSeconds
        {
            get => cooldownSeconds;
            set
            {
                cooldownSeconds = Mathf.Max(0f, value);
                cooldownRemainingSeconds = Mathf.Min(cooldownRemainingSeconds, cooldownSeconds);
                RefreshCooldownText();
            }
        }

        public float CooldownRemainingSeconds => cooldownRemainingSeconds;

        public int PrototypeLocalSkillDamage
        {
            get => prototypeLocalSkillDamage;
            set => prototypeLocalSkillDamage = Mathf.Max(1, value);
        }

        public TextMesh CooldownText
        {
            get => cooldownText;
            set
            {
                cooldownText = value;
                RefreshCooldownText();
            }
        }

        private void Awake()
        {
            RefreshCooldownText();
        }

        private void Update()
        {
            TickCooldown(Time.deltaTime);
            if (!Input.GetKeyDown(KeyCode.Space) && !Input.GetKeyDown(KeyCode.J) && !Input.GetKeyDown(KeyCode.K))
            {
                return;
            }

            TryCastSkill(playerMotor == null ? Vector2.down : playerMotor.LastMoveDirection);
        }

        public bool TryCastSkill(Vector2 aimDirection)
        {
            if (cooldownRemainingSeconds > Mathf.Epsilon)
            {
                RefreshCooldownText();
                return false;
            }

            if (hudController == null)
            {
                return false;
            }

            Vector2Int discreteAim = ToDiscreteAim(aimDirection);
            bool resolved = hudController.CurrentSnapshot.IsConnected
                ? hudController.QueueSkillCommand(skillId, discreteAim)
                : hudController.ApplyPrototypeSkillHit(skillId, discreteAim, prototypeLocalSkillDamage);
            if (resolved)
            {
                playerMotor?.PlayAttack(new Vector2(discreteAim.x, discreteAim.y));
                cooldownRemainingSeconds = cooldownSeconds;
                RefreshCooldownText();
            }

            return resolved;
        }

        public void TickCooldown(float deltaTime)
        {
            if (cooldownRemainingSeconds <= 0f)
            {
                cooldownRemainingSeconds = 0f;
                RefreshCooldownText();
                return;
            }

            cooldownRemainingSeconds = Mathf.Max(0f, cooldownRemainingSeconds - Mathf.Max(0f, deltaTime));
            RefreshCooldownText();
        }

        public static Vector2Int ToDiscreteAim(Vector2 direction)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                return new Vector2Int(0, -1);
            }

            Vector2 normalized = direction.sqrMagnitude > 1f ? direction.normalized : direction;
            int x = Mathf.RoundToInt(Mathf.Clamp(normalized.x, -1f, 1f));
            int y = Mathf.RoundToInt(Mathf.Clamp(normalized.y, -1f, 1f));
            return x == 0 && y == 0 ? new Vector2Int(0, -1) : new Vector2Int(x, y);
        }

        private void RefreshCooldownText()
        {
            if (cooldownText == null)
            {
                return;
            }

            cooldownText.text = cooldownRemainingSeconds > Mathf.Epsilon
                ? $"Skill CD {cooldownRemainingSeconds:0.0}s"
                : "Skill Ready";
        }
    }
}
