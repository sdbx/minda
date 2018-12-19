# 개요

type 필드의 값을 기반으로 커맨드의 종류가 정해집니다.

## 접속하기

```
{
    type: "connect",
    id: string
}
```

로비서버가 준 id를 이용해 접속을 시도합니다.

## 수두기 

```
{
    type: "move",
    start: [좌표](model.md#좌표),
    end: [좌표](model.md#좌표),
    dir: [벡터](model.md#벡터)
}
```

start와 end사이의 돌들을 dir 방향으로 옮깁니다.

## 기권하기 

```
{
    type: "gg"
}
```

기권합니다. 웬만해서는 안하는 것이 좋습니다. "인간은 파괴될 순 있지만 gg하지는 않는다." 노인과 바다 중
