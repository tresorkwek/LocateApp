using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using log4net;
using Nancy;
using LocateApp.Models;
using System.Runtime.CompilerServices;

namespace LocateApp.Utilities
{
    public class Logger
    {
        public int ERROR_CODE { get; } = 0;
        public int SUCCESS_CODE { get; } = 1;
        public int WARNING_CODE { get; } = 2;
        public int INFO_CODE { get; } = 3;
        public int OK_CODE { get; } = 4;
        private static string DefaultFormat { get; } = "({0}) {1}.{2}:{3} : {4}";
        public static string Format { get; set; } = DefaultFormat;
        private static string GetFileName(string path) => Path.GetFileNameWithoutExtension(path.Substring(path.LastIndexOfAny(new[] { '/', '\\' }) + 1));

        #region Instanciation
        private ILog Log { get; }

        private Logger(ILog logger)
        {
            Log = logger;
        }

        public static Logger GetLogger(object instance)
        {
            return GetLogger(instance.GetType());
        }

        public static Logger GetLogger(Type type)
        {
            return new Logger(LogManager.GetLogger(type));
        }
        #endregion

        #region Opérations
        public void Request(int id, string method, string route, string address, string useragent)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, "[{0}] {1} {2} - {3} ({4})", id, method, route, address, useragent);
        }

        public void Response(int id, HttpStatusCode status)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, "[{0}] {1} {2}", id, (int)status, status);
        }

        public void Debug(Identity identity, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.DebugFormat(CultureInfo.InvariantCulture, Format, GetName(identity), GetFileName(file), memberName, lineNumber, message);
        }

        public void Info(Identity identity, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.InfoFormat(CultureInfo.InvariantCulture, Format, GetName(identity), GetFileName(file), memberName, lineNumber, message);
        }

        public void Warning(Identity identity, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.WarnFormat(CultureInfo.InvariantCulture, Format, GetName(identity), GetFileName(file), memberName, lineNumber, message);
        }

        public void Error(Identity identity, string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = 0)
        {
            Log.ErrorFormat(CultureInfo.InvariantCulture, Format, GetName(identity), GetFileName(file), memberName, lineNumber, message);
        }

        private static string GetName(Identity identity) => identity?.UserName ?? "Anonymous";
        #endregion
    }
}