import React, { useState, useEffect, useCallback, useMemo } from "react";
import { withTranslation } from "react-i18next";

import { inject, observer } from "mobx-react";
import Button from "@appserver/components/button";
import withLoading from "../../../../../HOCs/withLoading";
import globalColors from "@appserver/components/utils/globalColors";
import styled, { css } from "styled-components";
import TabContainer from "@appserver/components/tabs-container";
import Preview from "./settingsAppearance/preview";

import ColorSchemeDialog from "./sub-components/colorSchemeDialog";

import DropDownItem from "@appserver/components/drop-down-item";
import DropDownContainer from "@appserver/components/drop-down";

import HexColorPickerComponent from "./sub-components/hexColorPicker";
import { isMobileOnly } from "react-device-detect";

import Loaders from "@appserver/common/components/Loaders";
const StyledComponent = styled.div`
  .container {
    display: flex;
  }

  .box {
    width: 46px;
    height: 46px;
    margin-right: 12px;
  }

  .add-theme {
    background: #d0d5da;
    padding-top: 16px;
    padding-left: 16px;
    box-sizing: border-box;
  }
`;

const Appearance = (props) => {
  const { appearanceTheme, selectedThemeId, sendAppearanceTheme } = props;

  const [selectedColor, setSelectedColor] = useState(1);

  const [previewTheme, setPreviewTheme] = useState("Light theme");

  const [showColorSchemeDialog, setShowColorSchemeDialog] = useState(false);

  const [headerColorSchemeDialog, setHeaderColorSchemeDialog] = useState(
    "Edit color scheme"
  );

  const [currentColorAccent, setCurrentColorAccent] = useState(null);
  const [currentColorButtons, setCurrentColorButtons] = useState(null);

  const [openHexColorPickerAccent, setOpenHexColorPickerAccent] = useState(
    false
  );
  const [openHexColorPickerButtons, setOpenHexColorPickerButtons] = useState(
    false
  );

  //TODO: Add default color
  const [appliedColorAccent, setAppliedColorAccent] = useState("#F97A0B");
  const [appliedColorButtons, setAppliedColorButtons] = useState("#FF9933");

  const [changeCurrentColorAccent, setChangeCurrentColorAccent] = useState(
    false
  );
  const [changeCurrentColorButtons, setChangeCurrentColorButtons] = useState(
    false
  );

  const [viewMobile, setViewMobile] = useState(false);

  const [showSaveButtonDialog, setShowSaveButtonDialog] = useState(false);
  const [
    showRestoreToDefaultButtonDialog,
    setShowRestoreToDefaultButtonDialog,
  ] = useState(false);

  const [isEditDialog, setIsEditDialog] = useState(false);
  const [isAddThemeDialog, setIsAddThemeDialog] = useState(false);

  const [selectThemeId, setSelectThemeId] = useState(selectedThemeId);

  const [changeTheme, setChangeTheme] = useState([]);

  const checkImg = <img src="/static/images/check.white.svg" />;

  const array_items = useMemo(
    () => [
      {
        key: "0",
        title: "Light theme",
        content: (
          <Preview previewTheme={previewTheme} selectedColor={selectedColor} />
        ),
      },
      {
        key: "1",
        title: "Dark theme",
        content: (
          <Preview previewTheme={previewTheme} selectedColor={selectedColor} />
        ),
      },
    ],
    [selectedColor, previewTheme]
  );

  useEffect(() => {
    onCheckView();
    window.addEventListener("resize", onCheckView);

    return () => window.removeEventListener("resize", onCheckView);
  }, []);

  useEffect(() => {
    if (
      changeCurrentColorAccent &&
      changeCurrentColorButtons &&
      isAddThemeDialog
    ) {
      setShowSaveButtonDialog(true);
    }

    if (
      (changeCurrentColorAccent || changeCurrentColorButtons) &&
      isEditDialog
    ) {
      setShowSaveButtonDialog(true);
    }
  }, [
    changeCurrentColorAccent,
    changeCurrentColorButtons,
    isAddThemeDialog,
    isEditDialog,
  ]);

  const onCheckView = () => {
    if (isMobileOnly || window.innerWidth <= 428) {
      setViewMobile(true);
    } else {
      setViewMobile(false);
    }
  };

  const onColorSelection = (e) => {
    if (!e.target.id) return;

    const colorNumber = +e.target.id;

    setSelectThemeId(colorNumber);

    //TODO: find id and give item
    //TODO: if changeTheme array = 0, then appearanceTheme, else changeTheme array
    // const theme = changeTheme?.find((item) => item.id === colorNumber);

    // If theme has already been edited before
    // if (theme) {
    //   theme.isSelected = true;

    //   setSelectThemeId(theme);
    // } else {
    //    If theme not has already been edited before
    //   const theme = appearanceTheme.find((item) => item.id === colorNumber);

    //   theme.isSelected = true;

    //   setSelectThemeId(theme);
    // }
  };

  const onShowCheck = (colorNumber) => {
    return selectThemeId && selectThemeId === colorNumber && checkImg;
  };

  const onChangePreviewTheme = (e) => {
    setPreviewTheme(e.title);
  };

  const onSaveSelectedColor = () => {
    sendAppearanceTheme({ selected: selectThemeId });
  };

  const onClickEdit = () => {
    appearanceTheme.map((item) => {
      if (item.id === selectedColor) {
        // TODO: give store Accent color and Buttons main to id

        setCurrentColorAccent(item.accentColor);
        setCurrentColorButtons(item.buttonsMain);
      }
    });

    setIsEditDialog(true);

    setHeaderColorSchemeDialog("Edit color scheme");

    //TODO: if position <=7 then default theme and show button RestoreToDefault
    setShowRestoreToDefaultButtonDialog(true);

    setShowColorSchemeDialog(true);
  };

  const onCloseColorSchemeDialog = () => {
    setShowColorSchemeDialog(false);

    setOpenHexColorPickerAccent(false);
    setOpenHexColorPickerButtons(false);

    setChangeCurrentColorAccent(false);
    setChangeCurrentColorButtons(false);

    setIsEditDialog(false);
    setIsAddThemeDialog(false);

    setShowSaveButtonDialog(false);
  };

  const onAddTheme = () => {
    setIsAddThemeDialog(true);
    setCurrentColorAccent(
      "url(/static/images/plus.theme.svg) 15px 15px no-repeat #D0D5DA"
    );
    setCurrentColorButtons(
      "url(/static/images/plus.theme.svg) 15px 15px no-repeat #D0D5DA"
    );

    setHeaderColorSchemeDialog("New color scheme");

    setShowColorSchemeDialog(true);
  };

  const onClickColor = (e) => {
    if (e.target.id === "accent") {
      setOpenHexColorPickerAccent(true);
      setOpenHexColorPickerButtons(false);
    } else {
      setOpenHexColorPickerButtons(true);
      setOpenHexColorPickerAccent(false);
    }
  };

  const onCloseHexColorPicker = () => {
    setOpenHexColorPickerAccent(false);
    setOpenHexColorPickerButtons(false);
  };

  const onAppliedColorAccent = useCallback(() => {
    setCurrentColorAccent(appliedColorAccent);

    onCloseHexColorPicker();

    if (appliedColorAccent === currentColorAccent) return;

    setChangeCurrentColorAccent(true);
  }, [appliedColorAccent, currentColorAccent]);

  const onAppliedColorButtons = useCallback(() => {
    setCurrentColorButtons(appliedColorButtons);

    onCloseHexColorPicker();

    if (appliedColorButtons === currentColorButtons) return;

    setChangeCurrentColorButtons(true);
  }, [appliedColorButtons]);

  const onSaveColorSchemeDialog = () => {
    // selectTheme.theme.accentColor = currentColorAccent;
    // selectTheme.theme.buttonsMain = currentColorButtons;

    const theme = {
      id: selectTheme.id,
      accentColor: currentColorAccent,
      buttonsMain: currentColorButtons,
      textColor: "#FFFFFF",
    };

    //setChangeTheme([...changeTheme, theme]);

    // onCloseColorSchemeDialog();
  };

  const nodeHexColorPickerButtons = viewMobile ? (
    <HexColorPickerComponent
      id="buttons-hex"
      onCloseHexColorPicker={onCloseHexColorPicker}
      onAppliedColor={onAppliedColorButtons}
      color={appliedColorButtons}
      setColor={setAppliedColorButtons}
      viewMobile={viewMobile}
    />
  ) : (
    <DropDownContainer
      directionX="right"
      manualY="62px"
      withBackdrop={false}
      isDefaultMode={false}
      open={openHexColorPickerButtons}
      clickOutsideAction={onCloseHexColorPicker}
    >
      <DropDownItem className="drop-down-item-hex">
        <HexColorPickerComponent
          id="buttons-hex"
          onCloseHexColorPicker={onCloseHexColorPicker}
          onAppliedColor={onAppliedColorButtons}
          color={appliedColorButtons}
          setColor={setAppliedColorButtons}
          viewMobile={viewMobile}
        />
      </DropDownItem>
    </DropDownContainer>
  );

  const nodeHexColorPickerAccent = viewMobile ? (
    <HexColorPickerComponent
      id="accent-hex"
      onCloseHexColorPicker={onCloseHexColorPicker}
      onAppliedColor={onAppliedColorAccent}
      color={appliedColorAccent}
      setColor={setAppliedColorAccent}
      viewMobile={viewMobile}
    />
  ) : (
    <DropDownContainer
      directionX="right"
      manualY="62px"
      withBackdrop={false}
      isDefaultMode={false}
      open={openHexColorPickerAccent}
      clickOutsideAction={onCloseHexColorPicker}
      viewMobile={viewMobile}
    >
      <DropDownItem className="drop-down-item-hex">
        <HexColorPickerComponent
          id="accent-hex"
          onCloseHexColorPicker={onCloseHexColorPicker}
          onAppliedColor={onAppliedColorAccent}
          color={appliedColorAccent}
          setColor={setAppliedColorAccent}
          viewMobile={viewMobile}
        />
      </DropDownItem>
    </DropDownContainer>
  );

  const nodeAccentColor = (
    <div
      id="accent"
      style={{ background: currentColorAccent }}
      className="color-button"
      onClick={onClickColor}
    ></div>
  );

  const nodeButtonsColor = (
    <div
      id="buttons"
      style={{ background: currentColorButtons }}
      className="color-button"
      onClick={onClickColor}
    ></div>
  );

  if (!(selectThemeId && selectedThemeId && appearanceTheme)) {
    return <Loaders.Rectangle />;
  }

  return (
    <StyledComponent>
      <div>Color</div>

      <div className="container">
        {appearanceTheme.map((item, index) => {
          return (
            <div
              key={index}
              id={item.id}
              style={{ background: item.accentColor }}
              className="box"
              onClick={onColorSelection}
            >
              {onShowCheck(item.id)}
            </div>
          );
        })}

        {/* <div className="add-theme box" onClick={onAddTheme}>
          <img src="/static/images/plus.theme.svg" />
        </div> */}
      </div>

      <div onClick={onClickEdit}>Edit color scheme</div>
      <ColorSchemeDialog
        nodeButtonsColor={nodeButtonsColor}
        nodeAccentColor={nodeAccentColor}
        nodeHexColorPickerAccent={nodeHexColorPickerAccent}
        nodeHexColorPickerButtons={nodeHexColorPickerButtons}
        visible={showColorSchemeDialog}
        onClose={onCloseColorSchemeDialog}
        header={headerColorSchemeDialog}
        viewMobile={viewMobile}
        openHexColorPickerButtons={openHexColorPickerButtons}
        openHexColorPickerAccent={openHexColorPickerAccent}
        showRestoreToDefaultButtonDialog={showRestoreToDefaultButtonDialog}
        showSaveButtonDialog={showSaveButtonDialog}
        onSaveColorSchemeDialog={onSaveColorSchemeDialog}
      />
      <div>Preview</div>
      <TabContainer elements={array_items} onSelect={onChangePreviewTheme} />
      <Button label="Save" onClick={onSaveSelectedColor} primary size="small" />
    </StyledComponent>
  );
};

export default inject(({ auth }) => {
  const { settingsStore } = auth;
  const {
    appearanceTheme,
    selectedThemeId,
    sendAppearanceTheme,
  } = settingsStore;

  return {
    appearanceTheme,
    selectedThemeId,
    sendAppearanceTheme,
  };
})(observer(Appearance));
