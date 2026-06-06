using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Presentation;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class CameraFollow2DTests
    {
        [Test]
        public void SnapToTargetCopiesTargetXYAndKeepsCameraZ()
        {
            var target = new GameObject("target");
            target.transform.position = new Vector3(2.5f, -1.25f, 0f);
            var cameraObject = new GameObject("camera");
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);
            var follow = cameraObject.AddComponent<CameraFollow2D>();
            follow.Target = target.transform;

            follow.SnapToTarget();

            Assert.That(cameraObject.transform.position.x, Is.EqualTo(2.5f).Within(0.001f));
            Assert.That(cameraObject.transform.position.y, Is.EqualTo(-1.25f).Within(0.001f));
            Assert.That(cameraObject.transform.position.z, Is.EqualTo(-10f).Within(0.001f));
        }

        [Test]
        public void SnapToTargetDoesNothingWithoutTarget()
        {
            var cameraObject = new GameObject("camera");
            cameraObject.transform.position = new Vector3(1f, 2f, -8f);
            var follow = cameraObject.AddComponent<CameraFollow2D>();

            follow.SnapToTarget();

            Assert.That(cameraObject.transform.position, Is.EqualTo(new Vector3(1f, 2f, -8f)));
        }
    }
}
