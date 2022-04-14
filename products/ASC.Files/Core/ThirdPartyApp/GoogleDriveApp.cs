// (c) Copyright Ascensio System SIA 2010-2022
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

using MimeMapping = ASC.Common.Web.MimeMapping;

namespace ASC.Web.Files.ThirdPartyApp;

public class GoogleDriveApp : Consumer, IThirdPartyApp, IOAuthProvider
{
    public const string AppAttr = "gdrive";

    public string Scopes => string.Empty;
    public string CodeUrl => string.Empty;
    public string AccessTokenUrl => _googleLoginProvider.Instance.AccessTokenUrl;
    public string RedirectUri => this["googleDriveAppRedirectUrl"];
    public string ClientID => this["googleDriveAppClientId"];
    public string ClientSecret => this["googleDriveAppSecretKey"];
    public bool IsEnabled => !string.IsNullOrEmpty(ClientID) && !string.IsNullOrEmpty(ClientSecret);

    public ILog Logger { get; }
    private readonly PathProvider _pathProvider;
    private readonly TenantUtil _tenantUtil;
    private readonly AuthContext _authContext;
    private readonly SecurityContext _securityContext;
    private readonly UserManager _userManager;
    private readonly UserManagerWrapper _userManagerWrapper;
    private readonly CookiesManager _cookiesManager;
    private readonly MessageService _messageService;
    private readonly Global _global;
    private readonly GlobalStore _globalStore;
    private readonly EmailValidationKeyProvider _emailValidationKeyProvider;
    private readonly FilesLinkUtility _filesLinkUtility;
    private readonly SettingsManager _settingsManager;
    private readonly PersonalSettingsHelper _personalSettingsHelper;
    private readonly BaseCommonLinkUtility _baseCommonLinkUtility;
    private readonly FileUtility _fileUtility;
    private readonly FilesSettingsHelper _filesSettingsHelper;
    private readonly IOptionsSnapshot<AccountLinker> _snapshot;
    private readonly SetupInfo _setupInfo;
    private readonly GoogleLoginProvider _googleLoginProvider;
    private readonly TokenHelper _tokenHelper;
    private readonly DocumentServiceConnector _documentServiceConnector;
    private readonly ThirdPartyAppHandlerService _thirdPartyAppHandlerService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpClientFactory _clientFactory;
    private readonly RequestHelper _requestHelper;
    private readonly OAuth20TokenHelper _oAuth20TokenHelper;

    public GoogleDriveApp() { }

    public GoogleDriveApp(
        PathProvider pathProvider,
        TenantUtil tenantUtil,
        AuthContext authContext,
        SecurityContext securityContext,
        UserManager userManager,
        UserManagerWrapper userManagerWrapper,
        CookiesManager cookiesManager,
        MessageService messageService,
        Global global,
        GlobalStore globalStore,
        EmailValidationKeyProvider emailValidationKeyProvider,
        FilesLinkUtility filesLinkUtility,
        SettingsManager settingsManager,
        PersonalSettingsHelper personalSettingsHelper,
        BaseCommonLinkUtility baseCommonLinkUtility,
        IOptionsMonitor<ILog> option,
        FileUtility fileUtility,
        FilesSettingsHelper filesSettingsHelper,
        IOptionsSnapshot<AccountLinker> snapshot,
        SetupInfo setupInfo,
        GoogleLoginProvider googleLoginProvider,
        TokenHelper tokenHelper,
        DocumentServiceConnector documentServiceConnector,
        ThirdPartyAppHandlerService thirdPartyAppHandlerService,
        IServiceProvider serviceProvider,
        TenantManager tenantManager,
        CoreBaseSettings coreBaseSettings,
        CoreSettings coreSettings,
        IConfiguration configuration,
        ICacheNotify<ConsumerCacheItem> cache,
        ConsumerFactory consumerFactory,
        IHttpClientFactory clientFactory,
            OAuth20TokenHelper oAuth20TokenHelper,
            RequestHelper requestHelper,
        string name, int order, Dictionary<string, string> additional)
        : base(tenantManager, coreBaseSettings, coreSettings, configuration, cache, consumerFactory, name, order, additional)
    {
        Logger = option.CurrentValue;
        _pathProvider = pathProvider;
        _tenantUtil = tenantUtil;
        _authContext = authContext;
        _securityContext = securityContext;
        _userManager = userManager;
        _userManagerWrapper = userManagerWrapper;
        _cookiesManager = cookiesManager;
        _messageService = messageService;
        _global = global;
        _globalStore = globalStore;
        _emailValidationKeyProvider = emailValidationKeyProvider;
        _filesLinkUtility = filesLinkUtility;
        _settingsManager = settingsManager;
        _personalSettingsHelper = personalSettingsHelper;
        _baseCommonLinkUtility = baseCommonLinkUtility;
        _fileUtility = fileUtility;
        _filesSettingsHelper = filesSettingsHelper;
        _snapshot = snapshot;
        _setupInfo = setupInfo;
        _googleLoginProvider = googleLoginProvider;
        _tokenHelper = tokenHelper;
        _documentServiceConnector = documentServiceConnector;
        _thirdPartyAppHandlerService = thirdPartyAppHandlerService;
        _serviceProvider = serviceProvider;
        _clientFactory = clientFactory;
        _oAuth20TokenHelper = oAuth20TokenHelper;
        _requestHelper = requestHelper;
    }

    public async Task<bool> RequestAsync(HttpContext context)
    {
        switch ((context.Request.Query[FilesLinkUtility.Action].FirstOrDefault() ?? "").ToLower())
        {
            case "stream":
                await StreamFileAsync(context);
                return true;
            case "convert":
                await ConfirmConvertFileAsync(context);
                return true;
            case "create":
                await CreateFileAsync(context);
                return true;
        }

        if (!string.IsNullOrEmpty(context.Request.Query["code"]))
        {
            await RequestCodeAsync(context);

            return true;
        }

        return false;
    }

    public string GetRefreshUrl()
    {
        return AccessTokenUrl;
    }

    public File<string> GetFile(string fileId, out bool editable)
    {
        Logger.Debug("GoogleDriveApp: get file " + fileId);
        fileId = ThirdPartySelector.GetFileId(fileId);

        var token = _tokenHelper.GetToken(AppAttr);
        var driveFile = GetDriveFile(fileId, token);
        editable = false;

        if (driveFile == null)
        {
            return null;
        }

        var jsonFile = JObject.Parse(driveFile);

        var file = _serviceProvider.GetService<File<string>>();
        file.Id = ThirdPartySelector.BuildAppFileId(AppAttr, jsonFile.Value<string>("id"));
        file.Title = Global.ReplaceInvalidCharsAndTruncate(GetCorrectTitle(jsonFile));
        file.CreateOn = _tenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("createdTime"));
        file.ModifiedOn = _tenantUtil.DateTimeFromUtc(jsonFile.Value<DateTime>("modifiedTime"));
        file.ContentLength = Convert.ToInt64(jsonFile.Value<string>("size"));
        file.ModifiedByString = jsonFile["lastModifyingUser"]["displayName"].Value<string>();
        file.ProviderKey = "Google";

        var owners = jsonFile["owners"];
        if (owners != null)
        {
            file.CreateByString = owners[0]["displayName"].Value<string>();
        }

        editable = jsonFile["capabilities"]["canEdit"].Value<bool>();

        return file;
    }

    public string GetFileStreamUrl(File<string> file)
    {
        if (file == null)
        {
            return string.Empty;
        }

        var fileId = ThirdPartySelector.GetFileId(file.Id);

        return GetFileStreamUrl(fileId);
    }

    private string GetFileStreamUrl(string fileId)
    {
        Logger.Debug("GoogleDriveApp: get file stream url " + fileId);

        var uriBuilder = new UriBuilder(_baseCommonLinkUtility.GetFullAbsolutePath(_thirdPartyAppHandlerService.HandlerPath));
        if (uriBuilder.Uri.IsLoopback)
        {
            uriBuilder.Host = Dns.GetHostName();
        }

        var query = uriBuilder.Query;
        query += FilesLinkUtility.Action + "=stream&";
        query += FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(fileId) + "&";
        query += CommonLinkUtility.ParamName_UserUserID + "=" + HttpUtility.UrlEncode(_authContext.CurrentAccount.ID.ToString()) + "&";
        query += FilesLinkUtility.AuthKey + "=" + _emailValidationKeyProvider.GetEmailKey(fileId + _authContext.CurrentAccount.ID) + "&";
        query += ThirdPartySelector.AppAttr + "=" + AppAttr;

        return uriBuilder.Uri + "?" + query;
    }

    public async Task SaveFileAsync(string fileId, string fileType, string downloadUrl, Stream stream)
    {
        Logger.Debug("GoogleDriveApp: save file stream " + fileId +
                            (stream == null
                                 ? " from - " + downloadUrl
                                 : " from stream"));
        fileId = ThirdPartySelector.GetFileId(fileId);

        var token = _tokenHelper.GetToken(AppAttr);

        var driveFile = GetDriveFile(fileId, token);
        if (driveFile == null)
        {
            Logger.Error("GoogleDriveApp: file is null");

            throw new Exception("File not found");
        }

        var jsonFile = JObject.Parse(driveFile);
        var currentType = GetCorrectExt(jsonFile);
        if (!fileType.Equals(currentType))
        {
            try
            {
                if (stream != null)
                {
                    downloadUrl = await _pathProvider.GetTempUrlAsync(stream, fileType);
                    downloadUrl = _documentServiceConnector.ReplaceCommunityAdress(downloadUrl);
                }

                Logger.Debug("GoogleDriveApp: GetConvertedUri from " + fileType + " to " + currentType + " - " + downloadUrl);

                var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);

                var resultTuple = await _documentServiceConnector.GetConvertedUriAsync(downloadUrl, fileType, currentType, key, null, null, null, false);
                downloadUrl = resultTuple.ConvertedDocumentUri;

                stream = null;
            }
            catch (Exception e)
            {
                Logger.Error("GoogleDriveApp: Error convert", e);
            }
        }

        var httpClient = _clientFactory.CreateClient();

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(GoogleLoginProvider.GoogleUrlFileUpload + "/{fileId}?uploadType=media".Replace("{fileId}", fileId)),
            Method = HttpMethod.Patch
        };
        request.Headers.Add("Authorization", "Bearer " + token);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(currentType));

        if (stream != null)
        {
            request.Content = new StreamContent(stream);
        }
        else
        {
            using var response = await httpClient.SendAsync(request);
            using var downloadStream = new ResponseStream(response);

            request.Content = new StreamContent(downloadStream);
        }

        try
        {
            using var response = await httpClient.SendAsync(request);
            using var responseStream = await response.Content.ReadAsStreamAsync();
            string result = null;
            if (responseStream != null)
            {
                using var readStream = new StreamReader(responseStream);
                result = await readStream.ReadToEndAsync();
            }

            Logger.Debug("GoogleDriveApp: save file stream response - " + result);
        }
        catch (HttpRequestException e)
        {
            Logger.Error("GoogleDriveApp: Error save file stream", e);
            if (e.StatusCode == HttpStatusCode.Forbidden || e.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
            }

            throw;
        }
    }

    private async Task RequestCodeAsync(HttpContext context)
    {
        var state = context.Request.Query["state"];
        Logger.Debug("GoogleDriveApp: state - " + state);
        if (string.IsNullOrEmpty(state))
        {
            Logger.Error("GoogleDriveApp: empty state");

            throw new Exception("Empty state");
        }

        var token = GetToken(context.Request.Query["code"]);
        if (token == null)
        {
            Logger.Error("GoogleDriveApp: token is null");

            throw new SecurityException("Access token is null");
        }

        var stateJson = JObject.Parse(state);

        var googleUserId = stateJson.Value<string>("userId");

        if (_authContext.IsAuthenticated)
        {
            if (!CurrentUser(googleUserId))
            {
                Logger.Debug("GoogleDriveApp: logout for " + googleUserId);
                _cookiesManager.ClearCookies(CookiesType.AuthKey);
                _authContext.Logout();
            }
        }

        if (!_authContext.IsAuthenticated)
        {
            var userInfo = GetUserInfo(token, out var isNew);

            if (userInfo == null)
            {
                Logger.Error("GoogleDriveApp: UserInfo is null");

                throw new Exception("Profile is null");
            }

            var cookiesKey = _securityContext.AuthenticateMe(userInfo.Id);
            _cookiesManager.SetCookies(CookiesType.AuthKey, cookiesKey);
            _messageService.Send(MessageAction.LoginSuccessViaSocialApp);

            if (isNew)
            {
                var userHelpTourSettings = _settingsManager.LoadForCurrentUser<UserHelpTourSettings>();
                userHelpTourSettings.IsNewUser = true;
                _settingsManager.SaveForCurrentUser(userHelpTourSettings);

                _personalSettingsHelper.IsNewUser = true;
                _personalSettingsHelper.IsNotActivated = true;
            }

            if (!string.IsNullOrEmpty(googleUserId) && !CurrentUser(googleUserId))
            {
                AddLinker(googleUserId);
            }
        }

        _tokenHelper.SaveToken(token);

        var action = stateJson.Value<string>("action");
        switch (action)
        {
            case "create":
                //var folderId = stateJson.Value<string>("folderId");

                //context.Response.Redirect(App.Location + "?" + FilesLinkUtility.FolderId + "=" + HttpUtility.UrlEncode(folderId), true);
                return;
            case "open":
                var idsArray = stateJson.Value<JArray>("ids") ?? stateJson.Value<JArray>("exportIds");
                if (idsArray == null)
                {
                    Logger.Error("GoogleDriveApp: ids is empty");

                    throw new Exception("File id is null");
                }
                var fileId = idsArray.ToObject<List<string>>().FirstOrDefault();

                var driveFile = GetDriveFile(fileId, token);
                if (driveFile == null)
                {
                    Logger.Error("GoogleDriveApp: file is null");

                    throw new Exception("File not found");
                }

                var jsonFile = JObject.Parse(driveFile);
                var ext = GetCorrectExt(jsonFile);
                if (_fileUtility.ExtsMustConvert.Contains(ext)
                    || GoogleLoginProvider.GoogleDriveExt.Contains(ext))
                {
                    Logger.Debug("GoogleDriveApp: file must be converted");
                    if (_filesSettingsHelper.ConvertNotify)
                    {
                        //context.Response.Redirect(App.Location + "?" + FilesLinkUtility.FileId + "=" + HttpUtility.UrlEncode(fileId), true);
                        return;
                    }

                    fileId = await CreateConvertedFileAsync(driveFile, token);
                }

                context.Response.Redirect(_filesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);

                return;
        }

        Logger.Error("GoogleDriveApp: Action not identified");

        throw new Exception("Action not identified");
    }

    private async Task StreamFileAsync(HttpContext context)
    {
        try
        {
            var fileId = context.Request.Query[FilesLinkUtility.FileId];
            var auth = context.Request.Query[FilesLinkUtility.AuthKey];
            var userId = context.Request.Query[CommonLinkUtility.ParamName_UserUserID];

            Logger.Debug("GoogleDriveApp: get file stream " + fileId);

            var validateResult = _emailValidationKeyProvider.ValidateEmailKey(fileId + userId, auth, _global.StreamUrlExpire);
            if (validateResult != EmailValidationKeyProvider.ValidationResult.Ok)
            {
                var exc = new HttpException((int)HttpStatusCode.Forbidden, FilesCommonResource.ErrorMassage_SecurityException);

                Logger.Error(string.Format("GoogleDriveApp: validate error {0} {1}: {2}", FilesLinkUtility.AuthKey, validateResult, context.Request.Url()), exc);

                throw exc;
            }

            Token token = null;

            if (Guid.TryParse(userId, out var userIdGuid))
            {
                token = _tokenHelper.GetToken(AppAttr, userIdGuid);
            }

            if (token == null)
            {
                Logger.Error("BoxApp: token is null");

                throw new SecurityException("Access token is null");
            }

            var driveFile = GetDriveFile(fileId, token);

            var jsonFile = JObject.Parse(driveFile);

            var downloadUrl = GoogleLoginProvider.GoogleUrlFile + fileId + "?alt=media";

            if (string.IsNullOrEmpty(downloadUrl))
            {
                Logger.Error("GoogleDriveApp: downloadUrl is null");

                throw new Exception("downloadUrl is null");
            }

            Logger.Debug("GoogleDriveApp: get file stream downloadUrl - " + downloadUrl);

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(downloadUrl),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            var httpClient = _clientFactory.CreateClient();
            using var response = await httpClient.SendAsync(request);
            using var stream = new ResponseStream(response);
            await stream.CopyToAsync(context.Response.Body);

            var contentLength = jsonFile.Value<string>("size");
            Logger.Debug("GoogleDriveApp: get file stream contentLength - " + contentLength);
            context.Response.Headers.Add("Content-Length", contentLength);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            await context.Response.WriteAsync(ex.Message);
            Logger.Error("GoogleDriveApp: Error request " + context.Request.Url(), ex);
        }
        try
        {
            await context.Response.Body.FlushAsync();
            //TODO
            //context.Response.SuppressContent = true;
            //context.ApplicationInstance.CompleteRequest();
        }
        catch (HttpException ex)
        {
            Logger.Error("GoogleDriveApp StreamFile", ex);
        }
    }

    private async Task ConfirmConvertFileAsync(HttpContext context)
    {
        var fileId = context.Request.Query[FilesLinkUtility.FileId];
        Logger.Debug("GoogleDriveApp: ConfirmConvertFile - " + fileId);

        var token = _tokenHelper.GetToken(AppAttr);

        var driveFile = GetDriveFile(fileId, token);
        if (driveFile == null)
        {
            Logger.Error("GoogleDriveApp: file is null");

            throw new Exception("File not found");
        }

        fileId = await CreateConvertedFileAsync(driveFile, token);

        context.Response.Redirect(_filesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);
    }

    private async Task CreateFileAsync(HttpContext context)
    {
        var folderId = context.Request.Query[FilesLinkUtility.FolderId];
        var fileName = context.Request.Query[FilesLinkUtility.FileTitle];
        Logger.Debug("GoogleDriveApp: CreateFile folderId - " + folderId + " fileName - " + fileName);

        var token = _tokenHelper.GetToken(AppAttr);

        var culture = _userManager.GetUsers(_authContext.CurrentAccount.ID).GetCulture();
        var storeTemplate = _globalStore.GetStoreTemplate();

        var path = FileConstant.NewDocPath + culture + "/";
        if (!await storeTemplate.IsDirectoryAsync(path))
        {
            path = FileConstant.NewDocPath + "default/";
        }

        var ext = _fileUtility.InternalExtension[FileUtility.GetFileTypeByFileName(fileName)];
        path += "new" + ext;
        fileName = FileUtility.ReplaceFileExtension(fileName, ext);

        string driveFile;
        using (var content = await storeTemplate.GetReadStreamAsync("", path))
        {
            driveFile = await CreateFileAsync(content, fileName, folderId, token);
        }
        if (driveFile == null)
        {
            Logger.Error("GoogleDriveApp: file is null");

            throw new Exception("File not created");
        }

        var jsonFile = JObject.Parse(driveFile);
        var fileId = jsonFile.Value<string>("id");

        context.Response.Redirect(_filesLinkUtility.GetFileWebEditorUrl(ThirdPartySelector.BuildAppFileId(AppAttr, fileId)), true);
    }

    private Token GetToken(string code)
    {
        try
        {
            Logger.Debug("GoogleDriveApp: GetAccessToken by code " + code);
            var token = _oAuth20TokenHelper.GetAccessToken<GoogleDriveApp>(ConsumerFactory, code);

            return new Token(token, AppAttr);
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }

        return null;
    }

    private bool CurrentUser(string googleId)
    {
        var linker = _snapshot.Get("webstudio");
        var linkedProfiles = linker.GetLinkedObjectsByHashId(HashHelper.MD5($"{ProviderConstants.Google}/{googleId}"));
        linkedProfiles = linkedProfiles.Concat(linker.GetLinkedObjectsByHashId(HashHelper.MD5($"{ProviderConstants.OpenId}/{googleId}")));

        return linkedProfiles.Any(profileId => Guid.TryParse(profileId, out var tmp) && tmp == _authContext.CurrentAccount.ID);
    }

    private void AddLinker(string googleUserId)
    {
        Logger.Debug("GoogleDriveApp: AddLinker " + googleUserId);
        var linker = _snapshot.Get("webstudio");
        linker.AddLink(_authContext.CurrentAccount.ID.ToString(), googleUserId, ProviderConstants.Google);
    }

    private UserInfo GetUserInfo(Token token, out bool isNew)
    {
        isNew = false;
        if (token == null)
        {
            Logger.Error("GoogleDriveApp: token is null");

            throw new SecurityException("Access token is null");
        }

        LoginProfile loginProfile = null;
        try
        {
            loginProfile = _googleLoginProvider.Instance.GetLoginProfile(token.GetRefreshedToken(_tokenHelper, _oAuth20TokenHelper));
        }
        catch (Exception ex)
        {
            Logger.Error("GoogleDriveApp: userinfo request", ex);
        }

        if (loginProfile == null)
        {
            Logger.Error("Error in userinfo request");

            return null;
        }

        var userInfo = _userManager.GetUserByEmail(loginProfile.EMail);
        if (Equals(userInfo, Constants.LostUser))
        {
            userInfo = loginProfile.ProfileToUserInfo(CoreBaseSettings);

            var cultureName = loginProfile.Locale;
            if (string.IsNullOrEmpty(cultureName))
            {
                cultureName = Thread.CurrentThread.CurrentUICulture.Name;
            }

            var cultureInfo = _setupInfo.EnabledCultures.Find(c => string.Equals(c.Name, cultureName, StringComparison.InvariantCultureIgnoreCase));
            if (cultureInfo != null)
            {
                userInfo.CultureName = cultureInfo.Name;
            }
            else
            {
                Logger.DebugFormat("From google app new personal user '{0}' without culture {1}", userInfo.Email, cultureName);
            }

            try
            {
                _securityContext.AuthenticateMeWithoutCookie(ASC.Core.Configuration.Constants.CoreSystem);
                userInfo = _userManagerWrapper.AddUser(userInfo, UserManagerWrapper.GeneratePassword());
            }
            finally
            {
                _securityContext.Logout();
            }

            isNew = true;

            Logger.Debug("GoogleDriveApp: new user " + userInfo.Id);
        }

        return userInfo;
    }

    private string GetDriveFile(string googleFileId, Token token)
    {
        if (token == null)
        {
            Logger.Error("GoogleDriveApp: token is null");

            throw new SecurityException("Access token is null");
        }
        try
        {
            var requestUrl = GoogleLoginProvider.GoogleUrlFile + googleFileId + "?fields=" + HttpUtility.UrlEncode(GoogleLoginProvider.FilesFields);
            var resultResponse = _requestHelper.PerformRequest(requestUrl,
                                                          headers: new Dictionary<string, string> { { "Authorization", "Bearer " + token } });
            Logger.Debug("GoogleDriveApp: file response - " + resultResponse);

            return resultResponse;
        }
        catch (Exception ex)
        {
            Logger.Error("GoogleDriveApp: file request", ex);
        }
        return null;
    }

    private async Task<string> CreateFileAsync(string contentUrl, string fileName, string folderId, Token token)
    {
        if (string.IsNullOrEmpty(contentUrl))
        {
            Logger.Error("GoogleDriveApp: downloadUrl is null");

            throw new Exception("downloadUrl is null");
        }

        Logger.Debug("GoogleDriveApp: create from - " + contentUrl);

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(contentUrl)
        };

        var httpClient = _clientFactory.CreateClient();
        using var response = await httpClient.SendAsync(request);
        using var content = new ResponseStream(response);

        return await CreateFileAsync(content, fileName, folderId, token);
    }

    private async Task<string> CreateFileAsync(Stream content, string fileName, string folderId, Token token)
    {
        Logger.Debug("GoogleDriveApp: create file");

        var httpClient = _clientFactory.CreateClient();

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri(GoogleLoginProvider.GoogleUrlFileUpload + "?uploadType=multipart")
        };

        using (var tmpStream = new MemoryStream())
        {
            var boundary = DateTime.UtcNow.Ticks.ToString("x");

            var folderdata = string.IsNullOrEmpty(folderId) ? "" : $",\"parents\":[\"{folderId}\"]";
            var metadata = "{{\"name\":\"" + fileName + "\"" + folderdata + "}}";
            var metadataPart = $"\r\n--{boundary}\r\nContent-Type: application/json; charset=UTF-8\r\n\r\n{metadata}";
            var bytes = Encoding.UTF8.GetBytes(metadataPart);
            await tmpStream.WriteAsync(bytes, 0, bytes.Length);

            var mediaPartStart = $"\r\n--{boundary}\r\nContent-Type: {MimeMapping.GetMimeMapping(fileName)}\r\n\r\n";
            bytes = Encoding.UTF8.GetBytes(mediaPartStart);
            await tmpStream.WriteAsync(bytes, 0, bytes.Length);

            await content.CopyToAsync(tmpStream);

            var mediaPartEnd = $"\r\n--{boundary}--\r\n";
            bytes = Encoding.UTF8.GetBytes(mediaPartEnd);
            await tmpStream.WriteAsync(bytes, 0, bytes.Length);

            request.Method = HttpMethod.Post;
            request.Headers.Add("Authorization", "Bearer " + token);
            request.Content.Headers.ContentType = new MediaTypeHeaderValue("multipart/related; boundary=" + boundary);

            Logger.Debug("GoogleDriveApp: create file totalSize - " + tmpStream.Length);

            request.Content = new StreamContent(tmpStream);
        }

        try
        {
            using var response = await httpClient.SendAsync(request);
            using var responseStream = await response.Content.ReadAsStreamAsync();
            string result = null;
            if (responseStream != null)
            {
                using var readStream = new StreamReader(responseStream);
                result = await readStream.ReadToEndAsync();
            }

            Logger.Debug("GoogleDriveApp: create file response - " + result);

            return result;
        }
        catch (HttpRequestException e)
        {
            Logger.Error("GoogleDriveApp: Error create file", e);

            if (e.StatusCode == HttpStatusCode.Forbidden || e.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
            }
        }

        return null;
    }

    private async Task<string> ConvertFileAsync(string fileId, string fromExt)
    {
        Logger.Debug("GoogleDriveApp: convert file");

        var downloadUrl = GetFileStreamUrl(fileId);

        var toExt = _fileUtility.GetInternalExtension(fromExt);
        try
        {
            Logger.Debug("GoogleDriveApp: GetConvertedUri- " + downloadUrl);

            var key = DocumentServiceConnector.GenerateRevisionId(downloadUrl);

            var resultTuple = await _documentServiceConnector.GetConvertedUriAsync(downloadUrl, fromExt, toExt, key, null, null, null, false);
            downloadUrl = resultTuple.ConvertedDocumentUri;

        }
        catch (Exception e)
        {
            Logger.Error("GoogleDriveApp: Error GetConvertedUri", e);
        }

        return downloadUrl;
    }

    private async Task<string> CreateConvertedFileAsync(string driveFile, Token token)
    {
        var jsonFile = JObject.Parse(driveFile);
        var fileName = GetCorrectTitle(jsonFile);

        var folderId = (string)jsonFile.SelectToken("parents[0]");

        Logger.Info("GoogleDriveApp: create copy - " + fileName);

        var ext = GetCorrectExt(jsonFile);
        var fileId = jsonFile.Value<string>("id");

        if (GoogleLoginProvider.GoogleDriveExt.Contains(ext))
        {
            var internalExt = _fileUtility.GetGoogleDownloadableExtension(ext);
            fileName = FileUtility.ReplaceFileExtension(fileName, internalExt);
            var requiredMimeType = MimeMapping.GetMimeMapping(internalExt);

            var downloadUrl = GoogleLoginProvider.GoogleUrlFile + $"{fileId}/export?mimeType={HttpUtility.UrlEncode(requiredMimeType)}";

            var httpClient = _clientFactory.CreateClient();

            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(downloadUrl),
                Method = HttpMethod.Get
            };
            request.Headers.Add("Authorization", "Bearer " + token);

            Logger.Debug("GoogleDriveApp: download exportLink - " + downloadUrl);
            try
            {
                using var response = await httpClient.SendAsync(request);
                using var fileStream = new ResponseStream(response);
                driveFile = await CreateFileAsync(fileStream, fileName, folderId, token);
            }
            catch (HttpRequestException e)
            {
                Logger.Error("GoogleDriveApp: Error download exportLink", e);

                if (e.StatusCode == HttpStatusCode.Forbidden || e.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new SecurityException(FilesCommonResource.ErrorMassage_SecurityException, e);
                }
            }
        }
        else
        {
            var convertedUrl = await ConvertFileAsync(fileId, ext);

            if (string.IsNullOrEmpty(convertedUrl))
            {
                Logger.ErrorFormat("GoogleDriveApp: Error convertUrl. size {0}", FileSizeComment.FilesSizeToString(jsonFile.Value<int>("size")));

                throw new Exception(FilesCommonResource.ErrorMassage_DocServiceException + " (convert)");
            }

            var toExt = _fileUtility.GetInternalExtension(fileName);
            fileName = FileUtility.ReplaceFileExtension(fileName, toExt);
            driveFile = await CreateFileAsync(convertedUrl, fileName, folderId, token);
        }

        jsonFile = JObject.Parse(driveFile);

        return jsonFile.Value<string>("id");
    }


    private string GetCorrectTitle(JToken jsonFile)
    {
        var title = jsonFile.Value<string>("name") ?? "";
        var extTitle = FileUtility.GetFileExtension(title);
        var correctExt = GetCorrectExt(jsonFile);

        if (extTitle != correctExt)
        {
            title += correctExt;
        }

        return title;
    }

    private string GetCorrectExt(JToken jsonFile)
    {
        var mimeType = (jsonFile.Value<string>("mimeType") ?? "").ToLower();

        var ext = MimeMapping.GetExtention(mimeType);
        if (!GoogleLoginProvider.GoogleDriveExt.Contains(ext))
        {
            var title = (jsonFile.Value<string>("name") ?? "").ToLower();
            ext = FileUtility.GetFileExtension(title);

            if (MimeMapping.GetMimeMapping(ext) != mimeType)
            {
                var originalFilename = (jsonFile.Value<string>("originalFilename") ?? "").ToLower();
                ext = FileUtility.GetFileExtension(originalFilename);

                if (MimeMapping.GetMimeMapping(ext) != mimeType)
                {
                    ext = MimeMapping.GetExtention(mimeType);

                    Logger.Debug("GoogleDriveApp: Try GetCorrectExt - " + ext + " for - " + mimeType);
                }
            }
        }

        return ext;
    }
}
