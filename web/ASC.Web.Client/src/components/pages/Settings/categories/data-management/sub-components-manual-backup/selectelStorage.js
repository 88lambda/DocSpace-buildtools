import React from "react";
import { withTranslation } from "react-i18next";
import Button from "@appserver/components/button";
import TextInput from "@appserver/components/text-input";
import { saveToSessionStorage } from "../../../utils";

class SelectelStorage extends React.Component {
  constructor(props) {
    super(props);
    const { availableStorage, selectedId } = this.props;

    this.state = {
      private_container: "",
      public_container: "",
      isError: false,
    };

    this.isDisabled =
      availableStorage[selectedId] && !availableStorage[selectedId].isSet;

    this.privatePlaceholder =
      availableStorage[selectedId] &&
      availableStorage[selectedId].properties[0].title;

    this.publicPlaceholder =
      availableStorage[selectedId] &&
      availableStorage[selectedId].properties[1].title;
  }

  onChange = (event) => {
    const { target } = event;
    const value = target.value;
    const name = target.name;

    this.setState({ [name]: value });
  };

  isInvalidForm = () => {
    const { private_container, public_container } = this.state;
    if (private_container || public_container) return false;

    this.setState({
      isError: true,
    });
    return true;
  };

  onMakeCopy = () => {
    const { fillInputValueArray } = this.props;
    const { private_container, public_container } = this.state;

    if (this.isInvalidForm()) return;

    saveToSessionStorage("selectedManualStorageType", "thirdPartyStorage");

    const valuesArray = [private_container, public_container];

    const inputNumber = valuesArray.length;

    this.setState({
      isChangedInput: false,
      isError: false,
    });
    fillInputValueArray(inputNumber, valuesArray);
  };

  render() {
    const { private_container, public_container, isError } = this.state;
    const { t, isLoadingData, isLoading, maxProgress } = this.props;

    return (
      <>
        <TextInput
          name="private_container"
          className="backup_text-input"
          scale={true}
          value={private_container}
          hasError={isError}
          onChange={this.onChange}
          isDisabled={isLoadingData || isLoading || this.isDisabled}
          placeholder={this.privatePlaceholder || ""}
          tabIndex={1}
        />
        <TextInput
          name="public_container"
          className="backup_text-input"
          scale={true}
          value={public_container}
          hasError={isError}
          onChange={this.onChange}
          isDisabled={isLoadingData || isLoading || this.isDisabled}
          placeholder={this.publicPlaceholder || ""}
          tabIndex={1}
        />

        <div className="manual-backup_buttons">
          <Button
            label={t("MakeCopy")}
            onClick={this.onMakeCopy}
            primary
            isDisabled={!maxProgress || this.isDisabled}
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
export default withTranslation("Settings")(SelectelStorage);
