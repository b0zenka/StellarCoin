using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace StellarCoin.Models;

public class BlockChain : IBlockChainService
{
    private List<Transaction> currentTransactions;

    private List<Block> chain;

    private readonly HashSet<string> nodes;

    private static SHA256 sha256 = SHA256.Create();

    public BlockChain()
    {
        currentTransactions = new List<Transaction>();
        chain = new List<Block>();
        nodes = new HashSet<string>();

        NewBlock(previousHash: "1", proof: 100);
    }

    public void RegisterNode(string address)
    {
        var parseUrl = new Uri(address);

        if (!string.IsNullOrEmpty(parseUrl.Host))
            nodes.Add(parseUrl.Host);
        else if (!string.IsNullOrEmpty(parseUrl.LocalPath))
            nodes.Add(parseUrl.LocalPath);
        else
            throw new ArgumentException("Invalid URL");
    }

    public bool ValidChain(List<Block> chain)
    {
        Block lastBlock = chain.First();
        int currentIndex = 1;

        while (currentIndex < chain.Count)
        {
            Block block = chain[currentIndex];

            string lastBlockHash = Hash(lastBlock);

            if (block.PreviousHash != lastBlockHash)
                return false;

            if (!ValidProof(lastBlock.Proof, block.Proof, lastBlockHash))
                return false;

            lastBlock = block;
            currentIndex++;
        }

        return true;
    }

    public bool ResolveConflicts()
    {
        HashSet<string> neighbors = nodes;
        List<Block> newChain = null;

        int maxLength = chain.Count;

        foreach (var node in neighbors)
        {
            using var client = new HttpClient();
            HttpResponseMessage response = client.GetAsync($"http://{node}/chain").Result;

            if (!response.IsSuccessStatusCode)
                continue;

            var data = response.Content.ReadAsStringAsync().Result;

            dynamic json = JsonConvert.DeserializeObject(data);
            int length = json["length"];
            List<Block> remoteChain = json["chain"].ToObject<List<Block>>();

            if (length > maxLength && ValidChain(remoteChain))
            {
                maxLength = length;
                newChain = remoteChain;
            }
        }

        if (newChain != null)
        {
            chain = newChain;
            return true;
        }

        return false;
    }

    public int NewTransaction(string sender, string recipient, int amount)
    {
        Transaction transaction = new()
        {
            Sender = sender,
            Recipient = recipient,
            Amount = amount
        };

        currentTransactions.Add(transaction);
        return LastBlock().Index + 1;
    }

    public Block LastBlock() => chain.Last();

    public int ProofOfWork(Block lastBlock)
    {
        int lastProof = lastBlock.Proof;
        string lastHash = Hash(lastBlock);

        int proof = 0;

        while (!ValidProof(lastProof, proof, lastHash))
            proof++;

        return proof;
    }

    public Block NewBlock(string previousHash, int proof)
    {
        Block block = new()
        {
            Index = chain.Count + 1,
            Timestam = DateTime.UtcNow,
            Transactions = currentTransactions,
            Proof = proof,
            PreviousHash = previousHash ?? Hash(LastBlock())
        };

        currentTransactions.Clear();
        chain.Add(block);

        return block;
    }

    private bool ValidProof(int lastProof, int proof, string lastHash)
    {
        string guess = $"{lastProof}{proof}{lastHash}";
        byte[] guessBytes = Encoding.UTF8.GetBytes(guess);
        byte[] guessHasBytes = sha256.ComputeHash(guessBytes);
        string guessHash = BitConverter.ToString(guessHasBytes).Replace("-", "").ToLower();

        return guessHash.StartsWith("0000");
    }

    public static string Hash(Block block)
    {
        string blockString = JsonConvert.SerializeObject(
            block,
            Formatting.None,
            new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

        byte[] blockBytes = Encoding.UTF8.GetBytes(blockString);
        byte[] hashBytes = sha256.ComputeHash(blockBytes);

        return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    }

    public IList<Block> GetChain() => chain;

    public IList<string> GetNodes() => nodes.ToList();

    public decimal GetWalletBalance(string address)
    {
        decimal balance = 0;

        foreach (var block in GetChain())
        {
            foreach (var transaction in block.Transactions)
            {
                if (transaction.Sender == address)
                    balance -= transaction.Amount;

                if (transaction.Recipient == address)
                    balance += transaction.Amount;
            }
        }

        return balance;
    }
}