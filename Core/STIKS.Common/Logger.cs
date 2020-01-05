using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace STIKS.Common
{
    public enum MessageType
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        Fatal = 5
    }

    public class Logger
    {
        #region Const(s)

        //Layout of date format 
        private const string _layout = @"${date:universalTime=true:format=dd.MM.yyyy HH\:mm\:ss} ${level}: ${message} ${exception:format=toString,Data}";

        //Describes path for "info" file
        private const string _pathInfo = @"${basedir}/logs/${date:format=dd.MM.yyyy:cached=false}.info.log";

        //Describes path for "warn" file
        private const string _pathWarn = @"${basedir}/logs/${date:format=dd.MM.yyyy:cached=false}.warn.log";

        //Describes path for "error" file
        private const string _pathError = @"${basedir}/logs/${date:format=dd.MM.yyyy:cached=false}.error.log";

        private const string _traceInfo = @"${basedir}/logs/${date:format=dd.MM.yyyy:cached=false}.trace.log";

        #endregion

        #region Field(s)
        /// <summary>
        /// Current dictionary contains messageType as key, and delegate Action<string> as a value 
        /// </summary>
        private Dictionary<MessageType, Action<string>> _funcMessage = new Dictionary<MessageType, Action<string>>();

        /// <summary>
        /// Current dictionary contains messageType as key, and delegate Action<Exception> as a value 
        /// </summary>
        private Dictionary<MessageType, Action<Exception>> _funcExc = new Dictionary<MessageType, Action<Exception>>();

        /// <summary>
        /// Current dictionary contains messageType as key, and delegate Action<Exception,string> as a value 
        /// </summary>
        private Dictionary<MessageType, Action<Exception, string>> _funcExcMessage = new Dictionary<MessageType, Action<Exception, string>>();

        /// <summary>
        /// Current dictionary contains messageType as key, and name of target as a value
        /// </summary>
        private Dictionary<MessageType, string> _targetNames = new Dictionary<MessageType, string>();

        private static Logger _instance = new Logger();

        public static Logger Instance { get => _instance; }

        #endregion

        #region Constructor(s)
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="settingsService"> Provide parameters from config file </param>
        public Logger()
        {
            Init();
        }

        #endregion

        #region Public method

        /// <summary>
        /// Current method write only messages into files devided by specific message type.Default type is Info
        /// </summary>
        /// <param name="message">string parameter which will be written in file</param>
        /// <param name="type"> the type of message </param>
        public void Save(string message, MessageType type = MessageType.Info)
        {
            try { _funcMessage[type](message); } catch (Exception) { }
        }

        /// <summary>
        /// Current method write only errors into files devided by specific message type.Default type is Error
        /// </summary>
        /// <param name="ex">Exseption object contains information about exception</param>
        /// <param name="type">the type of message</param>
        public void Save(Exception ex, MessageType type = MessageType.Error)
        {
            try { _funcExc[type](ex); } catch (Exception) { }
        }

        /// <summary>
        /// Current method write exception  errors with specific messages. Default type is Error
        /// </summary>
        /// <param name="ex">Exception object contains information about exceptions</param>
        /// <param name="message">This field provide additional information which will be written into file</param>
        /// <param name="type">the type of message</param>
        public void Save(Exception ex, string message, MessageType type = MessageType.Error)
        {
            try { _funcExcMessage[type](ex, message); } catch (Exception) { }
        }
        /// <summary>
        /// Current method is used only in unit tests.
        /// </summary>
        /// <param name="type">Type of message </param>
        /// <returns></returns>
        public bool CheckFolder(MessageType type)
        {
            var fileTarget = (FileTarget)LogManager.Configuration.FindTargetByName(_targetNames[type]);
            var logEventInfo = new LogEventInfo { TimeStamp = DateTime.Now };
            string fileName = fileTarget.FileName.Render(logEventInfo);
            if (!File.Exists(fileName)) return false;
            return true;
        }

        #endregion

        #region Private method(s)

        /// <summary>
        /// Set settings for Nlog config
        /// </summary>
        private void Init()
        {
            _targetNames.Add(MessageType.Info, "info");
            _targetNames.Add(MessageType.Warn, "warn");
            _targetNames.Add(MessageType.Error, "error");
            _targetNames.Add(MessageType.Trace, "trace");


            //var layout = settingsService.Get("logger:layout", _layout);
            //var pathInfo = settingsService.Get("logger:path:info", _pathInfo);
            //var pathWarn = settingsService.Get("logger:path:warn", _pathWarn);
            //var pathError = settingsService.Get("logger:path:error", _pathError);

            var config = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget("console")
            {
                Layout = _layout
            };

            //var consoleTargetError = new ColoredConsoleTarget("consoleError")
            //{
            //    Layout = _layout
            //};


            //var ruleWarging = new ConsoleWordHighlightingRule();

            //ruleWarging.ForegroundColor = ConsoleOutputColor.DarkGreen;
            //consoleTargetError.WordHighlightingRules.Add(ruleWarging);
            //consoleTarget.WordHighlightingRules[]

            var infoTarget = new FileTarget(_targetNames[MessageType.Info])
            {
                FileName = _pathInfo,
                Layout = _layout,
                AutoFlush = true,
                ConcurrentWrites = true
            };

            var warnTarget = new FileTarget(_targetNames[MessageType.Warn])
            {
                FileName = _pathWarn,
                Layout = _layout,
                AutoFlush = true,
                ConcurrentWrites = true
            };

            var errorTarget = new FileTarget(_targetNames[MessageType.Error])
            {
                FileName = _pathError,
                Layout = _layout,
                AutoFlush = true,
                ConcurrentWrites = true
            };

            var traceTarget = new FileTarget(_targetNames[MessageType.Trace])
            {
                FileName = _traceInfo,
                Layout = _layout,
                AutoFlush = true,
                ConcurrentWrites = true
            };


            config.AddTarget(consoleTarget);
            config.AddTarget(infoTarget);
            config.AddTarget(warnTarget);
            config.AddTarget(errorTarget);
            config.AddTarget(traceTarget);

            config.AddRuleForOneLevel(LogLevel.Info, infoTarget);
            config.AddRuleForOneLevel(LogLevel.Warn, warnTarget);
            config.AddRuleForOneLevel(LogLevel.Error, errorTarget);
            config.AddRuleForOneLevel(LogLevel.Trace, traceTarget);

            //config.AddRuleForOneLevel(LogLevel.Error, consoleTargetError);

            config.AddRuleForAllLevels(consoleTarget);

            LogManager.Configuration = config;

            init_func();
        }

        /// <summary>
        /// Set methods for delegate Action.  
        /// </summary>
        private void init_func()
        {
            //Get current class from NLog library
            var logger = LogManager.GetCurrentClassLogger();

            _funcMessage[MessageType.Debug] = logger.Debug;
            _funcMessage[MessageType.Info] = logger.Info;
            _funcMessage[MessageType.Warn] = logger.Warn;
            _funcMessage[MessageType.Error] = logger.Error;
            _funcMessage[MessageType.Trace] = logger.Trace;

            _funcExc[MessageType.Debug] = logger.Debug;
            _funcExc[MessageType.Info] = logger.Info;
            _funcExc[MessageType.Warn] = logger.Warn;
            _funcExc[MessageType.Trace] = logger.Trace;
            _funcExc[MessageType.Error] = logger.Error;

            _funcExcMessage[MessageType.Debug] = logger.Debug;
            _funcExcMessage[MessageType.Info] = logger.Info;
            _funcExcMessage[MessageType.Warn] = logger.Warn;
            _funcExcMessage[MessageType.Trace] = logger.Trace;
            _funcExcMessage[MessageType.Error] = logger.Error;
        }

        #endregion

        #region IStart interface methods

        #endregion IStart interface methods
    }
}
