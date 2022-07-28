import React from "react";
import { inject, observer } from "mobx-react";
import { useTranslation } from "react-i18next";

import ComboBox from "@appserver/components/combobox";
import FieldContainer from "@appserver/components/field-container";

import StyledInputWrapper from "../styled-containers/StyledInputWrapper";

const ModalComboBox = (props) => {
  const { t } = useTranslation("SingleSignOn");
  const { isDisabled, spAction, setComboBoxOption } = props;

  const certificateOptions = [
    { key: "signing", label: t("Signing") },
    { key: "encrypt", label: t("Encryption") },
    { key: "signing and encrypt", label: t("SigningAndEncryption") },
  ];

  const currentOption = certificateOptions.find(
    (option) => option.key === spAction
  );

  return (
    <FieldContainer isVertical labelText={t("UsedFor")}>
      <StyledInputWrapper>
        <ComboBox
          onSelect={setComboBoxOption}
          options={certificateOptions}
          scaled
          scaledOptions
          selectedOption={currentOption}
          showDisabledItems
          isDisabled={isDisabled}
        />
      </StyledInputWrapper>
    </FieldContainer>
  );
};

export default inject(({ ssoStore }) => {
  const { spAction, setComboBoxOption } = ssoStore;

  return { spAction, setComboBoxOption };
})(observer(ModalComboBox));
