namespace StellarCoin.Models;

public class Block
{
    public int Index { get; set; }

    public DateTime Timestam { get; set; }

    public List<Transaction> Transactions { get; set; }

    public int Proof { get; set; }

    public string PreviousHash { get; set; }
}