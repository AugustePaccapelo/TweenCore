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
- "PauseAll()"
- "ResumeAll()"
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

**Events :**
- "OnStart"
- "OnFinish"

### TweenCorePropertyBase
Abstract class, parent of TweenCoreProperty<TweenValueType> to manage multiple properties of different TweenValueTypes.

**Methods :**
- "Update(float deltaTime)"
- "Start()"
- "Stop()"
- "AddNextProperty(TweenCorePropertyBase property)"

**Events :**
- "OnStart"
- "OnUpdate"
- "OnFinish"

### TweenCoreProperty<TweenValueType>
Calculates the current value of type TweenValueType, and if wanted set the given property or field.

**Methods :**
- "SetDelay(float tweenDelay)"
- "SetType(TweenType newType)"
- "SetCustomType(Func<float, float> customType)"
- "SetEase(TweenType newEase)"
- "SetCustomEase(Func<float, Func<float, float>, float> customEase)"
- "GetCurrentValue()"
- "From(TweenValueType value)"
- "Pause()"
- "Resume()"

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