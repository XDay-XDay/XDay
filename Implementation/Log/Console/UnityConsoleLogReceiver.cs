/*
 * Copyright (c) 2024-2025 XDay
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


using Cysharp.Text;
using System;

namespace XDay
{
    internal class UnityConsoleLogReceiver : LogReceiver
    {
        public override void OnLogReceived(Utf16ValueStringBuilder builder, LogType type, LogColor color, bool fromUnityDebug)
        {
            if (!fromUnityDebug)
            {
                if (type == LogType.Log)
                {
#if PLATFORM_UNITY
                    if (color == LogColor.None)
                    {
                        UnityEngine.Debug.Log(builder.ToString());
                    }
                    else
                    {

                        Utf16ValueStringBuilder full = ZString.CreateStringBuilder();
                        switch (color)
                        {
                            case LogColor.Blue:
                                full.Append("<color=#0000FF>");
                                break;
                            case LogColor.Red:
                                full.Append("<color=#FF0000>");
                                break;
                            case LogColor.Yellow:
                                full.Append("<color=#FFFF00>");
                                break;
                            case LogColor.Magenta:
                                full.Append("<color=#FF00FF>");
                                break;
                            case LogColor.Cyan:
                                full.Append("<color=#00FFFF>");
                                break;
                            case LogColor.Green:
                                full.Append("<color=#00FF00>");
                                break;
                            case LogColor.White:
                                full.Append("<color=#FFFFFF>");
                                break;
                            default:
                                break;
                        }
                        full.Append(builder);
                        full.Append("</color>");
                        UnityEngine.Debug.Log(full.ToString());
                        full.Dispose();
                    }

#endif
                }
                else if (type == LogType.Warning)
                {
#if PLATFORM_UNITY
                    UnityEngine.Debug.LogWarning(builder.ToString());
#endif
                }
                else
                {
#if PLATFORM_UNITY
                    UnityEngine.Debug.LogError(builder.ToString());
#endif
                }
            }
            builder.Dispose();
        }
    }
}

