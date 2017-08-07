using System;

namespace AnimateBaseStationAdsB.Util
{
    class Lumberjack
    {
        /// <summary>
        /// Prints a message to console with a little flair
        /// </summary>
        /// <param name="message"></param>
        /// <param name="level"></param>
        public static void Log(object message, LogLevel level = LogLevel.Info)
        {
            switch (level)
            {
                case LogLevel.Info:
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
                case LogLevel.Warn:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case LogLevel.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
            Console.WriteLine($"[{level}]\t{message}");
        }

        /// <summary>
        /// Logs and kills with an exit code
        /// </summary>
        /// <param name="message"></param>
        /// <param name="errorCode"></param>
        public static void Kill(object message, ErrorCode errorCode)
        {
            Log(message, LogLevel.Error);
            Environment.Exit((int)errorCode);
        }
    }
}
