import React from "react";
import PropTypes from "prop-types";
import Section from "@docspace/common/components/Section";
import toastr from "@docspace/components/toast/toastr";

import {
  SectionHeaderContent,
  SectionBodyContent,
  SectionFooterContent,
} from "./Section";

import Dialogs from "../Home/Section/AccountsBody/Dialogs";

import withCultureNames from "@docspace/common/hoc/withCultureNames";
import { inject, observer } from "mobx-react";
import { withTranslation } from "react-i18next";

class Profile extends React.Component {
  componentDidMount() {
    const {
      fetchProfile,
      profile,
      t,
      setDocumentTitle,
      setFirstLoad,
      setIsLoading,
      setIsEditTargetUser,
      setLoadedProfile,
      isVisitor,
      selectedTreeNode,
      setSelectedNode,
      setFirstLoadGlobal,
    } = this.props;
    const userId = "@self";

    setFirstLoad(false);
    setFirstLoadGlobal(false);
    setIsEditTargetUser(false);

    isVisitor
      ? !selectedTreeNode.length && setSelectedNode(["@rooms"])
      : setSelectedNode(["accounts"]);

    setDocumentTitle(t("Common:Profile"));
    this.documentElement = document.getElementsByClassName("hidingHeader");
    // const queryString = ((location && location.search) || "").slice(1);
    // const queryParams = queryString.split("&");
    // const arrayOfQueryParams = queryParams.map((queryParam) =>
    //   queryParam.split("=")
    // );
    // const linkParams = Object.fromEntries(arrayOfQueryParams);

    // if (linkParams.email_change && linkParams.email_change === "success") {
    //   toastr.success(t("ChangeEmailSuccess"));
    // }
    if (!profile || profile.userName !== userId) {
      setIsLoading(true);
      setLoadedProfile(false);
      fetchProfile(userId).finally(() => {
        setIsLoading(false);
        setLoadedProfile(true);
      });
    }

    if (!profile && this.documentElement) {
      for (var i = 0; i < this.documentElement.length; i++) {
        this.documentElement[i].style.transition = "none";
      }
    }
  }

  componentDidUpdate(prevProps) {
    const { fetchProfile, profile, setIsLoading } = this.props;
    // const { userId } = match.params;
    // const prevUserId = prevProps.match.params.userId;

    // if (userId !== undefined && userId !== prevUserId) {
    //   setIsLoading(true);
    //   fetchProfile(userId).finally(() => setIsLoading(false));
    // }

    if (profile && this.documentElement) {
      for (var i = 0; i < this.documentElement.length; i++) {
        this.documentElement[i].style.transition = "";
      }
    }
  }

  render() {
    // console.log("Profile render");

    const { profile, showCatalog } = this.props;

    return (
      <>
        <Section withBodyAutoFocus viewAs="profile">
          <Section.SectionHeader>
            <SectionHeaderContent profile={profile} />
          </Section.SectionHeader>

          <Section.SectionBody>
            <SectionBodyContent profile={profile} />
          </Section.SectionBody>

          <Section.SectionFooter>
            <SectionFooterContent profile={profile} />
          </Section.SectionFooter>
        </Section>
        <Dialogs />
      </>
    );
  }
}

Profile.propTypes = {
  fetchProfile: PropTypes.func.isRequired,
  profile: PropTypes.object,
  language: PropTypes.string,
};

export default inject(({ auth, peopleStore, filesStore, treeFoldersStore }) => {
  const { setDocumentTitle, language } = auth;
  const { setFirstLoad: setFirstLoadGlobal } = filesStore;
  const { targetUserStore, loadingStore } = peopleStore;
  const {
    getTargetUser: fetchProfile,
    targetUser: profile,
    isEditTargetUser,
    setIsEditTargetUser,
  } = targetUserStore;
  const { setFirstLoad, setIsLoading, setLoadedProfile } = loadingStore;
  const { selectedTreeNode, setSelectedNode } = treeFoldersStore;
  return {
    setFirstLoadGlobal,
    setDocumentTitle,
    language,
    fetchProfile,
    profile,
    setFirstLoad,
    setIsLoading,
    isEditTargetUser,
    setIsEditTargetUser,
    setLoadedProfile,
    showCatalog: auth.settingsStore.showCatalog,
    selectedTreeNode,
    setSelectedNode,
    isVisitor: auth.userStore.user.isVisitor,
  };
})(observer(withTranslation(["Profile", "Common"])(withCultureNames(Profile))));
