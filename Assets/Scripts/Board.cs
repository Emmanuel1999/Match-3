using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class Board : MonoBehaviour
{
    public static Board Instance {get; private set; }

    [SerializeField] private AudioClip collectSound;

    [SerializeField] private AudioSource audioSource;

    public Row[] rows;
    public Tile[,] Tiles {get; private set; }

    public int Width => Tiles.GetLength(dimension:0);
    public int Height => Tiles.GetLength(dimension:1);

    private const float TweenDuration = 0.25f;

    private readonly List<Tile> _selection = new List<Tile>();  


    private void Awake() => Instance = this;

    private void Start()
    {
        Tiles = new Tile[rows.Max(row => row.tiles.Length), rows.Length];

        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = rows[y].tiles[x];

                tile.x = x;
                tile.y = y;

                tile.Item = Itemdatabase.Items[Random.Range(0, Itemdatabase.Items.Length)];

                Tiles[x, y] = tile;
            }
        }
    }

    public async void Select(Tile tile)
    {
        if(!_selection.Contains(tile))
        {
            if(_selection.Count < 0)
            {
                //Checks if the second tile (to be added to the selection) is one of the 1st tiles neighbors, otherwise it won't do anything
                if (Array.IndexOf(_selection[0].Neighbors, tile) != -1)
                    _selection.Add(tile);
            }
            else
            {
                _selection.Add(tile);
            }


        } //if there are no tiles selected add a tile
        
        if (_selection.Count < 2) return;  

        Debug.Log($"Selected tiles at ({_selection[0].x}, {_selection[0].y}) and ({_selection[1].x}, {_selection[1].y})");

        await Swap(_selection[0], _selection[1]); //awaits the completion of swapping two tiles.

        if (CanPop())
        {
            Pop();
        }
        else
        {
            await Swap(_selection[0], _selection[1]);   
        }

        _selection.Clear(); 
    }

    public async Task Swap(Tile tile1, Tile tile2)
    {
        var icon1 = tile1.icon; //Referencing 1st and 2nd icons used
        var icon2 = tile2.icon;

        var icon1Transform = icon1.transform; //Needing both 1st and 2nd icons transforms 
        var icon2Transform = icon2.transform;

        //Animation starts here
        var sequence = DOTween.Sequence(); //Starts sequence of two sprites moving from one position to the other.

        sequence.Join(icon1Transform.DOMove(icon2Transform.position, TweenDuration))
            .Join(icon2Transform.DOMove(icon2Transform.position, TweenDuration)); //Moves one icon position to another but changes nothing
                                                                                  // about the tile itself.

        await sequence.Play().AsyncWaitForCompletion(); //Awaits for our sequence to play and finishes it successfully.


        //To change the parents of the icons & the icons of the tiles
        icon1Transform.SetParent(tile2.transform); //Changes the parent of the first icon to be the second tile,
        icon2Transform.SetParent(tile1.transform); // & vice versa.

        tile1.icon = icon2; //Set the icons.
        tile2.icon = icon1;

        //To change the item
        var tile1Item = tile1.Item;

        tile1.Item = tile2.Item;
        tile2.Item = tile1Item; //Stores it in a temporary storage space to hold the items.
    }

    private bool CanPop() //Minus the first tile, it checks if the connected tiles are >= 2 then returns true, otherwise false.
    {
        for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
                if (Tiles[x, y].GetConnectedTiles().Skip(1).Count() >= 2)
                    return true;
        return false;
    }

    private async void Pop()
    {
        for (var y = 0;y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                var tile = Tiles[x, y];

                var connectedTiles = tile.GetConnectedTiles();

                if (connectedTiles.Skip(1).Count() < 2) continue;

                var deflateSequence = DOTween.Sequence();

                foreach (var connectedTile in connectedTiles) deflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.zero, TweenDuration));

                audioSource.PlayOneShot(collectSound);
                
                ScoreCounter.Instance.Score += tile.Item.value * connectedTiles.Count; //Produces a score depending on the number of tiles popped.


                await deflateSequence.Play().AsyncWaitForCompletion();

                

                var inflateSequence = DOTween.Sequence();

                //Once deflate sequence is complete, we iterate over the connected tiles & randomize their item
                foreach (var connectedTile in connectedTiles)
                {
                    connectedTile.Item = Itemdatabase.Items[Random.Range(0, Itemdatabase.Items.Length)];

                    inflateSequence.Join(connectedTile.icon.transform.DOScale(Vector3.one, TweenDuration));
                }

                await inflateSequence.Play().AsyncWaitForCompletion();

                x = 0;
                y = 0;
            }
        }
    }
}
