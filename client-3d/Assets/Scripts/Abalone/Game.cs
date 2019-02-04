using System.Collections;
using System.Collections.Generic;
using System.IO;
using Abalone.Data;
using UnityEngine;

namespace Abalone
{
    public class Game : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Board board;
        private GameData gamedata;
        private GameContext context;

        private bool dragStarted;
        private Vector3 dragStartMousePosition;
        private CubeCoord dragStartMarblePosition;
        private Marble draggingMarble;
        private CubeDirection dragDirection;
        private CubeDirection chooseDirection;
        private CubeCoord chosenMarbleStart;
        private int howManyIsChosen = 1;
        private int howManyWillPush;
        private bool dragDirectionFixed;
        private bool wasValidMove;
        private bool opponentPush;
        private bool gameOver;

        private void Awake()
        {
            var boardString = File.ReadAllText("Assets/Maps/basic.abalone");
            var gameData = BoardStringParser.Parse(boardString);
            context = new GameContext(gameData);
            board.Create(gameData);
            context = board.context;
            gamedata = gameData;
            //broadCast
            string[] lines = new string[9];
            string[] emoji = new string[5] { "🔵", "⚫", "⚪", "🎱", "🍥" };

            for (var x = 0; x < board.settings.arraySize; x++)
            {
                for (var z = 0; z < board.settings.arraySize; z++)
                {
                    var zAlpha = board.settings.arraySize - 1 - z;

                    if (Mathf.Abs(x + board.settings.placementOffset.x + zAlpha + board.settings.placementOffset.z) > board.settings.cutThreshold) continue;
                        lines[x] += emoji[gamedata.placement[x, zAlpha]];
                }
                if (x == 0) lines[x] += "             ◼ : " + context.fallenMarbles[1];
                if (x == 1) lines[x] += "         ◻ : " + context.fallenMarbles[0];
            }

            using (StreamWriter outputFile = new StreamWriter("Assets/Bot/bot.txt"))
            {
                foreach (string line in lines)
                {
                    outputFile.WriteLine(line);
                }
            }
            System.Diagnostics.Process.Start(@"Assets\Bot\ubot\node.exe", @"Assets\Bot\ubot\Unfwsed_bot.js");
        }

        private void Update()
        {
            if (!gameOver)
            {
                HandleMarbleMove();
            }
        }

        private void GameOver()
        {
            gameOver = true;
            for (var x = 0; x < board.settings.arraySize; x++)
            {
                for (var z = 0; z < board.settings.arraySize; z++)
                {
                    if (Mathf.Abs(x + board.settings.placementOffset.x + z + board.settings.placementOffset.z) > board.settings.cutThreshold) continue;
                    if (FindWithCoord(new AxialCoord(x, z)) == null) continue;
                    FindWithCoord(new AxialCoord(x, z)).gameObject.GetComponent<Rigidbody>().isKinematic = false;
                    FindWithCoord(new AxialCoord(x, z)).gameObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 10.0f);
                }
            }
            Debug.Log($"{new string[2] { "검댕이", "흰둥이" }[context.fallenMarbles[0] / 6]}가 이겼습니다.");

            //broadCast
            System.Threading.Thread.Sleep(5000);
            var text = new string[2] { "흑", "백" }[context.fallenMarbles[0] / 6] + "이 이겼습니다";
            var winner = new string[2] { "⚫", "⚪" }[context.fallenMarbles[0] / 6];

            using (StreamWriter outputFile = new StreamWriter("Assets/Bot/bot.txt"))
            {
                for (var i = 0; i < board.settings.arraySize; i++)
                {
                    var j = board.settings.cutThreshold + 1 + Mathf.PingPong(i, board.settings.cutThreshold);
                    var message = "";
                    for (var k = 1; k <= j; k++)
                    {
                        message += winner;
                    }
                    if (j == 2 * board.settings.cutThreshold + 1)
                    {
                        message = "";
                        message += winner + winner + ' ' + text + ' ' + winner + winner;
                    }
                    outputFile.WriteLine(message);
                }
            }
        }

        private void Motion(Vector3 worldChosenPosition, Vector3 worldMovePosition, float t, CubeCoord directionCoord, Marble marble)
        {
            var marbleY = marble.yCurve.Evaluate(t);
            var moveRotation = new Vector3(directionCoord.ToWorld().z, directionCoord.ToWorld().y, -directionCoord.ToWorld().x);

            marble.transform.localPosition = Vector3.Lerp(worldChosenPosition, worldMovePosition, t) + new Vector3(0, marbleY, 0);
            marble.transform.localRotation = Quaternion.identity;
            marble.transform.Rotate(moveRotation, 360 * Mathf.Min(t, 1));
        }

        private bool CanPushMarble(CubeCoord chosenStart, CubeDirection chosenDirection, int howMany, CubeCoord moveDirection)
        {
            howManyWillPush = 0;
            opponentPush = false;

            for (int i = 0; i < howMany; i++)
            {
                var positionToBeMoved = chosenStart + chosenDirection.ToCoord() * i + moveDirection;

                if (positionToBeMoved.x * positionToBeMoved.x >= board.settings.side * board.settings.side)
                    return false;
                if (positionToBeMoved.z * positionToBeMoved.z >= board.settings.side * board.settings.side)
                    return false;
                if (Mathf.Abs(positionToBeMoved.x + positionToBeMoved.z) > board.settings.cutThreshold)
                    return false;

                if (FindWithCoord((AxialCoord)positionToBeMoved - board.settings.placementOffset) != null)
                {
                    var marbleToBeMoved = FindWithCoord((AxialCoord)positionToBeMoved - board.settings.placementOffset);

                    if (context.currentPlayerIndex == marbleToBeMoved.playerIndex && !WasChosen(marbleToBeMoved.visiblePosition - chosenStart, chosenDirection, howMany))
                        return false;

                    if (context.currentPlayerIndex != marbleToBeMoved.playerIndex && chosenDirection.ToCoord() != moveDirection && chosenDirection.ToCoord() != moveDirection * (-1))
                        return false;
                }
            }

            if (chosenDirection.ToCoord() == moveDirection || chosenDirection.ToCoord() == moveDirection * (-1))
            {
                while (true)
                {
                    CubeCoord pushTargetPosition = new CubeCoord(0, 0, 0);

                    if(chosenDirection.ToCoord() == moveDirection)
                        pushTargetPosition = chosenStart + chosenDirection.ToCoord() * howMany + moveDirection * howManyWillPush;
                    if(chosenDirection.ToCoord() == moveDirection * (-1))
                        pushTargetPosition = chosenStart - chosenDirection.ToCoord() + moveDirection * howManyWillPush;

                    if (FindWithCoord((AxialCoord)pushTargetPosition - board.settings.placementOffset) != null)
                    {
                        var pushTarget = FindWithCoord((AxialCoord)pushTargetPosition - board.settings.placementOffset);

                        if (pushTarget.playerIndex == context.currentPlayerIndex)
                            return false;

                        howManyWillPush++;
                    }

                    else
                    {
                        break;
                    }
                }
                if (howMany <= howManyWillPush)
                    return false;
                else
                    opponentPush = true;
            }

            return true;
        }

        private Marble FindWithCoord(AxialCoord axialCoord)
        {
            if (axialCoord.x < 0 || axialCoord.x >= board.settings.arraySize)
                return null;
            if (axialCoord.z < 0 || axialCoord.z >= board.settings.arraySize)
                return null;
            if (Mathf.Abs(axialCoord.x + board.settings.placementOffset.x + axialCoord.z + board.settings.placementOffset.z) > board.settings.cutThreshold)
                return null;
            if (context.marbles[axialCoord.x, axialCoord.z] != null)
                return context.marbles[axialCoord.x, axialCoord.z].GetComponent<Marble>();
            return null;
        }

        private bool WasChosen(CubeCoord cubeCoord, CubeDirection cubeDirection, int chosen)
        {
            switch (chosen) {
                case 1:
                    return cubeCoord == new CubeCoord(0, 0, 0);
                case 2: 
                    return cubeCoord == new CubeCoord(0, 0, 0) || cubeCoord == cubeDirection.ToCoord();
                case 3:
                    return cubeCoord == new CubeCoord(0, 0, 0) || cubeCoord == cubeDirection.ToCoord() || cubeCoord == cubeDirection.ToCoord() * 2;
                default:
                    return false;
            }
        }

        private void SelectCancel()
        {
            for (int i = 0; i < howManyIsChosen; i++)
            {
                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                marbles.PaintOrigin(true);
            }
            context.playerContext = "Choose";
        }

        private void HandleMarbleMove()
        {
            var currentMousePosition = MouseUtil.GetWorld(mainCamera);

            if (Input.GetKeyDown(KeyCode.Escape) && context.playerContext == "Move")
            {
                SelectCancel();
            }

            if (!dragStarted && Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out var hit))
                {
                    var marble = hit.transform.GetComponent<Marble>();
                    
                    if (marble != null)
                    {
                        switch (context.playerContext)
                        {
                            case "Choose":
                                if (context.currentPlayerIndex == marble.playerIndex && !marble.fallen)
                                {
                                    marble.PaintSelectColor();
                                    draggingMarble = marble;
                                    dragStarted = true;
                                    dragStartMousePosition = marble.visiblePosition.ToWorld();
                                    dragStartMarblePosition = marble.visiblePosition;
                                }
                                break;

                            case "Move":
                                if (WasChosen(marble.visiblePosition - chosenMarbleStart, chooseDirection, howManyIsChosen))
                                {
                                     dragStarted = true;
                                     dragStartMousePosition = marble.visiblePosition.ToWorld();
                                }

                                else
                                {
                                    SelectCancel();
                                }
                                break;

                            default:
                                break;
                        }
                        if (marble.fallen)
                        {
                            draggingMarble = marble;
                            dragStarted = true;
                            dragStartMousePosition = marble.visiblePosition.ToWorld();
                            dragStartMarblePosition = marble.visiblePosition;
                        }
                    }
                }

                else if (context.playerContext == "Move")
                {
                    SelectCancel();
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                switch (context.playerContext) {
                    case "Choose":
                        if (draggingMarble != null)
                        {
                            if (!draggingMarble.fallen) {
                                chosenMarbleStart = dragStartMarblePosition;
                                chooseDirection = dragDirection;
                                context.playerContext = "Move";
                                draggingMarble = null;
                            }
                        }
                        break;
                    case "Move":
                        if (wasValidMove)
                        {
                            for (int i = 0; i < howManyIsChosen; i++)
                            {
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var worldMovePosition = (chosenPosition + dragDirection.ToCoord()).ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                marbles.transform.localPosition = worldMovePosition;
                                marbles.SetArrayPosition((AxialCoord)chosenPosition - board.settings.placementOffset + (AxialCoord)(dragDirection.ToCoord()));
                                marbles.PaintOrigin(true);
                            }

                            if (opponentPush)
                            {
                                for (int i = 0; i < howManyWillPush; i++)
                                {
                                    CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                        chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                        chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + dragDirection.ToCoord() * i;

                                    var movePosition = chosenPosition + dragDirection.ToCoord();
                                    var worldChosenPosition = chosenPosition.ToWorld();
                                    var worldMovePosition = (movePosition).ToWorld();
                                    var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                    marbles.transform.localPosition = worldMovePosition;
                                    marbles.SetArrayPosition((AxialCoord)chosenPosition - board.settings.placementOffset + (AxialCoord)dragDirection.ToCoord());
                                }

                                for (int j = 0; j < howManyWillPush; j++)
                                {
                                    int i;
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() || chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                        i = howManyWillPush - j - 1;
                                    else
                                        i = j;

                                    CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                        chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                        chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + dragDirection.ToCoord() * i;

                                    var beforePosition = (AxialCoord)chosenPosition - board.settings.placementOffset;
                                    var afterPosition = beforePosition + (AxialCoord)(dragDirection.ToCoord());
                                    gamedata.SetAt(beforePosition, 0);

                                    if (afterPosition.x < 0 || afterPosition.x >= board.settings.arraySize)
                                    {
                                        FindWithCoord(beforePosition).FallAnimation(dragDirection.ToCoord().ToWorld());
                                        context.marbles[beforePosition.x, beforePosition.z] = null;
                                        continue;
                                    }
                                    if (afterPosition.z < 0 || afterPosition.z >= board.settings.arraySize)
                                    {
                                        FindWithCoord(beforePosition).FallAnimation(dragDirection.ToCoord().ToWorld());
                                        context.marbles[beforePosition.x, beforePosition.z] = null;
                                        continue;
                                    }

                                    if (Mathf.Abs(afterPosition.x + board.settings.placementOffset.x + afterPosition.z + board.settings.placementOffset.z) > board.settings.cutThreshold)
                                    {
                                        FindWithCoord(beforePosition).FallAnimation(dragDirection.ToCoord().ToWorld());
                                        context.marbles[beforePosition.x, beforePosition.z] = null;
                                        continue;
                                    }
                                    gamedata.SetAt(afterPosition, FindWithCoord(beforePosition).playerIndex);
                                    context.MoveData(beforePosition, afterPosition);
                                }
                            }

                            for (int j = 0; j < howManyIsChosen; j++)
                            {
                                int i;
                                if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                    i = howManyIsChosen - j - 1;
                                else
                                    i = j;
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var beforePosition = (AxialCoord)chosenPosition - board.settings.placementOffset;
                                var afterPosition = beforePosition + (AxialCoord)(dragDirection.ToCoord());
                                gamedata.SetAt(beforePosition, 0);
                                gamedata.SetAt(afterPosition, context.currentPlayerIndex);
                                context.MoveData(beforePosition, afterPosition);
                            }
                            //AI
                            if (context.currentPlayerIndex == 1)
                            {
                                var targetCase = GetComponent<AI>().Thinking(gamedata.placement, board, 2);
                                StartCoroutine(FindWithCoord(targetCase.target).HelloWorld());
                            }
                            //broadCast ON
                            string[] lines = new string[9];
                            string[] emoji = new string[5] { "🔵", "⚫", "⚪", "🎱", "🍥" };

                            for (var x = 0; x < board.settings.arraySize; x++)
                            {
                                for (var z = 0; z < board.settings.arraySize; z++)
                                {
                                    var zAlpha = board.settings.arraySize - 1 - z;

                                    if (Mathf.Abs(x + board.settings.placementOffset.x + zAlpha + board.settings.placementOffset.z) > board.settings.cutThreshold) continue;

                                    var isFirstMarble = FindWithCoord(new AxialCoord(x, zAlpha)) == FindWithCoord((AxialCoord)(chosenMarbleStart + dragDirection.ToCoord()) - board.settings.placementOffset);
                                    var isSecondMarble = howManyIsChosen >= 2 && FindWithCoord(new AxialCoord(x, zAlpha)) == FindWithCoord((AxialCoord)(chosenMarbleStart + chooseDirection.ToCoord() + dragDirection.ToCoord()) - board.settings.placementOffset);
                                    var isThirdMarble = howManyIsChosen >= 3 && FindWithCoord(new AxialCoord(x, zAlpha)) == FindWithCoord((AxialCoord)(chosenMarbleStart + chooseDirection.ToCoord() * 2 + dragDirection.ToCoord()) - board.settings.placementOffset);

                                    if (isFirstMarble || isSecondMarble || isThirdMarble)
                                        lines[x] += emoji[gamedata.placement[x, zAlpha] + context.playerCount];
                                    else
                                        lines[x] += emoji[gamedata.placement[x, zAlpha]];
                                }
                                if (x == 0) lines[x] += "             ◼ : " + context.fallenMarbles[1];
                                if (x == 1) lines[x] += "         ◻ : " + context.fallenMarbles[0];
                            }

                            using (StreamWriter outputFile = new StreamWriter("Assets/Bot/bot.txt"))
                            {
                                foreach (string line in lines)
                                {
                                    outputFile.WriteLine(line);
                                }
                            }
                            //broadCast OFF
                            if (context.fallenMarbles[0] == 6 || context.fallenMarbles[1] == 6)
                                GameOver();
                            context.NextTurn();
                            context.playerContext = "Choose";
                            wasValidMove = false;
                        }
                        else
                        {
                            for (int i = 0; i < howManyIsChosen; i++)
                            {
                                var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                marbles.transform.localPosition = worldChosenPosition;
                                marbles.transform.localRotation = Quaternion.identity;
                            }

                            if (opponentPush)
                            {
                                for (int i = 0; i < howManyWillPush; i++)
                                {
                                    CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord())
                                        chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                    if (chooseDirection.ToCoord() == dragDirection.ToCoord() * (-1))
                                        chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + dragDirection.ToCoord() * i;

                                    var worldChosenPosition = chosenPosition.ToWorld();
                                    var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                    marbles.transform.localPosition = worldChosenPosition;
                                    marbles.transform.localRotation = Quaternion.identity;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }

                if (draggingMarble != null)
                {
                    if (draggingMarble.fallen)
                    {
                        draggingMarble.GetComponent<Rigidbody>().isKinematic = false;
                    }
                }

                dragDirectionFixed = false;
                dragStarted = false;

            }

            if (dragStarted)
            {
                var angle = Mathf.Atan2(dragStartMousePosition.z - currentMousePosition.z, dragStartMousePosition.x - currentMousePosition.x) * Mathf.Rad2Deg;

                if (!dragDirectionFixed)
                {
                    dragDirection = (CubeDirection)Mathf.Round((angle + 150) / 60);
                }
                var directionCoord = dragDirection.ToCoord();

                var startPosition = dragStartMarblePosition;
                var endPosition = startPosition + directionCoord;

                var worldStartPosition = startPosition.ToWorld();
                var worldEndPosition = endPosition.ToWorld();

                const float dragThreshold = 6f;
                var dragLength = Vector3.Dot(currentMousePosition - dragStartMousePosition, directionCoord.ToWorld().normalized);
                var t = dragLength / dragThreshold;
                dragDirectionFixed = (t > 0.15);

                switch (context.playerContext)
                {
                    case "Choose":
                        var secondMarble = FindWithCoord((AxialCoord)(startPosition + directionCoord) - board.settings.placementOffset);
                        var thirdMarble = FindWithCoord((AxialCoord)(startPosition + directionCoord * 2) - board.settings.placementOffset);
                        
                        if (draggingMarble.fallen)
                        {
                            secondMarble = null;
                            thirdMarble = null;
                        }

                        if (t <= 0.15)
                        {
                            if (secondMarble != null)
                                secondMarble.PaintOrigin(false);
                            howManyIsChosen = 1;
                        }

                        if (t > 0.15 && secondMarble != null && secondMarble.playerIndex == context.currentPlayerIndex)
                        {
                            secondMarble.PaintSelectColor();
                            if(thirdMarble != null)
                                thirdMarble.PaintOrigin(false);
                            howManyIsChosen = 2;
                        }

                        if (t > 0.43 && thirdMarble != null && thirdMarble.playerIndex == context.currentPlayerIndex)
                        {
                            thirdMarble.PaintSelectColor();
                            howManyIsChosen = 3;
                        }

                        break;
                    case "Move":
                        if (!CanPushMarble(chosenMarbleStart, chooseDirection, howManyIsChosen, directionCoord))
                        {
                            t = 0.05f;
                        }

                        for (int i = 0; i < howManyIsChosen; i++) {
                            var chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * i;
                            var worldChosenPosition = chosenPosition.ToWorld();
                            var worldMovePosition = (chosenPosition + directionCoord).ToWorld();
                            var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                            Motion(worldChosenPosition, worldMovePosition, t, directionCoord, marbles);
                        }
                        if (opponentPush)
                        {
                            for (int i = 0; i < howManyWillPush; i++)
                            {
                                CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                                if (chooseDirection.ToCoord() == directionCoord)
                                    chosenPosition = chosenMarbleStart + chooseDirection.ToCoord() * (howManyIsChosen + i);
                                if (chooseDirection.ToCoord() == directionCoord * (-1))
                                    chosenPosition = chosenMarbleStart - chooseDirection.ToCoord() + directionCoord * i;
                                var worldChosenPosition = chosenPosition.ToWorld();
                                var worldMovePosition = (chosenPosition + directionCoord).ToWorld();
                                var marbles = FindWithCoord((AxialCoord)chosenPosition - board.settings.placementOffset);
                                Motion(worldChosenPosition, worldMovePosition, t, directionCoord, marbles);
                            }
                        }

                        wasValidMove = (t >= 1);
                        break;
                    default:
                        break;
                }

                if (draggingMarble != null) {
                    if (draggingMarble.fallen)
                        draggingMarble.transform.localPosition = draggingMarble.DragLimit(draggingMarble.transform.localPosition, currentMousePosition);
                }
            }
        }
    }
}