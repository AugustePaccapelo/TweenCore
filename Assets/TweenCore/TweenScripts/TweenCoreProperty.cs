using System;
using UnityEngine;

namespace TweenCore.Runtime
{
    [Serializable]
    [Obsolete("Use TweenProperty<TweenValueType> instead.")]
    public class TweenCoreProperty<TweenValueType> : TweenProperty<TweenValueType>
    {
        public TweenCoreProperty()
        {
        }

        public TweenCoreProperty(TweenValueType startVal, TweenValueType finalVal, float time)
            : base(startVal, finalVal, time)
        {
        }

        public TweenCoreProperty(Action<TweenValueType> function, TweenValueType startVal, TweenValueType finalVal, float duration)
            : base(function, startVal, finalVal, duration)
        {
        }

        public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType finalVal, float duration)
            : base(obj, method, finalVal, duration)
        {
        }

        public TweenCoreProperty(UnityEngine.Object obj, string method, TweenValueType startVal, TweenValueType finalVal, float duration)
            : base(obj, method, startVal, finalVal, duration)
        {
        }
    }
}
