# HTML Slide Editor

슬라이드/페이지 단위로 구성된 **HTML 보고서를 페이지별로 편집**하는 Windows 데스크톱 에디터입니다.
오른쪽에 보고서가 실제 모양 그대로 렌더링되고, 화면의 글자를 직접 클릭해 수정(WYSIWYG)할 수 있습니다.

설치형 런타임 없이 동작하는 **단일 실행 파일(EXE)** 로 빌드됩니다. (Windows에 기본 포함된 WebView2/Edge 엔진 사용)

## 주요 기능

- **페이지 단위 편집** — 각 페이지(`.slide` / `.slide-wrap` / `.deck` 직속 요소)를 한 장씩 표시하고 이동·추가·복제·삭제·순서변경
- **WYSIWYG 편집** — 렌더링된 화면에서 글자를 직접 클릭해 수정 (굵게·기울임·밑줄·취소선, 글자색·형광펜, 목록, 정렬, 링크, 서식지우기, 실행취소)
- **표 편집** — 표 삽입(행×열), 행 추가(위/아래)·삭제, 열 추가(왼/오)·삭제, 셀 배경색, 표 전체 삭제 (커서를 표 안 셀에 둔 채 사용)
- **이미지 편집** — 커서 위치에 이미지 삽입, 선택한 이미지 교체(클릭 후 교체)·크기 조절(크게/작게)·삭제 (이미지는 클릭하면 빨간 테두리로 선택됨)
- **소스 편집** — 페이지별 HTML 코드를 직접 수정하는 모드 전환
- **원본 디자인 보존** — 보고서의 `<style>` 과 내장 이미지를 그대로 유지한 채 편집 → 저장 시 동일한 모양
- **편집본 저장** — `원본이름_편집본.html` 로 내려받기 (원본 보존), `Ctrl+S` 지원
- **전체 미리보기** — 편집한 전체 보고서를 새 창에서 확인

## 지원하는 보고서 형식

각 페이지가 다음 중 하나로 구성된 슬라이드형 HTML이면 동작합니다.

- `.slide-wrap` (권장)
- `section.slide` / `div.slide`
- 또는 `.deck` 컨테이너의 직속 자식 요소

페이지 폭은 1280px 기준으로 디자인된 덱(예: 1280×720)에 최적화되어 있으며, 창 크기에 맞춰 자동 축소 표시됩니다.

## 사용법 (배포된 EXE)

1. `HtmlSlideEditor.exe` 더블클릭 — 설치 불필요, 동반 파일 불필요
2. 편집할 HTML 파일을 창으로 드래그하거나 `📂 HTML 파일 열기` 로 선택
3. 오른쪽 화면에서 수정, 왼쪽에서 페이지 이동/관리
4. `💾 편집본 저장` (또는 `Ctrl+S`)

> 처음 실행 시 `%LOCALAPPDATA%\HtmlSlideEditor` 폴더가 생성됩니다(WebView2 작업 데이터).
> 미서명 실행 파일이라 SmartScreen 경고가 뜨면 **추가 정보 → 실행** 을 누르세요.

## 직접 빌드하기

**요구사항**
- Windows 10/11
- .NET Framework 4.x (Windows 기본 포함 — `csc.exe` 사용)
- WebView2 런타임 (Windows 11 기본 포함, Win10은 [여기](https://developer.microsoft.com/microsoft-edge/webview2/)서 설치)

**빌드**
```powershell
.\build.ps1
```
스크립트가 NuGet에서 WebView2 SDK를 받아 `editor.html` 과 DLL을 EXE에 내장해 `HtmlSlideEditor.exe` 를 생성합니다.

## 구성

| 파일 | 설명 |
|---|---|
| `editor.html` | 에디터 앱 본체(단독 HTML로도 브라우저에서 열어 사용 가능) |
| `src/App.cs` | EXE 셸 (WinForms + WebView2, DLL/HTML을 리소스로 내장·자동 추출) |
| `build.ps1` | WebView2 SDK 다운로드 + `csc.exe` 컴파일 스크립트 |
| `HtmlSlideEditor.exe` | 빌드 산출물(단일 실행 파일) |

> `editor.html` 만 따로 웹브라우저(Chrome/Edge)로 열어도 동일하게 동작합니다. EXE는 전용 앱 창으로 띄워주는 셸입니다.

## 라이선스

[MIT](LICENSE)
