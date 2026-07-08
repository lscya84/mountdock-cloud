# Drive Letter 검증 계획

## 목적

MountDock Cloud의 기본 UX는 Windows Cloud Files API sync root다. 하지만 기존 MountDock 사용자는 드라이브 문자 기반 사용 경험에 익숙하므로, `subst` 또는 다른 방식으로 sync root를 드라이브 문자에 노출했을 때 다음 항목이 유지되는지 검증해야 한다.

## 검증 질문

1. `subst X: <sync-root>`로 연결한 경로에서도 Cloud Files placeholder가 유지되는가?
2. Explorer 상태 아이콘이 `X:` 경유에서도 표시되는가?
3. `X:`에서 파일을 열 때 hydration callback이 발생하는가?
4. pin / unpin / dehydrate 상태가 `X:` 경유에서도 유지되는가?
5. Office, 메모장, 복사 작업에서 동작 차이가 있는가?

## 수동 검증 절차

1. Windows에서 CfAPI spike sync root를 등록한다.
2. sync root에 `hello.txt` placeholder를 생성한다.
3. 일반 sync root 경로에서 상태 아이콘과 hydration 동작을 확인한다.
4. 다음 명령으로 드라이브 문자를 연결한다.

```cmd
subst X: "%USERPROFILE%\MountDockCloudSpike"
```

5. `X:\hello.txt`를 Explorer에서 확인한다.
6. `X:\hello.txt`를 메모장으로 열어 hydration 발생 여부를 확인한다.
7. 파일 상태 아이콘이 일반 sync root 경로와 동일한지 확인한다.
8. 다음 명령으로 연결을 해제한다.

```cmd
subst X: /D
```

## 판정 기준

| 결과 | 판단 |
|---|---|
| placeholder + 아이콘 + hydration 모두 동일 | 드라이브 문자 UX 지원 가능 |
| hydration은 되지만 아이콘이 깨짐 | sync root UX를 기본으로, drive letter는 실험 옵션 |
| hydration이 안 됨 | Cloud Files 모드에서 drive letter 미지원 |
| 일부 앱만 실패 | 앱 호환성 표 작성 후 제한 지원 |

## 결과 기록 위치

검증 후 결과는 다음 문서에 기록한다.

```text
docs/CLOUD_FILES_SPIKE_RESULTS.md
```
