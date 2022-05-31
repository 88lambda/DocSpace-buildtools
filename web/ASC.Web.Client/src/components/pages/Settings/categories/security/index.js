import React, { useEffect, useState } from "react";
import Submenu from "@appserver/components/submenu";
import { withRouter } from "react-router";
import { withTranslation } from "react-i18next";
import { inject, observer } from "mobx-react";
import { AppServerConfig } from "@appserver/common/constants";
import { combineUrl } from "@appserver/common/utils";
import config from "../../../../../../package.json";

import AccessRights from "./access-rights/index.js";
import AccessPortal from "./access-portal/index.js";
import AppLoader from "@appserver/common/components/AppLoader";

const SecurityWrapper = (props) => {
  const { t, history, loadBaseInfo } = props;
  const [currentTab, setCurrentTab] = useState(0);
  const [isLoading, setIsLoading] = useState(false);

  const data = [
    {
      id: "access-portal",
      name: t("PortalAccess"),
      content: <AccessPortal />,
    },
    {
      id: "access-rights",
      name: t("AccessRights"),
      content: <AccessRights />,
    },
  ];

  const load = async () => {
    await loadBaseInfo();
    const path = location.pathname;
    const currentTab = data.findIndex((item) => path.includes(item.id));
    if (currentTab !== -1) setCurrentTab(currentTab);
    setIsLoading(true);
  };

  useEffect(() => {
    load();
  }, []);

  const onSelect = (e) => {
    history.push(
      combineUrl(
        AppServerConfig.proxyURL,
        config.homepage,
        `/settings/security/${e.id}`
      )
    );
  };

  if (!isLoading) return <AppLoader />;
  return (
    <Submenu
      data={data}
      startSelect={currentTab}
      onSelect={(e) => onSelect(e)}
    />
  );
};

export default inject(({ setup }) => {
  const { initSettings } = setup;

  return {
    loadBaseInfo: async () => {
      await initSettings();
    },
  };
})(withTranslation("Settings")(withRouter(observer(SecurityWrapper))));