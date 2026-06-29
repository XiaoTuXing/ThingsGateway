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

using Microsoft.AspNetCore.SignalR;

namespace ThingsGateway.InstantMessaging;

/// <summary>
/// 即时通信静态类
/// </summary>
public static class IM
{
    /// <summary>
    /// 获取集线器实例
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IHubContext<THub> GetHub<THub>(IServiceProvider serviceProvider = default)
        where THub : Hub
    {
        return App.GetService<IHubContext<THub>>(serviceProvider);
    }

    /// <summary>
    /// 获取强类型集线器实例
    /// </summary>
    /// <typeparam name="THub"></typeparam>
    /// <typeparam name="TStronglyTyped"></typeparam>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public static IHubContext<THub, TStronglyTyped> GetHub<THub, TStronglyTyped>(IServiceProvider serviceProvider = default)
        where THub : Hub<TStronglyTyped>
        where TStronglyTyped : class
    {
        return App.GetService<IHubContext<THub, TStronglyTyped>>(serviceProvider);
    }
}