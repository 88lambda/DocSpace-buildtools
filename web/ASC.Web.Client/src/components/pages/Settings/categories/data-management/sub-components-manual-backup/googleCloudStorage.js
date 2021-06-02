import React from "react";
import { withTranslation } from "react-i18next";

import Button from "@appserver/components/button";
import TextInput from "@appserver/components/text-input";
import { saveToSessionStorage } from "../../../utils";

class GoogleCloudStorage extends React.Component {
  constructor(props) {
    super(props);
    const { availableStorage, selectedId } = this.props;

    this.state = {
      bucket: "",
      isError: false,
      isChangedInput: false,
    };
    this.isDisabled =
      availableStorage[selectedId] && !availableStorage[selectedId].isSet;

    this.placeholder =
      availableStorage[selectedId] &&
      availableStorage[selectedId].properties[0].title;
    this._isMounted = false;
  }

  onChange = (event) => {
    const { target } = event;
    const value = target.value;
    const name = target.name;

    this.setState({ [name]: value });
  };

  isInvalidForm = () => {
    const { bucket } = this.state;
    if (bucket) return false;

    this.setState({
      isError: true,
    });
    return true;
  };

  onMakeCopy = () => {
    const { fillInputValueArray } = this.props;
    const { bucket } = this.state;

    if (this.isInvalidForm()) return;

    saveToSessionStorage("selectedManualStorageType", "thirdPartyStorage");

    const inputNumber = 1;
    const valuesArray = [bucket];

    this.setState({
      isError: false,
    });
    fillInputValueArray(inputNumber, valuesArray);
  };
  render() {
    const { bucket, isError } = this.state;
    const { t, isLoadingData, isLoading, maxProgress } = this.props;

    return (
      <>
        <TextInput
          name="bucket"
          className="backup_text-input"
          scale={true}
          value={bucket}
          hasError={isError}
          onChange={this.onChange}
          isDisabled={isLoadingData || isLoading || this.isDisabled}
          placeholder={this.placeholder}
          tabIndex={1}
        />

        <div className="manual-backup_buttons">
          <Button
            label={t("MakeCopy")}
            onClick={this.onMakeCopy}
            primary
            isDisabled={!maxProgress}
            size="medium"
            tabIndex={10}
          />
          {!maxProgress && (
            <Button
              label={t("Copying")}
              onClick={() => console.log("click")}
              isDisabled={true}
              size="medium"
              style={{ marginLeft: "8px" }}
              tabIndex={11}
            />
          )}
        </div>
      </>
    );
  }
}
export default withTranslation("Settings")(GoogleCloudStorage);
