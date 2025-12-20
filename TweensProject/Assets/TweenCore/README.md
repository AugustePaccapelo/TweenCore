# TweenCore - Documentation

Author : Auguste Paccapelo

----------

System used to make animations on objects, for example to move an object from a point to another.
You can choose to use Reflection, a Function, or get the value and change it yourself.

Include a TweenComponent to make any tween from the editor without any code. They only use Reflection.

----------

Class : 
TweenManager : Need to be in the game, manage all tweens.
Tween : Contain and manage one or multiple TweenProperty.
TweenProperty<TweenValueType> : Calculates the current value of type TweenValueType, and if wanted set the given property or field.
TweenPropertyBase : Abstract class, parent of TweenProperty<TweenValueType> to manage multiple properties of different TweenValueTypes.

----------

examples of uses : 
- Reflection : 
- - example 1 :

Tween tween = Tween.CreateTween();
TweenProperty<Vector3> property = tween.NewProperty(transform, "position", Vector3.zero, new Vector3(5, 2, 0), 2f);
property.SetEase(TweenEase.Out);
property.SetType(TweenType.Bounce);

tween.Play();

- - example 2 :

Tween tween = Tween.CreateTween();
tween.NewProperty(transform, TweenTarget.Transform.GLOBAL_POSITION, new Vector3(5, 2, 0), 2f)
    .SetEase(TweenEase.Out).SetType(TweenType.Bounce);

tween.Play();


- Function :

Tween tween = Tween.CreateTween();

tween.NewProperty(f => _target.transform.localScale = f, Vector3.zero, Vector3.one, _time * 2)
    .SetType(TweenType.Bounce).SetEase(TweenEase.Out);

tween.Play();


- Manual :

Tween tween = Tween.CreateTween();

TweenProperty<Vector3> property = tween.NewProperty(Vector3.zero, Vector3.one, _time * 2)
    .SetType(TweenType.Bounce).SetEase(TweenEase.Out);

tween.Play();

transform.localScale = property.CurrentValue;

----------

- - TweenManager : 
- Methods
.PauseAll()
.ResumeAll()
.AddTween(Tween tween)
.RemoveTween(Tween tween)
.StopAll()

- - Tween :
- methods : 
.CreateTween() (static)
.Play()
.Pause()
.Resume()
.Stop()
.Update(float deltaTime)
.DestroyTweenProperty(TweenPropertyBase property)
.NewProperty(...) 4 overloads
.SetParallel(bool isParallel)
.SetChain(bool isChain)
.Parallel()
.Chain()

- Events :
OnStart
OnFinish

- - TweenPropertyBase
- methods :
.Update(float deltaTime)
.Start()
.Stop()
.AddNextProperty(TweenPropertyBase property)

- Events
OnStart
OnFinish

- - TweenProperty :
- Methods

.SetDelay(float tweenDelay)
.SetType(TweenType newType)
.SetCustomType(Func<float, float> customType)
.SetEase(TweenType newEase)
.SetCustomEase(Func<float, Func<float, float>, float> customEase)
.GetCurrentValue()
.From(TweenValueType value)
.Pause()
.Resume()

- Events :
OnUpdate<TweenValueType>

----------

Currently supported types : 
C# :
float, double, int, uint, long, ulong, decimal
Unity :
Vector2, Vector3, Vector4, Quaternion, Color, Color32
