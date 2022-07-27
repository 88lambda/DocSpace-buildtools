import React, { forwardRef } from "react";
import { inject, observer } from "mobx-react";

import {
  ButtonTheme,
  MainButtonTheme,
  CatalogItemTheme,
  BadgeTheme,
  SubmenuTextTheme,
  SubmenuItemLabelTheme,
  ToggleButtonTheme,
  TabContainerTheme,
  IconButtonTheme,
  MainButtonMobileTheme,
  IndicatorFilterButtonTheme,
  FilterBlockItemTagTheme,
} from "./styled";
import { ThemeType } from "./constants";

// TODO: default
const ColorTheme = forwardRef((props, ref) => {
  switch (props.type) {
    case ThemeType.Button: {
      return <ButtonTheme ref={ref} {...props} />;
    }
    case ThemeType.MainButton: {
      return <MainButtonTheme ref={ref} {...props} />;
    }
    case ThemeType.CatalogItem: {
      return <CatalogItemTheme ref={ref} {...props} />;
    }
    case ThemeType.Badge: {
      return <BadgeTheme ref={ref} {...props} />;
    }
    case ThemeType.SubmenuText: {
      return <SubmenuTextTheme ref={ref} {...props} />;
    }
    case ThemeType.SubmenuItemLabel: {
      return <SubmenuItemLabelTheme ref={ref} {...props} />;
    }
    case ThemeType.ToggleButton: {
      return <ToggleButtonTheme ref={ref} {...props} />;
    }
    case ThemeType.TabContainer: {
      return <TabContainerTheme ref={ref} {...props} />;
    }
    case ThemeType.IconButton: {
      return <IconButtonTheme ref={ref} {...props} />;
    }
    case ThemeType.MainButtonMobile: {
      return <MainButtonMobileTheme ref={ref} {...props} />;
    }
    case ThemeType.IndicatorFilterButton: {
      return <IndicatorFilterButtonTheme ref={ref} {...props} />;
    }
    case ThemeType.FilterBlockItemTag: {
      return <FilterBlockItemTagTheme ref={ref} {...props} />;
    }
  }
});

export default inject(({ auth }) => {
  const { settingsStore } = auth;
  const { currentColorScheme } = settingsStore;

  return {
    currentColorScheme,
  };
})(observer(ColorTheme));
