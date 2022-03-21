import IconButton from "@appserver/components/icon-button";
import { isTablet, mobile, tablet } from "@appserver/components/utils/device";
import { set } from "mobx";
import { inject } from "mobx-react";
import PropTypes from "prop-types";
import React, { useEffect } from "react";
import styled from "styled-components";

const StyledInfoPanelWrapper = styled.div.attrs(({ title }) => ({
  title: title,
}))`
  height: auto;
  width: auto;
  background: rgba(6, 22, 38, 0.2);
  backdrop-filter: blur(18px);

  @media ${tablet} {
    z-index: 1002;
    position: absolute;
    top: 0;
    bottom: 0;
    left: 0;
    right: 0;
  }
`;

const StyledInfoPanel = styled.div`
  height: 100%;
  width: 368px;
  background-color: #ffffff;
  border-left: 1px solid #eceef1;
  display: flex;
  flex-direction: column;

  @media ${tablet} {
    position: absolute;
    border: none;
    right: 0;
    width: 448px;
    max-width: calc(100vw - 69px);
  }

  @media ${mobile} {
    bottom: 0;
    height: 80%;
    width: 100vw;
    max-width: 100vw;
  }
`;

const StyledCloseButtonWrapper = styled.div`
  position: absolute;
  display: none;
  @media ${tablet} {
    display: block;
    top: 0;
    left: 0;
    margin-top: 18px;
    margin-left: -27px;
  }
  @media ${mobile} {
    right: 0;
    left: auto;
    margin-top: -27px;
    margin-right: 10px;
  }
`;

const InfoPanel = ({
  children,
  selectedItems,
  isVisible,
  setIsVisible,
  showCurrentFolder,
}) => {
  //if (!showCurrentFolder && selectedItems.length === 0) return null;
  if (!isVisible) return null;

  const closeInfoPanel = () => setIsVisible(false);

  useEffect(() => {
    const onMouseDown = (e) => {
      if (e.target.title === "InfoPanelWrapper") closeInfoPanel();
    };

    if (isTablet()) document.addEventListener("mousedown", onMouseDown);
    return () => document.removeEventListener("mousedown", onMouseDown);
  }, []);

  return (
    <StyledInfoPanelWrapper className="info-panel" title="InfoPanelWrapper">
      <StyledInfoPanel>
        <StyledCloseButtonWrapper>
          <IconButton
            onClick={closeInfoPanel}
            iconName="/static/images/cross.react.svg"
            size="17"
            color="#ffffff"
            hoverColor="#657077"
            isFill={true}
            className="info-panel-button"
          />
        </StyledCloseButtonWrapper>
        {children}
      </StyledInfoPanel>
    </StyledInfoPanelWrapper>
  );
};

InfoPanel.propTypes = {
  children: PropTypes.oneOfType([
    PropTypes.arrayOf(PropTypes.node),
    PropTypes.node,
    PropTypes.any,
  ]),
  isVisible: PropTypes.bool,
};

export default inject(({ infoPanelStore, filesStore }) => {
  let selectedItems = [];
  let isVisible = false;
  let showCurrentFolder = false;
  let setIsVisible = () => {};

  if (infoPanelStore) {
    showCurrentFolder = infoPanelStore.showCurrentFolder;
    isVisible = infoPanelStore.isVisible;
    setIsVisible = infoPanelStore.setIsVisible;
  }

  if (filesStore) {
    selectedItems = JSON.parse(JSON.stringify(filesStore.selection));
  }

  return {
    selectedItems,
    isVisible,
    showCurrentFolder,
    setIsVisible,
  };
})(InfoPanel);
