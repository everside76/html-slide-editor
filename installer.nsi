; HTML Slide Editor - Windows 설치 프로그램 (NSIS)
; 사용자 단위 설치(관리자 권한 불필요) + 시작메뉴/바탕화면 바로가기 + 제거 지원

Unicode true

!define APPID       "HtmlSlideEditor"
!define APPNAME     "HTML Slide Editor"
!define APPNAMEKR   "HTML 보고서 에디터"
!define COMPANY     "everside76"
!define VERSION     "1.4.0"
!define EXENAME     "HtmlSlideEditor.exe"
!define UNINSTKEY   "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APPID}"

Name "${APPNAMEKR}"
OutFile "HtmlSlideEditor-Setup-${VERSION}.exe"
RequestExecutionLevel user
InstallDir "$LOCALAPPDATA\Programs\${APPID}"
InstallDirRegKey HKCU "Software\${APPID}" "InstallDir"
SetCompressor /SOLID lzma
BrandingText "${APPNAMEKR} v${VERSION}"

VIProductVersion "1.4.0.0"
VIAddVersionKey "ProductName"     "${APPNAME}"
VIAddVersionKey "ProductVersion"  "${VERSION}"
VIAddVersionKey "FileVersion"     "1.4.0.0"
VIAddVersionKey "CompanyName"     "${COMPANY}"
VIAddVersionKey "LegalCopyright"  "MIT License"
VIAddVersionKey "FileDescription" "${APPNAME} Setup"

!define MUI_ICON   "icon.ico"
!define MUI_UNICON "icon.ico"
!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_RUN "$INSTDIR\${EXENAME}"
!define MUI_FINISHPAGE_RUN_TEXT "지금 ${APPNAMEKR} 실행"

!include "MUI2.nsh"

; 페이지 (설치)
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH
; 페이지 (제거)
!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "Korean"
!insertmacro MUI_LANGUAGE "English"

Section "Install"
  SetOutPath "$INSTDIR"
  File "HtmlSlideEditor.exe"
  File "icon.ico"
  File "CHANGELOG.md"

  ; 바로가기
  CreateDirectory "$SMPROGRAMS\${APPNAMEKR}"
  CreateShortCut "$SMPROGRAMS\${APPNAMEKR}\${APPNAMEKR}.lnk" "$INSTDIR\${EXENAME}" "" "$INSTDIR\icon.ico"
  CreateShortCut "$SMPROGRAMS\${APPNAMEKR}\${APPNAMEKR} 제거.lnk" "$INSTDIR\uninstall.exe"
  CreateShortCut "$DESKTOP\${APPNAMEKR}.lnk" "$INSTDIR\${EXENAME}" "" "$INSTDIR\icon.ico"

  ; 제거 프로그램 + 레지스트리 등록
  WriteUninstaller "$INSTDIR\uninstall.exe"
  WriteRegStr   HKCU "Software\${APPID}" "InstallDir" "$INSTDIR"
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayName"     "${APPNAMEKR}"
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayVersion"  "${VERSION}"
  WriteRegStr   HKCU "${UNINSTKEY}" "Publisher"       "${COMPANY}"
  WriteRegStr   HKCU "${UNINSTKEY}" "DisplayIcon"     "$INSTDIR\icon.ico"
  WriteRegStr   HKCU "${UNINSTKEY}" "InstallLocation" "$INSTDIR"
  WriteRegStr   HKCU "${UNINSTKEY}" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegStr   HKCU "${UNINSTKEY}" "QuietUninstallString" '"$INSTDIR\uninstall.exe" /S'
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoModify" 1
  WriteRegDWORD HKCU "${UNINSTKEY}" "NoRepair" 1
SectionEnd

Section "Uninstall"
  Delete "$INSTDIR\${EXENAME}"
  Delete "$INSTDIR\icon.ico"
  Delete "$INSTDIR\CHANGELOG.md"
  Delete "$INSTDIR\uninstall.exe"
  RMDir  "$INSTDIR"

  Delete "$SMPROGRAMS\${APPNAMEKR}\${APPNAMEKR}.lnk"
  Delete "$SMPROGRAMS\${APPNAMEKR}\${APPNAMEKR} 제거.lnk"
  RMDir  "$SMPROGRAMS\${APPNAMEKR}"
  Delete "$DESKTOP\${APPNAMEKR}.lnk"

  DeleteRegKey HKCU "${UNINSTKEY}"
  DeleteRegKey HKCU "Software\${APPID}"
SectionEnd
