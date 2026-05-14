using System.Collections.Generic;

public enum MapLayer
{
  Water,
  Foam,
  Shadow,
  Ground,
  Cliff,
  BoundaryCollider
}

public static class LayerPriority
{
  private static readonly Dictionary<MapLayer, int> _order = new()
  {
    { MapLayer.Water, 0 },
    { MapLayer.Foam, 1 },
    { MapLayer.Shadow, 2},
    { MapLayer.Ground, 3 },
    { MapLayer.Cliff, 4 },
    { MapLayer.BoundaryCollider, 5 },
  };

  public static int GetOrder(MapLayer layer) => _order[layer];
}
