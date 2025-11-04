using System;

namespace Monopoly.Core.Entities;

public class PropertyTrade
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int CellPosition { get; set; }
    public int BuyerPlayerId { get; set; }
    public int SellerPlayerId { get; set; }
    public int OfferedPrice { get; set; }
    public TradeStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }

    // Navigation properties
    public Game Game { get; set; } = null!;
    public GamePlayer BuyerPlayer { get; set; } = null!;
    public GamePlayer SellerPlayer { get; set; } = null!;
}

public enum TradeStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}
