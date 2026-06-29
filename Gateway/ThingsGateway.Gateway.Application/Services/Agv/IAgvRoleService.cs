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

namespace ThingsGateway.Gateway.Application;

/// <summary>
/// ��ɫ����ӿ�
/// </summary>
public interface IAgvRoleService
{
    /// <summary>
    /// ��ȡ��ɫӵ�е�OpenApiȨ��
    /// </summary>
    /// <param name="id">��ɫid</param>
    Task<GrantPermissionData> ApiOwnPermissionAsync(long id);

    /// <summary>
    /// ��ȡ��ɫ��
    /// </summary>
    Task<List<RoleTreeOutput>> TreeAsync();

    /// <summary>
    /// ɾ����ɫ
    /// </summary>
    /// <param name="ids">id�б�</param>
    Task<bool> DeleteRoleAsync(HashSet<long> ids);

    /// <summary>
    /// �ӻ���/���ݿ��ȡȫ����ɫ��Ϣ
    /// </summary>
    /// <returns>��ɫ�б�</returns>
    Task<List<AgvRole>> GetAllAsync();

    /// <summary>
    /// ���ݽ�ɫid��ȡ��ɫ�б�
    /// </summary>
    /// <param name="input">��ɫid�б�</param>
    /// <returns>��ɫ�б�</returns>
    Task<IEnumerable<AgvRole>> GetRoleListByIdListAsync(HashSet<long> input);

    /// <summary>
    /// �����û�id��ȡ��ɫ�б�
    /// </summary>
    /// <param name="userId">�û�id</param>
    /// <returns>��ɫ�б�</returns>
    Task<IEnumerable<AgvRole>> GetRoleListByUserIdAsync(long userId);

    /// <summary>
    /// ��ȨOpenApiȨ��
    /// </summary>
    /// <param name="input">��Ȩ��Ϣ</param>
    Task GrantApiPermissionAsync(GrantPermissionData input);

    /// <summary>
    /// ��Ȩ��Դ
    /// </summary>
    /// <param name="input">��Ȩ��Ϣ</param>
    Task GrantResourceAsync(GrantResourceData input);

    /// <summary>
    /// ��Ȩ�û�
    /// </summary>
    /// <param name="input">��Ȩ����</param>
    Task GrantUserAsync(GrantUserOrRoleInput input);

    /// <summary>
    /// ��ȡӵ�е���Դ
    /// </summary>
    /// <param name="id">id</param>
    /// <param name="category">����</param>
    Task<GrantResourceData> OwnResourceAsync(long id, RelationCategoryEnum category = RelationCategoryEnum.RoleHasResource);

    /// <summary>
    /// ��ȡ��ɫ���û�id�б�
    /// </summary>
    /// <param name="id">��ɫid</param>
    /// <returns></returns>
    Task<IEnumerable<long>> OwnUserAsync(long id);

    /// <summary>
    /// ������ѯ
    /// </summary>
    /// <param name="option">��ѯ����</param>
    /// <param name="queryFunc">��ѯ����</param>
    Task<QueryData<AgvRole>> PageAsync(QueryPageOptions option, Func<ISugarQueryable<AgvRole>, ISugarQueryable<AgvRole>>? queryFunc = null);

    /// <summary>
    /// ˢ�»���
    /// </summary>
    void RefreshCache();

    /// <summary>
    /// �����ɫ
    /// </summary>
    /// <param name="input">��ɫ</param>
    /// <param name="type">��������</param>
    Task<bool> SaveRoleAsync(AgvRole input, ItemChangedType type);
}
