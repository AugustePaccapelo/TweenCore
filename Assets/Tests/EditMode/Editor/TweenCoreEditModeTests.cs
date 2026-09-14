using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class TweenCoreEditModeTests
{
    private class ReflectionTarget : ScriptableObject
    {
        public float Value { get; set; }
    }

    [Test]
    public void Stop_StopsStartedPropertiesInOriginalOrder()
    {
        TweenCore tween = new TweenCore().DontDestroyWhenFinish();
        TweenCoreProperty<float> first = tween.NewProperty(0f, 1f, 10f);
        TweenCoreProperty<float> second = tween.NewProperty(0f, 1f, 10f);
        List<string> finishOrder = new List<string>();

        first.OnFinish += _ => finishOrder.Add("first");
        second.OnFinish += _ => finishOrder.Add("second");

        tween.Play();
        tween.Stop(false);

        CollectionAssert.AreEqual(new[] { "first", "second" }, finishOrder);
        Assert.IsTrue(tween.IsFinished);
    }

    [Test]
    public void Update_InvokesValueUpdateOnlyOncePerFrame()
    {
        TweenCore tween = new TweenCore().DontDestroyWhenFinish();
        TweenCoreProperty<float> property = tween.NewProperty(0f, 10f, 1f);
        int updateCount = 0;
        float lastValue = 0f;

        property.OnUpdateValue += (_, value) =>
        {
            updateCount++;
            lastValue = value;
        };

        tween.Play();
        tween.Update(0.5f);

        Assert.AreEqual(1, updateCount);
        Assert.AreEqual(5f, lastValue, 0.0001f);
    }

    [Test]
    public void AdditiveColor32_ClampsFinalChannels()
    {
        TweenCore tween = new TweenCore().DontDestroyWhenFinish();
        TweenCoreProperty<Color32> property = tween.NewProperty(
            new Color32(250, 10, 20, 250),
            new Color32(10, 20, 250, 10),
            1f);

        property.SetIsAdditive(true);

        tween.Play();
        tween.Update(1f);

        Assert.AreEqual(new Color32(255, 30, 255, 255), property.CurrentValue);
    }

    [Test]
    public void AdditiveQuaternion_ComposesRelativeRotation()
    {
        TweenCore tween = new TweenCore().DontDestroyWhenFinish();
        Quaternion start = Quaternion.Euler(0f, 30f, 0f);
        Quaternion delta = Quaternion.Euler(0f, 45f, 0f);
        TweenCoreProperty<Quaternion> property = tween.NewProperty(start, delta, 1f);

        property.SetIsAdditive(true);

        tween.Play();
        tween.Update(1f);

        Assert.Less(Quaternion.Angle(start * delta, property.CurrentValue), 0.01f);
    }

    [Test]
    public void ReflectionTween_SetsPropertyValue()
    {
        ReflectionTarget target = ScriptableObject.CreateInstance<ReflectionTarget>();
        TweenCore tween = new TweenCore().DontDestroyWhenFinish();

        tween.NewProperty(target, nameof(ReflectionTarget.Value), 0f, 10f, 1f);

        tween.Play();
        tween.Update(0.5f);

        Assert.AreEqual(5f, target.Value, 0.0001f);
        Object.DestroyImmediate(target);
    }

    [Test]
    public void ChainWithDestroyOnFinish_StartsNextPropertyAfterRemovingCurrent()
    {
        TweenCore tween = new TweenCore().Chain();
        TweenCoreProperty<float> first = tween.NewProperty(0f, 1f, 0.1f);
        TweenCoreProperty<float> second = tween.NewProperty(0f, 1f, 0.1f);

        tween.Play();
        tween.Update(0.1f);

        Assert.IsTrue(first.IsFinish);
        Assert.IsTrue(second.HasStarted);
    }
}
