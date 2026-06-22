using NUnit.Framework;
using UnityEngine;
using Wenjian.Client.Net;
using Wenjian.Client.Presentation;

namespace Wenjian.Client.Tests.Presentation
{
    public sealed class PrototypeCombatFeedbackPresenterTests
    {
        [Test]
        public void ApplySkillEventShowsSwordQiInAimDirection()
        {
            var presenter = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            var swordQi = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var slashWake = GameObject.CreatePrimitive(PrimitiveType.Quad);
            presenter.SwordQi = swordQi.transform;
            presenter.SlashWake = slashWake.transform;

            FirstChainMessage.TryParse(
                "type=SKILL_EVENT code=OK casterId=1000001 skillId=2001 x=3200 y=2400 aimX=1 aimY=0",
                out var message);

            bool applied = presenter.ApplySkillEvent(message);

            Assert.That(applied, Is.True);
            Assert.That(presenter.LastSkillId, Is.EqualTo(2001));
            Assert.That(presenter.LastAimDirection, Is.EqualTo(new Vector2Int(1, 0)));
            Assert.That(swordQi.activeSelf, Is.True);
            Assert.That(swordQi.transform.localPosition.x, Is.GreaterThan(0f));
            Assert.That(slashWake.activeSelf, Is.True);
            Assert.That(slashWake.transform.localScale.x, Is.GreaterThan(swordQi.transform.localScale.x));
        }

        [Test]
        public void ApplySkillEventShowsHitRangePreviewInAimDirection()
        {
            var presenter = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            var hitRangePreview = GameObject.CreatePrimitive(PrimitiveType.Quad);
            presenter.HitRangePreview = hitRangePreview.transform;

            FirstChainMessage.TryParse(
                "type=SKILL_EVENT code=OK casterId=1000001 skillId=2001 x=3200 y=2400 aimX=0 aimY=1",
                out var message);

            bool applied = presenter.ApplySkillEvent(message);

            Assert.That(applied, Is.True);
            Assert.That(hitRangePreview.activeSelf, Is.True);
            Assert.That(hitRangePreview.transform.localPosition.y, Is.GreaterThan(0f));
            Assert.That(hitRangePreview.transform.localScale.x, Is.GreaterThan(1.2f));

            presenter.TickFeedback(1f);

            Assert.That(hitRangePreview.activeSelf, Is.False);
        }

        [Test]
        public void ApplyDamageEventShowsHitBurstDamageTextAndHidesAfterTimer()
        {
            var presenter = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            var damageText = new GameObject("DamageText").AddComponent<TextMesh>();
            var hitBurst = GameObject.CreatePrimitive(PrimitiveType.Quad);
            presenter.DamageText = damageText;
            presenter.HitBurst = hitBurst.transform;

            FirstChainMessage.TryParse(
                "type=DAMAGE_EVENT code=OK sourceId=1000001 targetId=2000001 hpDelta=-12",
                out var message);
            Vector3 startPosition = damageText.transform.localPosition;

            bool applied = presenter.ApplyDamageEvent(message);

            Assert.That(applied, Is.True);
            Assert.That(presenter.LastDamageDelta, Is.EqualTo(-12));
            Assert.That(damageText.text, Is.EqualTo("-12"));
            Assert.That(damageText.gameObject.activeSelf, Is.True);
            Assert.That(hitBurst.activeSelf, Is.True);

            presenter.TickFeedback(0.2f);

            Assert.That(damageText.transform.localPosition.y, Is.GreaterThan(startPosition.y));

            presenter.TickFeedback(1f);

            Assert.That(damageText.gameObject.activeSelf, Is.False);
            Assert.That(hitBurst.activeSelf, Is.False);
        }

        [Test]
        public void ApplyDamageEventStaggersTargetThenReturnsToIdlePulse()
        {
            var presenter = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            var enemy = new GameObject("TrainingEnemy");
            var stateText = new GameObject("EnemyStateText").AddComponent<TextMesh>();
            var renderer = enemy.AddComponent<SpriteRenderer>();
            presenter.TargetTransform = enemy.transform;
            presenter.TargetRenderer = renderer;
            presenter.EnemyStateText = stateText;

            presenter.TickFeedback(0.25f);

            Assert.That(presenter.LastEnemyState, Is.EqualTo("Idle"));
            Assert.That(enemy.transform.localScale.y, Is.GreaterThan(1f));

            FirstChainMessage.TryParse(
                "type=DAMAGE_EVENT code=OK sourceId=1000001 targetId=2000001 hpDelta=-12",
                out var message);

            bool applied = presenter.ApplyDamageEvent(message);

            Assert.That(applied, Is.True);
            Assert.That(presenter.LastEnemyState, Is.EqualTo("Hit"));
            Assert.That(stateText.text, Is.EqualTo("Hit"));
            Assert.That(enemy.transform.localPosition.x, Is.GreaterThan(0f));

            presenter.TickFeedback(1f);

            Assert.That(presenter.LastEnemyState, Is.EqualTo("Idle"));
            Assert.That(stateText.text, Is.EqualTo("Idle"));
            Assert.That(enemy.transform.localPosition, Is.EqualTo(Vector3.zero));
        }

        [Test]
        public void ApplyEnemyDefeatedLeavesEnemyInDeadState()
        {
            var presenter = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            var enemy = new GameObject("TrainingEnemyVisual");
            var renderer = enemy.AddComponent<SpriteRenderer>();
            var stateText = new GameObject("EnemyStateText").AddComponent<TextMesh>();
            var hitBurst = GameObject.CreatePrimitive(PrimitiveType.Quad);
            presenter.TargetTransform = enemy.transform;
            presenter.TargetRenderer = renderer;
            presenter.EnemyStateText = stateText;
            presenter.HitBurst = hitBurst.transform;

            presenter.ApplyEnemyDefeated();
            presenter.TickFeedback(1f);

            Assert.That(presenter.LastEnemyState, Is.EqualTo("Dead"));
            Assert.That(stateText.text, Is.EqualTo("Dead"));
            Assert.That(enemy.transform.localScale.y, Is.LessThan(1f));
            Assert.That(hitBurst.activeSelf, Is.True);
        }

        [Test]
        public void ResetEnemyStateRestoresDefeatedEnemyForNextRoom()
        {
            var presenter = new GameObject("CombatFeedback").AddComponent<PrototypeCombatFeedbackPresenter>();
            var enemy = new GameObject("TrainingEnemyVisual");
            var renderer = enemy.AddComponent<SpriteRenderer>();
            var stateText = new GameObject("EnemyStateText").AddComponent<TextMesh>();
            var hitBurst = GameObject.CreatePrimitive(PrimitiveType.Quad);
            presenter.TargetTransform = enemy.transform;
            presenter.TargetRenderer = renderer;
            presenter.EnemyStateText = stateText;
            presenter.HitBurst = hitBurst.transform;
            presenter.ApplyEnemyDefeated();

            presenter.ResetEnemyState();

            Assert.That(presenter.LastEnemyState, Is.EqualTo("Idle"));
            Assert.That(stateText.text, Is.EqualTo("Idle"));
            Assert.That(enemy.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(enemy.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(hitBurst.activeSelf, Is.False);
        }
    }
}
