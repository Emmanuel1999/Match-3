using UnityEngine;


public static class Itemdatabase
{
    public static Item[] Items { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize() => Items = Resources.LoadAll<Item>("Items/"); //Grabs all the  assets in our asset folder 
                                                                                    //stores them in an array for ease of access

}
