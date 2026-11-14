using UnityEngine;

[CreateAssetMenu(fileName = "AmmoDefinition", menuName = "Doomlike/Ammo Definition")]
public sealed class AmmoDefinition : ScriptableObject
{
    public string DisplayName = "Ammo";
    public int MaxAmount = 200;
}
