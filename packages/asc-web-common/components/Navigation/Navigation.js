import React from "react";
import PropTypes from "prop-types";

import Loaders from "@appserver/common/components/Loaders";

import StyledContainer from "./StyledNavigation";
import ArrowButton from "./sub-components/arrow-btn";
import Text from "./sub-components/text";
import ControlButtons from "./sub-components/control-btn";
import DropBox from "./sub-components/drop-box";

import { isMobileOnly } from "react-device-detect";

import { Consumer } from "@appserver/components/utils/context";

import DomHelpers from "@appserver/components/utils/domHelpers";
import Backdrop from "@appserver/components/backdrop";

const Navigation = ({
  tReady,
  showText,
  isRootFolder,
  title,
  canCreate,
  isDesktop,
  isTabletView,
  personal,
  onClickFolder,
  navigationItems,
  getContextOptionsPlus,
  getContextOptionsFolder,
  onBackToParentFolder,
  isRecycleBinFolder,
  isEmptyFilesList,
  clearTrash,
  showFolderInfo,
  isCurrentFolderInfo,
  toggleInfoPanel,
  isInfoPanelVisible,
  ...rest
}) => {
  const [isOpen, setIsOpen] = React.useState(false);
  const [firstClick, setFirstClick] = React.useState(true);
  const [dropBoxWidth, setDropBoxWidth] = React.useState(0);
  const [maxHeight, setMaxHeight] = React.useState(false);

  const dropBoxRef = React.useRef(null);
  const containerRef = React.useRef(null);

  const onMissClick = (e) => {
    e.preventDefault;
    const path = e.path || (e.composedPath && e.composedPath());

    if (!firstClick) {
      !path.includes(dropBoxRef.current) ? toggleDropBox() : null;
    } else {
      setFirstClick((prev) => !prev);
    }
  };

  const onClickAvailable = React.useCallback(
    (id) => {
      onClickFolder && onClickFolder(id);
      toggleDropBox();
    },
    [onClickFolder, toggleDropBox]
  );

  const toggleDropBox = () => {
    if (isRootFolder) return setIsOpen(false);
    setIsOpen((prev) => !prev);

    setDropBoxWidth(DomHelpers.getOuterWidth(containerRef.current));

    const { top } = DomHelpers.getOffset(containerRef.current);

    setMaxHeight(`calc(100vh - ${top}px)`);

    setFirstClick(true);
  };

  const onResize = React.useCallback(() => {
    setDropBoxWidth(DomHelpers.getOuterWidth(containerRef.current));
  }, [containerRef.current]);

  React.useEffect(() => {
    if (isOpen) {
      window.addEventListener("click", onMissClick);
      window.addEventListener("resize", onResize);
    } else {
      window.removeEventListener("click", onMissClick);
      window.addEventListener("resize", onResize);
      setFirstClick(true);
    }

    return () => {
      window.removeEventListener("click", onMissClick);
      window.addEventListener("resize", onResize);
    };
  }, [isOpen, onResize, onMissClick]);

  const onBackToParentFolderAction = React.useCallback(() => {
    setIsOpen((val) => !val);
    onBackToParentFolder && onBackToParentFolder();
  }, [onBackToParentFolder]);

  return (
    <Consumer>
      {(context) => (
        <>
          {isOpen && (
            <>
              {isMobileOnly && (
                <Backdrop
                  isAside={true}
                  visible={isOpen}
                  withBackground={true}
                  zIndex={400}
                />
              )}
              <DropBox
                {...rest}
                ref={dropBoxRef}
                maxHeight={maxHeight}
                dropBoxWidth={dropBoxWidth}
                sectionHeight={context.sectionHeight}
                showText={showText}
                isRootFolder={isRootFolder}
                onBackToParentFolder={onBackToParentFolderAction}
                title={title}
                personal={personal}
                canCreate={canCreate}
                navigationItems={navigationItems}
                getContextOptionsFolder={getContextOptionsFolder}
                getContextOptionsPlus={getContextOptionsPlus}
                toggleDropBox={toggleDropBox}
                toggleInfoPanel={toggleInfoPanel}
                isInfoPanelVisible={isInfoPanelVisible}
                onClickAvailable={onClickAvailable}
              />
            </>
          )}
          <StyledContainer
            ref={containerRef}
            width={context.sectionWidth}
            isRootFolder={isRootFolder}
            canCreate={canCreate}
            title={title}
            isDesktop={isDesktop}
            isTabletView={isTabletView}
            isRecycleBinFolder={isRecycleBinFolder}
          >
            <ArrowButton
              isRootFolder={isRootFolder}
              onBackToParentFolder={onBackToParentFolder}
            />
            <Text
              title={title}
              isOpen={false}
              isRootFolder={isRootFolder}
              onClick={toggleDropBox}
            />
            <ControlButtons
              personal={personal}
              isRootFolder={isRootFolder}
              canCreate={canCreate}
              getContextOptionsFolder={getContextOptionsFolder}
              getContextOptionsPlus={getContextOptionsPlus}
              isRecycleBinFolder={isRecycleBinFolder}
              isEmptyFilesList={isEmptyFilesList}
              clearTrash={clearTrash}
              toggleInfoPanel={toggleInfoPanel}
              isInfoPanelVisible={isInfoPanelVisible}
            />
          </StyledContainer>
        </>
      )}
    </Consumer>
  );
};

Navigation.propTypes = {
  tReady: PropTypes.bool,
  isRootFolder: PropTypes.bool,
  title: PropTypes.string,
  canCreate: PropTypes.bool,
  isDesktop: PropTypes.bool,
  isTabletView: PropTypes.bool,
  personal: PropTypes.bool,
  onClickFolder: PropTypes.func,
  navigationItems: PropTypes.arrayOf(PropTypes.object),
  getContextOptionsPlus: PropTypes.func,
  getContextOptionsFolder: PropTypes.func,
  onBackToParentFolder: PropTypes.func,
};

export default React.memo(Navigation);