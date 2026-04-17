using UnityEngine;

namespace Project.Scripts.Utility.Easing
{
    public static class EaseUtility
    {
        private const float k_backOvershoot = 1.70158f;
        private const float k_backOvershootScale = 1.525f;
        private const float k_bounceMultiplier = 7.5625f;
        private const float k_bounceDivider = 2.75f;
        private const float k_elasticConstant4 = 2f * Mathf.PI / 3f;
        private const float k_elasticConstant5 = 2f * Mathf.PI / 4.5f;

        public static float EaseInSine(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - Mathf.Cos(clampedValue * Mathf.PI * 0.5f);
        }

        public static float EaseInCirc(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - Mathf.Sqrt(1f - clampedValue * clampedValue);
        }

        public static float EaseOutSine(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return Mathf.Sin(clampedValue * Mathf.PI * 0.5f);
        }

        public static float EaseInOutSine(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return -(Mathf.Cos(Mathf.PI * clampedValue) - 1f) * 0.5f;
        }

        public static float EaseInQuad(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return clampedValue * clampedValue;
        }

        public static float EaseOutQuad(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - ((1f - clampedValue) * (1f - clampedValue));
        }

        public static float EaseInOutQuad(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return clampedValue < 0.5f ? 2f * clampedValue * clampedValue : 1f - (Mathf.Pow(-2f * clampedValue + 2f, 2f) * 0.5f);
        }

        public static float EaseInCubic(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return clampedValue * clampedValue * clampedValue;
        }

        public static float EaseOutCubic(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - clampedValue, 3f);
        }

        public static float EaseInOutCubic(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return clampedValue < 0.5f ? 4f * clampedValue * clampedValue * clampedValue : 1f - (Mathf.Pow(-2f * clampedValue + 2f, 3f) * 0.5f);
        }

        public static float EaseInQuart(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return Mathf.Pow(clampedValue, 4f);
        }

        public static float EaseOutQuart(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - clampedValue, 4f);
        }

        public static float EaseInOutQuart(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return clampedValue < 0.5f ? 8f * Mathf.Pow(clampedValue, 4f) : 1f - (Mathf.Pow(-2f * clampedValue + 2f, 4f) * 0.5f);
        }

        public static float EaseInQuint(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return Mathf.Pow(clampedValue, 5f);
        }

        public static float EaseOutQuint(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - Mathf.Pow(1f - clampedValue, 5f);
        }

        public static float EaseInOutQuint(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return clampedValue < 0.5f ? 16f * Mathf.Pow(clampedValue, 5f) : 1f - (Mathf.Pow(-2f * clampedValue + 2f, 5f) * 0.5f);
        }

        public static float EaseInExpo(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue <= 0f)
            {
                return 0f;
            }

            return Mathf.Pow(2f, (10f * clampedValue) - 10f);
        }

        public static float EaseOutExpo(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue >= 1f)
            {
                return 1f;
            }

            return 1f - Mathf.Pow(2f, -10f * clampedValue);
        }

        public static float EaseInOutExpo(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue <= 0f)
            {
                return 0f;
            }

            if (clampedValue >= 1f)
            {
                return 1f;
            }

            return clampedValue < 0.5f ? Mathf.Pow(2f, (20f * clampedValue) - 10f) * 0.5f : (2f - Mathf.Pow(2f, (-20f * clampedValue) + 10f)) * 0.5f;
        }

        public static float EaseOutCirc(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            float adjustedValue = clampedValue - 1f;
            return Mathf.Sqrt(1f - (adjustedValue * adjustedValue));
        }

        public static float EaseInOutCirc(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue < 0.5f)
            {
                float scaledValue = 2f * clampedValue;
                return (1f - Mathf.Sqrt(1f - (scaledValue * scaledValue))) * 0.5f;
            }

            float inverseValue = (-2f * clampedValue) + 2f;
            return (Mathf.Sqrt(1f - (inverseValue * inverseValue)) + 1f) * 0.5f;
        }

        public static float EaseInBack(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            float overshoot = k_backOvershoot + 1f;
            return (overshoot * clampedValue * clampedValue * clampedValue) - (k_backOvershoot * clampedValue * clampedValue);
        }

        public static float EaseOutBack(float value)
        {
            float clampedValue = Mathf.Clamp01(value) - 1f;
            float overshoot = k_backOvershoot + 1f;
            return 1f + (overshoot * clampedValue * clampedValue * clampedValue) + (k_backOvershoot * clampedValue * clampedValue);
        }

        public static float EaseInOutBack(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            float overshoot = k_backOvershoot * k_backOvershootScale;

            if (clampedValue < 0.5f)
            {
                float scaledValue = 2f * clampedValue;
                return (scaledValue * scaledValue * (((overshoot + 1f) * scaledValue) - overshoot)) * 0.5f;
            }

            float inverseValue = (2f * clampedValue) - 2f;
            return ((inverseValue * inverseValue * (((overshoot + 1f) * inverseValue) + overshoot)) + 2f) * 0.5f;
        }

        public static float EaseInElastic(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue <= 0f)
            {
                return 0f;
            }

            if (clampedValue >= 1f)
            {
                return 1f;
            }

            return -Mathf.Pow(2f, (10f * clampedValue) - 10f) * Mathf.Sin(((clampedValue * 10f) - 10.75f) * k_elasticConstant4);
        }

        public static float EaseOutElastic(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue <= 0f)
            {
                return 0f;
            }

            if (clampedValue >= 1f)
            {
                return 1f;
            }

            return (Mathf.Pow(2f, -10f * clampedValue) * Mathf.Sin(((clampedValue * 10f) - 0.75f) * k_elasticConstant4)) + 1f;
        }

        public static float EaseInOutElastic(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue <= 0f)
            {
                return 0f;
            }

            if (clampedValue >= 1f)
            {
                return 1f;
            }

            if (clampedValue < 0.5f)
            {
                return -(Mathf.Pow(2f, (20f * clampedValue) - 10f) * Mathf.Sin(((20f * clampedValue) - 11.125f) * k_elasticConstant5)) * 0.5f;
            }

            return ((Mathf.Pow(2f, (-20f * clampedValue) + 10f) * Mathf.Sin(((20f * clampedValue) - 11.125f) * k_elasticConstant5)) * 0.5f) + 1f;
        }

        public static float EaseInBounce(float value)
        {
            float clampedValue = Mathf.Clamp01(value);
            return 1f - EaseOutBounce(1f - clampedValue);
        }

        public static float EaseOutBounce(float value)
        {
            return EaseOutBounceInternal(Mathf.Clamp01(value));
        }

        public static float EaseInOutBounce(float value)
        {
            float clampedValue = Mathf.Clamp01(value);

            if (clampedValue < 0.5f)
            {
                return (1f - EaseOutBounceInternal(1f - (2f * clampedValue))) * 0.5f;
            }

            return (1f + EaseOutBounceInternal((2f * clampedValue) - 1f)) * 0.5f;
        }

        private static float EaseOutBounceInternal(float value)
        {
            if (value < 1f / k_bounceDivider)
            {
                return k_bounceMultiplier * value * value;
            }

            if (value < 2f / k_bounceDivider)
            {
                float adjustedValue = value - (1.5f / k_bounceDivider);
                return (k_bounceMultiplier * adjustedValue * adjustedValue) + 0.75f;
            }

            if (value < 2.5f / k_bounceDivider)
            {
                float adjustedValue = value - (2.25f / k_bounceDivider);
                return (k_bounceMultiplier * adjustedValue * adjustedValue) + 0.9375f;
            }

            float finalAdjustedValue = value - (2.625f / k_bounceDivider);
            return (k_bounceMultiplier * finalAdjustedValue * finalAdjustedValue) + 0.984375f;
        }
    }
}