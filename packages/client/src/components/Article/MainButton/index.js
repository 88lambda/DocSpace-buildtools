import React from "react";

import { inject, observer } from "mobx-react";

import MainButton from "@docspace/components/main-button";
import { withTranslation } from "react-i18next";
import { isMobile } from "react-device-detect";
import Loaders from "@docspace/common/components/Loaders";
import { AppServerConfig, FileAction } from "@docspace/common/constants";
import { encryptionUploadDialog } from "../../../helpers/desktop";
import { withRouter } from "react-router";

import MobileView from "./MobileView";
import { combineUrl } from "@docspace/common/utils";
import config from "PACKAGE_FILE";
import withLoader from "../../../HOCs/withLoader";
import { Events } from "@docspace/client/src/helpers/filesConstants";
import { getMainButtonItems } from "SRC_DIR/helpers/plugins";

import toastr from "client/toastr";

const ArticleMainButtonContent = (props) => {
  const {
    t,
    isMobileArticle,
    isDisabled,
    canCreate,
    isPrivacy,
    encryptedFile,
    encrypted,
    startUpload,
    setAction,
    setCreateRoomDialogVisible,
    setSelectFileDialogVisible,
    isArticleLoading,
    isFavoritesFolder,
    isShareFolder,
    isRecentFolder,
    isCommonFolder,
    isRecycleBinFolder,
    history,
    currentFolderId,
    isRoomsFolder,
    isArchiveFolder,

    selectedTreeNode,

    enablePlugins,
  } = props;
  const isAccountsPage = selectedTreeNode[0] === "accounts";

  const inputFilesElement = React.useRef(null);
  const inputFolderElement = React.useRef(null);

  const [actions, setActions] = React.useState([]);
  const [uploadActions, setUploadActions] = React.useState([]);
  const [model, setModel] = React.useState([]);

  const onCreate = React.useCallback(
    (e) => {
      const format = e.action || null;

      const event = new Event(Events.CREATE);

      const payload = {
        extension: format,
        id: -1,
      };
      event.payload = payload;

      window.dispatchEvent(event);
    },
    [setAction]
  );

  const onCreateRoom = React.useCallback(() => {
    const event = new Event(Events.ROOM_CREATE);
    window.dispatchEvent(event);
  }, []);

  const onShowSelectFileDialog = React.useCallback(() => {
    setSelectFileDialogVisible(true);
  }, [setSelectFileDialogVisible]);

  const onFileChange = React.useCallback(
    (e) => {
      startUpload(e.target.files, null, t);
    },
    [startUpload, t]
  );

  const onUploadFileClick = React.useCallback(() => {
    if (isPrivacy) {
      encryptionUploadDialog((encryptedFile, encrypted) => {
        encryptedFile.encrypted = encrypted;
        startUpload([encryptedFile], null, t);
      });
    } else {
      inputFilesElement.current.click();
    }
  }, [
    isPrivacy,
    encrypted,
    encryptedFile,
    encryptionUploadDialog,
    startUpload,
  ]);

  const onUploadFolderClick = React.useCallback(() => {
    inputFolderElement.current.click();
  }, []);

  const onInputClick = React.useCallback((e) => (e.target.value = null), []);

  const onShowGallery = () => {
    history.push(
      combineUrl(
        AppServerConfig.proxyURL,
        config.homepage,
        `/form-gallery/${currentFolderId}/`
      )
    );
  };

  const onInvite = React.useCallback((e) => {
    const type = e.action;
    toastr.warning("Work in progress " + type);
    console.log("invite ", type);
  }, []);

  const onInviteAgain = React.useCallback(() => {
    console.log("invite again");
    toastr.warning("Work in progress (invite again)");
  }, []);

  React.useEffect(() => {
    const folderUpload = !isMobile
      ? [
          {
            id: "main-button_upload-folders",
            className: "main-button_drop-down",
            icon: "images/actions.upload.react.svg",
            label: t("UploadFolder"),
            disabled: isPrivacy,
            onClick: onUploadFolderClick,
            key: "upload-folder",
          },
        ]
      : [];

    const formActions = [
      {
        className: "main-button_drop-down",
        icon: "images/form.react.svg",
        label: t("Translations:NewForm"),
        key: "new-form",
        items: [
          {
            className: "main-button_drop-down_sub",
            icon: "images/form.blank.react.svg",
            label: t("Translations:SubNewForm"),
            onClick: onCreate,
            action: "docxf",
            key: "docxf",
          },
          {
            className: "main-button_drop-down_sub",
            icon: "images/form.file.react.svg",
            label: t("Translations:SubNewFormFile"),
            onClick: onShowSelectFileDialog,
            disabled: isPrivacy,
            key: "form-file",
          },
          {
            className: "main-button_drop-down_sub",
            icon: "images/form.gallery.react.svg",
            label: t("Common:OFORMsGallery"),
            onClick: onShowGallery,
            disabled: isPrivacy,
            key: "form-gallery",
          },
        ],
      },
    ];

    const actions = isAccountsPage
      ? [
          {
            id: "main-button_administrator",
            className: "main-button_drop-down",
            icon: "/static/images/person.admin.react.svg",
            label: t("People:Administrator"),
            onClick: onInvite,
            action: "administrator",
            key: "administrator",
          },
          {
            id: "main-button_manager",
            className: "main-button_drop-down",
            icon: "/static/images/person.manager.react.svg",
            label: t("People:Manager"),
            onClick: onInvite,
            action: "manager",
            key: "manager",
          },
          {
            id: "main-button_user",
            className: "main-button_drop-down",
            icon: "/static/images/person.user.react.svg",
            label: t("Common:User"),
            onClick: onInvite,
            action: "user",
            key: "user",
          },
        ]
      : isRoomsFolder
      ? [
          {
            id: "main-button_new-room",
            className: "main-button_drop-down",
            icon: "images/folder.locked.react.svg",
            label: t("Files:NewRoom"),
            onClick: onCreateRoom,
            action: "room",
            key: "room",
          },
        ]
      : [
          {
            id: "main-button_new-document",
            className: "main-button_drop-down",
            icon: "images/actions.documents.react.svg",
            label: t("NewDocument"),
            onClick: onCreate,
            action: "docx",
            key: "docx",
          },
          {
            id: "main-button_new-spreadsheet",
            className: "main-button_drop-down",
            icon: "images/spreadsheet.react.svg",
            label: t("NewSpreadsheet"),
            onClick: onCreate,
            action: "xlsx",
            key: "xlsx",
          },
          {
            id: "main-button_new-presentation",
            className: "main-button_drop-down",
            icon: "images/actions.presentation.react.svg",
            label: t("NewPresentation"),
            onClick: onCreate,
            action: "pptx",
            key: "pptx",
          },
          ...formActions,
          {
            id: "main-button_new-folder",
            className: "main-button_drop-down",
            icon: "images/catalog.folder.react.svg",
            label: t("NewFolder"),
            onClick: onCreate,
            key: "new-folder",
          },
        ];

    const uploadActions = isAccountsPage
      ? [
          {
            id: "main-button_invite-again",
            className: "main-button_drop-down",
            icon: "/static/images/invite.again.react.svg",
            label: t("People:LblInviteAgain"),
            onClick: onInviteAgain,
            action: "invite-again",
            key: "invite-again",
          },
        ]
      : [
          {
            id: "main-button_upload-files",
            className: "main-button_drop-down",
            icon: "images/actions.upload.react.svg",
            label: t("UploadFiles"),
            onClick: onUploadFileClick,
            key: "upload-files",
          },
          ...folderUpload,
        ];

    const menuModel = [...actions];

    if (!isRoomsFolder) {
      menuModel.push({
        isSeparator: true,
        key: "separator",
      });

      menuModel.push(...uploadActions);
      setUploadActions(uploadActions);
    }
    if (enablePlugins) {
      const pluginOptions = getMainButtonItems();

      if (pluginOptions) {
        pluginOptions.forEach((option) => {
          menuModel.splice(option.value.position, 0, {
            key: option.key,
            ...option.value,
          });
        });
      }
    }

    setModel(menuModel);
    setActions(actions);
  }, [
    t,
    isPrivacy,
    currentFolderId,
    isRoomsFolder,

    isAccountsPage,

    enablePlugins,

    onCreate,
    onCreateRoom,
    onInvite,
    onInviteAgain,
    onShowSelectFileDialog,
    onUploadFileClick,
    onUploadFolderClick,
  ]);

  const canInvite = isAccountsPage && selectedTreeNode[1] === "filter";
  const mainButtonText = isAccountsPage
    ? t("Common:Invite")
    : t("Common:Actions");

  return (
    <>
      {isMobileArticle ? (
        <>
          {!isFavoritesFolder &&
            !isRecentFolder &&
            !isCommonFolder &&
            !isShareFolder &&
            !isRecycleBinFolder &&
            !isArchiveFolder &&
            !isArticleLoading &&
            (canCreate || canInvite) && (
              <MobileView
                t={t}
                titleProp={t("Upload")}
                actionOptions={actions}
                buttonOptions={uploadActions}
                isRooms={isRoomsFolder}
              />
            )}
        </>
      ) : (
        <MainButton
          id="files_main-button"
          isDisabled={isDisabled ? isDisabled : !canCreate && !canInvite}
          isDropdown={true}
          text={mainButtonText}
          model={model}
        />
      )}

      <input
        id="customFileInput"
        className="custom-file-input"
        multiple
        type="file"
        onChange={onFileChange}
        onClick={onInputClick}
        ref={inputFilesElement}
        style={{ display: "none" }}
      />
      <input
        id="customFolderInput"
        className="custom-file-input"
        webkitdirectory=""
        mozdirectory=""
        type="file"
        onChange={onFileChange}
        onClick={onInputClick}
        ref={inputFolderElement}
        style={{ display: "none" }}
      />
    </>
  );
};

export default inject(
  ({
    auth,
    filesStore,
    dialogsStore,
    uploadDataStore,
    treeFoldersStore,
    selectedFolderStore,
  }) => {
    const { isLoaded, firstLoad, isLoading, canCreate } = filesStore;
    const {
      isPrivacyFolder,
      isFavoritesFolder,
      isRecentFolder,
      isCommonFolder,
      isRecycleBinFolder,
      isShareFolder,
      isRoomsFolder,
      isArchiveFolder,
      selectedTreeNode,
    } = treeFoldersStore;
    const { startUpload } = uploadDataStore;
    const {
      setCreateRoomDialogVisible,
      setSelectFileDialogVisible,
    } = dialogsStore;

    const isArticleLoading = (!isLoaded || isLoading) && firstLoad;

    const { enablePlugins } = auth.settingsStore;

    const currentFolderId = selectedFolderStore.id;

    return {
      showText: auth.settingsStore.showText,
      isMobileArticle: auth.settingsStore.isMobileArticle,

      isArticleLoading,
      isPrivacy: isPrivacyFolder,
      isFavoritesFolder,
      isRecentFolder,
      isCommonFolder,
      isRecycleBinFolder,
      isShareFolder,
      isRoomsFolder,
      isArchiveFolder,
      selectedTreeNode,

      canCreate,

      startUpload,

      setCreateRoomDialogVisible,
      setSelectFileDialogVisible,

      isLoading,
      isLoaded,
      firstLoad,
      currentFolderId,

      enablePlugins,
    };
  }
)(
  withTranslation(["Article", "UploadPanel", "Common", "Files", "People"])(
    withLoader(observer(withRouter(ArticleMainButtonContent)))(
      <Loaders.ArticleButton />
    )
  )
);