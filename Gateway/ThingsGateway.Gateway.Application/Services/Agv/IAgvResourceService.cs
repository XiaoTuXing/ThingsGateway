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
/// ��Դ����ӿڣ���������Դ��ز����Ľӿڷ���
/// </summary>
public interface IAgvResourceService
{
    /// <summary>
    /// ���ĸ���
    /// </summary>
    /// <param name="id"></param>
    /// <param name="parentMenuId"></param>
    /// <returns></returns>
    Task ChangeParentAsync(long id, long parentMenuId);

    /// <summary>
    /// ��������
    /// </summary>
    /// <param name="resourceList">��Դ�б�</param>
    /// <param name="parentId">��ID</param>
    /// <returns></returns>
    IEnumerable<AgvResource> ConstructMenuTrees(List<AgvResource> resourceList, long parentId = 0);

    /// <summary>
    /// ������Դ������ģ��
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="moduleId"></param>
    /// <returns></returns>
    Task CopyAsync(IEnumerable<long> ids, long moduleId);

    /// <summary>
    /// ɾ����Դ
    /// </summary>
    /// <param name="ids">id�б�</param>
    /// <returns></returns>
    Task<bool> DeleteResourceAsync(HashSet<long> ids);

    /// <summary>
    /// �ӻ���/���ݿ��ȡȫ����Դ�б�
    /// </summary>
    /// <returns>ȫ����Դ�б�</returns>
    Task<List<AgvResource>> GetAllAsync();

    /// <summary>
    /// ���ݲ˵�Id��ȡ�˵��б�
    /// </summary>
    /// <param name="menuIds">�˵�id�б�</param>
    /// <returns>�˵��б�</returns>
    Task<IEnumerable<AgvResource>> GetMenuByMenuIdsAsync(IEnumerable<long> menuIds);

    /// <summary>
    /// ����ģ��Id��ȡģ���б�
    /// </summary>
    /// <param name="moduleIds">ģ��id�б�</param>
    /// <returns>�˵��б�</returns>
    Task<IEnumerable<AgvResource>> GetMuduleByMuduleIdsAsync(IEnumerable<long> moduleIds);

    /// <summary>
    /// ��ȡ���˵�����
    /// </summary>
    /// <param name="allMenuList">���в˵��б�</param>
    /// <param name="myMenus">�ҵĲ˵��б�</param>
    /// <returns></returns>
    IEnumerable<AgvResource> GetMyParentResources(IEnumerable<AgvResource> allMenuList, IEnumerable<AgvResource> myMenus);

    /// <summary>
    /// ��ȡ��Դ�����¼����������תΪ����
    /// </summary>
    /// <param name="resourceList">��Դ�б�</param>
    /// <param name="parentId">��Id</param>
    /// <returns></returns>
    IEnumerable<AgvResource> GetResourceChilden(IEnumerable<AgvResource> resourceList, long parentId);

    /// <summary>
    /// ��ȡ��Դ���и������������תΪ����
    /// </summary>
    /// <param name="resourceList">��Դ�б�</param>
    /// <param name="resourceId">Id</param>
    /// <returns></returns>
    IEnumerable<AgvResource> GetResourceParent(IEnumerable<AgvResource> resourceList, long resourceId);

    /// <summary>
    /// �����ѯ
    /// </summary>
    /// <param name="options">��ѯ����</param>
    /// <param name="searchModel">��ѯ����</param>
    /// <returns></returns>
    Task<QueryData<AgvResource>> PageAsync(QueryPageOptions options, ResourceTableSearchModel searchModel);

    /// <summary>
    /// ˢ�»���
    /// </summary>
    void RefreshCache();

    /// <summary>
    /// ������Դ
    /// </summary>
    /// <param name="input">��Դ</param>
    /// <param name="type">��������</param>
    Task<bool> SaveResourceAsync(AgvResource input, ItemChangedType type);
}
