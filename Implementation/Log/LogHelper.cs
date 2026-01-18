/*
 * Copyright (c) 2024-2026 XDay
 *
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 *
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
 * IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
 * CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
 * TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
 * SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */



using System;
using System.Collections.Generic;

namespace XDay
{
    internal static class LogHelper
    {
        public static string GetString(this Dictionary<string, object> dict, string key, string defaultValue = null)
        {
            if (dict.TryGetValue(key, out var value))
            {
                var ret = value as string;
                if (!string.IsNullOrEmpty(ret))
                {
                    return ret;
                }
                
            }
            return defaultValue;
        }

        public static long GetInt64(this Dictionary<string, object> dict, string key, long defaultValue = 0)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return Convert.ToInt64(value);
            }
            return defaultValue;
        }

        public static bool GetBoolean(this Dictionary<string, object> dict, string key, bool defaultValue = false)
        {
            if (dict.TryGetValue(key, out var value))
            {
                return Convert.ToBoolean(value);
            }
            return defaultValue;
        }

        public const string LOG_IGNORE_KEY = "##";
        public const int MAX_STACK_TRACE_SIZE = 2048;
    }
}
