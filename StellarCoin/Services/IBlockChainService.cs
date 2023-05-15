namespace StellarCoin.Models
{
    public interface IBlockChainService
    {
        Block LastBlock();

        int NewTransaction(string sender, string recipient, int amount);

        void RegisterNode(string address);

        bool ResolveConflicts();

        bool ValidChain(List<Block> chain);

        int ProofOfWork(Block lastBlock);

        Block NewBlock(string previousHash, int proof);

        IList<Block> GetChain();

        IList<string> GetNodes();

        decimal GetWalletBalance(string address);
    }
}