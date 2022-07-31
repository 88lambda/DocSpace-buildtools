import React from "react";
import { inject, observer } from "mobx-react";

import ComboBox from "@appserver/components/combobox";
import FieldContainer from "@appserver/components/field-container";

import StyledInputWrapper from "../styled-containers/StyledInputWrapper";

const SsoComboBox = (props) => {
  const {
    labelText,
    name,
    options,
    tabIndex,
    value,
    setComboBox,
    enableSso,
    isLoadingXml,
  } = props;

  const currentOption =
    options.find((option) => option.key === value) || options[0];

  const onSelect = () => {
    setComboBox(currentOption, name);
  };

  return (
    <FieldContainer isVertical labelText={labelText}>
      <StyledInputWrapper>
        <ComboBox
          isDisabled={!enableSso || isLoadingXml}
          onSelect={onSelect}
          options={options}
          scaled
          scaledOptions
          selectedOption={currentOption}
          showDisabledItems
          tabIndex={tabIndex}
        />
      </StyledInputWrapper>
    </FieldContainer>
  );
};

export default inject(({ ssoStore }) => {
  const { setComboBox, enableSso, isLoadingXml } = ssoStore;

  return {
    setComboBox,
    enableSso,
    isLoadingXml,
  };
})(observer(SsoComboBox));
