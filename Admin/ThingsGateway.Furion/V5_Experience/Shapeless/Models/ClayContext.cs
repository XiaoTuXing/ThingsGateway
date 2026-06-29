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
///     流变对象上下文
/// </summary>
/// <remarks>用于动态调用自定义委托时提供上下文 <see cref="Clay" /> 实例。无需外部手动初始化。</remarks>
public sealed class ClayContext
{
    /// <summary>
    ///     <inheritdoc cref="ClayContext" />
    /// </summary>
    /// <param name="current">上下文 <see cref="Clay" /> 实例</param>
    internal ClayContext(Clay current)
    {
        // 空检查
        ArgumentNullException.ThrowIfNull(current);

        Current = current;
    }

    /// <summary>
    ///     上下文 <see cref="Clay" /> 实例
    /// </summary>
    public dynamic Current { get; }
}