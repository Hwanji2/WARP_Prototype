# DragManager  
마우스 드래그 기반 **영역 생성 / 공간 이동 시스템**

프로토타입 및 이식용으로 작성된 핵심 모듈입니다.  
`DragManager.cs` 하나만 이해하면 전체 메커니즘을 파악할 수 있습니다.

---

## Requirements
- Unity 2021+  
- Main Camera (ScreenToWorldPoint 사용)  
- `Zone` 프리팹 (SpriteRenderer + Collider2D)  
- `Draggable` 태그 (드래그 영향 대상 지정)

---

## Directory Structure
/Scripts/DragManager.cs
/Prefabs/Zone.prefab
/Scenes/2dflat (테스트 씬)

---

## Setup
1. `DragManager.cs`를 빈 GameObject에 추가  
2. Inspector에서 `zonePrefab` 연결  
3. 이동 대상 오브젝트에 `Draggable` 태그 부여  
4. `Scenes/2dflat` 실행하여 테스트

---

## Core Logic
### Drag Start
- Left Mouse Button Down  
- 드래그 시작점 기록  
- `Zone` 프리팹 인스턴스 생성  

### Dragging
- 마우스 위치를 World 좌표로 변환  
- 시작점 대비 영역 크기 실시간 업데이트  
- `Zone` 크기 및 위치 정규화 처리  

### Drag End
- Left Mouse Button Up  
- Zone 확정 (Collider 범위 고정)  
- Zone 내부 `Draggable` 검색  
- 이후 생성되는 Zone으로 대상 Teleport  

---

## Teleport Rule
- `Zone A`에서 `Zone B`로 이동할 때  
- `A` 내부의 모든 `Draggable`  
  → `B.center`로 즉시 스냅 이동  
- Player 또한 동일한 규칙 적용  

---

## Why `Draggable` Tag?
- 드래그 영향 대상을 명확히 제한하기 위함  
- 전 오브젝트 이동 시 물리 및 충돌 문제 발생  
- 태그 기반 스코프 분리로 안정성 확보  

---

## Test Scene (2dflat)
테스트용 씬에는 다음이 포함되어 있습니다.
- DragManager
- 기본 플랫포머 Player
- Draggable 샘플 오브젝트
- Zone 프리팹

---

## Notes
- 이동은 Rigidbody2D 기반이 아닌 Transform 스냅 방식  
- Draggable 오브젝트는 Collider2D 필수  
- Zone은 드래그 벡터 기반으로 자동 크기 조정  
- 모든 입력 처리는 DragManager 단일 모듈로 관리 (Global Input Layer)

---

## Execution Flow
LMB Down
→ record startPos
→ instantiate ZonePrefab

LMB Hold
→ update Zone scale/position

LMB Up
→ lock Zone
→ find Draggable inside
→ on next Zone → teleport to center


---

## Summary
이 시스템은 “영역 그리기 → 공간 이동”이라는 새로운 조작 경험에 집중한 프로토타입입니다.  
단일 Manager 기반 구조로 이식성이 높으며, 최소 리소스로 높은 상호작용 효과를 얻을 수 있도록 설계되었습니다.
