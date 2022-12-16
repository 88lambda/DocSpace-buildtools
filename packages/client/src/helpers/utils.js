import authStore from "@docspace/common/store/AuthStore";
import { toCommunityHostname } from "@docspace/common/utils";
import history from "@docspace/common/history";
import { useEffect, useState } from "react";
import { CategoryType } from "./constants";
import { FolderType } from "@docspace/common/constants";

import AboutPageSvgUrl from "ASSETS_DIR/images/logo/aboutpage.svg?url";
import DarkLoginPageSvgUrl from "ASSETS_DIR/images/logo/dark_loginpage.svg?url";
import LeftMenuSvgUrl from "ASSETS_DIR/images/logo/leftmenu.svg?url";
import DarkAboutPageSvgUrl from "ASSETS_DIR/images/logo/dark_aboutpage.svg?url";
import DocseditorSvgUrl from "ASSETS_DIR/images/logo/docseditor.svg?url";
import LightSmallSvgUrl from "ASSETS_DIR/images/logo/lightsmall.svg?url";
import DarkLeftMenuSvgUrl from "ASSETS_DIR/images/logo/dark_leftmenu.svg?url";
import DocsEditoRembedSvgUrl from "ASSETS_DIR/images/logo/docseditorembed.svg?url";
import LoginPageSvgUrl from "ASSETS_DIR/images/logo/loginpage.svg?url";
import DarkLightSmallSvgUrl from "ASSETS_DIR/images/logo/dark_lightsmall.svg?url";

export const setDocumentTitle = (subTitle = null) => {
  const { isAuthenticated, settingsStore, product: currentModule } = authStore;
  const { organizationName } = settingsStore;

  let title;
  if (subTitle) {
    if (isAuthenticated && currentModule) {
      title = subTitle + " - " + currentModule.title;
    } else {
      title = subTitle + " - " + organizationName;
    }
  } else if (currentModule && organizationName) {
    title = currentModule.title + " - " + organizationName;
  } else {
    title = organizationName;
  }

  document.title = title;
};

export const checkIfModuleOld = (link) => {
  if (
    !link ||
    link.includes("files") ||
    link.includes("people") ||
    link.includes("settings")
  ) {
    return false;
  } else {
    return true;
  }
};

export const getLink = (link) => {
  if (!link) return;

  if (!checkIfModuleOld(link)) {
    return link;
  }

  if (link.includes("mail") || link.includes("calendar")) {
    link = link.replace("products", "addons");
  } else {
    link = link.replace("products", "Products");
    link = link.replace("crm", "CRM");
    link = link.replace("projects", "Projects");
  }

  const { protocol, hostname } = window.location;

  const communityHostname = toCommunityHostname(hostname);

  return `${protocol}//${communityHostname}${link}?desktop_view=true`;
};

export const onItemClick = (e) => {
  if (!e) return;
  e.preventDefault();

  const link = e.currentTarget.dataset.link;

  if (checkIfModuleOld(link)) {
    return window.open(link, "_blank");
  }

  history.push(link);
};

export const getPasswordErrorMessage = (t, settings) => {
  return `${t("Common:PasswordMinimumLength")} ${
    settings ? settings.minLength : 8
  } ${settings.digits ? t("Common:PasswordLimitDigits") : ""} ${
    settings.upperCase ? t("Common:PasswordLimitUpperCase") : ""
  } ${settings.specSymbols ? t("Common:PasswordLimitSpecialSymbols") : ""}`;
};

export const useThemeDetector = () => {
  const isDesktopClient = window["AscDesktopEditor"] !== undefined;
  const [systemTheme, setSystemTheme] = useState(
    isDesktopClient
      ? window.RendererProcessVariable?.theme?.type === "dark"
        ? "Dark"
        : "Base"
      : window.matchMedia("(prefers-color-scheme: dark)").matches
      ? "Dark"
      : "Base"
  );

  const systemThemeListener = (e) => {
    setSystemTheme(e.matches ? "Dark" : "Base");
  };

  useEffect(() => {
    if (isDesktopClient) return;

    const mediaQuery = window.matchMedia("(prefers-color-scheme: dark)");
    mediaQuery.addListener(systemThemeListener);

    return () => {
      if (isDesktopClient) return;

      mediaQuery.removeListener(systemThemeListener);
    };
  }, []);

  return systemTheme;
};

export const getCategoryType = (location) => {
  let categoryType = CategoryType.Shared;
  const { pathname } = location;

  if (pathname.startsWith("/rooms")) {
    if (pathname.indexOf("personal") > -1) {
      categoryType = CategoryType.Personal;
    } else if (pathname.indexOf("shared") > -1) {
      categoryType =
        pathname.indexOf("shared/filter") > -1
          ? CategoryType.Shared
          : CategoryType.SharedRoom;
    } else if (pathname.indexOf("archive") > -1) {
      categoryType = CategoryType.Archive;
    }
  } else if (pathname.startsWith("/favorite") > -1) {
    categoryType = CategoryType.Favorite;
  } else if (pathname.startsWith("/recent") > -1) {
    categoryType = CategoryType.Recent;
  } else if (pathname.startsWith("/trash") > -1) {
    categoryType = CategoryType.Trash;
  }

  return categoryType;
};

export const getCategoryTypeByFolderType = (folderType, parentId) => {
  switch (folderType) {
    case FolderType.Rooms:
      return parentId > 0 ? CategoryType.SharedRoom : CategoryType.Shared;

    case FolderType.Archive:
      return CategoryType.Archive;

    case FolderType.Favorites:
      return CategoryType.Favorite;

    case FolderType.Recent:
      return CategoryType.Recent;

    case FolderType.TRASH:
      return CategoryType.Trash;

    default:
      return CategoryType.Personal;
  }
};

export const getCategoryUrl = (categoryType, folderId = null) => {
  const cType = categoryType;

  switch (cType) {
    case CategoryType.Personal:
      return "/rooms/personal/filter";

    case CategoryType.Shared:
      return "/rooms/shared/filter";

    case CategoryType.SharedRoom:
      return `/rooms/shared/${folderId}/filter`;

    case CategoryType.Archive:
      return "/rooms/archived/filter";

    case CategoryType.ArchivedRoom:
      return "/rooms/archived/${folderId}/filter";

    case CategoryType.Favorite:
      return "/files/favorite/filter";

    case CategoryType.Recent:
      return "/files/recent/filter";

    case CategoryType.Trash:
      return "/files/trash/filter";

    default:
      throw new Error("Unknown category type");
  }
};

export const getLogoFromPath = (path) => {
  if (!path || path.indexOf("images/logo/") === -1) return path;

  const name = path.split("/").pop();
  switch (name) {
    case "aboutpage.svg":
      return AboutPageSvgUrl;
    case "dark_loginpage.svg":
      return DarkLoginPageSvgUrl;
    case "leftmenu.svg":
      return LeftMenuSvgUrl;
    case "dark_aboutpage.svg":
      return DarkAboutPageSvgUrl;
    case "docseditor.svg":
      return DocseditorSvgUrl;
    case "lightsmall.svg":
      return LightSmallSvgUrl;
    case "dark_leftmenu.svg":
      return DarkLeftMenuSvgUrl;
    case "docseditorembed.svg":
      return DocsEditoRembedSvgUrl;
    case "loginpage.svg":
      return LoginPageSvgUrl;
    case "dark_lightsmall.svg":
      return DarkLightSmallSvgUrl;
    default:
      break;
  }

  return path;
};
