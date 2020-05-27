public enum HexDirection {
    NE, E, SE, SW, W, NW
}

public enum HexEdgeType {

    /// <summary>
    /// 平的
    /// </summary>
    Flat,
    /// <summary>
    /// 山坡
    /// </summary>
    Slope,
    /// <summary>
    /// 悬崖
    /// </summary>
    Cliff
}

public static class HexDirectionExtensions {

    public static HexDirection Opposite(this HexDirection direction) {
        return (int)direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDirection Previous(this HexDirection direction) {
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction) {
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
}