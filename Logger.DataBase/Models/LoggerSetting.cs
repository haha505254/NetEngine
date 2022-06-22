﻿using System.Reflection;

namespace Logger.DataBase.Models
{
    public class LoggerSetting
    {


        /// <summary>
        /// Repository 的数据库连接地址
        /// </summary>
        public string DataBaseConnection { get; set; }


        /// <summary>
        /// App标记
        /// </summary>
        public string AppSign { get; set; } = Assembly.GetEntryAssembly()?.GetName().Name!;

    }
}
