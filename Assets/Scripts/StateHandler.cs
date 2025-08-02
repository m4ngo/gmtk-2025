using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StateHandler : MonoBehaviour
{
    [System.Serializable]
    private class BoardState
    {
        public List<Vector2Int> snakePositions;
        public BoardTile[,] boardTiles;
        public int maxSnakeSize;
        public Vector2Int facingDir;

        public BoardState(List<Vector2Int> snakePositions, BoardTile[,] boardTiles, int maxSnakeSize, Vector2Int facingDir)
        {
            // Make sure to clone all references! Don't get mutated
            this.snakePositions = new List<Vector2Int>(snakePositions);
            this.boardTiles = CloneBoardTiles(boardTiles);
            this.maxSnakeSize = maxSnakeSize;
            this.facingDir = facingDir;
        }
    }

    [System.Serializable]
    private class BoardTile
    {
        public enum Type
        {
            WALL,
            APPLE,
            PORTAL,
            BONFIRE,
        }

        public Type type;
        public GameObject tileObject;
        public bool isActive;

        public BoardTile Clone()
        {
            return new BoardTile(this.type, this.tileObject, this.isActive);
        }

        public BoardTile(Type type, GameObject tileObject, bool isActive = true)
        {
            this.type = type;
            this.tileObject = tileObject;
            this.isActive = isActive;
        }

        public void Update()
        {
            if (tileObject != null)
            {
                tileObject.SetActive(isActive);
            }
        }

        public void SetActive(bool isActive)
        {
            this.isActive = isActive;
            tileObject.SetActive(isActive);
        }
    }

    private static BoardTile[,] CloneBoardTiles(BoardTile[,] tiles)
    {
        BoardTile[,] boardTiles = new BoardTile[tiles.GetLength(0), tiles.GetLength(1)];
        for (int x = 0; x < boardTiles.GetLength(0); x++)
        {
            for (int y = 0; y < boardTiles.GetLength(1); y++)
            {
                boardTiles[x, y] = tiles[x, y].Clone();
            }
        }
        return boardTiles;
    }

    [Header("Board State")]
    [SerializeField] private Vector2Int boardSize = new Vector2Int(8, 8);
    [SerializeField] private SpriteRenderer boardSprite;
    [SerializeField] private List<BoardState> boardHistory = new List<BoardState>();
    private List<Vector2Int> lanterns = new List<Vector2Int>();
    private bool gameover = false;

    [Header("Board Init Detector")]
    [SerializeField] private LayerMask boardTileMask;
    private BoardTile[,] boardTiles;

    [Header("Snake State")]
    [SerializeField] private List<Vector2Int> snakePositions = new List<Vector2Int>();
    [SerializeField] private int maxSnakeSize = 8;
    private SnakeRenderer snake;
    [SerializeField] private Vector2Int facingDir;

    public bool HasSnakePos(Vector2Int pos)
    {
        return snakePositions.Contains(pos);
    }

    private void Start()
    {
        // Initialize board
        InitializeBoard();

        // Initialize snake
        snake = GetComponent<SnakeRenderer>();
        if (snakePositions.Count >= 2)
        {
            facingDir = snakePositions[snakePositions.Count - 1] - snakePositions[snakePositions.Count - 2];
        }
        snake.SetPositions(snakePositions, facingDir);
        UpdateBoardState(facingDir);
    }

    private void InitializeBoard()
    {
        boardSprite.size = boardSize;
        boardSprite.transform.position = new Vector2(boardSize.x * 0.5f - 0.5f, boardSize.y * 0.5f - 0.5f);
        GameObject.FindGameObjectWithTag("CameraParent").transform.position = boardSprite.transform.position;
        boardTiles = new BoardTile[boardSize.x, boardSize.y];

        // Check every tile on the board
        for (int x = 0; x < boardSize.x; x++)
        {
            for (int y = 0; y < boardSize.y; y++)
            {
                Collider2D point = Physics2D.OverlapCircle(new Vector2(x, y), 0.05f, boardTileMask);
                if (point == null)
                {
                    boardTiles[x, y] = new BoardTile(BoardTile.Type.WALL, null, false);
                }
                else
                {
                    // Autodetect the tile's type using GameObject tags
                    foreach (BoardTile.Type type in Enum.GetValues(typeof(BoardTile.Type)))
                    {
                        if (point.CompareTag(type.ToString()))
                        {
                            boardTiles[x, y] = new BoardTile(type, point.gameObject);
                            if (type == BoardTile.Type.BONFIRE)
                            {
                                lanterns.Add(new Vector2Int(x, y));
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    private void Update()
    {
        GetInput();
    }

    private void GetInput()
    {
        if (gameover)
        {
            return;
        }

        bool paused = GUIManager.Instance.GetPage("game_settings").pageObject.activeInHierarchy;
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            GUIManager.Instance.SetPage(!paused ? "game_settings" : "game");
        }

        if (paused)
        {
            return;
        }

        // Undo
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Undo();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            SoundManager.Instance.Play(4, 0.15f, true);
            SoundManager.Instance.Play(6, 0.15f, true);
            SoundManager.Instance.Play(0, 0.15f, true);
            SoundManager.Instance.Play(2, 0.15f, true);
        }

        // Snake Movement
        Vector2Int dir = Vector2Int.zero;
        dir.x = Input.GetKeyDown(KeyCode.RightArrow) ? 1 : Input.GetKeyDown(KeyCode.LeftArrow) ? -1 : 0;
        dir.y = Input.GetKeyDown(KeyCode.UpArrow) ? 1 : Input.GetKeyDown(KeyCode.DownArrow) ? -1 : 0;

        // Prevent moving in two directions at once, in case of frame perfect input
        if (dir.sqrMagnitude > 1)
        {
            dir.y *= 0;
        }

        if (dir != Vector2.zero)
        {
            Vector2Int end = Vector2Int.RoundToInt(snakePositions[snakePositions.Count - 1] + dir);
            // Prevent the snake from moving out of bounds or over itself
            if (end.x >= boardSize.x || end.y >= boardSize.y || end.x < 0 || end.y < 0)
            {
                snake.Shake(dir);
                return;
            }
            if (snakePositions.Contains(end))
            {
                if (snakePositions.Count >= 2)
                {
                    if (facingDir + snakePositions[snakePositions.Count - 1] == snakePositions[0] && end == snakePositions[0])
                    {
                        CheckWinState();
                    }
                }
                if (!gameover)
                {
                    snake.Shake(dir);
                }
                return;
            }

            if (boardTiles[end.x, end.y].isActive)
            {
                // World interactions
                switch (boardTiles[end.x, end.y].type)
                {
                    case BoardTile.Type.WALL:
                        snake.Shake(dir);
                        return;
                    case BoardTile.Type.BONFIRE:
                        snake.Shake(dir);
                        return;
                    case BoardTile.Type.APPLE:
                        RemoveTile(end);
                        maxSnakeSize++;
                        SoundManager.Instance.Play(12, 0.6f, true);
                        break;
                    case BoardTile.Type.PORTAL:
                        EffectManager.Instance.SpawnEffect(0, end);
                        snakePositions.Add(end);

                        if (snakePositions.Count > maxSnakeSize)
                        {
                            snakePositions.RemoveAt(0);
                        }
                        end = Vector2Int.RoundToInt(boardTiles[end.x, end.y].tileObject.transform.GetChild(0).position);
                        EffectManager.Instance.SpawnEffect(0, end);
                        SoundManager.Instance.Play(11, 0.6f, true);
                        break;
                }
            }

            facingDir = dir;
            snakePositions.Add(end);

            if (snakePositions.Count > maxSnakeSize)
            {
                snakePositions.RemoveAt(0);
            }

            snake.SetPositions(snakePositions, facingDir);
            MoveSound();

            // Board state has changed
            UpdateBoardState(dir);
        }
    }

    private void CheckWinState()
    {
        foreach (Vector2Int lantern in lanterns)
        {
            // Ensure all lanterns are lit
            if (!boardTiles[lantern.x, lantern.y].tileObject.transform.GetChild(0).gameObject.activeInHierarchy)
            {
                return;
            }
        }

        gameover = true;
        StartCoroutine(Win());
    }

    private System.Collections.IEnumerator Win()
    {
        Debug.Log("Game complete!");

        SoundManager.Instance.Play(0, 0.3f, true);
        yield return new WaitForSecondsRealtime(0.05f);
        SoundManager.Instance.Play(2, 0.3f, true);
        yield return new WaitForSecondsRealtime(0.05f);
        SoundManager.Instance.Play(4, 0.3f, true);
        yield return new WaitForSecondsRealtime(0.05f);
        SoundManager.Instance.Play(7, 0.3f, true);

        snakePositions.Add(snakePositions[0]);
        snake.SetPositions(snakePositions, facingDir);
        snake.Bite(snakePositions[0]);

        yield return new WaitForSecondsRealtime(1f);

        GameManager.Instance.CompleteLevel();
    }

    private void RemoveTile(Vector2Int pos)
    {
        boardTiles[pos.x, pos.y].SetActive(false);
    }

    private void UpdateBoardState(Vector2Int dir)
    {
        CheckLanterns();
        GameManager.Instance.SetSnakeMeter(maxSnakeSize, snakePositions.Count);
        boardHistory.Add(new BoardState(snakePositions, boardTiles, maxSnakeSize, dir));
    }

    private void Undo()
    {
        if (boardHistory.Count <= 1)
        {
            return;
        }
        boardHistory.RemoveAt(boardHistory.Count - 1);
        MoveSound();
        SetBoardState(boardHistory[boardHistory.Count - 1]);
    }

    private void MoveSound()
    {
        if ((boardHistory.Count - 1) % 7 == 0 && boardHistory.Count > 2)
        {
            SoundManager.Instance.Play(7, 0.2f, true);
            SoundManager.Instance.Play(0, 0.2f, true);
        }
        else
        {
            SoundManager.Instance.Play((boardHistory.Count - 1) % 7, 0.3f, true);
        }
    }

    private void LanternSound(bool ignite)
    {
        if (ignite)
        {
            SoundManager.Instance.Play(13, 0.5f, true);
            return;
        }
        SoundManager.Instance.Play(14, 0.4f, true);
    }

    private void CheckLanterns()
    {
        foreach (Vector2Int lantern in lanterns)
        {
            bool active =
                HasSnakePos(lantern + Vector2Int.up) &&
                HasSnakePos(lantern + Vector2Int.down) &&
                HasSnakePos(lantern + Vector2Int.left) &&
                HasSnakePos(lantern + Vector2Int.right);
            GameObject orb = boardTiles[lantern.x, lantern.y].tileObject.transform.GetChild(0).gameObject;
            if (active && !orb.activeInHierarchy)
            {
                EffectManager.Instance.SpawnEffect(1, lantern);
                LanternSound(true);
            }
            else if (!active && orb.activeInHierarchy)
            {
                EffectManager.Instance.SpawnEffect(2, lantern);
                LanternSound(false);
            }
            orb.SetActive(active);
        }
    }

    private void SetBoardState(BoardState state)
    {
        snakePositions = new List<Vector2Int>(state.snakePositions);
        snake.SetPositions(snakePositions, state.facingDir);
        maxSnakeSize = state.maxSnakeSize;
        facingDir = state.facingDir;

        GameManager.Instance.SetSnakeMeter(maxSnakeSize, snakePositions.Count);

        boardTiles = CloneBoardTiles(state.boardTiles);
        for (int x = 0; x < boardTiles.GetLength(0); x++)
        {
            for (int y = 0; y < boardTiles.GetLength(1); y++)
            {
                boardTiles[x, y].Update();
            }
        }

        CheckLanterns();
    }
}
