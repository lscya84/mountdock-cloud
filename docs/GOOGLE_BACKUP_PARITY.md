# Google 암호화 rclone.conf 백업 parity 계획

## 목표

기존 MountDock의 Google Drive `appDataFolder` 기반 암호화 `rclone.conf` 백업/복원 기능을 MountDock Cloud에서도 유지한다.

## 유지할 보안 원칙

1. Google 로그인은 복호화 키가 아니다.
2. 복호화는 사용자가 입력한 passphrase가 있어야만 가능하다.
3. Google Drive에는 평문 `rclone.conf`를 저장하지 않는다.
4. 복원 전 기존 로컬 `rclone.conf`는 백업한다.
5. 토큰, passphrase, rclone.conf 내용은 로그에 남기지 않는다.

## 기본 계획

- OAuth scope는 `https://www.googleapis.com/auth/drive.appdata`를 사용한다.
- 기본 백업 파일명은 `mountdock_rclone_conf_v1.json`을 유지한다.
- Windows에서는 passphrase 저장에 Windows Credential Manager 또는 DPAPI를 사용한다.
- 암호화 payload 형식은 기존 MountDock과 호환 가능한지 먼저 확인한다.

## 구현 단계

1. `GoogleBackupPlan`으로 parity 요구사항을 코드화한다.
2. 암호화/복호화 payload 포맷을 기존 MountDock과 비교한다.
3. Google OAuth desktop flow를 C#에서 구현한다.
4. appDataFolder upload/download client를 구현한다.
5. 복원 전 `.bak-YYYYMMDD-HHMMSS` 백업을 구현한다.
6. 통합 테스트는 실제 Google 계정 없이 mock client로 먼저 작성한다.

## 현재 상태

- parity 계획 객체와 기본 인터페이스만 추가됨.
- 실제 OAuth/암호화/upload/download 구현은 후속 작업이다.
