using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CodeGenerator
{
    public static class StringExtensions
    {
        /// <summary>
        /// 将驼峰字符串按单词拆分并转换成小写，再以特定字符串分隔
        /// </summary>
        /// <param name="str">待转换的字符串</param>
        /// <param name="splitStr">分隔符字符</param>
        /// <returns></returns>
        public static string UpperToLowerAndSplit(this string str, string splitStr = "-")
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            List<string> words = new List<string>();
            while (str.Length > 0)
            {
                char c = str.FirstOrDefault(char.IsUpper);
                if (c == default(char))
                {
                    words.Add(str);
                    break;
                }
                int upperIndex = str.IndexOf(c);
                if (upperIndex < 0) //admin
                {
                    return str;
                }
                if (upperIndex > 0) //adminAdmin
                {
                    string first = str.Substring(0, upperIndex);
                    words.Add(first);
                    str = str.Substring(upperIndex, str.Length - upperIndex);
                    continue;
                }
                str = char.ToLowerInvariant(str[0]) + str.Substring(1, str.Length - 1);
            }
            return ExpandAndToString(words, splitStr);
        }

        /// <summary>
        /// 将驼峰字符串按单词拆分并转换成大写，再以特定字符串分隔
        /// </summary>
        /// <param name="str">待转换的字符串</param>
        /// <param name="splitStr">分隔符字符</param>
        /// <returns></returns>
        public static string LowerToUpperAndSplit(this string str, string splitStr = "-")
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            List<string> words = new List<string>();
            while (str.Length > 0)
            {
                char c = str.FirstOrDefault(char.IsUpper);
                if (c == default(char))
                {
                    words.Add(str);
                    str = str.UpperFirstChar();
                    break;
                }
                int upperIndex = str.IndexOf(c);
                if (upperIndex < 0) //admin
                {
                    return str;
                }
                if (upperIndex > 0) //adminAdmin
                {
                    string first = str.Substring(0, upperIndex);
                    words.Add(first);
                    str = str.Substring(upperIndex, str.Length - upperIndex);
                    continue;
                }
                str = char.ToUpperInvariant(str[0]) + str[1..];
            }
            return ExpandAndToString(words, splitStr);
        }

        /// <summary>
        /// 将第一个字符小写
        /// </summary>
        public static string LowerFirstChar(this string str)
        {
            if (string.IsNullOrEmpty(str) || !char.IsUpper(str[0]))
            {
                return str;
            }
            if (char.IsUpper(str[0]))
            {
                if (str.Length == 1)
                {
                    return char.ToLowerInvariant(str[0]).ToString();
                }

                return char.ToLowerInvariant(str[0]) + str[1..];
            }
            return str;
        }

        /// <summary>
        /// 将第一个字符大写
        /// </summary>
        public static string UpperFirstChar(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }
            if (char.IsLower(str[0]))
            {
                if (str.Length == 1)
                {
                    return char.ToUpperInvariant(str[0]).ToString();
                }

                return char.ToUpperInvariant(str[0]) + str[1..];
            }
            return str;
        }

        /// <summary>
        /// 将字符串转化为符合大驼峰命名的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToUpperCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var splitStr = str.Split('_');
            var list = new List<string>(splitStr.Length);
            foreach (var s in splitStr)
            {
                list.Add(s.UpperFirstChar());
            }

            return string.Join("", list);
        }

        /// <summary>
        /// 将字符串转化为符合小驼峰命名的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToLowerCamelCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            var splitStr = str.Split('_');
            var list = new List<string>(splitStr.Length);
            foreach (var s in splitStr)
            {
                list.Add(s.LowerFirstChar());
            }

            return string.Join("", list);
        }

        /// <summary>
        /// 将字符串转化为符合蛇形命名的字符串
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static string ToSnakeCase(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            if (str.FirstOrDefault(char.IsUpper) == default)
            {
                return str;
            }

            var list = new List<char>();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (char.IsUpper(c))
                {
                    if (i == 0)
                    {
                        list.Add(char.ToLowerInvariant(c));
                    }
                    else
                    {
                        list.Add('_');
                        list.Add(char.ToLowerInvariant(c));
                    }
                }
                else
                {
                    list.Add(c);
                }
            }

            return new string(list.ToArray());
        }

        /// <summary>
        /// 将集合展开并分别转换成字符串，再以指定的分隔符衔接，拼成一个字符串返回。默认分隔符为逗号
        /// </summary>
        /// <param name="collection"> 要处理的集合 </param>
        /// <param name="separator"> 分隔符，默认为逗号 </param>
        /// <returns> 拼接后的字符串 </returns>
        private static string ExpandAndToString<T>(IEnumerable<T> collection, string separator = ",")
        {
            return ExpandAndToString(collection, t => t?.ToString(), separator);
        }

        /// <summary>
        /// 循环集合的每一项，调用委托生成字符串，返回合并后的字符串。默认分隔符为逗号
        /// </summary>
        /// <param name="collection">待处理的集合</param>
        /// <param name="itemFormatFunc">单个集合项的转换委托</param>
        /// <param name="separator">分隔符，默认为逗号</param>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns></returns>
        private static string ExpandAndToString<T>(IEnumerable<T> collection, Func<T, string> itemFormatFunc, string separator = ",")
        {
            collection = collection as IList<T> ?? collection.ToList();
            if (!collection.Any())
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            int i = 0;
            int count = collection.Count();
            foreach (T t in collection)
            {
                if (i == count - 1)
                {
                    sb.Append(itemFormatFunc(t));
                }
                else
                {
                    sb.Append(itemFormatFunc(t) + separator);
                }
                i++;
            }
            return sb.ToString();
        }
    }
}
