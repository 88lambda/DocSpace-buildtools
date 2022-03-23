﻿// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.Files.Api;

public class SecutiryController : ApiControllerBase
{
    private readonly FileStorageService<int> _fileStorageServiceInt;
    private readonly FileStorageService<string> _fileStorageServiceString;
    private readonly SecurityControllerHelper<int> _securityControllerHelperInt;
    private readonly SecurityControllerHelper<string> _securityControllerHelperString;

    public SecutiryController(
        FileStorageService<int> fileStorageServiceInt,
        FileStorageService<string> fileStorageServiceString,
        SecurityControllerHelper<int> securityControllerHelperInt,
        SecurityControllerHelper<string> securityControllerHelperString)
    {
        _fileStorageServiceInt = fileStorageServiceInt;
        _fileStorageServiceString = fileStorageServiceString;
        _securityControllerHelperInt = securityControllerHelperInt;
        _securityControllerHelperString = securityControllerHelperString;
    }

    [Create("owner")]
    public IAsyncEnumerable<FileEntryDto> ChangeOwnerFromBodyAsync([FromBody] ChangeOwnerRequestDto inDto)
    {
        return ChangeOwnerAsync(inDto);
    }

    [Create("owner")]
    [Consumes("application/x-www-form-urlencoded")]
    public IAsyncEnumerable<FileEntryDto> ChangeOwnerFromFormAsync([FromForm] ChangeOwnerRequestDto inDto)
    {
        return ChangeOwnerAsync(inDto);
    }

    /// <summary>
    ///   Returns the external link to the shared file with the ID specified in the request
    /// </summary>
    /// <summary>
    ///   File external link
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <param name="share">Access right</param>
    /// <category>Files</category>
    /// <returns>Shared file link</returns>
    [Update("{fileId}/sharedlinkAsync")]
    public async Task<object> GenerateSharedLinkFromBodyAsync(string fileId, [FromBody] GenerateSharedLinkRequestDto inDto)
    {
        return await _securityControllerHelperString.GenerateSharedLinkAsync(fileId, inDto.Share);
    }

    [Update("{fileId:int}/sharedlinkAsync")]
    public async Task<object> GenerateSharedLinkFromBodyAsync(int fileId, [FromBody] GenerateSharedLinkRequestDto inDto)
    {
        return await _securityControllerHelperInt.GenerateSharedLinkAsync(fileId, inDto.Share);
    }

    [Update("{fileId}/sharedlinkAsync")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<object> GenerateSharedLinkFromFormAsync(string fileId, [FromForm] GenerateSharedLinkRequestDto inDto)
    {
        return await _securityControllerHelperString.GenerateSharedLinkAsync(fileId, inDto.Share);
    }

    [Update("{fileId:int}/sharedlinkAsync")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<object> GenerateSharedLinkFromFormAsync(int fileId, [FromForm] GenerateSharedLinkRequestDto inDto)
    {
        return await _securityControllerHelperInt.GenerateSharedLinkAsync(fileId, inDto.Share);
    }

    /// <summary>
    /// Returns the detailed information about shared file with the ID specified in the request
    /// </summary>
    /// <short>File sharing</short>
    /// <category>Sharing</category>
    /// <param name="fileId">File ID</param>
    /// <returns>Shared file information</returns>
    [Read("file/{fileId}/share")]
    public Task<IEnumerable<FileShareDto>> GetFileSecurityInfoAsync(string fileId)
    {
        return _securityControllerHelperString.GetFileSecurityInfoAsync(fileId);
    }

    [Read("file/{fileId:int}/share")]
    public Task<IEnumerable<FileShareDto>> GetFileSecurityInfoAsync(int fileId)
    {
        return _securityControllerHelperInt.GetFileSecurityInfoAsync(fileId);
    }

    /// <summary>
    /// Returns the detailed information about shared folder with the ID specified in the request
    /// </summary>
    /// <short>Folder sharing</short>
    /// <param name="folderId">Folder ID</param>
    /// <category>Sharing</category>
    /// <returns>Shared folder information</returns>
    [Read("folder/{folderId}/share")]
    public Task<IEnumerable<FileShareDto>> GetFolderSecurityInfoAsync(string folderId)
    {
        return _securityControllerHelperString.GetFolderSecurityInfoAsync(folderId);
    }

    [Read("folder/{folderId:int}/share")]
    public Task<IEnumerable<FileShareDto>> GetFolderSecurityInfoAsync(int folderId)
    {
        return _securityControllerHelperInt.GetFolderSecurityInfoAsync(folderId);
    }

    [Create("share")]
    public async Task<IEnumerable<FileShareDto>> GetSecurityInfoFromBodyAsync([FromBody] BaseBatchRequestDto inDto)
    {
        var (folderIntIds, folderStringIds) = FileOperationsManager.GetIds(inDto.FolderIds);
        var (fileIntIds, fileStringIds) = FileOperationsManager.GetIds(inDto.FileIds);

        var result = new List<FileShareDto>();
        result.AddRange(await _securityControllerHelperInt.GetSecurityInfoAsync(fileIntIds, folderIntIds));
        result.AddRange(await _securityControllerHelperString.GetSecurityInfoAsync(fileStringIds, folderStringIds));

        return result;
    }

    [Create("share")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IEnumerable<FileShareDto>> GetSecurityInfoFromFormAsync([FromForm][ModelBinder(BinderType = typeof(BaseBatchModelBinder))] BaseBatchRequestDto inDto)
    {
        var (folderIntIds, folderStringIds) = FileOperationsManager.GetIds(inDto.FolderIds);
        var (fileIntIds, fileStringIds) = FileOperationsManager.GetIds(inDto.FileIds);

        var result = new List<FileShareDto>();
        result.AddRange(await _securityControllerHelperInt.GetSecurityInfoAsync(fileIntIds, folderIntIds));
        result.AddRange(await _securityControllerHelperString.GetSecurityInfoAsync(fileStringIds, folderStringIds));

        return result;
    }

    /// <summary>
    ///   Removes sharing rights for the group with the ID specified in the request
    /// </summary>
    /// <param name="folderIds">Folders ID</param>
    /// <param name="fileIds">Files ID</param>
    /// <short>Remove group sharing rights</short>
    /// <category>Sharing</category>
    /// <returns>Shared file information</returns>
    [Delete("share")]
    public async Task<bool> RemoveSecurityInfoAsync(BaseBatchRequestDto inDto)
    {
        var (folderIntIds, folderStringIds) = FileOperationsManager.GetIds(inDto.FolderIds);
        var (fileIntIds, fileStringIds) = FileOperationsManager.GetIds(inDto.FileIds);

        await _securityControllerHelperInt.RemoveSecurityInfoAsync(fileIntIds, folderIntIds);
        await _securityControllerHelperString.RemoveSecurityInfoAsync(fileStringIds, folderStringIds);

        return true;
    }

    [Update("{fileId:int}/setacelink")]
    public Task<bool> SetAceLinkAsync(int fileId, [FromBody] GenerateSharedLinkRequestDto inDto)
    {
        return _fileStorageServiceInt.SetAceLinkAsync(fileId, inDto.Share);
    }

    [Update("{fileId}/setacelink")]
    public Task<bool> SetAceLinkAsync(string fileId, [FromBody] GenerateSharedLinkRequestDto inDto)
    {
        return _fileStorageServiceString.SetAceLinkAsync(fileId, inDto.Share);
    }

    /// <summary>
    /// Sets sharing settings for the file with the ID specified in the request
    /// </summary>
    /// <param name="fileId">File ID</param>
    /// <param name="share">Collection of sharing rights</param>
    /// <param name="notify">Should notify people</param>
    /// <param name="sharingMessage">Sharing message to send when notifying</param>
    /// <short>Share file</short>
    /// <category>Sharing</category>
    /// <remarks>
    /// Each of the FileShareParams must contain two parameters: 'ShareTo' - ID of the user with whom we want to share and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc) 
    /// </remarks>
    /// <returns>Shared file information</returns>
    [Update("file/{fileId}/share")]
    public Task<IEnumerable<FileShareDto>> SetFileSecurityInfoFromBodyAsync(string fileId, [FromBody] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperString.SetFileSecurityInfoAsync(fileId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    [Update("file/{fileId:int}/share")]
    public Task<IEnumerable<FileShareDto>> SetFileSecurityInfoFromBodyAsync(int fileId, [FromBody] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperInt.SetFileSecurityInfoAsync(fileId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    [Update("file/{fileId}/share")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IEnumerable<FileShareDto>> SetFileSecurityInfoFromFormAsync(string fileId, [FromForm] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperString.SetFileSecurityInfoAsync(fileId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    [Update("file/{fileId:int}/share")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IEnumerable<FileShareDto>> SetFileSecurityInfoFromFormAsync(int fileId, [FromForm] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperInt.SetFileSecurityInfoAsync(fileId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    /// <summary>
    /// Sets sharing settings for the folder with the ID specified in the request
    /// </summary>
    /// <short>Share folder</short>
    /// <param name="folderId">Folder ID</param>
    /// <param name="share">Collection of sharing rights</param>
    /// <param name="notify">Should notify people</param>
    /// <param name="sharingMessage">Sharing message to send when notifying</param>
    /// <remarks>
    /// Each of the FileShareParams must contain two parameters: 'ShareTo' - ID of the user with whom we want to share and 'Access' - access type which we want to grant to the user (Read, ReadWrite, etc) 
    /// </remarks>
    /// <category>Sharing</category>
    /// <returns>Shared folder information</returns>
    [Update("folder/{folderId}/share")]
    public Task<IEnumerable<FileShareDto>> SetFolderSecurityInfoFromBodyAsync(string folderId, [FromBody] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperString.SetFolderSecurityInfoAsync(folderId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    [Update("folder/{folderId:int}/share")]
    public Task<IEnumerable<FileShareDto>> SetFolderSecurityInfoFromBodyAsync(int folderId, [FromBody] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperInt.SetFolderSecurityInfoAsync(folderId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    [Update("folder/{folderId}/share")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IEnumerable<FileShareDto>> SetFolderSecurityInfoFromFormAsync(string folderId, [FromForm] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperString.SetFolderSecurityInfoAsync(folderId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    [Update("folder/{folderId:int}/share")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IEnumerable<FileShareDto>> SetFolderSecurityInfoFromFormAsync(int folderId, [FromForm] SecurityInfoRequestDto inDto)
    {
        return _securityControllerHelperInt.SetFolderSecurityInfoAsync(folderId, inDto.Share, inDto.Notify, inDto.SharingMessage);
    }

    [Update("share")]
    public Task<IEnumerable<FileShareDto>> SetSecurityInfoFromBodyAsync([FromBody] SecurityInfoRequestDto inDto)
    {
        return SetSecurityInfoAsync(inDto);
    }

    [Update("share")]
    [Consumes("application/x-www-form-urlencoded")]
    public Task<IEnumerable<FileShareDto>> SetSecurityInfoFromFormAsync([FromForm] SecurityInfoRequestDto inDto)
    {
        return SetSecurityInfoAsync(inDto);
    }

    private async IAsyncEnumerable<FileEntryDto> ChangeOwnerAsync(ChangeOwnerRequestDto inDto)
    {
        var (folderIntIds, folderStringIds) = FileOperationsManager.GetIds(inDto.FolderIds);
        var (fileIntIds, fileStringIds) = FileOperationsManager.GetIds(inDto.FileIds);

        var result = AsyncEnumerable.Empty<FileEntry>();
        result.Concat(_fileStorageServiceInt.ChangeOwnerAsync(folderIntIds, fileIntIds, inDto.UserId));
        result.Concat(_fileStorageServiceString.ChangeOwnerAsync(folderStringIds, fileStringIds, inDto.UserId));

        await foreach (var e in result)
        {
            yield return await _securityControllerHelperInt.GetFileEntryWrapperAsync(e);
        }
    }

    private async Task<IEnumerable<FileShareDto>> SetSecurityInfoAsync(SecurityInfoRequestDto inDto)
    {
        var (folderIntIds, folderStringIds) = FileOperationsManager.GetIds(inDto.FolderIds);
        var (fileIntIds, fileStringIds) = FileOperationsManager.GetIds(inDto.FileIds);

        var result = new List<FileShareDto>();
        result.AddRange(await _securityControllerHelperInt.SetSecurityInfoAsync(fileIntIds, folderIntIds, inDto.Share, inDto.Notify, inDto.SharingMessage));
        result.AddRange(await _securityControllerHelperString.SetSecurityInfoAsync(fileStringIds, folderStringIds, inDto.Share, inDto.Notify, inDto.SharingMessage));

        return result;
    }
}