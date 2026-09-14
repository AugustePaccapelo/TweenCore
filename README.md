# TweenCore

TweenCore is a lightweight tween system for Unity. It is built around a small runtime API and a `TweenCoreComponent` that lets you create tweens directly from the Inspector, with a workflow inspired by Godot tweens.

## Features

- Create tweens from the Unity Inspector with `TweenCoreComponent`.
- Use tweens from code with a fluent API.
- Tween reflected object properties, callback-driven values, or manually-read values.
- Run multiple properties in parallel or as an ordered chain.
- Add delays, loops, custom curves, and UnityEvents.
- Use additive tweens for supported value types.

## Basic Usage

Runtime types live in the `TweenCore.Runtime` namespace:

```csharp
using TweenCore.Runtime;
```

### Inspector Workflow

Add `TweenCoreComponent` to a GameObject, then add one or more tween properties in the Inspector.

For each property:

- Choose the value type.
- Assign a target GameObject.
- Choose a component and property to tween.
- Configure start/end value, duration, delay, type, ease, loop, and events.

`TweenCoreComponent` only uses reflected properties for Inspector-authored tweens.

### Reflection From Code

```csharp
Tween tween = Tween.CreateTween();

tween.NewProperty(
        transform,
        TweenCoreTarget.Transform.GLOBAL_POSITION,
        Vector3.zero,
        new Vector3(5f, 2f, 0f),
        2f)
    .SetEase(TweenCoreEase.Out)
    .SetType(TweenCoreType.Bounce);

tween.Play();
```

If you want the tween to start from the current value, use the overload without an explicit start value:

```csharp
Tween tween = Tween.CreateTween();

tween.NewProperty(
        transform,
        TweenCoreTarget.Transform.LOCAL_SCALE,
        Vector3.one * 2f,
        0.5f)
    .SetEase(TweenCoreEase.Out)
    .SetType(TweenCoreType.Back);

tween.Play();
```

### Callback Setter

This avoids reflected setting during updates and is the preferred code path when writing tweens by script.

```csharp
Tween tween = Tween.CreateTween();

tween.NewProperty(
        value => transform.localScale = value,
        Vector3.zero,
        Vector3.one,
        0.5f)
    .SetEase(TweenCoreEase.Out)
    .SetType(TweenCoreType.Back);

tween.Play();
```

### Manual Value

Use this when you want TweenCore to calculate the value but apply it yourself.

```csharp
Tween tween = Tween.CreateTween();

TweenCoreProperty<Vector3> property = tween.NewProperty(
        Vector3.zero,
        Vector3.one,
        1f)
    .SetEase(TweenCoreEase.InOut)
    .SetType(TweenCoreType.Sine);

property.OnUpdateValue += (_, value) =>
{
    transform.localScale = value;
};

tween.Play();
```

## Tween Modes

### Parallel

Parallel mode starts all properties at the same time. It is the default.

```csharp
tween.Parallel();
```

### Chain

Chain mode starts properties one after another in the order they are stored in the tween.

```csharp
tween.Chain();
```

## Loops

```csharp
tween.SetLoop(true, 3);
```

`numIteration` controls how many times the tween runs:

- Negative values loop forever.
- `0` stops immediately.
- Positive values run that many iterations.

## Additive Tweens

Additive tweens treat the final value as a value to add to the current start value.

```csharp
tween.NewProperty(transform, TweenCoreTarget.Transform.GLOBAL_POSITION, Vector3.right * 2f, 1f)
    .SetIsAdditive(true);
```

For example, if the current position is `(10, 0, 0)` and the final value is `(2, 0, 0)`, the tween ends at `(12, 0, 0)`.

Supported additive types:

- `float`
- `double`
- `int`
- `uint`
- `long`
- `ulong`
- `decimal`
- `Vector2`
- `Vector3`
- `Vector4`
- `Quaternion`
- `Color`
- `Color32`

For `Quaternion`, additive means relative rotation composition: `start * deltaRotation`.

For `Color32`, channels are added and clamped between `0` and `255`.

## Supported Tween Value Types

Runtime interpolation supports:

- `float`
- `double`
- `int`
- `uint`
- `long`
- `ulong`
- `decimal`
- `Vector2`
- `Vector3`
- `Vector4`
- `Quaternion`
- `Color`
- `Color32`

The current Inspector add menu supports:

- `float`
- `double`
- `int`
- `uint`
- `long`
- `ulong`
- `decimal`
- `Vector2`
- `Vector3`
- `Vector4`
- `Quaternion`
- `Color`
- `Color32`

## Tween API
Create a tween with:

```csharp
Tween tween = Tween.CreateTween();
```

Common methods:

- `Play()`
- `Pause()`
- `Resume()`
- `Stop(bool setToFinalValue = true)`
- `Parallel()`
- `Chain()`
- `SetLoop(bool isLoop, int numIteration = -1)`
- `SetDestroyWhenFinish(bool destroy)`
- `SetSurviveOnUnload(bool survive)`
- `DestroyTween()`

Events:

- `OnStart`
- `OnUpdate`
- `OnFinish`
- `OnLoopFinish`

## TweenCoreProperty

Common methods:

- `SetDelay(float tweenDelay)`
- `SetType(TweenCoreType type)`
- `SetType(AnimationCurve curve)`
- `SetEase(TweenCoreEase ease)`
- `SetEase(AnimationCurve curve)`
- `From(TweenValueType value)`
- `FromCurrent()`
- `SetIsAdditive(bool isAdditive)`
- `Pause()`
- `Resume()`
- `Stop(bool setToFinalValue = true)`
- `SetToFinalVals()`

Events:

- `OnStart`
- `OnUpdate`
- `OnUpdateValue`
- `OnFinish`

## Performance Notes

- Callback setter tweens are the fastest code path.
- Reflected tweens resolve their target member once, then use cached accessors when possible.
- Inspector-authored tweens use reflection for convenience.
- Avoid using per-frame UnityEvents for large numbers of tweens unless you need Inspector wiring.

## Notes

- A `TweenCoreManager` is created automatically when needed.
- `TweenCoreComponent` can optionally play on start.
- `DestroyWhenFinish` removes the tween from the manager when it completes.
- Editor scripts are wrapped in `#if UNITY_EDITOR`, so they are excluded from builds.
