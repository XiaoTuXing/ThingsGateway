//------------------------------------------------------------------------------
//  �˴����Ȩ����Ϊȫ�ļ����ǣ�����ԭ�����ر������������·��ֶ�����
//  �˴����Ȩ�����ر�������Ĵ��룩�����߱���Diego����
//  Դ����ʹ��Э����ѭ���ֿ�Ŀ�ԴЭ�鼰����Э��
//  GiteeԴ����ֿ⣺https://gitee.com/diego2098/ThingsGateway
//  GithubԴ����ֿ⣺https://github.com/kimdiego2098/ThingsGateway
//  ʹ���ĵ���https://thingsgateway.cn/
//  QQȺ��605534569
//------------------------------------------------------------------------------

using ThingsGateway.Admin.Application;

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ϵͳ��ϵ��
///</summary>
[SugarTable("agv_relation", TableDescription = "ϵͳ��ϵ��")]
[Tenant(SqlSugarConst.DB_Custom)]
public class AgvRelation : PrimaryKeyEntity
{
    /// <summary>
    /// ����
    ///</summary>
    [SugarColumn(ColumnDescription = "����")]
    public RelationCategoryEnum Category { get; set; }

    /// <summary>
    /// ����ID
    ///</summary>
    [SugarColumn(ColumnDescription = "����ID")]
    public long ObjectId { get; set; }

    /// <summary>
    /// Ŀ��ID
    ///</summary>
    [SugarColumn(ColumnDescription = "Ŀ��ID", IsNullable = true)]
    public string? TargetId { get; set; }
}
