# 🔫 2-Player Co-op Arcade Shooter (Netcode)

> Unity Netcode for GameObjects(NGO)를 활용한 비대칭 멀티플레이어 슈팅 게임 프로젝트입니다.  
> 서버가 뷰(View)와 로직을 전담하고, 클라이언트는 입력(Input) 장치로 기능을 구현했습니다.



## 🏗 시스템 구조 및 역할 (Architecture)

### 핵심 역할 분담
* 🖥️ Server (Host/PC): 물리 연산, AI 로직, 충돌 판정, 메인 렌더링 담당
* 📱 Client (Tablet): 사용자 입력 전송(RPC), 피드백 담당

## ✨ 주요 기술 및 특징 (Key Features)

### 1. 🔌 네트워크 안정성 & 자동 복구 (Auto-Recovery)
* 연결 끊김 대응: 클라이언트 연결 해제 시 즉시 타이틀 화면으로 복귀하며, 1초 간격 자동 재접속(Auto-Reconnect) 로직이 동작합니다.
* 상태 초기화: 플레이어 이탈 시 서버 UI(점수판)가 즉시 리셋되어 화면 잔상을 제거합니다.

### 2. 📡 능동적 UX: UDP Broadcasting
* 기존 TCP 접속 방식의 수동적인 한계를 보완하기 위해 **UDP 소켓 프로그래밍**을 도입했습니다.
* 서버가 주기적으로 "L:1|R:0" (역할 선점 상태)를 로컬 네트워크에 브로드캐스팅합니다.
* 클라이언트는 이를 수신하여 접속 전에 이미 선점된 역할 버튼을 비활성화함으로써 불필요한 접속 시도를 원천 차단합니다.

### 3. 📦 데이터 주도 설계 (Data-Driven)
* `ScriptableObject`를 활용하여 WorldTheme(우주/해양/인체) 데이터를 관리합니다.
* 코드 수정 없이 데이터 교체만으로 적 프리팹, 배경, 총알 리소스를 손쉽게 변경할 수 있습니다.

### 4. ⌨️ 한글 오토마타 (Algorithm)
* Unity 기본 입력기(InputField)의 한계를 극복하기 위해 비트 연산을 활용한 한글 자소 조합기를 직접 구현했습니다.
* 이를 통해 끊김 없는 실시간 닉네임 입력 처리가 가능합니다.

## 🛠 설치 및 실행 방법 (How to Run)

이 프로젝트는 Server(Host)**와 Client가 분리되어 실행되어야 합니다.

1. Releases에서 빌드 파일을 다운로드합니다.
2. PC에서 `Server.exe`를 실행하여 호스트를 엽니다. (실행 시 자동으로 UDP 방송이 시작됩니다.)
3. 태블릿(또는 다른 PC)에서 `Client.exe`를 실행합니다.
4. 타이틀 화면에서 활성화된 Left / Right 버튼을 눌러 게임에 참여합니다.

## 📂 프로젝트 구조 (Structure)

* `Scripts/Core`: 네트워크 매니저 및 전역 관리 (`GameNetworkManager` 등)
* `Scripts/Player`: 플레이어 동기화 및 입력 처리 (`PlayerStateManager`, `GunController`)
* `Scripts/UI`: 서버 뷰 및 클라이언트 UI 로직
* `Scripts/Algorithm`: 한글 오토마타 및 유틸리티 알고리즘
