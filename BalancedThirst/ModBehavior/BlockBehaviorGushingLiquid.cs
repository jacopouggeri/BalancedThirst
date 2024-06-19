using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace BalancedThirst.ModBehavior;

public class BlockBehaviorGushingLiquid : BlockBehavior
{
    private bool IsLiquidSourceBlock(Block b) => b.LiquidLevel == 7;
    private bool IsSameLiquid(Block b, Block o) => b.LiquidCode == o.LiquidCode;
    
    public BlockBehaviorGushingLiquid(Block block) : base(block)
    {
    }

    public override void OnBlockPlaced(IWorldAccessor world, BlockPos blockPos, ref EnumHandling handling)
    {
        base.OnBlockPlaced(world, blockPos, ref handling);
        if (world is IServerWorldAccessor serverWorld)
        {
            serverWorld.RegisterCallbackUnique(DoTryRise, blockPos, 150);
        }
    }

    public override void OnNeighbourBlockChange(IWorldAccessor world, BlockPos pos, BlockPos neibpos, ref EnumHandling handling)
    {
        base.OnNeighbourBlockChange(world, pos, neibpos, ref handling);
        BtCore.Logger.Warning("Gushing water neighbour change");
        if (world is IServerWorldAccessor serverWorld)
        {
            serverWorld.RegisterCallbackUnique(DoTryRise, pos, 150);
        }
    }
    
    public void DoTryRise(IWorldAccessor world, BlockPos pos, float dt)
    {
        if (2*CountColumnHeight(world.BlockAccessor, pos, this.block) < CountConnectedSourceBlocks(world.BlockAccessor, pos, this.block))
        {
            TryRise(world, pos);
        }
    }
    
    private void TryRise(IWorldAccessor world, BlockPos pos)
    {
        BtCore.Logger.Warning("Trying to rise");
        BlockPos abovePos = pos.UpCopy();
        var blockAccessor = world.BlockAccessor;
        if (blockAccessor.GetBlock(abovePos).IsReplacableBy(this.block) &&
            IsSurroundedBySolidBlocks(blockAccessor, abovePos))
        {
            blockAccessor.SetBlock(this.block.BlockId, abovePos, BlockLayersAccess.Fluid);
        }
    }

    private bool IsSurroundedBySolidBlocks(IBlockAccessor blockAccessor, BlockPos pos)
    {
        foreach (BlockFacing facing in BlockFacing.HORIZONTALS)
        {
            BlockPos adjacentPos = pos.AddCopy(facing);
            if (!blockAccessor.GetBlock(adjacentPos).SideSolid[facing.Opposite.Index])
            {
                return false;
            }
        }
        return true;
    }
    
    private int CountConnectedSourceBlocks(IBlockAccessor blockAccessor, BlockPos startPos, Block ourBlock)
    {
        HashSet<BlockPos> visited = new HashSet<BlockPos>();
        Queue<BlockPos> queue = new Queue<BlockPos>();
        queue.Enqueue(startPos);
        int count = 0;

        while (queue.Count > 0)
        {
            BlockPos currentPos = queue.Dequeue();
            if (!visited.Add(currentPos)) continue;

            Block block = blockAccessor.GetBlock(currentPos);
            if (IsSameLiquid(ourBlock, block) && IsLiquidSourceBlock(block))
            {
                count++;

                // Enqueue all adjacent blocks within a 4x4x4 cube
                for (int dx = -2; dx <= 2; dx++)
                {
                    for (int dy = -2; dy <= 2; dy++)
                    {
                        for (int dz = -2; dz <= 2; dz++)
                        {
                            BlockPos adjacentPos = currentPos.AddCopy(dx, dy, dz);
                            queue.Enqueue(adjacentPos);
                        }
                    }
                }
            }
        }

        return count;
    }
    
    private int CountColumnHeight(IBlockAccessor blockAccessor, BlockPos startPos, Block ourBlock)
    {
        int count = 0;
        BlockPos currentPos = startPos.DownCopy();
        while (true)
        {
            Block block = blockAccessor.GetBlock(currentPos);
            if (IsSameLiquid(ourBlock, block) && IsLiquidSourceBlock(block))
            {
                count++;
                currentPos.Down();
            }
            else
            {
                break;
            }
        }
        return count;
    }
}