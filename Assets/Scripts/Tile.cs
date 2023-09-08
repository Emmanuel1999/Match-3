using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public sealed class Tile : MonoBehaviour
{
    public int x;
    public int y;

    private Item _item;

    public Item Item{
        get => _item;

        set {
            if(_item == value) return;

            _item = value;
            icon.sprite = _item.sprite;
        }
    }

    public Image icon;
    public Button button;

    //Properties that return the adjacent tiles to this tile
    public Tile Left => x > 0 ? Board.Instance.Tiles[x - 1, y] : null; //Checks if x > 0, return to the left, otherwise return null
    public Tile Top => y > 0 ? Board.Instance.Tiles[x, y] : null; //Moving clockwise - Left, Top, Right, Down.
    public Tile Right => x > Board.Instance.Width - 1 ? Board.Instance.Tiles[x + 1, y] : null;
    public Tile Down => x > Board.Instance.Height - 1 ? Board.Instance.Tiles[x, y + 1] : null;

   //Property to return all these points
    public Tile[] Neighbors => new[] { Left, Top, Right, Down }; //Turns the neighbors in order each time it's called

    private void Start()
    {
        button.onClick.AddListener(() => Board.Instance.Select(this)); //Tells the Board to select this tile on click
    }

    //Function that returns the adjacent or the connected tiles to this tile.
    public List<Tile> GetConnectedTiles(List<Tile> exclude = null) //Recursor function, MUST return a value.
    {
        var result = new List<Tile> {this, }; //Returns all the tiles connected to the tile directly or via it's neighbors
        
        if(exclude == null) //Prevents infinite loops with the connected tiles.
        {
            exclude = new List<Tile> { this, };
        }
        else
        {
            exclude.Add(this);
        }

        foreach (var neighbor in Neighbors) //Loops or iterates over the neighbors; checks for null neighbors & ignores it,
                                             //if it's excluded ignore it and if it doesn't have the same item, ignore it,
                                             //otherwise, return all the tiles connected to our neighbor.
        {
            if (neighbor == null || exclude.Contains(neighbor) || neighbor.Item != Item) continue;
    
            result.AddRange(neighbor.GetConnectedTiles(exclude)); 
        }

        return result;
    }


}
