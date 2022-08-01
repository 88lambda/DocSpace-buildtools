import React from "react";

import { FileAction } from "@appserver/common/constants";
import { Events } from "../../helpers/constants";

import CreateEvent from "./CreateEvent";
import RenameEvent from "./RenameEvent";
import CreateRoomEvent from "./CreateRoomEvent";
import EditRoomEvent from "./EditRoomEvent";

const GlobalEvents = () => {
  const [createDialogProps, setCreateDialogProps] = React.useState({
    visible: false,
    id: null,
    type: null,
    extension: null,
    title: "",
    templateId: null,
    fromTemplate: null,
    onClose: null,
  });

  const [renameDialogProps, setRenameDialogProps] = React.useState({
    visible: false,
    item: null,
    onClose: null,
  });

  const [createRoomDialogProps, setCreateRoomDialogProps] = React.useState({
    visible: false,
    onClose: null,
  });

  const [editRoomDialogProps, setEditRoomDialogProps] = React.useState({
    visible: false,
    item: null,
    onClose: null,
  });

  const onCreate = React.useCallback((e) => {
    const { payload } = e;

    const visible = payload.id ? true : false;

    setCreateDialogProps({
      visible: visible,
      id: payload.id,
      type: FileAction.Create,
      extension: payload.extension,
      title: payload.title || null,
      templateId: payload.templateId || null,
      fromTemplate: payload.fromTemplate || null,
      onClose: () => {
        setCreateDialogProps({
          visible: false,
          id: null,
          type: null,
          extension: null,
          title: "",
          templateId: null,
          fromTemplate: null,
          onClose: null,
        });
      },
    });
  }, []);

  const onRename = React.useCallback((e) => {
    const visible = e.item ? true : false;

    setRenameDialogProps({
      visible: visible,
      type: FileAction.Rename,
      item: e.item,
      onClose: () => {
        setRenameDialogProps({
          visible: false,
          type: null,
          item: null,
        });
      },
    });
  }, []);

  const onCreateRoom = React.useCallback((e) => {
    setCreateRoomDialogProps({
      visible: true,
      onClose: () =>
        setCreateRoomDialogProps({ visible: false, onClose: null }),
    });
  }, []);

  const onEditRoom = React.useCallback((e) => {
    const visible = e.item ? true : false;

    setEditRoomDialogProps({
      visible: visible,
      item: e.item,
      onClose: () => {
        setEditRoomDialogProps({
          visible: false,
          item: null,
          onClose: null,
        });
      },
    });
  }, []);

  React.useEffect(() => {
    window.addEventListener(Events.CREATE, onCreate);
    window.addEventListener(Events.RENAME, onRename);
    window.addEventListener(Events.ROOM_CREATE, onCreateRoom);
    window.addEventListener(Events.ROOM_EDIT, onEditRoom);

    return () => {
      window.removeEventListener(Events.CREATE, onCreate);
      window.removeEventListener(Events.RENAME, onRename);
      window.removeEventListener(Events.ROOM_CREATE, onCreateRoom);
      window.removeEventListener(Events.ROOM_EDIT, onEditRoom);
    };
  }, [onRename, onCreate, onCreateRoom, onEditRoom]);

  return [
    createDialogProps.visible && (
      <CreateEvent key={Events.CREATE} {...createDialogProps} />
    ),
    renameDialogProps.visible && (
      <RenameEvent key={Events.RENAME} {...renameDialogProps} />
    ),
    createRoomDialogProps.visible && (
      <CreateRoomEvent key={Events.ROOM_CREATE} {...createRoomDialogProps} />
    ),
    editRoomDialogProps.visible && (
      <EditRoomEvent key={Events.ROOM_EDIT} {...editRoomDialogProps} />
    ),
  ];
};

export default React.memo(GlobalEvents);
