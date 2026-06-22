using UnityEngine;
using Wenjian.Client.Net;

namespace Wenjian.Client.Presentation
{
    public sealed class RogueRoomScenePresenter : MonoBehaviour
    {
        [SerializeField]
        private Transform roomActiveTint;

        [SerializeField]
        private Transform exitSeal;

        [SerializeField]
        private Transform exitPortal;

        [SerializeField]
        private Transform monsterMarkerRoot;

        [SerializeField]
        private TextMesh roomStatusText;

        [SerializeField]
        private TextMesh exitPromptText;

        [SerializeField]
        private Color activeRoomColor = new(0.42f, 0.72f, 0.52f, 0.24f);

        [SerializeField]
        private Color finishedRoomColor = new(0.74f, 0.86f, 0.42f, 0.20f);

        public Transform RoomActiveTint
        {
            get => roomActiveTint;
            set => roomActiveTint = value;
        }

        public Transform ExitSeal
        {
            get => exitSeal;
            set => exitSeal = value;
        }

        public Transform ExitPortal
        {
            get => exitPortal;
            set => exitPortal = value;
        }

        public Transform MonsterMarkerRoot
        {
            get => monsterMarkerRoot;
            set => monsterMarkerRoot = value;
        }

        public TextMesh RoomStatusText
        {
            get => roomStatusText;
            set
            {
                roomStatusText = value;
                SetStatusText(LastRoomState);
            }
        }

        public TextMesh ExitPromptText
        {
            get => exitPromptText;
            set
            {
                exitPromptText = value;
                SetExitPromptVisible(false);
            }
        }

        public string LastRoomState { get; private set; } = "Field";

        public bool ExitInteractable { get; private set; }

        private void Awake()
        {
            ApplyFieldState();
        }

        public void ApplyFieldState()
        {
            LastRoomState = "Field";
            ExitInteractable = false;
            SetActive(roomActiveTint, false);
            SetActive(exitSeal, false);
            SetActive(exitPortal, false);
            SetExitPromptVisible(false);
            SetMonsterMarkerCount(0);
            SetStatusText("Field");
        }

        public bool ApplyRogueStart(FirstChainMessage message)
        {
            if (message == null || message.Type != "ROGUE_START")
            {
                return false;
            }

            int rogueId = message.Has("rogueId") ? message.GetInt("rogueId") : 0;
            int monsters = message.Has("monsters") ? Mathf.Max(0, message.GetInt("monsters")) : 0;
            LastRoomState = rogueId > 0 ? $"Rogue {rogueId}" : "Rogue";
            ExitInteractable = false;

            SetActive(roomActiveTint, true);
            SetRendererColor(roomActiveTint, activeRoomColor);
            SetActive(exitSeal, true);
            SetActive(exitPortal, false);
            SetExitPromptVisible(false);
            SetMonsterMarkerCount(monsters);
            SetStatusText($"{LastRoomState}  Monsters {monsters}");
            return true;
        }

        public bool ApplyRogueFinish(FirstChainMessage message)
        {
            if (message == null || message.Type != "ROGUE_FINISH")
            {
                return false;
            }

            bool success = !message.Has("success") || message.GetString("success") == "true";
            int itemId = message.Has("itemId") ? message.GetInt("itemId") : 0;
            int count = message.Has("count") ? message.GetInt("count") : 0;
            LastRoomState = success ? "Finished" : "Failed";
            ExitInteractable = false;

            SetActive(roomActiveTint, true);
            SetRendererColor(roomActiveTint, finishedRoomColor);
            SetActive(exitSeal, !success);
            SetActive(exitPortal, success);
            SetExitPromptVisible(false);
            SetMonsterMarkerCount(0);
            SetStatusText(itemId > 0 && count > 0 ? $"Reward {itemId} x{count}" : LastRoomState);
            return true;
        }

        public void ApplyRoomCleared()
        {
            LastRoomState = "Cleared";
            ExitInteractable = true;
            SetActive(roomActiveTint, true);
            SetRendererColor(roomActiveTint, finishedRoomColor);
            SetActive(exitSeal, false);
            SetActive(exitPortal, true);
            SetExitPromptVisible(false);
            SetMonsterMarkerCount(0);
            SetStatusText("Room Cleared");
        }

        public void ApplyNextRoom(int roomIndex, int monsters)
        {
            int safeRoomIndex = Mathf.Max(1, roomIndex);
            int safeMonsterCount = Mathf.Max(0, monsters);
            LastRoomState = $"Room {safeRoomIndex}";
            ExitInteractable = false;
            SetActive(roomActiveTint, true);
            SetRendererColor(roomActiveTint, activeRoomColor);
            SetActive(exitSeal, true);
            SetActive(exitPortal, false);
            SetExitPromptVisible(false);
            SetMonsterMarkerCount(safeMonsterCount);
            SetStatusText($"{LastRoomState}  Monsters {safeMonsterCount}");
        }

        public void SetExitPromptVisible(bool visible)
        {
            if (exitPromptText == null)
            {
                return;
            }

            bool shouldShow = visible && ExitInteractable;
            exitPromptText.gameObject.SetActive(shouldShow);
            if (shouldShow)
            {
                exitPromptText.text = "Choose Reward";
            }
        }

        private void SetMonsterMarkerCount(int visibleCount)
        {
            if (monsterMarkerRoot == null)
            {
                return;
            }

            for (int i = 0; i < monsterMarkerRoot.childCount; i++)
            {
                monsterMarkerRoot.GetChild(i).gameObject.SetActive(i < visibleCount);
            }
        }

        private void SetStatusText(string value)
        {
            if (roomStatusText != null)
            {
                roomStatusText.text = string.IsNullOrWhiteSpace(value) ? "Field" : value;
            }
        }

        private static void SetRendererColor(Transform target, Color color)
        {
            if (target == null)
            {
                return;
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null)
            {
                renderer.sharedMaterial.color = color;
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
