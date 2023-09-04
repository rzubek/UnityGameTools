// Copyright (C) SomaSim LLC. 
// Open source software. Please see LICENSE file for details.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public class Logger
{
    public static bool UnityLoggingEnabled = true;

    public static Action<object> OnDebugCallback = delegate { };
    public static Action<object> OnLogCallback = delegate { };
    public static Action<object> OnWarningCallback = delegate { };
    public static Action<object> OnErrorCallback = delegate { };


    #region Stack logging settings

    public static void InitializeLogLevels (bool debug) {
        Dictionary<LogType, StackTraceLogType> LogLevels =
            new Dictionary<LogType, StackTraceLogType>() {
            { LogType.Error, StackTraceLogType.ScriptOnly },
            { LogType.Assert, StackTraceLogType.ScriptOnly },
            { LogType.Warning, StackTraceLogType.ScriptOnly },
            { LogType.Log, debug ? StackTraceLogType.ScriptOnly : StackTraceLogType.None },
            { LogType.Exception, debug ? StackTraceLogType.Full : StackTraceLogType.ScriptOnly },
        };

        foreach (var entry in LogLevels) {
            Application.SetStackTraceLogType(entry.Key, entry.Value);
        }
    }

    #endregion


    #region Editor-only dev debug

    public static Dictionary<string, string> DevKeys = new Dictionary<string, string>();

    [Conditional("UNITY_EDITOR")]
    public static void DevDebug (string devkey, string message) {
        if (UnityLoggingEnabled && DevKeys.ContainsKey(devkey)) {
            Debug($"<color={DevKeys[devkey]}>{devkey}</color>: {message}");
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void DevDebug (string devkey, params object[] messages) {
        if (UnityLoggingEnabled && DevKeys.ContainsKey(devkey)) {
            Debug($"<color={DevKeys[devkey]}>{devkey}</color>: {Stringify(messages)}");
        }
    }

    #endregion

    #region Editor-only asserts

    [Conditional("UNITY_EDITOR")]
    public static void AssertInEditor (bool value, object message) {
        if (!value) {
            Error(Stringify("ASSERTION FAILED: ", message));
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void AssertInEditor (bool value, object o1, object o2) {
        if (!value) {
            Error(Stringify("ASSERTION FAILED: ", o1, o2));
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void AssertInEditor (bool value, object o1, object o2, object o3) {
        if (!value) {
            Error(Stringify("ASSERTION FAILED: ", o1, o2, o3));
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void AssertInEditor (bool value, object o1, object o2, object o3, object o4) {
        if (!value) {
            Error(Stringify("ASSERTION FAILED: ", o1, o2, o3, o4));
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void AssertInEditor (bool value, object o1, object o2, object o3, object o4, object o5) {
        if (!value) {
            Error(Stringify("ASSERTION FAILED: ", o1, o2, o3, o4, o5));
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void AssertInEditor (bool value, object o1, object o2, object o3, object o4, object o5, object o6) {
        if (!value) {
            Error(Stringify("ASSERTION FAILED: ", o1, o2, o3, o4, o5, o6));
        }
    }

    [Conditional("UNITY_EDITOR")]
    public static void AssertInEditor (bool value, params object[] messages) {
        if (!value) {
            Error("ASSERTION FAILED: " + Stringify(messages));
        }
    }

    #endregion

    #region Editor-only logging

    [Conditional("UNITY_EDITOR")]
    public static void DebugInEditor (string message) {
        Debug(message);
    }

    [Conditional("UNITY_EDITOR")]
    public static void DebugInEditor (params object[] messages) {
        Debug(Stringify(messages));
    }

    [Conditional("UNITY_EDITOR")]
    public static void WarningInEditor (params object[] messages) {
        Warning(Stringify(messages));
    }

    [Conditional("UNITY_EDITOR")]
    public static void WarningInEditor (string message) {
        Warning(message);
    }

    #endregion

    private static void Debug (string message) {
        if (UnityLoggingEnabled) {
            UnityEngine.Debug.Log(message);
        }

        OnDebugCallback(message);
    }

    public static void LogAlways (params object[] messages) {
        LogAlways(Stringify(messages));
    }

    public static void LogAlways (string message) {
        if (UnityLoggingEnabled) {
            UnityEngine.Debug.Log(message);
        }

        OnLogCallback(message);
    }

    public static void Warning (params object[] messages) {
        Warning(Stringify(messages));
    }

    public static void Warning (string message) {
        if (UnityLoggingEnabled) {
            UnityEngine.Debug.LogWarning(message);
        }

        OnWarningCallback(message);
    }

    public static void Error (params object[] messages) {
        Error(Stringify(messages));
    }

    public static void Error (string message) {
        if (UnityLoggingEnabled) {
            UnityEngine.Debug.LogError(message);
        }

        OnErrorCallback(message);
    }


    #region Helpers

    private static string Stringify (params object[] objects) {
        StringBuilder sb = new StringBuilder(128);

        foreach (var obj in objects) {
            if (sb.Length > 0) {
                sb.Append(" ");
            }
            sb.Append(obj != null ? obj.ToString() : "null");
        }

        return sb.ToString();
    }

    #endregion
}
