import React, { useState, useEffect } from "react";
import { withTranslation } from "react-i18next";
import { inject, observer } from "mobx-react";

import Text from "@docspace/components/text";
import HelpButton from "@docspace/components/help-button";
import FieldContainer from "@docspace/components/field-container";
import TextInput from "@docspace/components/text-input";
import Button from "@docspace/components/button";
import Badge from "@docspace/components/badge";
import SaveCancelButtons from "@docspace/components/save-cancel-buttons";
import toastr from "@docspace/components/toast/toastr";

import WhiteLabelWrapper from "./StyledWhitelabel";
import LoaderWhiteLabel from "../sub-components/loaderWhiteLabel";

import Logo from "./sub-components/logo";
import { generateLogo, getLogoOptions } from "../../../utils/generateLogo";

const WhiteLabel = (props) => {
  const {
    t,
    isSettingPaid,
    logoText,
    logoUrls,
    logoSizes,
    restoreWhiteLabelSettings,
    getWhiteLabelLogoUrls,
  } = props;
  const [isLoadedData, setIsLoadedData] = useState(false);
  const [logoTextWhiteLabel, setLogoTextWhiteLabel] = useState("");
  const [logoUrlsWhiteLabel, setLogoUrlsWhiteLabel] = useState(null);

  useEffect(() => {
    if (logoText) {
      setLogoTextWhiteLabel(logoText);
    }
  }, [logoText]);

  useEffect(() => {
    if (logoUrls) {
      setLogoUrlsWhiteLabel(logoUrls);
    }
  }, [logoUrls]);

  useEffect(() => {
    if (logoTextWhiteLabel && logoUrlsWhiteLabel.length && !isLoadedData) {
      setIsLoadedData(true);
    }
  }, [isLoadedData, logoTextWhiteLabel, logoUrlsWhiteLabel]);

  const onChangeCompanyName = (e) => {
    const value = e.target.value;
    setLogoTextWhiteLabel(value);
  };

  const onUseTextAsLogo = () => {
    let newLogos = [];
    for (let i = 0; i < logoUrlsWhiteLabel.length; i++) {
      const width = logoUrlsWhiteLabel[i].width / 2;
      const height = logoUrlsWhiteLabel[i].height / 2;
      const options = getLogoOptions(i, logoTextWhiteLabel);
      console.log(i, logoUrlsWhiteLabel[i]);
      const logo = generateLogo(width, height, options.text, options.fontSize);
      newLogos.push(logo);
    }
    setLogoUrlsWhiteLabel(newLogos);
  };

  const onRestoreLogo = () => {
    try {
      restoreWhiteLabelSettings(true);
      getWhiteLabelLogoUrls();
      toastr.success(t("Settings:SuccessfullySaveSettingsMessage"));
    } catch (error) {
      toastr.error(error);
    }
  };

  return !isLoadedData ? (
    <LoaderWhiteLabel />
  ) : (
    <WhiteLabelWrapper>
      <Text className="subtitle" color="#657077">
        {t("BrandingSubtitle")}
      </Text>
      <div className="header-container">
        <Text fontSize="16px" fontWeight="700">
          {t("WhiteLabel")}
        </Text>
        {!isSettingPaid && <Badge backgroundColor="#EDC409" label="Paid" />}
      </div>
      <Text className="wl-subtitle settings_unavailable" fontSize="12px">
        {t("WhiteLabelSubtitle")}
      </Text>

      <div className="wl-helper">
        <Text className="settings_unavailable">{t("WhiteLabelHelper")}</Text>
        <HelpButton
          tooltipContent={t("WhiteLabelTooltip")}
          place="right"
          offsetRight={0}
          className="settings_unavailable"
        />
      </div>
      <div className="settings-block">
        <FieldContainer
          id="fieldContainerCompanyName"
          labelText={t("CompanyNameForCanvasLogo")}
          isVertical={true}
          className="settings_unavailable"
        >
          <TextInput
            className="input"
            value={logoTextWhiteLabel}
            onChange={onChangeCompanyName}
            isDisabled={!isSettingPaid}
            isReadOnly={!isSettingPaid}
            scale={true}
            isAutoFocussed={true}
            tabIndex={1}
            maxLength={30}
          />
          <Button
            id="btnUseAsLogo"
            className="use-as-logo"
            size="small"
            label={t("UseAsLogoButton")}
            onClick={onUseTextAsLogo}
            tabIndex={2}
            isDisabled={!isSettingPaid}
          />
        </FieldContainer>
      </div>

      <div className="logos-container">
        <div className="logo-wrapper">
          <Text
            fontSize="15px"
            fontWeight="600"
            className="settings_unavailable"
          >
            {t("LogoLightSmall")} ({logoUrlsWhiteLabel[0].width}x
            {logoUrlsWhiteLabel[0].height})
          </Text>
          <div className="logos-wrapper">
            <Logo
              title={t("Profile:LightTheme")}
              src={logoUrlsWhiteLabel[0]}
              imageClass="logo-header background-light"
              inputId="logoUploader_1"
              onChangeText={t("ChangeLogoButton")}
            />
            <Logo
              title={t("Profile:DarkTheme")}
              src={logoUrlsWhiteLabel[0]}
              imageClass="logo-header background-dark"
              inputId="logoUploader_1"
              onChangeText={t("ChangeLogoButton")}
            />
          </div>
        </div>

        <div className="logo-wrapper">
          <Text
            fontSize="15px"
            fontWeight="600"
            className="settings_unavailable"
          >
            {t("LogoCompact")} ({logoUrlsWhiteLabel[5].width}x
            {logoUrlsWhiteLabel[5].height})
          </Text>
          <div className="logos-wrapper">
            <Logo
              title={t("Profile:LightTheme")}
              src={logoUrlsWhiteLabel[5]}
              imageClass="border-img logo-compact background-light"
              inputId="logoUploader_6"
              onChangeText={t("ChangeLogoButton")}
            />
            <Logo
              title={t("Profile:DarkTheme")}
              src={logoUrlsWhiteLabel[5]}
              imageClass="border-img logo-compact background-dark"
              inputId="logoUploader_6"
              onChangeText={t("ChangeLogoButton")}
            />
          </div>
        </div>

        <div className="logo-wrapper">
          <Text
            fontSize="15px"
            fontWeight="600"
            className="settings_unavailable"
          >
            {t("LogoLogin")} ({logoUrlsWhiteLabel[1].width}x
            {logoUrlsWhiteLabel[1].height})
          </Text>
          <div className="logos-login-wrapper">
            <Logo
              title={t("Profile:LightTheme")}
              src={logoUrlsWhiteLabel[1]}
              imageClass="border-img logo-big"
              inputId="logoUploader_2"
              onChangeText={t("ChangeLogoButton")}
            />
            <Logo
              title={t("Profile:DarkTheme")}
              src={logoUrlsWhiteLabel[1]}
              imageClass="border-img logo-big background-dark"
              inputId="logoUploader_2"
              onChangeText={t("ChangeLogoButton")}
            />
          </div>
        </div>

        <div className="logo-wrapper">
          <Text
            fontSize="15px"
            fontWeight="600"
            className="settings_unavailable"
          >
            {t("LogoAbout")} ({logoUrlsWhiteLabel[6].width}x
            {logoUrlsWhiteLabel[6].height})
          </Text>
          <div className="logos-wrapper">
            <Logo
              title={t("Profile:LightTheme")}
              src={logoUrlsWhiteLabel[6]}
              imageClass="border-img logo-about"
              inputId="logoUploader_7"
              onChangeText={t("ChangeLogoButton")}
            />
            <Logo
              title={t("Profile:DarkTheme")}
              src={logoUrlsWhiteLabel[6]}
              imageClass="border-img logo-about background-dark"
              inputId="logoUploader_7"
              onChangeText={t("ChangeLogoButton")}
            />
          </div>
        </div>

        <div className="logo-wrapper">
          <Text
            fontSize="15px"
            fontWeight="600"
            className="settings_unavailable"
          >
            {t("LogoFavicon")} ({logoUrlsWhiteLabel[2].width}x
            {logoUrlsWhiteLabel[2].height})
          </Text>
          <Logo
            src={logoUrlsWhiteLabel[2]}
            imageClass="border-img logo-favicon"
            inputId="logoUploader_3"
            onChangeText={t("ChangeLogoButton")}
          />
        </div>

        <div className="logo-wrapper">
          <Text
            fontSize="15px"
            fontWeight="600"
            className="settings_unavailable"
          >
            {t("LogoDocsEditor")} ({logoUrlsWhiteLabel[3].width}x
            {logoUrlsWhiteLabel[3].height})
          </Text>
          <Logo
            isEditor={true}
            src={logoUrlsWhiteLabel[3]}
            inputId="logoUploader_4"
            onChangeText={t("ChangeLogoButton")}
          />
        </div>

        <div className="logo-wrapper">
          <Text
            fontSize="15px"
            fontWeight="600"
            className="settings_unavailable"
          >
            {t("LogoDocsEditorEmbedded")} ({logoUrlsWhiteLabel[4].width}x
            {logoUrlsWhiteLabel[4].height})
          </Text>
          <Logo
            src={logoUrlsWhiteLabel[4]}
            imageClass="border-img logo-embedded-editor"
            inputId="logoUploader_5"
            onChangeText={t("ChangeLogoButton")}
          />
        </div>
      </div>

      <SaveCancelButtons
        tabIndex={3}
        className="save-cancel-buttons"
        //onSaveClick={onSave}
        onCancelClick={onRestoreLogo}
        saveButtonLabel={t("Common:SaveButton")}
        cancelButtonLabel={t("RestoreDefaultButton")}
        displaySettings={true}
        showReminder={isSettingPaid}
        saveButtonDisabled={true}
        //isSaving={isSaving}
      />
    </WhiteLabelWrapper>
  );
};

export default inject(({ setup, auth, common }) => {
  const { setWhiteLabelSettings } = setup;

  const {
    whiteLabelLogoText,
    getWhiteLabelLogoText,
    whiteLabelLogoUrls,
    restoreWhiteLabelSettings,
  } = common;

  const { getWhiteLabelLogoUrls } = auth.settingsStore;

  return {
    theme: auth.settingsStore.theme,
    logoText: whiteLabelLogoText,
    logoUrls: whiteLabelLogoUrls,
    getWhiteLabelLogoText,
    getWhiteLabelLogoUrls,
    setWhiteLabelSettings,
    restoreWhiteLabelSettings,
  };
})(withTranslation(["Settings", "Profile", "Common"])(observer(WhiteLabel)));
