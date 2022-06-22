import React from "react";
import { observer } from "mobx-react";
import { useTranslation } from "react-i18next";

import Box from "@appserver/components/box";
import Button from "@appserver/components/button";
import FormStore from "@appserver/studio/src/store/SsoFormStore";

import ResetConfirmationModal from "./sub-components/ResetConfirmationModal";

const SubmitResetButtons = () => {
  const { t } = useTranslation(["SingleSignOn", "Settings", "Common"]);

  return (
    <Box alignItems="center" displayProp="flex" flexDirection="row">
      <Button
        className="save-button"
        label={t("Common:SaveButton")}
        onClick={FormStore.onSubmit}
        primary
        size="small"
        tabIndex={23}
      />
      <Button
        label={t("Settings:RestoreDefaultButton")}
        onClick={
          FormStore.isSsoEnabled
            ? FormStore.openResetModal
            : FormStore.resetForm
        }
        size="small"
        tabIndex={24}
      />
      {FormStore.confirmationResetModal && <ResetConfirmationModal />}
    </Box>
  );
};

export default observer(SubmitResetButtons);
