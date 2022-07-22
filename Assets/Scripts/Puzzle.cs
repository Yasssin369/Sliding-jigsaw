using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private int blocksPerLine = 4;
    [SerializeField] private Texture2D image;
    [SerializeField] private int shuffleLength=20;
    [SerializeField] private float moveDuration = .35f;
    [SerializeField] private float ShufflemoveDuration = .1f;

    enum PuzzleState { Solved, Shuffling, InPlay };
    PuzzleState state; 

    Block emptyBlock;
    Queue<Block> inputs;
    bool blockIsMoving;
    Block[,] blocks;
    int shuffleMoveRemaining;
    Vector2Int previousShuffleOffset;
    void Start()
    {
        CreatePuzzle();
        
    }
     void Update()
    {
        if (state == PuzzleState.Solved && Input.GetKeyDown(KeyCode.Space))
        {
            StartShuffle();
        }
    }
    void CreatePuzzle()
    {
        blocks = new Block[blocksPerLine, blocksPerLine];
        Texture2D[,] imageSlices = ImageSlicer.GetSllices(image, blocksPerLine);
        for (int y = 0; y < blocksPerLine; y++)
        {
            for (int x = 0; x < blocksPerLine; x++)
            {
                GameObject blockObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
                blockObject.transform.position = -Vector2.one * (blocksPerLine - 1) * .5f + new Vector2(x, y);
                blockObject.transform.parent = transform;
                Block block = blockObject.AddComponent<Block>();
                block.OnBlockPressed += PlayerMoveBlockInput;
                block.OnFinishedMoving += OnBlockFinishedMoving;
                block.Init(imageSlices[x, y], new Vector2Int(x, y));
                blocks[x, y] = block;
                if(y==0 && x==blocksPerLine-1)
                {
                    //blockObject.SetActive (false);
                    emptyBlock = block;
                }
            }
        }

        Camera.main.orthographicSize = blocksPerLine * .55f;
        inputs = new Queue<Block>();
    }
    void StartShuffle()
    {
        state = PuzzleState.Shuffling;
        shuffleMoveRemaining = shuffleLength;
        emptyBlock.gameObject.SetActive(false);
        MakeNextShuffleMove();
    }
    void PlayerMoveBlockInput(Block blockToMove)
    {
        if (state == PuzzleState.InPlay)
        {
            inputs.Enqueue(blockToMove);
            NextMove();
        }
    }
    void MoveBlock(Block blockToMove , float duration)
    {
        if ((blockToMove.coord - emptyBlock.coord).sqrMagnitude == 1)
        {
            blocks[blockToMove.coord.x, blockToMove.coord.y] = emptyBlock;
            blocks[emptyBlock.coord.x, emptyBlock.coord.y] = blockToMove;

            Vector2Int targetCoord = emptyBlock.coord;
            emptyBlock.coord = blockToMove.coord;
            blockToMove.coord = targetCoord;

            Vector2 targetPositon = emptyBlock.transform.position;
            emptyBlock.transform.position = blockToMove.transform.position;
            blockToMove.MoveToPosition(targetPositon, duration);
            blockIsMoving = true;
        }
    }
    void NextMove()
    {
        while (inputs.Count > 0 && !blockIsMoving)
        {
            MoveBlock(inputs.Dequeue(),moveDuration);

        }
    }
    void OnBlockFinishedMoving()
    {
        blockIsMoving = false;
        CheckIfSolved();
        if (state == PuzzleState.InPlay) 
        { 
        NextMove();
        }
        else if(state== PuzzleState.Shuffling)
        {
            if (shuffleMoveRemaining > 0)
            {
                MakeNextShuffleMove();
            }
            else
            {
                state = PuzzleState.InPlay;
            }
        }

    }
    void MakeNextShuffleMove()
    {
        Vector2Int[] offsets = { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };
        int randomIndex = Random.Range(0, offsets.Length);
        for (int i = 0; i < offsets.Length; i++)
        {

            Vector2Int offset = offsets[(randomIndex +i)% offsets.Length];
            if (offset != previousShuffleOffset * -1)
            {
                Vector2Int moveBlockCoord = emptyBlock.coord + offset;
                if (moveBlockCoord.x >= 0 && moveBlockCoord.x < blocksPerLine && moveBlockCoord.y >= 0 && moveBlockCoord.y < blocksPerLine)
                {
                    MoveBlock(blocks[moveBlockCoord.x, moveBlockCoord.y],ShufflemoveDuration);
                    shuffleMoveRemaining--;
                    previousShuffleOffset = offset;
                    break;
                }
            }
        }

    }
    void CheckIfSolved()
    {
        foreach (Block block in blocks)
        {
            if(!block.IsAtStartingCoord())
            {
                return;
            }
        }
        state = PuzzleState.Solved;
        emptyBlock.gameObject.SetActive(true);
    }
}
