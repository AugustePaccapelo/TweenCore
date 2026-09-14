# TweenCore - Documentation

## Overview

System used to make animations on objects, for example to move an object from a point to another.
You can choose to use Reflection, a Function, or get the value and change it yourself.

Include a TweenCoreComponent to make any tween from the editor without any code. They only use Reflection.

## Examples of uses

**Reflection :**
***Example 1 :***

TweenCore tween = TweenCore.CreateTween();
TweenCoreProperty<Vector3> property = tween.NewProperty(transform, "position", Vector3.zero, new Vector3(5, 2, 0), 2f);
property.SetEase(TweenEase.Out);
property.SetType(TweenType.Bounce);

tween.Play();

***Example 2 :***

TweenCore tween = TweenCore.CreateTween();
tween.NewProperty(transform, TweenTarget.Transform.GLOBAL_POSITION, new Vector3(5, 2, 0), 2f)
    .SetEase(TweenEase.Out).SetType(TweenType.Bounce);

tween.Play();


**Function :**

TweenCore tween = TweenCore.CreateTween();

tween.NewProperty(f => _target.transform.localScale = f, Vector3.zero, Vector3.one, _time * 2)
    .SetType(TweenType.Bounce).SetEase(TweenEase.Out);

tween.Play();


**Manual :**

TweenCore tween = TweenCore.CreateTween();

TweenCoreProperty<Vector3> property = tween.NewProperty(Vector3.zero, Vector3.one, _time * 2)
    .SetType(TweenType.Bounce).SetEase(TweenEase.Out);

tween.Play();

transform.localScale = property.CurrentValue;

## Classes

### TweenCoreManager
Need to be in the game, manage all tweens.

**Methods :**
- "PauseAll()" // Do not set all tweens in pause mode, only the manager
- "ResumeAll()" // Same as PauseAll()
- "AddTween(TweenCore tween)"
- "RemoveTween(TweenCore tween)"
- "StopAll()"

### TweenCore
Contain and manage one or multiple TweenProperty.

**Static Methods :**
- "CreateTween()"

**Instance Methods :**
- "Play()"
- "Pause()"
- "Resume()"
- "Stop(bool setToFinalValue = true)"
- "Update(float deltaTime)"
- "NewProperty(...)" 4 overloads

- "SetParallel(bool isParallel)"
- "SetChain(bool isChain)"
- "Parallel()"
- "Chain()"
- "SetLoop(bool isLoop, int numIteration = -1)" // Any negative value will be compute as infinite, 0 no iterration.
- "SurviveOnSceneLoad()"
- "KillOnSceneUnLoad()"
- "SetSurviveOnUnload(bool survive)"
- "DestroyWhenFinish()"
- "DontDestroyWhenFinish()"
- "SetDestroyWhenFinish(bool destroy)"
- "DestroyTween()"

- "AddProperty(TweenCorePropertyBase property)"

**Events :**
- "OnStart<TweenCore>"
- "OnFinish<TweenCore>"
- "OnFinish<TweenCore>"
- "OnLoopFinish<TweenCore>"

### TweenCorePropertyBase
Abstract class, parent of TweenCoreProperty<TweenValueType> to manage multiple properties of different TweenValueTypes.

**Methods :**
- "Update(float deltaTime)"
- "Start()"
- "Stop(bool setToFinalValue = true)"

- "SetToFinalVals()"

- "AddNextProperty(TweenCorePropertyBase property)"

**Events :**
- "OnStart<TweenCorePropertyBase>"
- "OnUpdate<TweenCorePropertyBase>"
- "OnFinish<TweenCorePropertyBase>"

### TweenCoreProperty<TweenValueType>
Calculates the current value of type TweenValueType, and if wanted set the given property or field.

**Methods :**
- "SetDelay(float tweenDelay)"
- "SetType()" 5 overloads
- "SetEase()" 5 overloads
- "GetCurrentValue()"
- "From(TweenValueType value)"
- "FromCurrent()"
- "Pause()"
- "Resume()"
- "Stop(bool setToFinalValue = true)"
- "SetToFinalVals()"
- "SetIsAdditive(bool isAdd)" // This need to also be fromCurrent, if isAdd is true, property will set fromCurrent to true, if false, it will not modify fromCurrent
// Quaternion and Color32 is not supported in this unique case

**Events :**
- "OnUpdate<TweenValueType>"

## Supported types

**C# :**
- float
- double
- int
- uint
- long
- ulong
- decimal

**Unity :**
- Vector2
- Vector3
- Vector4
- Quaternion
- Color
- Color32