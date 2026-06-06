using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Core;

namespace Wenjian.Client.Tests.Core
{
    public sealed class WorldCoordinateMapperTests
    {
        [Test]
        public void DefaultCombatFieldMapperPlacesConfiguredSpawnAtOrigin()
        {
            var mapper = WorldCoordinateMapper.CreateDefaultCombatField();

            var unityPosition = mapper.ServerToUnity(new Vector2Int(3200, 2400));

            Assert.That(unityPosition.x, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(unityPosition.y, Is.EqualTo(0f).Within(0.0001f));
            Assert.That(unityPosition.z, Is.EqualTo(0f).Within(0.0001f));
        }

        [Test]
        public void DefaultCombatFieldMapperMapsOneTilePerHundredServerUnits()
        {
            var mapper = WorldCoordinateMapper.CreateDefaultCombatField();

            var unityPosition = mapper.ServerToUnity(new Vector2Int(3300, 2500));

            Assert.That(unityPosition.x, Is.EqualTo(1f).Within(0.0001f));
            Assert.That(unityPosition.y, Is.EqualTo(1f).Within(0.0001f));
        }

        [Test]
        public void ServerAndUnityCoordinatesRoundTrip()
        {
            var mapper = WorldCoordinateMapper.CreateDefaultCombatField();
            var serverPosition = new Vector2Int(3500, 2100);

            var unityPosition = mapper.ServerToUnity(serverPosition);
            var roundTripped = mapper.UnityToServer(unityPosition);

            Assert.That(roundTripped, Is.EqualTo(serverPosition));
        }
    }
}
