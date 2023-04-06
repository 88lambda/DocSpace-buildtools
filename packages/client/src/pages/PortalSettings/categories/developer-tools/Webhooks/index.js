import Button from "@docspace/components/button";
import React, { useState, useEffect } from "react";
import WebhookDialog from "./sub-components/WebhookDialog";
import { WebhookInfo } from "./sub-components/WebhookInfo";
import WebhooksTable from "./sub-components/WebhooksTable";
import { setDocumentTitle } from "SRC_DIR/helpers/utils";

import { inject, observer } from "mobx-react";

import styled from "styled-components";
import { WebhookConfigsLoader } from "./sub-components/Loaders";

const MainWrapper = styled.div`
  width: 100%;

  .toggleButton {
    display: flex;
    align-items: center;
  }
`;

const Webhooks = (props) => {
  const { state, loadWebhooks, addWebhook, isWebhookExist, isWebhooksEmpty } = props;

  const [isModalOpen, setIsModalOpen] = useState(false);

  const onCreateWebhook = async (webhookInfo) => {
    if (!isWebhookExist(webhookInfo)) {
      await addWebhook(webhookInfo);
      closeModal();
    }
  };

  const closeModal = () => setIsModalOpen(false);
  const openModal = () => setIsModalOpen(true);

  useEffect(() => {
    setDocumentTitle("Developer Tools");
    loadWebhooks();
  }, []);

  return state === "pending" ? (
    <WebhookConfigsLoader />
  ) : state === "success" ? (
    <MainWrapper>
      <WebhookInfo />
      <Button label="Create webhook" primary size="small" onClick={openModal} />
      {!isWebhooksEmpty && <WebhooksTable />}
      <WebhookDialog
        visible={isModalOpen}
        onClose={closeModal}
        header="Create webhook"
        onSubmit={onCreateWebhook}
      />
    </MainWrapper>
  ) : state === "error" ? (
    "error"
  ) : (
    ""
  );
};

export default inject(({ webhooksStore }) => {
  const { state, loadWebhooks, addWebhook, isWebhookExist, isWebhooksEmpty } = webhooksStore;

  return {
    state,
    loadWebhooks,
    addWebhook,
    isWebhookExist,
    isWebhooksEmpty,
  };
})(observer(Webhooks));
