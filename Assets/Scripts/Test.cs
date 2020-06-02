using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class New : MonoBehaviour
{
 
    int allLvlArrayNum = 0;
    int allArrayLength = 2000;
    Vector3 newNodePos;
    float lineLength = 0.5f;
    float scaleGrid = 1f;
    int randomLoops;
    float extraDecrease;
    float minDistNode = 1f;
    float extraDecreaseStep = 0.1f;
    bool foundNodePos = false;
    Vector3 furtherstNewNodePos;
    int maxRandomLoops = 30;
    Vector3 camPos = new Vector3(0, 0, 5);
    Vector3[] NodePositions;
    Quaternion[] NodeRotations;
    Vector3[] NodeScales;
    Vector3 camUp = new Vector3(0, 1, 0);
    Vector3 fixedNormalScale = new Vector3(1f, 1f, 1f);
    int[,] linksContainerArray;
    int maxLinksPerLinkedNode = 9;
    int branchingLevels = 6;
    int linksPerNode = 3;
 
    void AssignValues()
    {
        NodePositions = new Vector3[allArrayLength];
        NodeRotations = new Quaternion[allArrayLength];
        NodeScales = new Vector3[allArrayLength];
        linksContainerArray = new int[allArrayLength, maxLinksPerLinkedNode];
    }
 
    void CreateLevel(int parentNodeNum, int childrenNum)
    {
        if (allLvlArrayNum < allArrayLength)
        {
            newNodePos = NodePositions[parentNodeNum] + Random.onUnitSphere * lineLength * scaleGrid;
            randomLoops = 0;
            extraDecrease = 0;
            float minDistScaled = minDistNode * scaleGrid;
            float lineLengthScaled = lineLength * scaleGrid;
            float extraDecreaseStepScaled = extraDecreaseStep * scaleGrid;
            float maxSqrMagnitude = 0f;
            float minUnderThresholdSq = 100f;
            int maxi = 0;
            //This inner loop checks the distance between the new node and all other nodes and ensures that the new nodes are no too close to existing nodes, gradually lowering the threshold from 1 unit by 0.1 units every 30 attempts
            while (foundNodePos == false)
            {
                foundNodePos = true;
                float minDist = minDistScaled - extraDecrease;
                float minDistSq = minDist * minDist;
                for (int i = 0; i < allLvlArrayNum; i++)
                {
                    //Discards the new node position if it is too close to other existing nodes. This might make it difficult to put into a job, since there is a dependency
                    Vector3 newNodeToAllNodes = NodePositions[i] - newNodePos;
                    float curSqrMagnitude = Vector3.SqrMagnitude(newNodeToAllNodes);
                    float underThresholdSq = minDistSq - curSqrMagnitude;
                    if (curSqrMagnitude < minDistSq)
                    {
                        //Debug.Log(underThresholdSq);
                        //if (underThresholdSq < minUnderThresholdSq && i > maxi)
                        if (underThresholdSq < minUnderThresholdSq)
                        {
                            minUnderThresholdSq = underThresholdSq;
                            maxSqrMagnitude = curSqrMagnitude;
                            furtherstNewNodePos = newNodePos;
                            maxi = i;
                        }
                        foundNodePos = false;
                        newNodePos = NodePositions[parentNodeNum] + Random.onUnitSphere * lineLengthScaled;
                        break;
                    }
                }
                randomLoops++;
                if (randomLoops > maxRandomLoops)
                {
                    extraDecrease += extraDecreaseStepScaled;
                    randomLoops = 0;
                    minDist = minDistScaled - extraDecrease;
                    minDistSq = minDist * minDist;
                    if (maxSqrMagnitude > minDistSq)
                    {
                        foundNodePos = true;
                        for (int i = 0; i < allLvlArrayNum; i++)
                        {
                            //Checks the node that was closest to the threshold so far against the new threshold
                            newNodePos = furtherstNewNodePos;
                            Vector3 newNodeToAllNodes = NodePositions[i] - newNodePos;
                            float curSqrMagnitude = Vector3.SqrMagnitude(newNodeToAllNodes);
                            float underThresholdSq = minDistSq - curSqrMagnitude;
                            if (curSqrMagnitude < minDistSq)
                            {
                                if (underThresholdSq < minUnderThresholdSq && i > maxi)
                                {
                                    minUnderThresholdSq = underThresholdSq;
                                    maxSqrMagnitude = curSqrMagnitude;
                                    maxi = i;
                                }
                                foundNodePos = false;
                                newNodePos = NodePositions[parentNodeNum] + Random.onUnitSphere * lineLengthScaled;
                                break;
                            }
                        }
                    }
                }
            }
            NodePositions[allLvlArrayNum] = newNodePos;
            Vector3 nodeToCam = camPos - NodePositions[allLvlArrayNum];
            NodeRotations[allLvlArrayNum] = Quaternion.LookRotation(nodeToCam, camUp);
            NodeScales[allLvlArrayNum] = fixedNormalScale;
            linksContainerArray[parentNodeNum, childrenNum + 1] = allLvlArrayNum;
            linksContainerArray[allLvlArrayNum, 0] = parentNodeNum;
            allLvlArrayNum++;
            foundNodePos = false;
        }
    }
 
    void CreateCenterNode()
    {
        NodePositions[0] = Vector3.zero;
        Vector3 nodeToCam = camPos - NodePositions[0];
        NodeRotations[allLvlArrayNum] = Quaternion.LookRotation(nodeToCam, camUp);
        NodeScales[allLvlArrayNum] = fixedNormalScale;
        linksContainerArray[0, 0] = 0;
        allLvlArrayNum++;
    }
 
    void CreateGrid()
    {
        CreateCenterNode();
        //We don't use fully nested for loops since this way we are building the tree gradually, which makes it more evenly spread
        //Level 1 - Node # = linksPerNode
        for (int i = 0; i < linksPerNode; i++)
        {
            CreateLevel(0, i);
        }
        //Level 2 - Node # = linksPerNode^2
        if (branchingLevels >= 2 && allLvlArrayNum < allArrayLength)
        {
            for (int i = 0; i < linksPerNode; i++)
            {
                for (int j = 0; j < linksPerNode; j++)
                {
                    CreateLevel(linksContainerArray[0, i + 1], j);
                }
            }
        }
        //Level 3 - Node # = linksPerNode^3
        if (branchingLevels >= 3 && allLvlArrayNum < allArrayLength)
        {
            for (int i = 0; i < linksPerNode; i++)
            {
                for (int j = 0; j < linksPerNode; j++)
                {
                    for (int k = 0; k < linksPerNode; k++)
                    {
                        CreateLevel(linksContainerArray[linksContainerArray[0, i + 1], j + 1], k);
                    }
                }
            }
        }
 
        //Level 4 - Node # = linksPerNode^4
        if (branchingLevels >= 4 && allLvlArrayNum < allArrayLength)
        {
            for (int i = 0; i < linksPerNode; i++)
            {
                for (int j = 0; j < linksPerNode; j++)
                {
                    for (int k = 0; k < linksPerNode; k++)
                    {
                        for (int l = 0; l < linksPerNode; l++)
                        {
                            CreateLevel(linksContainerArray[linksContainerArray[linksContainerArray[0, i + 1], j + 1], k + 1], l);
                        }
                    }
                }
            }
        }
        //Level 5 - Node # = linksPerNode^5
        if (branchingLevels >= 5 && allLvlArrayNum < allArrayLength)
        {
            for (int i = 0; i < linksPerNode; i++)
            {
                for (int j = 0; j < linksPerNode; j++)
                {
                    for (int k = 0; k < linksPerNode; k++)
                    {
                        for (int l = 0; l < linksPerNode; l++)
                        {
                            for (int m = 0; m < linksPerNode; m++)
                            {
                                CreateLevel(linksContainerArray[linksContainerArray[linksContainerArray[linksContainerArray[0, i + 1], j + 1], k + 1], l + 1], m);
                            }
                        }
                    }
                }
            }
        }
 
        //Level 6 - Node # = linksPerNode^6
        if (branchingLevels >= 6 && allLvlArrayNum < allArrayLength)
        {
            for (int i = 0; i < linksPerNode; i++)
            {
                for (int j = 0; j < linksPerNode; j++)
                {
                    for (int k = 0; k < linksPerNode; k++)
                    {
                        for (int l = 0; l < linksPerNode; l++)
                        {
                            for (int m = 0; m < linksPerNode; m++)
                            {
                                for (int n = 0; n < linksPerNode; n++)
                                {
                                    CreateLevel(linksContainerArray[linksContainerArray[linksContainerArray[linksContainerArray[linksContainerArray[0, i + 1], j + 1], k + 1], l + 1], m + 1], n);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
