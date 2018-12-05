using System;
using System.Collections.Generic;

namespace Improbable.Gdk.Core
{
    public static class CommandLineUtility
    {
        public static Dictionary<string, string> GetArguments()
        {
#if UNITY_ANDROID
            try
            {
                var unityPlayer = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var currentActivity = unityPlayer.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");

                var intent = currentActivity.Call<UnityEngine.AndroidJavaObject>("getIntent");
                var hasExtra = intent.Call<bool>("hasExtra", "arguments");

                if (hasExtra)
                {
                    var extras = intent.Call<UnityEngine.AndroidJavaObject>("getExtras");
                    var arguments = extras.Call<string>("getString", "arguments");
                    UnityEngine.Debug.Log(arguments);
                    return ParseCommandLineArgs(arguments.Split(' '));
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
            }

            return new Dictionary<string, string>();
#else
            return ParseCommandLineArgs(Environment.GetCommandLineArgs());
#endif
        }

        public static T GetCommandLineValue<T>(Dictionary<string, string> commandLineDictionary, string configKey,
            T defaultValue)
        {
            if (TryGetConfigValue(commandLineDictionary, configKey, out T configValue))
            {
                return configValue;
            }

            return defaultValue;
        }

        public static T GetCommandLineValue<T>(IList<string> arguments, string configKey, T defaultValue)
        {
            var dict = ParseCommandLineArgs(arguments);
            return GetCommandLineValue(dict, configKey, defaultValue);
        }

        private static bool TryGetConfigValue<T>(Dictionary<string, string> dictionary, string configName, out T value)
        {
            var desiredType = typeof(T);
            if (dictionary.TryGetValue(configName, out var strValue))
            {
                if (desiredType.IsEnum)
                {
                    try
                    {
                        value = (T) Enum.Parse(desiredType, strValue);
                        return true;
                    }
                    catch (Exception e)
                    {
                        throw new FormatException($"Unable to parse argument {strValue} as enum {desiredType.Name}.",
                            e);
                    }
                }

                value = (T) Convert.ChangeType(strValue, typeof(T));
                return true;
            }

            value = default(T);
            return false;
        }

        public static Dictionary<string, string> ParseCommandLineArgs(IList<string> args)
        {
            var config = new Dictionary<string, string>();
            for (var i = 0; i < args.Count; i++)
            {
                var flag = args[i];
                if (flag.StartsWith("+"))
                {
                    if (i + 1 >= args.Count)
                    {
                        throw new ArgumentException($"Flag \"{flag}\" requires an argument");
                    }

                    var flagArg = args[i + 1];
                    var strippedOfPlus = flag.Substring(1, flag.Length - 1);
                    config[strippedOfPlus] = flagArg;
                    // We've already processed the next argument, so skip it.
                    i++;
                }
            }

            return config;
        }
    }
}
