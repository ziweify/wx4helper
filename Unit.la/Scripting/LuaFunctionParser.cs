using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Unit.La.Scripting
{
    /// <summary>
    /// Lua 函数解析器
    /// 用于从 Lua 代码中提取函数定义信息
    /// </summary>
    public class LuaFunctionParser
    {
        /// <summary>
        /// Lua 函数信息
        /// </summary>
        public class FunctionInfo
        {
            public string Name { get; set; } = string.Empty;
            public List<ParameterInfo> Parameters { get; set; } = new();
            public int LineNumber { get; set; }
            public int ColumnNumber { get; set; }
            public string FullSignature { get; set; } = string.Empty;
            public bool IsLocal { get; set; }
        }

        /// <summary>
        /// 函数参数信息
        /// </summary>
        public class ParameterInfo
        {
            public string Name { get; set; } = string.Empty;
            public string? DefaultValue { get; set; }
            public bool HasDefaultValue => DefaultValue != null;
        }

        /// <summary>
        /// 从 Lua 代码中解析所有函数定义
        /// </summary>
        public static List<FunctionInfo> ParseFunctions(string luaCode)
        {
            var functions = new List<FunctionInfo>();
            
            if (string.IsNullOrEmpty(luaCode))
                return functions;

            // 正则表达式：匹配 function 定义
            // 支持：
            // - function name() end
            // - function name(param1, param2) end
            // - local function name() end
            // - name = function() end
            // - name = function(param1, param2) end
            var patterns = new[]
            {
                // 标准函数定义：function name(params) end
                @"(local\s+)?function\s+([a-zA-Z_][a-zA-Z0-9_]*)\s*\(([^)]*)\)",
                // 匿名函数赋值：name = function(params) end
                @"([a-zA-Z_][a-zA-Z0-9_]*)\s*=\s*function\s*\(([^)]*)\)"
            };

            var lines = luaCode.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                var line = lines[lineIndex];
                var lineNumber = lineIndex + 1;

                foreach (var pattern in patterns)
                {
                    var matches = Regex.Matches(line, pattern, RegexOptions.IgnoreCase);
                    
                    foreach (Match match in matches)
                    {
                        try
                        {
                            var funcInfo = new FunctionInfo
                            {
                                LineNumber = lineNumber,
                                ColumnNumber = match.Index + 1
                            };

                            if (pattern.Contains("local\\s+function"))
                            {
                                // local function name(params) end
                                funcInfo.IsLocal = true;
                                funcInfo.Name = match.Groups[2].Value.Trim();
                                var paramsStr = match.Groups[3].Value.Trim();
                                funcInfo.Parameters = ParseParameters(paramsStr);
                            }
                            else if (pattern.Contains("function\\s+([a-zA-Z_]"))
                            {
                                // function name(params) end
                                funcInfo.IsLocal = match.Groups[1].Value.Contains("local");
                                funcInfo.Name = match.Groups[2].Value.Trim();
                                var paramsStr = match.Groups[3].Value.Trim();
                                funcInfo.Parameters = ParseParameters(paramsStr);
                            }
                            else
                            {
                                // name = function(params) end
                                funcInfo.IsLocal = false;
                                funcInfo.Name = match.Groups[1].Value.Trim();
                                var paramsStr = match.Groups[2].Value.Trim();
                                funcInfo.Parameters = ParseParameters(paramsStr);
                            }

                            // 生成完整签名
                            funcInfo.FullSignature = GenerateSignature(funcInfo);
                            
                            functions.Add(funcInfo);
                        }
                        catch
                        {
                            // 忽略解析错误，继续处理下一行
                        }
                    }
                }
            }

            return functions;
        }

        /// <summary>
        /// 解析函数参数
        /// </summary>
        private static List<ParameterInfo> ParseParameters(string paramsStr)
        {
            var parameters = new List<ParameterInfo>();
            
            if (string.IsNullOrWhiteSpace(paramsStr))
                return parameters;

            // 分割参数（考虑默认值）
            var paramParts = paramsStr.Split(',');
            
            foreach (var part in paramParts)
            {
                var trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                var param = new ParameterInfo();

                // 检查是否有默认值：param = value
                if (trimmed.Contains("="))
                {
                    var equalIndex = trimmed.IndexOf('=');
                    param.Name = trimmed.Substring(0, equalIndex).Trim();
                    param.DefaultValue = trimmed.Substring(equalIndex + 1).Trim();
                }
                else
                {
                    param.Name = trimmed;
                }

                parameters.Add(param);
            }

            return parameters;
        }

        /// <summary>
        /// 生成函数签名
        /// </summary>
        private static string GenerateSignature(FunctionInfo funcInfo)
        {
            var signature = new System.Text.StringBuilder();
            
            if (funcInfo.IsLocal)
                signature.Append("local ");
            
            signature.Append("function ");
            signature.Append(funcInfo.Name);
            signature.Append("(");
            
            for (int i = 0; i < funcInfo.Parameters.Count; i++)
            {
                if (i > 0)
                    signature.Append(", ");
                
                var param = funcInfo.Parameters[i];
                signature.Append(param.Name);
                
                if (param.HasDefaultValue)
                {
                    signature.Append(" = ");
                    signature.Append(param.DefaultValue);
                }
            }
            
            signature.Append(")");
            
            return signature.ToString();
        }

        /// <summary>
        /// 从函数信息生成调用代码
        /// </summary>
        public static string GenerateFunctionCall(FunctionInfo funcInfo, Dictionary<string, string>? parameterValues = null)
        {
            var call = new System.Text.StringBuilder();
            call.Append(funcInfo.Name);
            call.Append("(");
            
            if (funcInfo.Parameters.Count > 0)
            {
                for (int i = 0; i < funcInfo.Parameters.Count; i++)
                {
                    if (i > 0)
                        call.Append(", ");
                    
                    var param = funcInfo.Parameters[i];
                    
                    if (parameterValues != null && parameterValues.ContainsKey(param.Name))
                    {
                        // 使用用户提供的值
                        var value = parameterValues[param.Name];
                        // 如果是字符串，需要加引号
                        if (value.StartsWith("\"") || value.StartsWith("'"))
                            call.Append(value);
                        else if (IsNumeric(value) || value == "true" || value == "false" || value == "nil")
                            call.Append(value);
                        else
                            call.Append("\"").Append(value).Append("\"");
                    }
                    else if (param.HasDefaultValue)
                    {
                        // 使用默认值
                        call.Append(param.DefaultValue);
                    }
                    else
                    {
                        // 使用 nil
                        call.Append("nil");
                    }
                }
            }
            
            call.Append(")");
            return call.ToString();
        }

        /// <summary>
        /// 检查字符串是否为数字
        /// </summary>
        private static bool IsNumeric(string value)
        {
            return double.TryParse(value, out _);
        }
    }
}
