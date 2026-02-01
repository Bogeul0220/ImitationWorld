# ImitaionWorld
Unity 엔진을 사용해 개발한 3D 기반 오픈월드 서바이벌 게임 프로젝트입니다.

---

## 📌 프로젝트 개요

- **장르**: 3D 서바이벌 오픈월드
- **개발 형태**: 개인 프로젝트
- **개발 기간**: 2025.05 ~ 2025.06 (1개월)
- **목적**: 환경 오브젝트와의 상호작용, 인벤토리 시스템, 아이템 제작 시스템, 스킬 시스템, 아군/적군 몬스터 AI 구현을 연습하기 위해 제작하였습니다.

---

## 🎮 게임 설명

- 플레이어는 환경 오브젝트를 통해 아이템을 수집하고 제작을 통해 새 아이템을 만들어내어 성장합니다.
- 필드 위 몬스터와 전투할 수 있으며, 몬스터를 포획하여 동료로써 함께 다닐 수 있습니다.
- 게임의 핵심 시스템은 아이템 크래프팅과 전투입니다.

---

## 🛠 사용 기술

- **Engine**: Unity 2022.3.34f1
- **Language**: C#
- **IDE**: Visual Studio 2022 / Cursor
- **Version Control**: Git / GitHub
- **ETC**: ...(추후 정리)

---

## 👨‍💻 담당 역할

- 전체 시스템 설계 및 구현
- 게임 플레이 로직 및 UI 개발
- 빌드 및 배포 환경 구성

---

## ✨ 핵심 구현 내용

- ScriptableObject를 활용한 데이터 관리 구조 설계
- Object Pooling을 적용한 몬스터/이펙트 관리
- FSM(State Machine) 기반 캐릭터 상태 관리
- Unity Animation / Animator를 활용한 캐릭터 제어

(기술적으로 강조하고 싶은 부분 위주로 작성)

---

## 🔍 기술적 문제 해결 경험

- **문제**: 몬스터 수 증가 시 프레임 드랍 발생  
- **원인**: Instantiate / Destroy 반복으로 인한 GC 발생  
- **해결**: Object Pooling 구조 도입  
- **결과**: 평균 FPS 안정화 및 성능 개선

(면접에서 질문으로 이어질 수 있는 부분)

---

## ▶️ 실행 방법 (Getting Started)

### 필요 환경 (Dependencies)

- Windows 10 이상 / macOS
- Unity Hub
- Unity 2022.3.34f1

---

### 설치 방법 (Installing)

1. 이 레포지토리를 클론합니다.
```bash
git clone https://github.com/username/project-name.git
