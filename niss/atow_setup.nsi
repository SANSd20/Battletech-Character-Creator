!ifndef VERSION
  !define VERSION "0.1.10-preview"
!endif

!ifndef PUBLISH_DIR
  !define PUBLISH_DIR "..\artifacts\publish\win-x64"
!endif

!define APP_NAME "A Time of War Character Creator"
!define INSTALL_NAME "A Time of War Character Creator Beta"
!define APP_EXE "BattletechCharacterCreator.App.exe"
!define REG_KEY "Software\A Time of War Character Creator Beta"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\A Time of War Character Creator Beta"

Name "${APP_NAME} ${VERSION}"
OutFile "atow-character-creator-${VERSION}-setup.exe"
Icon "..\ico1.ico"
UninstallIcon "..\ico1.ico"
InstallDir "$LOCALAPPDATA\${INSTALL_NAME}"
RequestExecutionLevel user

Page directory
Page instfiles

UninstPage uninstConfirm
UninstPage instfiles

Section "${APP_NAME}" Required
  SectionIn RO

  SetOutPath "$INSTDIR"
  File /r "${PUBLISH_DIR}\*.*"

  WriteRegStr HKCU "${REG_KEY}" "Install_Dir" "$INSTDIR"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayName" "${INSTALL_NAME}"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "DisplayVersion" "${VERSION}"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "Publisher" "SANSd20"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr HKCU "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKCU "${UNINSTALL_KEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTALL_KEY}" "NoRepair" 1

  CreateDirectory "$SMPROGRAMS\${INSTALL_NAME}"
  CreateShortCut "$SMPROGRAMS\${INSTALL_NAME}\A Time of War Character Creator Beta.lnk" "$INSTDIR\${APP_EXE}"
  CreateShortCut "$SMPROGRAMS\${INSTALL_NAME}\Uninstall.lnk" "$INSTDIR\uninstall.exe"

  WriteUninstaller "$INSTDIR\uninstall.exe"
SectionEnd

Section "Uninstall"
  SetOutPath "$TEMP"

  Delete "$SMPROGRAMS\${INSTALL_NAME}\A Time of War Character Creator Beta.lnk"
  Delete "$SMPROGRAMS\${INSTALL_NAME}\Uninstall.lnk"
  RMDir "$SMPROGRAMS\${INSTALL_NAME}"

  Delete "$INSTDIR\${APP_EXE}"
  RMDir /r "$INSTDIR"

  DeleteRegKey HKCU "${UNINSTALL_KEY}"
  DeleteRegKey HKCU "${REG_KEY}"
SectionEnd
