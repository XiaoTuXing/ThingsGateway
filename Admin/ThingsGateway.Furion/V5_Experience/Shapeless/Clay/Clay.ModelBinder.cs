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

using Microsoft.AspNetCore.Http;

using System.Reflection;

namespace ThingsGateway.Shapeless;

/// <summary>
///     流变对象
/// </summary>
/// <remarks>
///     <para>为最小 API 提供模型绑定。</para>
///     <para>参考文献：https://learn.microsoft.com/zh-cn/aspnet/core/fundamentals/minimal-apis/parameter-binding?view=aspnetcore-9.0#custom-binding。</para>
/// </remarks>
public partial class Clay
{
    /// <summary>
    ///     为最小 API 提供模型绑定
    /// </summary>
    /// <remarks>由运行时调用。</remarks>
    /// <param name="httpContext"><c>HttpContext</c> 实例</param>
    /// <param name="parameter">
    ///     <see cref="ParameterInfo" />
    /// </param>
    /// <returns>
    ///     <see cref="Clay" />
    /// </returns>
    public static async ValueTask<Clay?> BindAsync(HttpContext httpContext, ParameterInfo parameter) =>
        await ClayBinder.BindAsync(httpContext, parameter).ConfigureAwait(false)!;
}