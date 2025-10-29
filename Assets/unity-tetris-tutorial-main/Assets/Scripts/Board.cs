using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-1)]
public class Board : MonoBehaviour
{
    private TetrisManager manager;
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++) {
            tetrominoes[i].Initialize();
        }
    }

    private void Start()
    {
        manager = FindObjectOfType<TetrisManager>();
        if (manager == null)
        {
            Debug.LogError("HATA: TetrisManager sahnede bulunamad! Lütfen bir GameObject'e ekleyin.");
        }
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        // Board.cs - SpawnPiece() fonksiyonu içinde

if (tetrominoes.Length == 0)
{
    Debug.LogError("HATA: Tetrominoes dizisi boş! Parçaları Editor'de atayın.");
    return; // Parça yoksa fonksiyondan hemen çık
}

// Güvenli rastgele seçim
int random = Random.Range(0, tetrominoes.Length);

// Hata veren satır artık güvenli olmalı
TetrominoData data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition)) {
            Set(activePiece);
        } else {
            GameOver();
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();

        // Do anything else you want on game over here..
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition)) {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition)) {
                return false;
            }
        }

        return true;
    }

    // Board.cs (veya Grid.cs) scriptinizdeki ClearLines fonksiyonunun YENİ HALİ

public void ClearLines()
{
    RectInt bounds = Bounds;
    int row = bounds.yMin;
    
    // TEMİZLENEN HAT SAYACINI BURADA BAŞLATIYORUZ
    int linesCleared = 0; 

    // Clear from bottom to top
    while (row < bounds.yMax)
    {
        // Bir hat dolu mu kontrol et
        if (IsLineFull(row)) {
            // Eğer doluysa, hattı temizle ve...
            LineClear(row);
            
            // ... sayacı artır!
            linesCleared++; 
        } else {
            // Dolu değilse, bir sonraki satıra geç
            row++;
        }
    }

    // ------------------------------------------------------------------
    // SKOR GÜNCELLEME: Tüm döngü bittikten sonra, eğer hat temizlendiyse
    // ------------------------------------------------------------------
    if (linesCleared > 0 && manager != null) 
    {
        // Toplam temizlenen hat sayısını TetrisManager'daki AddScore metoduna gönder
        manager.AddScore(linesCleared); 
    }
}

// LineClear(int row) fonksiyonu değişmeden kalabilir.

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position)) {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }

}
