import React from "react";
import { withTranslation } from "react-i18next";
import { inject, observer } from "mobx-react";
import SelectFolderInput from "files/SelectFolderInput";
import SelectFolderDialog from "files/SelectFolderDialog";
import ScheduleComponent from "./scheduleComponent";

import SaveCancelButtons from "@appserver/components/save-cancel-buttons";
import {
  createBackupSchedule,
  getBackupSchedule,
} from "@appserver/common/api/portal";
import toastr from "@appserver/components/toast/toastr";
import TextInput from "@appserver/components/text-input";
import Box from "@appserver/components/box";
import Link from "@appserver/components/link";
import { saveToSessionStorage, getFromSessionStorage } from "../.././../utils";
import { StyledComponent } from "../styled-backup";

let numberPeriodFromSessionStorage = null;
let dayFromSessionStorage = "";
let timeFromSessionStorage = "";
let maxCopiesFromSessionStorage = "";

const settingNames = ["day", "time", "maxCopies", "numberPeriod"];
class ThirdPartyModule extends React.Component {
  constructor(props) {
    super(props);
    const {
      t,
      selectedWeekdayOption,
      defaultSelectedOption,
      defaultDay,
      defaultHour,
      defaultMaxCopies,
      monthlySchedule,
      weeklySchedule,
      dailySchedule,
      weekOptions,
      periodOptions,
    } = this.props;

    dayFromSessionStorage = getFromSessionStorage("day");
    timeFromSessionStorage = getFromSessionStorage("time");
    maxCopiesFromSessionStorage = getFromSessionStorage("maxCopies");

    numberPeriodFromSessionStorage = getFromSessionStorage("numberPeriod");
    const weekName =
      (numberPeriodFromSessionStorage === 2 && dayFromSessionStorage) ||
      (weeklySchedule && dayFromSessionStorage)
        ? weekOptions[+dayFromSessionStorage - 1].label
        : "";
    const periodName = numberPeriodFromSessionStorage
      ? periodOptions[+numberPeriodFromSessionStorage - 1].label
      : "";

    const numberMaxCopies = maxCopiesFromSessionStorage
      ? maxCopiesFromSessionStorage.substring(
          0,
          maxCopiesFromSessionStorage.indexOf(" ")
        )
      : "";

    const monthNumber = monthlySchedule
      ? dayFromSessionStorage || `${defaultDay}`
      : dayFromSessionStorage || "1";

    const weekdayNumber = weeklySchedule
      ? dayFromSessionStorage || defaultDay
      : dayFromSessionStorage || "2";

    const weekdayOption = numberPeriodFromSessionStorage
      ? numberPeriodFromSessionStorage === 2
      : weeklySchedule || false;
    const dayOption = numberPeriodFromSessionStorage
      ? numberPeriodFromSessionStorage === 1
      : dailySchedule || false;

    const monthOption = numberPeriodFromSessionStorage
      ? numberPeriodFromSessionStorage === 3
      : monthlySchedule || false;

    this.state = {
      isPanelVisible: false,
      isChanged: false,
      selectedFolder: "",
      isError: false,
      isLoading: false,

      monthlySchedule: monthOption,
      dailySchedule: dayOption,
      weeklySchedule: weekdayOption,

      selectedOption:
        periodName || defaultSelectedOption || t("DailyPeriodSchedule"),
      selectedWeekdayOption: weekName || selectedWeekdayOption,
      selectedNumberWeekdayOption: weekdayNumber,
      selectedTimeOption: timeFromSessionStorage || defaultHour || "12:00",
      selectedMonthOption: monthNumber,
      selectedMaxCopies:
        maxCopiesFromSessionStorage || defaultMaxCopies || "10",
      selectedNumberMaxCopies: numberMaxCopies || defaultMaxCopies || "10",

      isSetDefaultFolderPath: false,
    };
    this._isMounted = false;
  }

  componentDidMount() {
    this._isMounted = true;

    const {
      defaultStorageType,
      defaultSelectedFolder,
      onSetLoadingData,
    } = this.props;
    onSetLoadingData && onSetLoadingData(true);
    this.checkChanges();
    //debugger;
    this.setState({ isLoading: true }, function () {
      +defaultStorageType === 1
        ? SelectFolderDialog.getFolderPath(defaultSelectedFolder)
            .then(
              (folderPath) => (this.folderThirdPartyModulePath = folderPath)
            )
            .then(
              () =>
                this._isMounted &&
                this.setState({
                  selectedFolder: defaultSelectedFolder,
                })
            )
            .finally(
              () =>
                this.setState({
                  isLoading: false,
                }) &&
                onSetLoadingData &&
                onSetLoadingData(false)
            )
        : this.setState({
            isLoading: false,
          }) &&
          onSetLoadingData &&
          onSetLoadingData(false);
    });
  }

  componentWillUnmount() {
    this._isMounted = false;
  }
  componentDidUpdate(prevState) {
    const { isChanged, isSetDefaultFolderPath } = this.state;

    if (isChanged !== prevState.isChanged && isSetDefaultFolderPath) {
      this.setState({
        isSetDefaultFolderPath: false,
      });
    }
  }
  onClickInput = () => {
    this.setState({
      isPanelVisible: true,
    });
  };

  onClose = () => {
    this.setState({
      isPanelVisible: false,
    });
  };
  onSelectPeriod = (options) => {
    console.log("options", options);

    const key = options.key;
    const label = options.label;
    //debugger;

    saveToSessionStorage("numberPeriod", key);

    this.setState({ selectedOption: label });
    key === 1
      ? this.setState(
          {
            weeklySchedule: false,
            monthlySchedule: false,
            dailySchedule: true,
          },
          function () {
            this.checkChanges();
          }
        )
      : key === 2
      ? this.setState(
          {
            weeklySchedule: true,
            monthlySchedule: false,
            dailySchedule: false,
          },
          function () {
            this.checkChanges();
          }
        )
      : this.setState(
          {
            monthlySchedule: true,
            weeklySchedule: false,
            dailySchedule: false,
          },
          function () {
            this.checkChanges();
          }
        );
  };

  onSelectWeekDay = (options) => {
    console.log("options", options);

    const key = options.key;
    const label = options.label;
    //debugger;
    saveToSessionStorage("day", key);

    this.setState(
      {
        selectedNumberWeekdayOption: key,
        selectedWeekdayOption: label,
      },
      function () {
        this.checkChanges();
      }
    );
  };
  onSelectMonthNumberAndTimeOptions = (options) => {
    const key = options.key;
    const label = options.label;

    if (key <= 24) {
      saveToSessionStorage("time", label);
      this.setState({ selectedTimeOption: label }, function () {
        this.checkChanges();
      });
    } else {
      saveToSessionStorage("day", label);
      this.setState(
        {
          selectedMonthOption: label,
        },
        function () {
          this.checkChanges();
        }
      );
    }
  };

  onSelectMaxCopies = (options) => {
    const key = options.key;
    const label = options.label;

    saveToSessionStorage("maxCopies", label);

    this.setState(
      {
        selectedNumberMaxCopies: key,
        selectedMaxCopies: label,
      },
      function () {
        this.checkChanges();
      }
    );
  };

  onSelectFolder = (folderId) => {
    console.log("folderId", folderId);
    this._isMounted &&
      this.setState({
        selectedFolder: folderId,
        isChanged: true,
      });
  };

  checkChanges = () => {
    const { defaultStorageType } = this.props;
    const { isChanged } = this.state;
    //debugger;
    let changed;
    if (+defaultStorageType === 1) {
      changed = this.checkOptions();
      isChanged !== changed &&
        this.setState({
          isChanged: changed,
        });
      return;
    } else {
      isChanged !== changed &&
        this.setState({
          isChanged: true,
        });
      return;
    }
  };
  checkOptions = () => {
    const {
      selectedTimeOption,
      dailySchedule,
      monthlySchedule,
      weeklySchedule,
      selectedMonthOption,
      selectedNumberMaxCopies,
      selectedNumberWeekdayOption,
    } = this.state;

    const {
      defaultHour,
      defaultMaxCopies,
      defaultPeriod,
      defaultDay,
      defaultSelectedWeekdayOption,
    } = this.props;
    //debugger;
    if (selectedTimeOption !== defaultHour) {
      return true;
    }
    if (+selectedNumberMaxCopies !== +defaultMaxCopies) {
      return true;
    }
    if (+defaultPeriod === 0 && (monthlySchedule || weeklySchedule)) {
      return true;
    }

    if (+defaultPeriod === 1 && (monthlySchedule || dailySchedule)) {
      return true;
    }
    if (+defaultPeriod === 2 && (weeklySchedule || dailySchedule)) {
      return true;
    }
    if (monthlySchedule) {
      if (+selectedMonthOption !== defaultDay) {
        return true;
      }
    }

    if (weeklySchedule) {
      if (+selectedNumberWeekdayOption !== defaultSelectedWeekdayOption) {
        return true;
      }
    }

    return false;
  };

  onSaveModuleSettings = () => {
    const {
      selectedFolder,
      weeklySchedule,
      selectedTimeOption,
      monthlySchedule,
      selectedMonthOption,
      selectedNumberWeekdayOption,
      selectedNumberMaxCopies,
      isError,
    } = this.state;

    const {
      t,
      changedDefaultOptions,
      onSetDefaultOptions,
      onSetLoadingData,
    } = this.props;

    let storageParams = [];

    if (!selectedFolder) {
      this.setState({
        isError: true,
      });
      return;
    }
    onSetLoadingData && onSetLoadingData(true);
    settingNames.forEach((settingName) => {
      saveToSessionStorage(settingName, "");
    });
    this.setState({ isLoadingData: true }, function () {
      let period = weeklySchedule ? "1" : monthlySchedule ? "2" : "0";
      let defaultSelectedFolder;
      let day = weeklySchedule
        ? `${selectedNumberWeekdayOption}`
        : monthlySchedule
        ? `${selectedMonthOption}`
        : null;

      let time = selectedTimeOption.substring(
        0,
        selectedTimeOption.indexOf(":")
      );
      let storageType = "1";

      storageParams = [
        {
          key: "folderId",
          value: selectedFolder,
        },
      ];

      let folderId;
      createBackupSchedule(
        storageType,
        storageParams,
        selectedNumberMaxCopies,
        period,
        time,
        day
      )
        .then(() => getBackupSchedule())
        .then((selectedSchedule) => {
          if (selectedSchedule) {
            folderId = selectedSchedule.storageParams.folderId;

            defaultSelectedFolder = folderId;
            const defaultStorageType = `${selectedSchedule.storageType}`;
            const defaultHour = `${selectedSchedule.cronParams.hour}:00`;
            const defaultPeriod = `${selectedSchedule.cronParams.period}`;
            const defaultDay = selectedSchedule.cronParams.day;
            const defaultMaxCopies = `${selectedSchedule.backupsStored}`;

            changedDefaultOptions(
              defaultSelectedFolder,
              defaultStorageType,
              defaultHour,
              defaultPeriod,
              defaultDay,
              defaultMaxCopies
            );

            this.onSelectFolder(`${folderId}`);
          }
        })
        .then(() => SelectFolderDialog.getFolderPath(folderId))
        .then((folderPath) => {
          this.folderThirdPartyModulePath = folderPath;
        })
        .then(() => {
          this._isMounted && onSetDefaultOptions();
        })
        .then(() => toastr.success(t("SuccessfullySaveSettingsMessage")))
        .catch((error) => console.log("error", error))
        .finally(() => {
          this._isMounted && onSetLoadingData && onSetLoadingData(false);
          if (isError && this._isMounted) this.setState({ isError: false });
          this._isMounted &&
            this.setState({
              isLoadingData: false,
              isChanged: false,
              selectedFolder: defaultSelectedFolder,
            });
        });
    });
  };

  onCancelModuleSettings = () => {
    const {
      onCancelModuleSettings,
      periodOptions,
      weekdaysOptions,
      defaultPeriod,
      defaultHour,
      defaultMaxCopies,
      defaultDay,
      lng,
      defaultWeekly,
      defaultDaily,
      defaultMonthly,
      defaultStorageType,
      onSetLoadingData,
    } = this.props;
    const {
      monthlySchedule,
      weeklySchedule,

      isError,
    } = this.state;

    onSetLoadingData && onSetLoadingData(true);

    settingNames.forEach((settingName) => {
      saveToSessionStorage(settingName, "");
    });

    if (defaultStorageType) {
      this.setState({
        isSetDefaultFolderPath: true,
        selectedTimeOption: defaultHour,
        selectedMaxCopies: defaultMaxCopies,
        selectedNumberMaxCopies: defaultMaxCopies,
      });

      if (
        monthlySchedule === defaultWeekly ||
        monthlySchedule === defaultDaily
      ) {
        this.setState({
          monthlySchedule: false,
        });
      }

      if (
        weeklySchedule === defaultMonthly ||
        weeklySchedule === defaultDaily
      ) {
        this.setState({
          weeklySchedule: false,
        });
      }
      let defaultSelectedOption;
      let defaultSelectedWeekdayOption;

      if (+defaultPeriod === 1) {
        //Every Week option
        //debugger;
        const arrayIndex = lng === "en" ? defaultDay : defaultDay - 1; //selected number of week
        defaultSelectedOption = periodOptions[1].label;
        defaultSelectedWeekdayOption = defaultDay;
        // defaultWeekly = true;
        this.setState({
          selectedOption: defaultSelectedOption,
          weeklySchedule: true,
          selectedWeekdayOption: weekdaysOptions[arrayIndex].label,
          selectedNumberWeekdayOption: defaultDay,
        });
      } else {
        if (+defaultPeriod === 2) {
          //Every Month option
          defaultSelectedOption = periodOptions[2].label;
          this.setState({
            selectedOption: defaultSelectedOption,
            monthlySchedule: true,
            selectedMonthOption: `${defaultDay}`, //selected day of month
          });
        } else {
          defaultSelectedOption = periodOptions[0].label;
          this.setState({
            selectedOption: defaultSelectedOption,
          });
        }
      }
    }

    if (isError) this.setState({ isError: false });
    this.setState({ isChanged: false });
    onCancelModuleSettings();
    onSetLoadingData && onSetLoadingData(false);
  };

  render() {
    const {
      isChanged,
      isPanelVisible,
      selectedFolder,
      selectedWeekdayOption,
      selectedTimeOption,
      selectedMonthOption,
      selectedMaxCopies,
      selectedOption,
      isError,
      isSetDefaultFolderPath,
      weeklySchedule,
      monthlySchedule,
      isLoading,
    } = this.state;
    const {
      isLoadingData,
      weekOptions,
      periodOptions,
      monthNumberOptionsArray,
      timeOptionsArray,
      maxNumberCopiesArray,
      isCopyingToLocal,
      t,
      onSetLoadingData,
      helpUrlCreatingBackup,
    } = this.props;
    //console.log("selectedFolder THRDPARTY", selectedFolder);
    // console.log("third-party module render", this.folderThirdPartyModulePath);
    //console.log("selectedWeekdayOption", selectedWeekdayOption);
    return (
      <div className="category-item-wrapper">
        <>
          <Box marginProp="16px 0 16px 0">
            <Link
              color="#316DAA"
              target="_blank"
              isHovered={true}
              href={helpUrlCreatingBackup}
            >
              {t("Common:LearnMore")}
            </Link>
          </Box>

          {!isLoading ? (
            <SelectFolderInput
              onSelectFolder={this.onSelectFolder}
              name={"thirdParty"}
              onClose={this.onClose}
              onClickInput={this.onClickInput}
              isPanelVisible={isPanelVisible}
              folderPath={this.folderThirdPartyModulePath}
              isSetDefaultFolderPath={isSetDefaultFolderPath}
              isError={isError}
              onSetLoadingData={onSetLoadingData}
              foldersType="third-party"
              isSavingProcess={isLoadingData}
            />
          ) : (
            <StyledComponent>
              <TextInput className="input-with-folder-path" isDisabled={true} />
            </StyledComponent>
          )}
          <ScheduleComponent
            weeklySchedule={weeklySchedule}
            monthlySchedule={monthlySchedule}
            weekOptions={weekOptions}
            selectedOption={selectedOption}
            selectedWeekdayOption={selectedWeekdayOption}
            selectedTimeOption={selectedTimeOption}
            selectedMonthOption={selectedMonthOption}
            selectedMaxCopies={selectedMaxCopies}
            isLoadingData={isLoadingData}
            periodOptions={periodOptions}
            monthNumberOptionsArray={monthNumberOptionsArray}
            timeOptionsArray={timeOptionsArray}
            maxNumberCopiesArray={maxNumberCopiesArray}
            //onClickCheckbox={onClickCheckbox}
            onSelectMaxCopies={this.onSelectMaxCopies}
            onSelectMonthNumberAndTimeOptions={
              this.onSelectMonthNumberAndTimeOptions
            }
            onSelectWeekDay={this.onSelectWeekDay}
            onSelectPeriod={this.onSelectPeriod}
          />
          {isChanged && (
            <SaveCancelButtons
              className="THIRD-PARTY team-template_buttons"
              onSaveClick={this.onSaveModuleSettings}
              onCancelClick={this.onCancelModuleSettings}
              showReminder={false}
              reminderTest={t("YouHaveUnsavedChanges")}
              saveButtonLabel={t("Common:SaveButton")}
              cancelButtonLabel={t("Common:CancelButton")}
              isDisabled={isCopyingToLocal || isLoadingData}
            />
          )}
        </>
      </div>
    );
  }
}
export default inject(({ auth }) => {
  const { helpUrlCreatingBackup } = auth.settingsStore;
  return {
    helpUrlCreatingBackup,
  };
})(withTranslation(["Settings", "Common"])(observer(ThirdPartyModule)));
