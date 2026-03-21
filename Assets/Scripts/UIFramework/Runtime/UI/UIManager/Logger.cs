namespace GameLogic
{
    [System.Flags]
    public enum LogType : byte
    {
        None = 0,
        Assert = 1 << 1,
        Log = 1 << 2,
        Warning = 1 << 3,
        Error = 1 << 4,
        Exception = 1 << 5,
        All = byte.MaxValue,
    }

    public static class Logger
    {
        /// <summary> 是否打印日志 </summary>
        public static LogType Type { get; private set; } = LogType.All;

        /// <summary> 是否导出日志 </summary>
        public static bool IsExportLogFile { get; private set; } = false;

        /// <summary> 日志路径 </summary>
        public static string ExportLogBaseFolder { get; private set; } = string.Empty;

        /// <summary> 本次日志完整路径 </summary>
        public static string LogFileFullPath { get; private set; } = string.Empty;

        /// <summary>
        /// 打印普通日志
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object message)
        {
            if (HasLogType(LogType.Log))
            {
                Export(LogType.Log, message);
            }
        }

        /// <summary>
        /// 打印警告日志
        /// </summary>
        /// <param name="message"></param>
        public static void Warning(object message)
        {
            if (HasLogType(LogType.Warning))
            {
                Export(LogType.Warning, message);
            }
        }

        /// <summary>
        /// 打印错误日志
        /// </summary>
        /// <param name="message"></param>
        public static void Error(object message)
        {
            if (HasLogType(LogType.Error))
            {
                Export(LogType.Error, message);
            }
        }

        /// <summary>
        /// 打印Unity日志
        /// </summary>
        /// <param name="message"></param>
        public static void Assert(object message)
        {
            if (HasLogType(LogType.Assert))
            {
                Export(LogType.Assert, message);
            }
        }

        /// <summary>
        /// 打印Exception日志
        /// </summary>
        /// <param name="ex"></param>
        public static void Exception(System.Exception ex)
        {
            if (HasLogType(LogType.Exception))
            {
                Export(LogType.Exception,
                    $"HResult = {ex.HResult} \n Message = {ex.Message} \n StackTrace = {ex.StackTrace} \n Source = {ex.Source} \n InnerException = {ex.InnerException} \n TargetSite = {ex.TargetSite} \n Data = {ex.Data} , HelpLink = {ex.HelpLink}");
            }
        }

        private static void Export(LogType type, object message)
        {
            var exportMessage =
                $"[ {nameof(Logger)} => {type.ToString()} ({System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffff")}) ] => Log：{message}";

            switch (type)
            {
                case LogType.Assert:
                {
                    UnityEngine.Debug.LogAssertion(exportMessage);
                }
                    break;
                case LogType.Log:
                {
                    UnityEngine.Debug.Log(exportMessage);
                }
                    break;
                case LogType.Warning:
                {
                    UnityEngine.Debug.LogWarning(exportMessage);
                }
                    break;
                case LogType.Error:
                {
                    UnityEngine.Debug.LogError(exportMessage);
                }
                    break;
                case LogType.Exception:
                {
                    UnityEngine.Debug.LogError(exportMessage);
                }
                    break;
                default:
                {
                    exportMessage = $"Unknow Log：{exportMessage}";

                    UnityEngine.Debug.LogWarning(exportMessage);
                }
                    break;
            }

            //TODO：将日志导出成文件

            if (IsExportLogFile)
            {
                if (string.IsNullOrEmpty(LogFileFullPath))
                {
                    RefreshLogFile();
                }
                else
                {
                    AppendLogFile(exportMessage);
                }
            }
        }

        private static void RefreshLogFile()
        {
            if (IsExportLogFile)
            {
                #region 刷新路径并创建文件

                if (string.IsNullOrEmpty(ExportLogBaseFolder))
                    ExportLogBaseFolder = UnityEngine.Application.persistentDataPath;

                var logFileFolder = System.IO.Path.Combine(ExportLogBaseFolder,
                    $"{System.DateTime.Now.Year.ToString("0000")}{System.DateTime.Now.Month.ToString("00")}{System.DateTime.Now.Day.ToString("00")}");

                if (!System.IO.Directory.Exists(logFileFolder))
                {
                    System.IO.Directory.CreateDirectory(logFileFolder);
                }

                var fileList = System.IO.Directory.GetFiles(logFileFolder);

                var count = fileList != null && fileList.Length > 0 ? fileList.Length + 1 : 1;

                var lastFilePath = LogFileFullPath;

                LogFileFullPath =
                    $"{logFileFolder}/{count}_{System.DateTime.Now.Hour.ToString("00")}{System.DateTime.Now.Minute.ToString("00")}{System.DateTime.Now.Second.ToString("00")}.log";

                if (!System.IO.File.Exists(LogFileFullPath))
                {
                    System.IO.File.Create(LogFileFullPath);
                }

                #endregion

                if (!string.IsNullOrEmpty(lastFilePath))
                {
                    AppendLogFile($"Last Log File Path = {lastFilePath}");
                    AppendLogFile(string.Empty);
                }

                AppendLogFile(
                    $"****************************** {count} Start Record Log {System.DateTime.Now.ToString()} ******************************");
                AppendLogFile(string.Empty);
                AppendLogFile($"设备名称： {UnityEngine.SystemInfo.deviceName}");
                AppendLogFile($"操作系统:  {UnityEngine.SystemInfo.operatingSystem}");
                AppendLogFile($"系统内存大小:  {UnityEngine.SystemInfo.systemMemorySize}");
                AppendLogFile($"设备模型:  {UnityEngine.SystemInfo.deviceModel}");
                AppendLogFile($"设备唯一标识符:  {UnityEngine.SystemInfo.deviceUniqueIdentifier}");
                AppendLogFile($"处理器数量:  {UnityEngine.SystemInfo.processorCount}");
                AppendLogFile($"处理器类型:  {UnityEngine.SystemInfo.processorType}");
                AppendLogFile($"显卡标识符:  {UnityEngine.SystemInfo.graphicsDeviceID}");
                AppendLogFile($"显卡名称:  {UnityEngine.SystemInfo.graphicsDeviceName}");
                AppendLogFile($"显卡标识符:  {UnityEngine.SystemInfo.graphicsDeviceVendorID}");
                AppendLogFile($"显卡厂商:  {UnityEngine.SystemInfo.graphicsDeviceVendor}");
                AppendLogFile($"显卡版本:  {UnityEngine.SystemInfo.graphicsDeviceVersion}");
                AppendLogFile($"显存大小:  {UnityEngine.SystemInfo.graphicsMemorySize}");
                AppendLogFile($"显卡着色器级别:  {UnityEngine.SystemInfo.graphicsShaderLevel}");
                AppendLogFile($"是否支持内置阴影:  {UnityEngine.SystemInfo.supportsShadows}");
                AppendLogFile(string.Empty);
                AppendLogFile($"****************************** Record Log List ******************************");
                AppendLogFile(string.Empty);
            }
        }

        private static void AppendLogFile(string content)
        {
            if (IsExportLogFile && !string.IsNullOrEmpty(LogFileFullPath))
            {
                using (System.IO.FileStream nFile = new System.IO.FileStream(LogFileFullPath, System.IO.FileMode.Append,
                           System.IO.FileAccess.Write, System.IO.FileShare.ReadWrite, 2048))
                {
                    using (System.IO.StreamWriter sWriter = new System.IO.StreamWriter(nFile))
                    {
                        //写入数据
                        sWriter.Write($"{content}\n");
                    }
                }
            }
        }

        #region Enum Functioin

        /// <summary>
        /// 开启日志打印
        /// </summary>
        public static void EnableLogger()
        {
            Type = LogType.All;
        }

        /// <summary>
        /// 关闭日志打印
        /// </summary>
        public static void DisableLogger()
        {
            Type = LogType.None;
        }

        /// <summary>
        /// 判断是否包含改枚举值
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public static bool HasLogType(LogType logType)
        {
            return (Type & logType) != 0;
        }

        /// <summary>
        /// 增加枚举值
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public static LogType AddLogType(LogType logType)
        {
            return Type | logType;
        }

        /// <summary>
        /// 减少枚举值
        /// </summary>
        /// <param name="logType"></param>
        /// <returns></returns>
        public static LogType RemoveLogType(LogType logType)
        {
            return Type & ~logType;
        }

        #endregion
    }
}