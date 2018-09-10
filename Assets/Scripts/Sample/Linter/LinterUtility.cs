using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommandScripter;

namespace Scenario.Linter
{
    public class LinterUtility
    {
        /// <summary>
        /// 指定パラメータが存在しなければエラー
        /// </summary>
        public static void ExistRequiredParam(LinkedList<ErrorFormat> errList, Dictionary<string, string> param, string checker)
        {
            if (!param.ContainsKey(checker))
            {
                errList.AddLast(new ErrorFormat(checker + "Parameter not found"));
            }
        }

        /// <summary>
        /// 指定パラメータにint型が入ってきてなければエラー
        /// </summary>
        public static void TypeCheckInt(LinkedList<ErrorFormat> errList, Dictionary<string, string> param, string key)
        {
            if (!param.ContainsKey(key))
            {
                return;
            }
            int v = 0;
            string val = param[key];
            bool failed = !int.TryParse(val, out v);
            if (failed)
            {
                errList.AddLast(new ErrorFormat(string.Format("A non-integer character string ({1}) is specified in [{0}]", key, val)));
            }
        }

        /// <summary>
        /// 指定パラメータにfloat型が入ってきてなければエラー
        /// </summary>
        public static void TypeCheckFloat(LinkedList<ErrorFormat> errList, Dictionary<string, string> param, string key)
        {
            if (!param.ContainsKey(key))
            {
                return;
            }
            float v = 0.0f;
            string val = param[key];
            bool failed = !float.TryParse(val, out v);
            if (failed)
            {
                errList.AddLast(new ErrorFormat(string.Format("A non-numeric character string ({1}) is specified in [{0}]", key, val)));
            }
        }

        /// <summary>
        /// 指定パラメータに特定のenum内の要素が指定されていなければエラー
        /// </summary>
        public static void TypeCheckEnum<T>(LinkedList<ErrorFormat> errList, Dictionary<string, string> param, string key) where T : struct
        {
            if (!param.ContainsKey(key))
            {
                return;
            }
            var val = param[key];
            bool failed = !System.Enum.GetNames(typeof(T)).Any(_ => _.Equals(val));
            if (failed)
            {
                errList.AddLast(new ErrorFormat(string.Format("A constant ({1}) that does not exist in [{0}] is set", key, param[key])));
            }
        }

        /// <summary>
        /// 指定パラメータがbool型でなければエラー
        /// </summary>
        public static void TypeCheckBool(LinkedList<ErrorFormat> errList, Dictionary<string, string> param, string key)
        {
            if (!param.ContainsKey(key))
            {
                return;
            }
            string val = param[key];
            bool result = false;
            bool failed = !System.Boolean.TryParse(val, out result);
            if (failed)
            {
                errList.AddLast(new ErrorFormat(string.Format("A character string ({1}) other than a boolean value is set in [{0}]", key, val)));
            }
        }
    }
}
