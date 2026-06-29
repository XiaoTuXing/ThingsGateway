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

using ThingsGateway.Shapeless;

namespace Microsoft.AspNetCore.Mvc;

/// <summary>
///     <see cref="Controller" /> 拓展类
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    ///     创建一个 <see cref="ViewResult" /> 对象，并将视图模型设置为 <see cref="Clay" /> 类型
    /// </summary>
    /// <param name="controller">
    ///     <see cref="Controller" />
    /// </param>
    /// <param name="model">视图模型</param>
    /// <param name="clayOptions">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="ViewResult" />
    /// </returns>
    public static ViewResult ViewClay(this Controller controller, object? model, ClayOptions? clayOptions = null) =>
        controller.View(Clay.Parse(model, clayOptions));

    /// <summary>
    ///     创建一个 <see cref="ViewResult" /> 对象，并将视图模型设置为 <see cref="Clay" /> 类型
    /// </summary>
    /// <param name="controller">
    ///     <see cref="Controller" />
    /// </param>
    /// <param name="viewName">视图名称</param>
    /// <param name="model">视图模型</param>
    /// <param name="clayOptions">
    ///     <see cref="ClayOptions" />
    /// </param>
    /// <returns>
    ///     <see cref="ViewResult" />
    /// </returns>
    public static ViewResult ViewClay(this Controller controller, string? viewName, object? model,
        ClayOptions? clayOptions = null) =>
        controller.View(viewName, Clay.Parse(model, clayOptions));
}