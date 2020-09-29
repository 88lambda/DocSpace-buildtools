import React from 'react';
import PropTypes from "prop-types";
import styled from 'styled-components';

import { 
  ModalDialog, 
  EmailInput, 
  Button,
  Box,
  Text,
  utils 
} from 'asc-web-components';

const { tablet } = utils.device;

const BtnContainer = styled(Box)`
  width: 100px;
  
  @media ${tablet} {
    width: 293px;
  }
`;

const BodyContainer = styled(Box)`
  font: 13px 'Open Sans', normal;
  line-height: 20px;
`;

const ModalContainer = ({ 
  t, 
  errorLoading, 
  visibleModal, 
  errorMessage, 
  emailOwner,
  settings,
  onEmailChangeHandler,
  onSaveEmailHandler,
  onCloseModal,
  checkingMessages
}) => {

  let header, content, footer;

  const visible = errorLoading ? errorLoading : visibleModal;

  if(errorLoading) {
    header = t('errorLicenseTitle');
    content = <BodyContainer> 
        { errorMessage
            ? errorMessage
            : t('errorLicenseBody')}
    </BodyContainer>;

  } else if( visibleModal && checkingMessages.length < 1) {
    header = t('changeEmailTitle');

    content = <EmailInput
      tabIndex={1}
      scale={true}
      size='base'
      id="change-email"
      name="email-wizard"
      placeholder={t('placeholderEmail')}
      emailSettings={settings}
      value={emailOwner}
      onValidateInput={onEmailChangeHandler}
    />;

    footer = <BtnContainer>
      <Button
        key="saveBtn"
        label={t('changeEmailBtn')}
        primary={true}
        scale={true}
        size="big"
        onClick={onSaveEmailHandler}
      />
    </BtnContainer>;
  } else if ( visibleModal && checkingMessages.length > 0) {
    header = t('errorParamsTitle');

    content = <>
      <Text as="p">{ t('errorParamsBody') }</Text>
      {
        checkingMessages.map((el, index) => <Text key={index} as="p">- {el};</Text>)
      }
    </>;

    footer = <BtnContainer>
      <Button
        key="saveBtn"
        label={t('errorParamsFooter')}
        primary={true}
        scale={true}
        size="big"
        onClick={onCloseModal} />
    </BtnContainer>;
  }

  return (
    <ModalDialog
      visible={visible}
      displayType="auto"
      zIndex={310}
      headerContent={header}
      bodyContent={content}
      footerContent={footer}
      onClose={onCloseModal}
    />
  );
}

ModalContainer.propTypes = {
  t: PropTypes.func.isRequired,
  errorLoading: PropTypes.bool.isRequired,
  visibleModal: PropTypes.bool.isRequired,
  emailOwner: PropTypes.string,
  settings: PropTypes.object.isRequired,
  onEmailChangeHandler: PropTypes.func.isRequired,
  onSaveEmailHandler: PropTypes.func.isRequired,
  onCloseModal: PropTypes.func.isRequired
};

export default ModalContainer;