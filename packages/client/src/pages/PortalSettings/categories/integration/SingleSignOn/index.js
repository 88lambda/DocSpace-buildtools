import React, { useEffect } from "react";
import { isMobile } from "react-device-detect";
import { inject, observer } from "mobx-react";
import { useTranslation } from "react-i18next";

import Box from "@docspace/components/box";

import Certificates from "./Certificates";
import FieldMapping from "./FieldMapping";
import HideButton from "./sub-components/HideButton";
import IdpSettings from "./IdpSettings";
import ProviderMetadata from "./ProviderMetadata";
import StyledSsoPage from "./styled-containers/StyledSsoPageContainer";
import StyledSettingsSeparator from "SRC_DIR/pages/PortalSettings/StyledSettingsSeparator";
import SubmitResetButtons from "./SubmitButton";
import ToggleSSO from "./sub-components/ToggleSSO";

import BreakpointWarning from "SRC_DIR/components/BreakpointWarning";

const SingleSignOn = (props) => {
  const { load, serviceProviderSettings, spMetadata, isSSOAvailable } = props;
  const { t } = useTranslation(["SingleSignOn", "Settings"]);

  if (isMobile)
    return <BreakpointWarning sectionName={t("Settings:SingleSignOn")} />;

  return (
    <StyledSsoPage
      hideSettings={serviceProviderSettings}
      hideMetadata={spMetadata}
      isSettingPaid={isSSOAvailable}
    >
      <ToggleSSO isSSOAvailable={isSSOAvailable} />

      <HideButton
        text={t("ServiceProviderSettings")}
        label={t("ServiceProviderSettings")}
        value={serviceProviderSettings}
        isDisabled={!isSSOAvailable}
      />

      <Box className="service-provider-settings">
        <IdpSettings />

        <Certificates provider="IdentityProvider" />

        <Certificates provider="ServiceProvider" />

        <FieldMapping />

        <SubmitResetButtons />
      </Box>

      <StyledSettingsSeparator />

      <HideButton
        text={t("SpMetadata")}
        label={t("SpMetadata")}
        value={spMetadata}
        isDisabled={!isSSOAvailable}
      />

      <Box className="sp-metadata">
        <ProviderMetadata />
      </Box>
    </StyledSsoPage>
  );
};

export default inject(({ auth, ssoStore }) => {
  const { currentQuotaStore } = auth;
  const { isSSOAvailable } = currentQuotaStore;

  const { load, serviceProviderSettings, spMetadata } = ssoStore;

  return {
    load,
    serviceProviderSettings,
    spMetadata,
    isSSOAvailable,
  };
})(observer(SingleSignOn));
