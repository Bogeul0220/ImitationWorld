# ImitationWorld
Unity 엔진을 사용해 개발한 3D 기반 오픈월드 서바이벌 게임 프로젝트입니다.

---

## 📌 프로젝트 개요

- **장르**: 3D 서바이벌 오픈월드
- **개발 형태**: 개인 프로젝트
- **개발 기간**: 2025.05 ~ 2025.06 (약 6주)
- **목적**: 환경 오브젝트와의 상호작용, 인벤토리 시스템, 아이템 제작 시스템, 스킬 시스템, 아군/적군 몬스터 AI 구현을 연습하기 위해 제작하였습니다.

---

## 🎮 게임 설명

- 플레이어는 환경 오브젝트를 통해 아이템을 수집하고 제작을 통해 새 아이템을 만들어내어 성장합니다.
- 필드 위 몬스터와 전투할 수 있으며, 몬스터를 포획하여 동료로써 함께 다닐 수 있습니다.
- 게임의 핵심 시스템은 아이템 제작과 전투입니다.

---

## 🛠 사용 기술

- **Engine**: Unity 2022.3.34f1
- **Language**: C#
- **IDE**: Visual Studio 2022 / Cursor
- **Version Control**: Git / GitHub
- **ETC**: AI Navigation (NavMesh), Input System

---

## 📁 프로젝트 구조

Assets/Scripts/

├── Combat/ # 전투 시스템 (데미지, 포획)

├── Creature/ # 몬스터 AI, 스킬 시스템

├── Item/ # 아이템 데이터 및 제작 레시피

├── Mangers/ # 싱글톤 매니저 (Inventory, Creature, Environment 등)

├── Player/ # 플레이어 이동/전투 컨트롤러

├── UI/ # 인벤토리, 장비, 제작 UI

└── Weapon/ # 무기 시스템

---

## 👨‍💻 담당 역할

- 전체 시스템 설계 및 구현
- 게임 플레이 로직 및 UI 개발

---

## ✨ 핵심 구현 내용

### 1) ScriptableObject 기반 데이터 관리
- 아이템 데이터와 제작 레시피를 ScriptableObject로 분리
- 코드 수정 없이 에디터에서 새 아이템/레시피 추가 가능

### 2) FSM 기반 몬스터 AI
- Idle/Patrol/Escape/StandOff/Battle/TakeHit/Died 상태 전환 관리
- 호전성(Belligerent) 시스템: Peaceful/NonAggressive/Aggressive 3단계 행동 분기
- 아군/적군 전환 시스템으로 포획한 몬스터를 동료로 활용

### 3) 스킬 시스템
- `SkillBaseSO`를 상속한 투사체/범위 스킬 구현
- 스킬별 쿨다운을 Dictionary로 관리

### 4) Object Pooling
- 제네릭 기반 `ObjectPoolManager`로 몬스터, 환경 오브젝트, 이펙트 재사용
- 런타임 Instantiate/Destroy 최소화

### 5) 이벤트 기반 UI 업데이트
- `OnInventoryChanged` 이벤트로 데이터-UI 분리
- 인벤토리 변경 시 구독 UI 자동 갱신

---

## 🔍 문제 해결

### 문제: 아이템 레시피 하드코딩으로 확장성 저하
- **원인**: 레시피 구조가 코드에 직접 박혀 있어 수정/추가 시 복잡해짐
- **해결**: 레시피를 ScriptableObject로 분리하고 CraftObject가 레시피 데이터를 참조하도록 구조 변경
- **결과**: 레시피 추가/수정이 데이터 작업으로 단순화되고 유지보수성이 향상됨

### 문제: Terrain 환경물 상호작용 불가
- **원인**: Terrain의 트리/디테일 오브젝트는 게임오브젝트가 아니어서 콜라이더/스크립트를 붙일 수 없음
- **해결**: 상호작용 가능한 Prefab을 Terrain 랜덤 위치에 스폰하고, SampleHeight로 지면 높이 보정 + 노멀 기준 회전 정렬
- **결과**: 환경물과 타격/파괴 상호작용 가능, 지형 경사에 맞게 배치되어 자연스러운 배치 확보

### 문제: 상호작용 대상 선택이 불명확함
- **원인**: 근처 상호작용 대상이 여러 개일 때 입력 기준이 명확하지 않음
- **해결**: 플레이어와 상호작용 오브젝트 간 거리를 계산해 가장 가까운 대상만 선택하도록 변경
- **결과**: 상호작용 정확도/일관성 향상, UX 개선

---

[유튜브 링크](https://youtu.be/yFjqhBL_RS8)
