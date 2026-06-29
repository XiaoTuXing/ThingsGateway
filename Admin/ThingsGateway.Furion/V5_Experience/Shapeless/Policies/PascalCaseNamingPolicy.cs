// ------------------------------------------------------------------------
// 版权信息
// 版权归百小僧及百签科技（广东）有限公司所有。
// 所有权利保留。
// 官方网站：https://baiqian.com
//
// 许可证信息
// 项目主要遵循 MIT 许可证和 Apache 许可证（版本 2.0）进行分发和使用。
// 许可证的完整文本可以在源代码树根目录中的 LICENSE-APACHE 和 LICENSE-MIT 文件中找到。
// ------------------------------------------------------------------------

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ThingsGateway.Shapeless;

/// <summary>
///     帕斯卡（大驼峰）命名策略
/// </summary>
public sealed partial class PascalCaseNamingPolicy : JsonNamingPolicy
{
    /// <inheritdoc />
    public override string ConvertName(string name)
    {
        // 空检查
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        // 对于全部大写的单词（如 'URL', 'ID'），保持不变
        if (name.All(char.IsUpper) && !name.Any(char.IsDigit))
        {
            return name;
        }

        // 初始化 StringBuilder 实例
        var result = new StringBuilder();

        // 将字符串按非字母数字字符、大小写字母变化处分割成多个部分
        var parts = WordBoundaryRegex().Split(name);

        // 遍历并逐个处理各个部分
        foreach (var part in parts)
        {
            // 空检查
            if (string.IsNullOrWhiteSpace(part))
            {
                continue;
            }

            // 如果是连续的大写字母，假设是缩写，保持不变
            if (part.Length > 1 && part.All(char.IsUpper))
            {
                result.Append(part);
            }
            else
            {
                // 对每个部分的第一个字母进行大写转换，其余小写
                result.Append(char.ToUpper(part[0]));

                if (part.Length > 1)
                {
                    result.Append(part[1..].ToLower());
                }
            }
        }

        return result.ToString();
    }

    /// <summary>
    ///     单词边界正则表达式
    /// </summary>
    /// <returns>
    ///     <see cref="Regex" />
    /// </returns>
    [GeneratedRegex(
        @"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])|(?<=\d)(?=\D)|(?<=\D)(?=\d)")]
    private static partial Regex WordBoundaryRegex();
}