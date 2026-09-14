using System;

// Author : Auguste Paccapelo

namespace TweenCore.Runtime
{
    public enum TweenEase
    {
        In, Out, InOut, OutIn, Custom, CustomCurve
    }

    public enum TweenType
    {
        Linear, Sine, Cubic, Quint, Circ, Elastic, Quad, Quart, Expo, Back, Bounce, Custom, CustomCurve
    }

    public static class TweenTarget
    {
        public static class Transform
        {
            public const string GLOBAL_POSITION = "position";
            public const string LOCAL_POSITION = "localPosition";
            public const string LOCAL_SCALE = "localScale";
            public const string GLOBAL_ROTATION_QUATERNION = "rotation";
            public const string GLOBAL_ROTATION_EULER_ANGLE = "eulerAngles";
            public const string LOCAL_ROTATION_QUATERNION = "localRotation";
            public const string LOCAL_ROTATION_EULER_ANGLE = "localEulerAngles";
        }

        public static class Renderer
        {
            public const string COLOR = "color";
        }
    }

    [Obsolete("Use TweenTarget instead.")]
    public static class TweenCoreTarget
    {
        public static class Transform
        {
            public const string GLOBAL_POSITION = TweenTarget.Transform.GLOBAL_POSITION;
            public const string LOCAL_POSITION = TweenTarget.Transform.LOCAL_POSITION;
            public const string LOCAL_SCALE = TweenTarget.Transform.LOCAL_SCALE;
            public const string GLOBAL_ROTATION_QUATERNION = TweenTarget.Transform.GLOBAL_ROTATION_QUATERNION;
            public const string GLOBAL_ROTATION_EULER_ANGLE = TweenTarget.Transform.GLOBAL_ROTATION_EULER_ANGLE;
            public const string LOCAL_ROTATION_QUATERNION = TweenTarget.Transform.LOCAL_ROTATION_QUATERNION;
            public const string LOCAL_ROTATION_EULER_ANGLE = TweenTarget.Transform.LOCAL_ROTATION_EULER_ANGLE;
        }

        public static class Renderer
        {
            public const string COLOR = TweenTarget.Renderer.COLOR;
        }
    }
}
