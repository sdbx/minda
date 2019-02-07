using System.Collections;
using System.Collections.Generic;
using Abalone.Data;
using UnityEngine;

namespace Abalone
{
    public struct TheCase
    {
        public AxialCoord target;
        public int chooseDirection, moveDirection, howManyIsChosen, howManyWillPush;
        public Priority priority;
        public bool opponentPush, pushOut;

        public TheCase(AxialCoord target, int chooseDirection, int moveDirection, int howManyIsChosen, int howManyWillPush, Priority priority, bool opponentPush, bool pushOut)
        {
            this.target = target;
            this.chooseDirection = chooseDirection;
            this.moveDirection = moveDirection;
            this.howManyIsChosen = howManyIsChosen;
            this.howManyWillPush = howManyWillPush;
            this.priority = priority;
            this.opponentPush = opponentPush;
            this.pushOut = pushOut;
        }

        public override string ToString()
        {
            return $"({target}, {(CubeDirection)chooseDirection}, {(CubeDirection)moveDirection}, {howManyIsChosen}, {priority})";
        }
    }

    public enum Priority
    {
        Trinity = 1,
        attackAndDefense,
        defenseAndReduce,
        defense,
        attackAndReduce,
        attack,
        opponentPossibleReduce,
        nothing
    }

    public class AI : MonoBehaviour
    {
        private int howManyWillPush, howMuchIsPossible, pushOutables, superior;
        private TheCase[] possibles;
        private const int max = 136;
        private bool opponentPush, pushOut;
        private Board board;
        private int[,] data;
        public int aiIndex;

        public TheCase Thinking(int[,] data, Board board, int index)
        {
            aiIndex = index;
            this.board = board;
            this.data = (int[,])data.Clone();
            possibles = PossibleThingsGet(this.data, aiIndex);
            var superiors = GetSuperiors();
            //for (var i = 0; i < superior; i++)
            //    Debug.Log(superiors[i]);
            var result = superiors[Random.Range(0, superior)];
            //Debug.Log(result);
            return result;
        }

        private TheCase[] GetSuperiors()
        {
            var aiPossibles = howMuchIsPossible;

            var R = new int[aiPossibles];
            var A = new bool[aiPossibles];
            var D = new int[aiPossibles];
            var superiorCases = new TheCase[aiPossibles];

            var rMin = 136;
            var dMin = 24;
            var rank = (Priority)8;
            var j = 0;

            for (int i = 0; i < aiPossibles; i++)
            {
                var afterData = AfterData(possibles[i], aiIndex);
                var opponentPossibles = PossibleThingsGet(afterData, 3 - aiIndex);

                if (possibles[i].target == new AxialCoord(2, 8))
                {
                    for(var k=0; k<howMuchIsPossible; k++)
                    {
                        Debug.Log(opponentPossibles[k]+" ");
                    }
                }

                if (howMuchIsPossible <= rMin)
                {
                    if (howMuchIsPossible < rMin) R = new int[aiPossibles];
                    rMin = howMuchIsPossible;
                    R[i] = rMin;
                }

                if (possibles[i].pushOut)
                {
                    A[i] = true;
                }

                if (pushOutables <= dMin)
                {
                    if (pushOutables < dMin) D = new int[aiPossibles];
                    dMin = pushOutables;
                    D[i] = dMin;
                }
            }

            for(int i = 0; i < aiPossibles; i++)
            {
                var r = R[i] > 0;
                var a = A[i];
                var d = D[i] > 0;

                if (r && a && d) possibles[i].priority = Priority.Trinity;
                if (!r && a && d) possibles[i].priority = Priority.attackAndDefense;
                if (r && !a && d) possibles[i].priority = Priority.defenseAndReduce;
                if (!r && !a && d) possibles[i].priority = Priority.defense;
                if (r && a && !d) possibles[i].priority = Priority.attackAndReduce;
                if (!r && a && !d) possibles[i].priority = Priority.attack;
                if (r && !a && !d) possibles[i].priority = Priority.opponentPossibleReduce;
                if (!r && !a && !d) possibles[i].priority = Priority.nothing;

                if(rank > possibles[i].priority)
                {
                    rank = possibles[i].priority;
                    superiorCases = new TheCase[aiPossibles];
                    j = 0;
                }

                if (rank == possibles[i].priority)
                {
                    superiorCases[j] = possibles[i];
                    j++;
                }         
            }

            superior = j;
            return superiorCases;
        }

        private int[,] AfterData(TheCase theCase, int index)
        {
            var theData = (int[,])data.Clone();
            var chooseDirection = (CubeDirection)theCase.chooseDirection;
            var moveDirection = (CubeDirection)theCase.moveDirection;

            for (int j = 0; j < theCase.howManyWillPush; j++)
            {
                var i = theCase.howManyWillPush - j - 1;

                CubeCoord chosenPosition = new CubeCoord(0, 0, 0);
                if (chooseDirection.ToCoord() == moveDirection.ToCoord())
                    chosenPosition = theCase.target + (AxialCoord)chooseDirection.ToCoord() * (theCase.howManyIsChosen + i);
                if (chooseDirection.ToCoord() == moveDirection.ToCoord() * (-1))
                    chosenPosition = theCase.target - (AxialCoord)chooseDirection.ToCoord() + (AxialCoord)moveDirection.ToCoord() * i;
                
                //var chosenPosition = theCase.target + (AxialCoord)chooseDirection.ToCoord() * (theCase.howManyIsChosen + i);

                var beforePosition = (AxialCoord)chosenPosition;
                var afterPosition = beforePosition + (AxialCoord)moveDirection.ToCoord();
                theData[beforePosition.x, beforePosition.z] = 0;

                if (!IsInArea(afterPosition.x, afterPosition.z))
                    continue;

                theData[afterPosition.x, afterPosition.z] = 3 - index;
            }
            for (int j = 0; j < theCase.howManyIsChosen; j++)
            {
                int i;
                if (chooseDirection.ToCoord() == moveDirection.ToCoord())
                    i = theCase.howManyIsChosen - j - 1;
                else
                    i = j;

                var beforePosition = theCase.target + (AxialCoord)chooseDirection.ToCoord() * i;
                var afterPosition = beforePosition + (AxialCoord)moveDirection.ToCoord();
                theData[beforePosition.x, beforePosition.z] = 0;
                theData[afterPosition.x, afterPosition.z] = index;
            }
            return theData;
        }

        private TheCase[] PossibleThingsGet(int[,] data, int index)
        {
            var possibles = new TheCase[max];
            pushOutables = 0;
            howMuchIsPossible = 0;

            for (int i = 0; i < board.settings.arraySize; i++)
            {
                for (int j = 0; j < board.settings.arraySize; j++)
                {
                    if (data[i, j] != index) continue;

                    for (int m = 1; m <= 3; m++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            var chooseCoord = ((CubeDirection)k).ToCoord();
                            if (!CanChoose(data, new AxialCoord(i, j), chooseCoord, m, index)) continue;
                            if (m == 1) k = 2;

                            for (int l = 0; l < 6; l++)
                            {
                                var moveCoord = ((CubeDirection)l).ToCoord();

                                if (CanPushMarble(data, new AxialCoord(i, j), chooseCoord, moveCoord, m, index))
                                {
                                    possibles[howMuchIsPossible] = new TheCase(new AxialCoord(i, j), k, l, m, howManyWillPush, Priority.nothing, opponentPush, pushOut);
                                    if (pushOut) pushOutables++;
                                    howMuchIsPossible++;
                                }
                            }
                        }
                    }
                }
            }

            return possibles;
        }

        private bool CanChoose(int[,] data, AxialCoord start, CubeCoord chooseCoord, int howManyIsChosen, int index)
        {
            if (howManyIsChosen >= 2) {
                if (!IsInArea((start + (AxialCoord)chooseCoord).x, (start + (AxialCoord)chooseCoord).z))
                    return false;
                if (data[(start + (AxialCoord)chooseCoord).x, (start + (AxialCoord)chooseCoord).z] != index)
                    return false;
            }
            if (howManyIsChosen == 3) {
                if (!IsInArea((start + (AxialCoord)chooseCoord * 2).x, (start + (AxialCoord)chooseCoord * 2).z))
                    return false;
                if (data[(start + (AxialCoord)chooseCoord * 2).x, (start + (AxialCoord)chooseCoord * 2).z] != index)
                    return false;
            }
            return true;
        }

        private bool CanPushMarble(int[,] data, AxialCoord start, CubeCoord chooseCoord, CubeCoord moveCoord, int howManyIsChosen, int index)
        {
            howManyWillPush = 0;
            opponentPush = false;
            pushOut = false;

            for (int i = 0; i < howManyIsChosen; i++)
            {
                var positionToBeMoved = start + (AxialCoord)(chooseCoord * i) + (AxialCoord)moveCoord;

                if (!IsInArea(positionToBeMoved.x, positionToBeMoved.z))
                    return false;

                if (data[positionToBeMoved.x, positionToBeMoved.z] != 0)
                {
                    var indexToMove = data[positionToBeMoved.x, positionToBeMoved.z];

                    if (index == indexToMove && !WasChosen(positionToBeMoved - start, chooseCoord, howManyIsChosen))
                        return false;

                    if (index != indexToMove && chooseCoord != moveCoord)
                        return false;
                }
            }

            if (chooseCoord == moveCoord)
            {
                while (true)
                {
                    var pushTargetPosition = start + (AxialCoord)(chooseCoord * howManyIsChosen) + (AxialCoord)(moveCoord * howManyWillPush);

                    if (!IsInArea(pushTargetPosition.x, pushTargetPosition.z)) break;

                    if (data[pushTargetPosition.x, pushTargetPosition.z] != 0)
                    {
                        var pushTargetIndex = data[pushTargetPosition.x, pushTargetPosition.z];

                        if (pushTargetIndex == index)
                            return false;

                        howManyWillPush++;
                    }

                    else
                    {
                        break;
                    }
                }

                var finalPosition = start + (AxialCoord)(chooseCoord * howManyIsChosen) + (AxialCoord)(moveCoord * howManyWillPush);
                if (!IsInArea(finalPosition.x, finalPosition.z))
                    pushOut = true;


                if (howManyIsChosen <= howManyWillPush)
                    return false;
                else
                    opponentPush = true;
            }
            return true;
        }

        private bool WasChosen(AxialCoord axialCoord, AxialCoord cubeDirection, int chosen)
        {
            switch (chosen)
            {
                case 1:
                    return axialCoord == new AxialCoord(0, 0);
                case 2:
                    return axialCoord == new AxialCoord(0, 0) || axialCoord == cubeDirection;
                case 3:
                    return axialCoord == new AxialCoord(0, 0) || axialCoord == cubeDirection || axialCoord == cubeDirection * 2;
                default:
                    return false;
            }
        }

        private bool IsInArea(int areaX, int areaZ)
        {
            if (areaX < 0 || areaX >= board.settings.arraySize) return false;
            if (areaZ < 0 || areaZ >= board.settings.arraySize) return false;
            if (Mathf.Abs(areaX + board.settings.placementOffset.x + areaZ + board.settings.placementOffset.z) > board.settings.cutThreshold) return false;
            return true;
        }
    }
}
