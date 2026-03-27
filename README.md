# Foundry

Foundry는 콜로니 심을 비롯한 각종 시뮬레이션 게임의 프로토타입 개발 과정에서 함께 다듬어진 ECS-Like 프레임워크입니다.

이 프레임워크의 목적은 "진짜 ECS"의 극단적인 성능 최적화가 아니라, 시뮬레이션 규칙을 안정적으로 조직하고 확장하기 쉬운 구조를 제공하는 데 있습니다. 특히 로직과 뷰의 분리, 시스템과 컴포넌트의 재사용 가능한 조합, 그리고 실행 순서에 대한 세밀한 보장을 중점적으로 다룹니다.

## 무엇을 위해 만들었는가

시뮬레이션 게임은 시간이 지날수록 다음과 같은 문제가 빠르게 커집니다.

- 규칙이 늘어나면서 로직이 서로 강하게 얽힌다.
- 뷰 갱신 코드가 게임 규칙을 침범한다.
- 기능 추가는 쉬워도 실행 순서를 예측하기 어려워진다.
- 시스템 간 결합이 높아져 재사용과 테스트가 어려워진다.

Foundry는 이런 문제를 완화하기 위해, 엔티티와 컴포넌트라는 단순한 데이터 모델 위에 "명시적인 실행 단계"와 "명령 버퍼 기반 변경 적용"을 얹는 방향으로 설계되었습니다.

## 핵심 설계 목표

- 로직과 뷰를 분리할 수 있는 실행 구조
- 시스템을 작은 규칙 단위로 나누고 재조합하기 쉬운 구조
- 컴포넌트 조합만으로 개체의 상태와 역할을 표현할 수 있는 모델
- phase와 priority를 통한 세밀한 실행 순서 제어
- 프로토타입 단계에서 빠르게 규칙을 추가하고 바꾸기 쉬운 개발 경험

## 비목표

Foundry는 고성능 데이터 지향 ECS를 목표로 하지 않습니다.

- SoA 기반 메모리 레이아웃 최적화
- 대규모 병렬 처리 중심 설계
- 캐시 효율 극대화를 위한 엄격한 데이터 배치
- Unity DOTS 스타일의 "진성 ECS" 성능 모델

즉, Foundry는 성능 최적화보다 설계 명확성, 규칙 관리, 실행 순서 보장에 더 큰 비중을 둡니다.

## 핵심 개념

### World

`World`는 Foundry의 실행 중심입니다.

- 엔티티 생성과 파괴
- 컴포넌트 추가, 수정, 제거
- 시스템 등록과 실행
- tick 관리
- 이벤트 수집

모든 시뮬레이션은 `World.Update()`를 통해 한 틱씩 진행됩니다.

### Entity

엔티티는 식별자만 가진 개체입니다. 실제 상태는 컴포넌트 조합으로 표현됩니다.

예를 들어 하나의 주민, 작업 대상, 건물, 일시적인 이벤트 마커도 모두 엔티티로 다룰 수 있습니다.

### Component

컴포넌트는 상태를 담는 데이터 단위입니다. 엔티티에 필요한 상태를 조합해서 의미를 만듭니다.

이 접근은 상속 중심 구조보다 유연하며, 시뮬레이션 규칙을 작은 조각으로 쪼개는 데 유리합니다.

### System

시스템은 `World`와 `CommandBuffer`를 받아 규칙을 실행하는 로직 단위입니다.

시스템은 보통 다음 일을 담당합니다.

- 특정 조건의 엔티티 조회
- 상태 검사
- 변경 명령 예약
- 이벤트 발생

중요한 점은 시스템이 월드를 즉시 뒤흔드는 대신, 대체로 `CommandBuffer`를 통해 변경을 예약한다는 점입니다.

### CommandBuffer

`CommandBuffer`는 엔티티 생성, 컴포넌트 추가/수정/삭제, 이벤트 발생 같은 변경 사항을 모아 두었다가 정해진 시점에 재생합니다.

이 방식의 장점은 다음과 같습니다.

- 시스템 실행 중 컬렉션 변경 문제를 줄일 수 있다.
- 같은 phase 안에서는 읽기 기준이 비교적 안정적이다.
- phase 경계에서 변경을 명시적으로 적용할 수 있다.
- 실행 순서를 추론하기 쉬워진다.

### Query

`Query`는 특정 컴포넌트 조합을 만족하는 엔티티를 조회하기 위한 인터페이스입니다.

- `With<T>()`
- `Without<T>()`
- `WithChangedSince<T>(tick)`
- `QuerySingleton<T>()`

시뮬레이션 규칙을 "어떤 데이터 조합에 반응하는가"라는 형태로 기술할 수 있게 해 줍니다.

## 실행 모델

Foundry는 미리 정의된 phase 순서에 따라 시스템을 실행합니다.

1. `Input`
2. `Validation`
3. `Execution`
4. `Reaction`
5. `ViewCalculation`
6. `Cleanup`

각 phase 안에서는 priority 순서대로 시스템이 실행되고, 해당 phase가 끝날 때 `CommandBuffer`가 playback됩니다.

이 구조는 다음 같은 상황에서 특히 유용합니다.

- 입력을 먼저 수집하고
- 그 입력이 유효한지 검증한 뒤
- 실제 상태 변화를 적용하고
- 후속 반응을 처리하고
- 마지막에 뷰 계산이나 정리 작업을 분리하는 경우

즉, "무엇이 먼저 실행되어야 하는가"를 코드 구조 안에서 드러내는 것이 Foundry의 중요한 특징입니다.

## 왜 시뮬레이션 게임에 잘 맞는가

시뮬레이션 게임은 캐릭터, 작업, 자원, 시설, 명령, 반응, 연출이 서로 엮이며 복잡해집니다. Foundry는 이 복잡성을 다음 방식으로 관리하려고 합니다.

- 상태는 컴포넌트로 나눈다.
- 규칙은 시스템으로 나눈다.
- 실행 순서는 phase와 priority로 통제한다.
- 실제 변경은 command buffer를 통해 단계적으로 반영한다.
- 뷰용 계산은 별도 단계로 분리한다.

이 때문에 Foundry는 "많은 규칙이 얽히는 게임"의 프로토타이핑에 적합합니다.

## 간단한 사용 예시

### 컴포넌트 정의

```csharp
using Foundry;

public struct Position : IComponent
{
    public int X;
    public int Y;
}

public struct Velocity : IComponent
{
    public int X;
    public int Y;
}
```

### 시스템 정의

```csharp
using Foundry;
using Foundry.Queries;

public sealed class MoveSystem : ISystem
{
    public void OnUpdate(World world, CommandBuffer commandBuffer)
    {
        foreach (var entity in world.Query<Position, Velocity>())
        {
            var position = entity.GetComponent<Position>();
            var velocity = entity.GetComponent<Velocity>();

            position.X += velocity.X;
            position.Y += velocity.Y;

            commandBuffer.SetComponent(entity.Id, position);
        }
    }
}
```

### 월드 구성

```csharp
using Foundry;

var world = World.CreateDefault();

world.AddSystem(ExecutionPhase.Execution, new MoveSystem(), priority: 0);

var entityId = world.CreateEntity();
world.AddComponent(entityId, new Position { X = 0, Y = 0 });
world.AddComponent(entityId, new Velocity { X = 1, Y = 0 });

world.Update();
```

### 뷰 계산 분리 예시

게임 규칙 자체는 `Execution` 또는 `Reaction`에서 처리하고, 화면 표시를 위한 파생 값 계산은 `ViewCalculation`에 두는 식으로 역할을 나눌 수 있습니다. 이 방식은 뷰 코드가 핵심 시뮬레이션 규칙을 오염시키지 않도록 돕습니다.

## 현재 구조에서 기대할 수 있는 점

- phase 단위로 실행 순서를 설계할 수 있다.
- 시스템을 작은 단위로 유지하기 쉽다.
- 컴포넌트 조합으로 기능을 유연하게 확장할 수 있다.
- 시뮬레이션 로직과 표현 계층을 분리하기 좋다.
- 프로토타입 단계에서 규칙을 빠르게 실험하기 좋다.

## 현재 구조에서 감수해야 하는 점

- 대규모 엔티티 수를 전제로 한 극단적 성능 최적화 프레임워크는 아니다.
- "진짜 ECS"에서 기대하는 메모리 배치 최적화나 병렬 처리 모델은 제공하지 않는다.
- 설계의 명확성과 순서 제어를 위해 약간 더 명시적인 구조를 요구한다.

## 이런 프로젝트에 적합하다

- 콜로니 심
- 경영/생산 시뮬레이션
- 규칙 중심 샌드박스 게임
- 상태 변화와 반응 체인이 많은 프로토타입
- 초기 단계에서 구조를 빠르게 검증해야 하는 프로젝트

## 이런 프로젝트에는 덜 적합하다

- SoA 기반 고성능 처리 자체가 핵심 목표인 경우
- 수십만 개 이상의 엔티티를 극한까지 밀어야 하는 경우
- 데이터 지향 최적화와 병렬 실행이 최우선인 경우
