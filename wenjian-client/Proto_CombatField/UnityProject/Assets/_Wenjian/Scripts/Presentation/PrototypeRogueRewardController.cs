using System;
using UnityEngine;
using Wenjian.Client.Net;

namespace Wenjian.Client.Presentation
{
    public readonly struct PrototypeRogueRewardChoice
    {
        public PrototypeRogueRewardChoice(int rewardItemId, string summary, string detail)
        {
            RewardItemId = rewardItemId;
            Summary = string.IsNullOrWhiteSpace(summary) ? "Reward" : summary;
            Detail = string.IsNullOrWhiteSpace(detail) ? Summary : detail;
        }

        public int RewardItemId { get; }

        public string Summary { get; }

        public string Detail { get; }
    }

    public sealed class PrototypeRogueRewardController : MonoBehaviour
    {
        private static readonly PrototypeRogueRewardChoice[] LocalRewardChoices =
        {
            new(7001, "Sword Intent +10%", "Sword Intent +10%"),
            new(7002, "Max HP +20", "Max HP +20"),
            new(7003, "Cooldown -10%", "Cooldown -10%")
        };

        [SerializeField]
        private Transform player;

        [SerializeField]
        private Transform exitPoint;

        [SerializeField]
        private RogueRoomScenePresenter roomScenePresenter;

        [SerializeField]
        private FirstChainHudController hudController;

        [SerializeField]
        private Transform rewardPanel;

        [SerializeField]
        private TextMesh[] rewardChoiceTexts = Array.Empty<TextMesh>();

        [SerializeField]
        private TrainingEnemyMotor[] enemyWave = Array.Empty<TrainingEnemyMotor>();

        [SerializeField]
        private float exitInteractDistance = 1.25f;

        private Vector3[] enemySpawnPositions = Array.Empty<Vector3>();
        private int activeEnemyCount = 1;

        public Transform Player
        {
            get => player;
            set => player = value;
        }

        public Transform ExitPoint
        {
            get => exitPoint;
            set => exitPoint = value;
        }

        public RogueRoomScenePresenter RoomScenePresenter
        {
            get => roomScenePresenter;
            set => roomScenePresenter = value;
        }

        public FirstChainHudController HudController
        {
            get => hudController;
            set => hudController = value;
        }

        public Transform RewardPanel
        {
            get => rewardPanel;
            set
            {
                rewardPanel = value;
                HideRewardChoices();
            }
        }

        public TextMesh[] RewardChoiceTexts
        {
            get => rewardChoiceTexts;
            set
            {
                rewardChoiceTexts = value ?? Array.Empty<TextMesh>();
                ApplyRewardChoiceTexts();
            }
        }

        public float ExitInteractDistance
        {
            get => exitInteractDistance;
            set => exitInteractDistance = Mathf.Max(0f, value);
        }

        public bool RewardPanelVisible => rewardPanel != null && rewardPanel.gameObject.activeSelf;

        public int CurrentRoomIndex { get; private set; } = 1;

        public int ActiveRewardChoiceCount => LocalRewardChoices.Length;

        private void Awake()
        {
            CaptureEnemySpawns();
            ApplyRewardChoiceTexts();
            HideRewardChoices();
        }

        private void Update()
        {
            TickInteraction();
            if (!RewardPanelVisible)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                ChooseReward(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ChooseReward(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                ChooseReward(2);
            }
        }

        public void ConfigureEnemyWave(params TrainingEnemyMotor[] enemies)
        {
            enemyWave = enemies ?? Array.Empty<TrainingEnemyMotor>();
            CaptureEnemySpawns();
        }

        public void StartRoom(int roomIndex, int monsterCount)
        {
            CurrentRoomIndex = Mathf.Max(1, roomIndex);
            activeEnemyCount = Mathf.Clamp(monsterCount, 1, Mathf.Max(1, enemyWave.Length));
            HideRewardChoices();
            roomScenePresenter?.ApplyNextRoom(CurrentRoomIndex, activeEnemyCount);
            hudController?.ApplyPrototypeNextRoom(CurrentRoomIndex, activeEnemyCount);
            ResetEnemyWave(activeEnemyCount);
        }

        public void TickInteraction()
        {
            if (roomScenePresenter == null || !roomScenePresenter.ExitInteractable || player == null || exitPoint == null)
            {
                roomScenePresenter?.SetExitPromptVisible(false);
                HideRewardChoices();
                return;
            }

            float distance = Vector2.Distance(player.position, exitPoint.position);
            bool nearExit = distance <= exitInteractDistance;
            roomScenePresenter.SetExitPromptVisible(nearExit);
            if (nearExit)
            {
                ShowRewardChoices();
            }
            else
            {
                HideRewardChoices();
            }
        }

        public bool ChooseReward(int choiceIndex)
        {
            if (!RewardPanelVisible || choiceIndex < 0 || choiceIndex >= LocalRewardChoices.Length)
            {
                return false;
            }

            PrototypeRogueRewardChoice choice = LocalRewardChoices[choiceIndex];
            hudController?.ApplyPrototypeRewardChoice(choice.RewardItemId, choice.Summary);
            StartRoom(CurrentRoomIndex + 1, GetMonsterCountForRoom(CurrentRoomIndex + 1));
            return true;
        }

        private int GetMonsterCountForRoom(int roomIndex)
        {
            int cap = Mathf.Max(1, enemyWave.Length);
            return Mathf.Clamp(roomIndex + 1, 1, cap);
        }

        private void ShowRewardChoices()
        {
            ApplyRewardChoiceTexts();
            if (rewardPanel != null)
            {
                rewardPanel.gameObject.SetActive(true);
            }
        }

        private void HideRewardChoices()
        {
            if (rewardPanel != null)
            {
                rewardPanel.gameObject.SetActive(false);
            }
        }

        private void ApplyRewardChoiceTexts()
        {
            if (rewardChoiceTexts == null)
            {
                return;
            }

            for (int i = 0; i < rewardChoiceTexts.Length; i++)
            {
                if (rewardChoiceTexts[i] == null)
                {
                    continue;
                }

                rewardChoiceTexts[i].text = i < LocalRewardChoices.Length
                    ? $"{i + 1}. {LocalRewardChoices[i].Detail}"
                    : string.Empty;
            }
        }

        private void CaptureEnemySpawns()
        {
            if (enemyWave == null)
            {
                enemySpawnPositions = Array.Empty<Vector3>();
                return;
            }

            enemySpawnPositions = new Vector3[enemyWave.Length];
            for (int i = 0; i < enemyWave.Length; i++)
            {
                enemySpawnPositions[i] = enemyWave[i] == null ? Vector3.zero : enemyWave[i].transform.position;
            }
        }

        private void ResetEnemyWave(int monsterCount)
        {
            if (enemyWave == null)
            {
                return;
            }

            for (int i = 0; i < enemyWave.Length; i++)
            {
                TrainingEnemyMotor enemy = enemyWave[i];
                if (enemy == null)
                {
                    continue;
                }

                bool active = i < monsterCount;
                enemy.gameObject.SetActive(active);
                if (i < enemySpawnPositions.Length)
                {
                    enemy.transform.position = enemySpawnPositions[i];
                }

                if (player != null)
                {
                    enemy.Target = player;
                }
            }
        }
    }
}
