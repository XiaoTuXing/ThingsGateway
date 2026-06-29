// ------------------------------------------------------------------------------
// �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
// �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
// Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
// GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
// GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
// ʹ���ĵ���https://thingsgateway.cn/
// QQȺ��605534569
// ------------------------------------------------------------------------------

using BootstrapBlazor.Components;
using System.ComponentModel.DataAnnotations;
using ThingsGateway.Admin.Application;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ְλ��
///</summary>
[SugarTable("agv_position", TableDescription = "ְλ��")]
[Tenant(SqlSugarConst.DB_Custom)]
public class AgvPosition : BaseEntity
{
    /// <summary>
    /// ��֯id
    ///</summary>
    [SugarColumn(ColumnName = "OrgId", ColumnDescription = "��֯id")]
    [AutoGenerateColumn(Ignore = true)]
    [MinValue(1)]
    public virtual long OrgId { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnName = "Name", ColumnDescription = "����", Length = 200)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    [Required]
    public virtual string Name { get; set; }

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnName = "Code", ColumnDescription = "����", Length = 200)]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public string Code { get; set; }

    [SugarColumn(ColumnName = "Status", ColumnDescription = "����")]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public bool Status { get; set; } = true;

    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnName = "Category", ColumnDescription = "����")]
    [AutoGenerateColumn(Visible = true, Sortable = true, Filterable = true)]
    public virtual PositionCategoryEnum Category { get; set; }
}
