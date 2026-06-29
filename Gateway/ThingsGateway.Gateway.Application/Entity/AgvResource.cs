//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://thingsgateway.cn/
//  QQȺ��605534569
//------------------------------------------------------------------------------

using BootstrapBlazor.Components;
using Microsoft.AspNetCore.Components.Routing;

using Newtonsoft.Json;

using System.ComponentModel.DataAnnotations;
using ThingsGateway.Admin.Application;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ϵͳ��Դ��
///</summary>
[SugarTable("agv_resource", TableDescription = "ϵͳ��Դ��")]
[Tenant(SqlSugarConst.DB_Custom)]
public class AgvResource : BaseEntity
{
    /// <summary>
    /// ��id
    ///</summary>
    [SugarColumn(ColumnDescription = "��id")]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long ParentId { get; set; } = 0;

    /// <summary>
    /// ģ��
    ///</summary>
    [SugarColumn(ColumnDescription = "ģ��")]
    [AutoGenerateColumn(Ignore = true)]
    public virtual long Module { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����", Length = 200)]
    [Required]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true, Searchable = true)]
    public virtual string Title { get; set; }

    /// <summary>
    /// ͼ��
    ///</summary>
    [SugarColumn(ColumnDescription = "ͼ��", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = false, Filterable = false)]
    public virtual string? Icon { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����", Length = 200)]
    [Required]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true, IsVisibleWhenAdd = false, IsVisibleWhenEdit = false)]
    public virtual string Code { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����")]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public ResourceCategoryEnum Category { get; set; } = ResourceCategoryEnum.Menu;

    /// <summary>
    /// Ŀ������
    ///</summary>
    [SugarColumn(ColumnDescription = "Ŀ������", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual TargetEnum? Target { get; set; }

    /// <summary>
    /// �˵�ƥ������
    /// </summary>
    [SugarColumn(ColumnDescription = "�˵�ƥ������", IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual NavLinkMatch? NavLinkMatch { get; set; }

    /// <summary>
    /// ·��
    ///</summary>
    [SugarColumn(ColumnDescription = "·��", Length = 200, IsNullable = true)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true, Searchable = true)]
    public virtual string Href { get; set; }

    /// <summary>
    /// �ӽڵ�
    /// </summary>
    [SugarColumn(IsIgnore = true)]
    [AutoGenerateColumn(Ignore = true)]
    public List<AgvResource>? Children { get; set; }
}
