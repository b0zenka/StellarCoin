using Microsoft.AspNetCore.Mvc;
using StellarCoin.Models;

namespace StellarCoin.Controllers
{
    public class BlockChainController : ControllerBase
    {
        private readonly IBlockChainService blockChain;

        public BlockChainController(IBlockChainService blockChain)
        {
            this.blockChain = blockChain;
        }

        [HttpGet]
        [Route("mine/{address}")]
        public IActionResult Mine([FromRoute] string address)
        {
            var lastBlock = blockChain.LastBlock();
            int proof = blockChain.ProofOfWork(lastBlock);

            blockChain.NewTransaction(sender: "0", recipient: address, amount: 1);

            string previousHash = BlockChain.Hash(lastBlock);
            Block block = blockChain.NewBlock(previousHash, proof);

            var response = new
            {
                message = "New Block Forged",
                index = block.Index,
                transactions = block.Transactions,
                proof = block.Proof,
                previous_hash = block.PreviousHash
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("transactions/new")]
        public IActionResult NewTransaction(Transaction transaction)
        {
            if (transaction == null || string.IsNullOrEmpty(transaction.Sender))
            {
                return BadRequest("Invalid transaction data");
            }

            int index = blockChain.NewTransaction(transaction.Sender, transaction.Recipient, transaction.Amount);

            var response = new
            {
                message = $"Transaction will be added to Block {index}"
            };

            return Created("", response);
        }

        [HttpGet]
        [Route("chain")]
        public IActionResult FullChain()
        {
            var response = new
            {
                chain = blockChain.GetChain(),
                length = blockChain.GetChain().Count
            };

            return Ok(response);
        }

        [HttpPost]
        [Route("nodes/register")]
        public IActionResult RegisterNodes([FromBody] List<string> nodes)
        {
            if (nodes == null || nodes.Count == 0)
                return BadRequest("Invalid node data");

            foreach (string node in nodes)
                blockChain.RegisterNode(node);

            var response = new
            {
                message = "New nodes have been added",
                total_nodes = blockChain.GetNodes()
            };

            return Created("", response);
        }

        [HttpGet]
        [Route("nodes/resolve")]
        public IActionResult ResolveConflicts()
        {
            bool replaced = blockChain.ResolveConflicts();

            var response = new
            {
                message = replaced ? "Our chain was replaced" : "Our chain is authoritative",
                new_chain = blockChain.GetChain()
            };

            return Ok(response);
        }

        [HttpGet]
        [Route("count")]
        public IActionResult BlockCount()
        {
            int count = blockChain.GetChain().Count;
            var response = new
            {
                count
            };

            return Ok(response);
        }
    }
}