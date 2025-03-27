using System.Collections;

using UnityEngine.Networking;

namespace ATF
{
    public static class ClassExtensions_UnityWebRequest 
    {
        public static bool ATResult(this UnityWebRequest unityWeb)
        {
#if UNITY_2020_2_OR_NEWER
        if (unityWeb.result == UnityWebRequest.Result.Success)
#else
            if (!unityWeb.isHttpError && !unityWeb.isNetworkError) // 兼容 Unity 2019
#endif
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}