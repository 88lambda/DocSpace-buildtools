import React from "react";
import { observer } from "mobx-react";
import { useTranslation } from "react-i18next";

import { Text, HelpButton, InputBlock } from "@docspace/components";
import toastr from "@docspace/components/toast/toastr";

import copy from "copy-to-clipboard";

const MetadataUrlField = ({ labelText, name, placeholder, tooltipContent }) => {
  const { t } = useTranslation("Translations");

  const onCopyClick = () => {
    copy(placeholder);
    toastr.success(t("Translations:LinkCopySuccess"));
  };

  return (
    <div className="metadata-field">
      <Text className="label" fontSize="13px" as="div" fontWeight={600}>
        {labelText}
        <HelpButton tooltipContent={tooltipContent} />
      </Text>
      <InputBlock
        className="input"
        isDisabled
        name={name}
        placeholder={placeholder}
        iconName="/static/images/copy.react.svg"
        iconSize={16}
        onIconClick={onCopyClick}
      />
    </div>
  );
};

export default observer(MetadataUrlField);
