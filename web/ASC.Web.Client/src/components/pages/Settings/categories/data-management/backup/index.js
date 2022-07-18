import React from "react";
import { withTranslation, Trans } from "react-i18next";
import Submenu from "@appserver/components/submenu";
import Link from "@appserver/components/link";
import HelpButton from "@appserver/components/help-button";
import { AppServerConfig } from "@appserver/common/constants";
import { combineUrl } from "@appserver/common/utils";
import { inject, observer } from "mobx-react";
import AutoBackup from "./auto-backup";
import ManualBackup from "./manual-backup";
import config from "../../../../../../../package.json";

const Backup = ({ helpUrlCreatingBackup, buttonSize, t, history }) => {
  const renderTooltip = (helpInfo) => {
    return (
      <>
        <HelpButton
          iconName={"/static/images/help.react.svg"}
          tooltipContent={
            <>
              <Trans t={t} i18nKey={`${helpInfo}`} ns="Settings">
                {helpInfo}
              </Trans>
              <div>
                <Link
                  as="a"
                  href={helpUrlCreatingBackup}
                  target="_blank"
                  color="#555F65"
                  isBold
                  isHovered
                >
                  {t("Common:LearnMore")}
                </Link>
              </div>
            </>
          }
        />
      </>
    );
  };

  const data = [
    {
      id: "data-backup",
      name: t("DataBackup"),
      content: (
        <ManualBackup buttonSize={buttonSize} renderTooltip={renderTooltip} />
      ),
    },
    {
      id: "auto-backup",
      name: t("AutoBackup"),
      content: (
        <AutoBackup buttonSize={buttonSize} renderTooltip={renderTooltip} />
      ),
    },
  ];

  const onSelect = (e) => {
    history.push(
      combineUrl(
        AppServerConfig.proxyURL,
        config.homepage,
        `/settings/backup/${e.id}`
      )
    );
  };
  console.log("BACKUP INDEX");
  return (
    <Submenu data={data} startSelect={data[0]} onSelect={(e) => onSelect(e)} />
  );
};

export default inject(({ auth }) => {
  const { settingsStore } = auth;
  const { helpUrlCreatingBackup, isTabletView } = settingsStore;

  const buttonSize = isTabletView ? "normal" : "small";

  return {
    helpUrlCreatingBackup,
    buttonSize,
  };
})(observer(withTranslation(["Settings", "Common"])(Backup)));
