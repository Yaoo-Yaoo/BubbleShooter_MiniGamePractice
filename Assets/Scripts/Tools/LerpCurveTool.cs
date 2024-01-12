using UnityEngine;

namespace Game.Tool
{
    public enum CurveType
    {
        Linear,
        EaseInOut
    }
    
    public class LerpCurveTool
    {
        #region Singleton

        private static LerpCurveTool instance;
        public static LerpCurveTool Instance
        {
            get
            {
                if (instance == null)
                    instance = new LerpCurveTool();
                return instance;
            }
        }
        private LerpCurveTool(){}

        #endregion

        public static float GetLerpValue(float percentage, CurveType curveType = CurveType.Linear, float minValue = 0, float maxValue = 1)
        {
            if (percentage < 0 || percentage > 1)
            {
                Debug.LogError("Percentage must be between 0 and 1!");
                return 0;
            }
            
            if (curveType == CurveType.Linear)
            {
                AnimationCurve curve = AnimationCurve.Linear(0,minValue, 1, maxValue);
                return curve.Evaluate(percentage);
            }
            else if (curveType == CurveType.EaseInOut)
            {
                AnimationCurve curve = AnimationCurve.EaseInOut(0,minValue, 1, maxValue);
                return curve.Evaluate(percentage);
            }

            return 0;
        }
    }
}
