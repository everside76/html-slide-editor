# 변경 이력 (Changelog)

이 프로젝트의 모든 주요 변경 사항을 기록합니다.
형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.1.0/)를, 버전은 [유의적 버전(SemVer)](https://semver.org/lang/ko/)을 따릅니다.

## [1.5.0] - 2026-06-23
### Added
- **설정(⚙) 메뉴** 추가 — 편집본 저장 위치 지정
  - "지정한 폴더에 자동 저장" 또는 "저장할 때 위치 선택(저장 대화상자)"
  - 설정은 `%LOCALAPPDATA%\HtmlSlideEditor\settings.json` 에 저장
- **자동 업데이트** 추가 — GitHub Releases에서 최신 버전 확인 → 새 버전이면 설치 파일을 내려받아 자동 설치(앱 종료 후 교체)
  - 실행 시 조용히 새 버전 확인, 설정 창에서 수동 확인도 가능

## [1.4.0] - 2026-06-23
### Added
- **Windows 설치 프로그램(Setup.exe)** 추가 (NSIS 기반)
  - 시작 메뉴·바탕화면 바로가기 생성
  - Windows "앱 및 기능"에 등록, **제거(언인스톨)** 지원
  - 관리자 권한 불필요(사용자 단위 설치, `%LOCALAPPDATA%\Programs\HtmlSlideEditor`)

## [1.3.0] - 2026-06-23
### Added
- 프로그램 상단에 **버전 배지** 표시, 클릭 시 **버전 이력 모달**(앱 내 보기)
- 저장소 `CHANGELOG.md` 추가
- EXE 파일 속성 및 창 제목에 버전 정보 반영 (어셈블리 버전 1.3.0.0)

## [1.2.0] - 2026-06-23
### Added
- **표 편집**: 표 삽입(행×열), 행 추가(위/아래)·삭제, 열 추가(왼/오)·삭제, 셀 배경색, 표 전체 삭제
- **이미지 편집**: 커서 위치 삽입, 선택 이미지 교체, 크기 조절(크게/작게), 삭제
- 이미지 클릭 선택 표시(빨간 테두리), 하단 동작 안내 토스트

## [1.1.0] - 2026-06-23
### Added
- 앱 **아이콘** 추가 (겹친 슬라이드 + 빨간 헤더 + 연필), 다중 해상도 `.ico`
- EXE 파일 아이콘 및 창/작업표시줄 아이콘 적용

## [1.0.0] - 2026-06-23
### Added
- 최초 릴리스 — 페이지 단위 HTML 보고서 에디터
- WYSIWYG·소스 편집, 페이지 이동/추가/복제/삭제/순서변경
- 원본 디자인·이미지 보존, 편집본 저장(`_편집본.html`), 전체 미리보기
- 단일 실행 EXE (WebView2 내장, 무설치)

[1.5.0]: https://github.com/everside76/html-slide-editor/releases/tag/v1.5.0
[1.4.0]: https://github.com/everside76/html-slide-editor/releases/tag/v1.4.0
[1.3.0]: https://github.com/everside76/html-slide-editor/releases/tag/v1.3.0
[1.2.0]: https://github.com/everside76/html-slide-editor/releases/tag/v1.2.0
[1.1.0]: https://github.com/everside76/html-slide-editor/releases/tag/v1.1.0
[1.0.0]: https://github.com/everside76/html-slide-editor/releases/tag/v1.0.0
