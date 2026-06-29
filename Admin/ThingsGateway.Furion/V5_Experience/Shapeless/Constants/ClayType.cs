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

namespace ThingsGateway.Shapeless;

/// <summary>
///     流变对象的基本类型
/// </summary>
/// <remarks>用于区分是单一对象还是集合或数组形式。</remarks>
public enum ClayType
{
    /// <summary>
    ///     单一对象
    /// </summary>
    /// <remarks>缺省值。</remarks>
    Object = 0,

    /// <summary>
    ///     集合或数组形式
    /// </summary>
    Array
}