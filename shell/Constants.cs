using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shell
{
    public class Constants
    {
        /// <summary>
        /// 呼び出しステータス
        /// </summary>
        public enum InvocationStatus
        {
            // レディ
            Ready,

            // 呼び出し中
            Invoking,

            // 呼び出し後
            Invoked,
        }

        /// <summary>
        /// 呼び出し結果
        /// </summary>
        public enum InvocationResultKind
        {
            // 空
            Empty,

            // 通常
            Normal,

            // エラー
            Error,
        }
    }
}