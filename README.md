# [Unity 3D] ZombieSurvival
## 1. 소개

<div align="center">
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/%EB%A9%94%EC%9D%B8%ED%99%94%EB%A9%B4.JPG" width="49%" height="300"/>
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/%EB%8C%80%EA%B8%B0%EC%8B%A4.JPG" width="49%" height="300"/>
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/%EB%AC%B4%EA%B8%B0%20%EC%83%81%EC%A0%90.JPG" width="49%" height="300"/>
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/%EC%86%8C%EB%B9%84%EC%83%B5.JPG" width="49%" height="300"/>
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/%EC%97%85%EA%B7%B8%EB%A0%88%EC%9D%B4%EB%93%9C%20%EC%83%B5.JPG" width="49%" height="300"/>
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/%EC%9B%A8%EC%9D%B4%EB%B8%8C1_2.JPG" width="49%" height="300"/>
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/5%20%EB%8B%A8%EA%B3%84.JPG" width="99%" height="600"/>
  <img src="https://github.com/k660323/ZombieSurvivor/blob/main/Images/10%20%EB%8B%A8%EA%B3%84.JPG" width="99%" height="600"/>
  
  < 게임 플레이 사진 >
</div>

+ ZombeiSurvivor 이란?
  + 탑뷰 시점의 생존 게임 입니다.
 
+ 목표
  + 매 라운드마다 몰려오는 좀비들을 쓰러트리면 된다. (총 10 라운드)

+ 게임 흐름
  + 매 라운드마다 가지고 있는 소지금으로 물품을 상점에서 구매할 수 있습니다.
  + 구매한 물품을 가지고 텐트밖으로 나가면 라운드가 시작되며 라운드가 끝날때까지는 텐트로 돌아가실 수 없습니다.
  + 라운드는 총 10라운드가 있으며 5라운드, 10라운드는 보스 라운드 입니다.
  + 모두 클리어하거나 도중에 캐릭터가 사망하면 총 점수 합계를 기록하여 저장합니다.

<br>

## 2. 프로젝트 정보

+ 사용 엔진 : UNITY
  
+ 엔진 버전 : 2020.3.19f1 LTS

+ 사용 언어 : C#
  
+ 작업 인원 : 1명
  
+ 작업 영역 : 콘텐츠 제작, 디자인, 기획
  
+ 장르      : 생존 게임
  
+ 소개      : 탑 뷰 시점의 좀비 생존 게임 입니다.
  
+ 플랫폼    : PC
  
+ 개발기간  : 2021.01.23 ~ 2021.02.13
  
+ 형상관리  : GitHub Desktop

<br>

---

<br>

## 3. 구현에 어려웠던 점과 해결과정
+ 유니티 에서 지원하는 함수들을 잘 숙지되지 않아서 구현하는데 어려움을 겪었습니다.
  + 유니티 공식 문서를 참고하거나 여러 포트폴리오를 만들면서 엔진에 대한 지식 및 기능에 대해 익숙해졌습니다.
  
+ 몬스터 AI 코드를 하드코딩으로 구현해서 추가 수정하기 너무 어려웠습니다.
  + 추후에 FSM(유한 상태 머신)을 이용하여 구현하여 효율적인 코딩이 가능해졌습니다.
    
## 4. 느낀점
+ 모든 객체를 생성하고 파괴를 하다보니 유니티 GC에서 수거할때 프레임 드랍이 발생하여 오브젝트 풀링의 구현의 필요성을 느낌
+ 코드가 너무 일관성과 모듈성의 필요성을 느꼈다. 점점 코드가 커지니까 유지보수하기가 너무 어려워 진다. 코드를 수정하는 시간보다 찾는시간이 더 걸리는거 같다. 그리고 일관성이 없어서 코드가 어떤 의미로 사용되었는지 잘 모를때가 있다.
+ 모바일 기능도 구현하다보니 작업량 늘고 코드가 너무 하드 코딩이 되었다. 찾아보니 NewInput이라는 다중 플레폼의 입력을 담당하는 유니티 공식 에셋이 있어 사용법을 배워 추후 프로젝트에 사용할 생각이다.

## 5. 플레이 영상
+ https://www.youtube.com/watch?v=3mXmK7Ga8ro
