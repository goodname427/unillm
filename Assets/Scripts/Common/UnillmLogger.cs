namespace unillm
{
    public static class UnillmLogger
    {
        public static void Log(string message)
        {
            UnityEngine.Debug.Log(message);
        }

        public static void Warrning(string message)
        {
            UnityEngine.Debug.LogWarning(message);
        }

        public static void Error(string message)
        {
            UnityEngine.Debug.LogError(message);
        }
    }
}