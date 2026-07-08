# MountDock Cloud TODO

## 마일스톤 0 — 저장소 기본 골격

- [x] 새 저장소 골격 생성
- [x] Provider 우선 언어로 C#/.NET 선택
- [x] 아키텍처 문서 추가
- [x] Cloud Files API 스파이크 계획 추가
- [x] rclone 백엔드 설계 추가

## 마일스톤 1 — CfAPI 가능성 검증 스파이크

- [x] Hermes 환경에서 구조적 빌드/테스트용 .NET SDK 확인
- [ ] 실제 CfAPI 실행을 위한 Windows 빌드/런타임 환경 확인
- [ ] sync root 등록용 `CloudFilesApi` P/Invoke 시그니처 구현
- [ ] 테스트 sync root 등록 구현
- [ ] 단일 파일 placeholder 생성 구현
- [ ] 단일 파일 hydration callback 구현
- [ ] dehydrate/free-space 디버그 동작 구현
- [ ] 결과를 `docs/CLOUD_FILES_SPIKE_RESULTS.md`에 기록

## 마일스톤 2 — rclone 읽기 전용 백엔드

- [x] `RcloneCommandBuilder`에 `lsjson` / `copyto` / `deletefile` / `moveto` 지원 추가
- [x] `RcloneClient`에 취소 토큰 및 stdout/stderr 캡처 지원 추가
- [x] hydration/download 작업용 명시적 timeout wrapper 추가
- [ ] 테스트 remote 디렉터리를 `lsjson`으로 조회
- [ ] remote metadata로 placeholder 생성
- [ ] rclone을 통해 remote 파일 내용을 hydrate

## 마일스톤 3 — metadata 상태 저장소

- [x] SQLite 상태 DB 추가
- [x] `items` 테이블 추가
- [x] `operations` 테이블 추가
- [x] `conflicts` 테이블 추가
- [x] 기본 persistence 및 state transition 테스트 추가
- [x] operation queue helper 및 conflict helper 추가

## 마일스톤 4 — write-back 안전성

- [ ] 로컬 dirty 파일 감지
- [ ] upload queue 등록
- [ ] 업로드 전 remote metadata 확인
- [ ] 충돌 시 양쪽 버전 보존
- [ ] 자동 파괴적 delete propagation 방지

## 마일스톤 5 — Explorer UX

- [ ] Cloud Files 상태를 Explorer-visible 상태로 매핑
- [ ] pin/offline 지원
- [ ] dehydrate/free-space 지원
- [ ] drive-letter 방식과 sync-root 방식의 동작 차이 검증

## 미해결 질문

- [ ] `subst`가 Cloud Files placeholder 동작과 Explorer 상태 아이콘을 보존하는가?
- [ ] provider를 C# 단독으로 갈지, C# GUI + C++ CfAPI helper로 나눌지 결정해야 하는가?
- [ ] 지원 대상 Windows 버전은 어디까지인가?
- [ ] installer/register/unregister lifecycle은 어떻게 처리할 것인가?
