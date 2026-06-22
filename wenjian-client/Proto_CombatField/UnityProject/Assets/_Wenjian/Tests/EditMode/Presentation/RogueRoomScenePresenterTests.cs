using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class RogueRoomScenePresenterTests
    {
        [Test]
        public void ApplyRogueStartShowsActiveRoomSealAndMonsterMarkers()
        {
            var presenter = new GameObject("RogueRoomScene").AddComponent<RogueRoomScenePresenter>();
            presenter.RoomActiveTint = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitSeal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.MonsterMarkerRoot = CreateMonsterMarkers(3);
            presenter.RoomStatusText = new GameObject("RoomStatusText").AddComponent<TextMesh>();
            presenter.ApplyFieldState();

            FirstChainMessage.TryParse(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 x=1500 y=1500 monsterId=3001 monsters=2 rewardPoolId=5001 entities=9",
                out var message);

            bool applied = presenter.ApplyRogueStart(message);

            Assert.That(applied, Is.True);
            Assert.That(presenter.LastRoomState, Is.EqualTo("Rogue 4001"));
            Assert.That(presenter.RoomActiveTint.gameObject.activeSelf, Is.True);
            Assert.That(presenter.ExitSeal.gameObject.activeSelf, Is.True);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(0).gameObject.activeSelf, Is.True);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(1).gameObject.activeSelf, Is.True);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(2).gameObject.activeSelf, Is.False);
            Assert.That(presenter.RoomStatusText.text, Is.EqualTo("Rogue 4001  Monsters 2"));
        }

        [Test]
        public void ApplyRogueFinishOpensExitAndShowsRewardSummary()
        {
            var presenter = new GameObject("RogueRoomScene").AddComponent<RogueRoomScenePresenter>();
            presenter.RoomActiveTint = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitSeal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.MonsterMarkerRoot = CreateMonsterMarkers(2);
            presenter.RoomStatusText = new GameObject("RoomStatusText").AddComponent<TextMesh>();
            FirstChainMessage.TryParse(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 monsters=2 rewardPoolId=5001 entities=9",
                out var startMessage);
            presenter.ApplyRogueStart(startMessage);

            FirstChainMessage.TryParse(
                "type=ROGUE_FINISH code=OK instanceId=9000001 success=true itemId=6001 count=3",
                out var finishMessage);

            bool applied = presenter.ApplyRogueFinish(finishMessage);

            Assert.That(applied, Is.True);
            Assert.That(presenter.LastRoomState, Is.EqualTo("Finished"));
            Assert.That(presenter.ExitSeal.gameObject.activeSelf, Is.False);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(0).gameObject.activeSelf, Is.False);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(1).gameObject.activeSelf, Is.False);
            Assert.That(presenter.RoomStatusText.text, Is.EqualTo("Reward 6001 x3"));
        }

        [Test]
        public void ApplyRoomClearedOpensExitClearsMarkersAndShowsVictoryStatus()
        {
            var presenter = new GameObject("RogueRoomScene").AddComponent<RogueRoomScenePresenter>();
            presenter.RoomActiveTint = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitSeal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitPortal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitPromptText = new GameObject("ExitPromptText").AddComponent<TextMesh>();
            presenter.MonsterMarkerRoot = CreateMonsterMarkers(2);
            presenter.RoomStatusText = new GameObject("RoomStatusText").AddComponent<TextMesh>();
            FirstChainMessage.TryParse(
                "type=ROGUE_START code=OK playerId=1000001 rogueId=4001 instanceId=9000001 mapId=2001 monsters=2 rewardPoolId=5001 entities=9",
                out var startMessage);
            presenter.ApplyRogueStart(startMessage);

            presenter.ApplyRoomCleared();

            Assert.That(presenter.LastRoomState, Is.EqualTo("Cleared"));
            Assert.That(presenter.ExitInteractable, Is.True);
            Assert.That(presenter.ExitSeal.gameObject.activeSelf, Is.False);
            Assert.That(presenter.ExitPortal.gameObject.activeSelf, Is.True);
            Assert.That(presenter.ExitPromptText.gameObject.activeSelf, Is.False);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(0).gameObject.activeSelf, Is.False);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(1).gameObject.activeSelf, Is.False);
            Assert.That(presenter.RoomStatusText.text, Is.EqualTo("Room Cleared"));
        }

        [Test]
        public void ApplyNextRoomResealsExitHidesPromptAndResetsMarkers()
        {
            var presenter = new GameObject("RogueRoomScene").AddComponent<RogueRoomScenePresenter>();
            presenter.RoomActiveTint = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitSeal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitPortal = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
            presenter.ExitPromptText = new GameObject("ExitPromptText").AddComponent<TextMesh>();
            presenter.MonsterMarkerRoot = CreateMonsterMarkers(3);
            presenter.RoomStatusText = new GameObject("RoomStatusText").AddComponent<TextMesh>();
            presenter.ApplyRoomCleared();
            presenter.SetExitPromptVisible(true);

            presenter.ApplyNextRoom(2, 3);

            Assert.That(presenter.LastRoomState, Is.EqualTo("Room 2"));
            Assert.That(presenter.ExitInteractable, Is.False);
            Assert.That(presenter.ExitSeal.gameObject.activeSelf, Is.True);
            Assert.That(presenter.ExitPortal.gameObject.activeSelf, Is.False);
            Assert.That(presenter.ExitPromptText.gameObject.activeSelf, Is.False);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(0).gameObject.activeSelf, Is.True);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(1).gameObject.activeSelf, Is.True);
            Assert.That(presenter.MonsterMarkerRoot.GetChild(2).gameObject.activeSelf, Is.True);
            Assert.That(presenter.RoomStatusText.text, Is.EqualTo("Room 2  Monsters 3"));
        }

        private static Transform CreateMonsterMarkers(int count)
        {
            var root = new GameObject("MonsterMarkerRoot").transform;
            for (int i = 0; i < count; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Quad);
                marker.name = $"MonsterMarker_{i:00}";
                marker.transform.SetParent(root);
            }

            return root;
        }
    }
}
