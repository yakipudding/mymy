using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Mymy.Data
{
    public static class Common
    {
        public enum DebugModeEnum
        {
            /// <summary>
            /// Tracに接続してチケットを取得します
            /// </summary>
            Trac,
            /// <summary>
            /// Tracに接続せずに、ローカルのC:\tmp\[project_id]\test.csvを読み取りに行きます
            /// </summary>
            LocalCsv,
        }

        /// <summary>
        /// デバッグモード
        /// </summary>
        public static DebugModeEnum DebugMode = ConfigurationManager.AppSettings["IsUseTrac"] == "true" ? DebugModeEnum.Trac : DebugModeEnum.LocalCsv;
    }
}