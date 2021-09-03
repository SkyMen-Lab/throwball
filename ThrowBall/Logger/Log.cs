using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ThrowBall.Logger
{
    public static class Log
    {
        private static Action<string> OnInfoLog;
        private static Action<string> OnWarningLog;
        private static Action<string, Exception> OnErrorLog;

        private static bool IsSetup = false;

        public static void SetupLogger(Action<string> infoAction, Action<string> warningAction, Action<string, Exception> errorAction) {
            OnInfoLog = infoAction;
            OnWarningLog = warningAction;
            OnErrorLog =  errorAction;
            IsSetup = true;
        }

        public static void Info(string info, [CallerMemberName] string caller = null) {
            // if (!IsSetup) {
            //     throw new Exception("Logger has not been setup!");
            // }
            string callerInfo = caller == null ? "" : $" - Point of message is <{caller}>";
            string message = $"INFO: {DateTime.Now.Date} - {DateTime.Now.TimeOfDay} {info}" + callerInfo;
            OnInfoLog?.Invoke(message);
        }

        public static void Warning(string info, [CallerMemberName] string caller = null) {
            // if (!IsSetup)
            // {
            //     throw new Exception("Logger has not been setup");
            // }
            string callerInfo = caller == null ? "" : $" - Point of message is <{caller}>";
            string message = $"Warning: {DateTime.Now.Day} - {DateTime.Now.TimeOfDay} {info}" + callerInfo;
            OnWarningLog?.Invoke(message);
        }

        public static void Error(string info, Exception e)
        {
            // if (!IsSetup)
            // {
            //     throw new Exception("Logger has not been setup");
            // }
            string message = $"Error: {DateTime.Now.Date} - {DateTime.Now.TimeOfDay} {info}\n" + e.Message;
            OnErrorLog?.Invoke(message, e);
        }

    }
}