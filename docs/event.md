# 개요

type 필드의 값을 기반으로 이벤트의 종류가 정해집니다.

## 접속 성공

```
{
    type: "connected",
    roomname: string
}
```

[connect](commands.md#개요#접속하기) 명령을 이용해 접속에 성공했을 때 발생합니다. roomname은 접속된 방의 이름을 의미합니다.

## 게임 시작

```
{
    type: "gamestart",
    black: string,
    white: string,
    board: [보드](model.md#보드),
    turn: [플레이어](model.md#플레이어)
}
```

게임이 시작되었거나 게임이 이미 시작된 방에 들어갔을 때 발생합니다. black과 white는 유저의 이름, board는 게임 판, turn은 현재 차례를 의미합니다.

## 방 입장

```
{
    type: "enter",
    username: string
}
```

방에 누군가 들어왔을 때 발생합니다. username은 방에 들어온 유저의 이름을 의미합니다.

## 수 둬짐

```
{
    type: "move",
    player: [플레이어](model.md#플레이어),
    start: [좌표](model.md#좌표),
    end: [좌표](model.md#좌표),
    dir: [벡터](model.md#벡터)
}
```

수가 둬졌을 때 발생합니다.