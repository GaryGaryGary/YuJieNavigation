namespace YuJie.Navigation
{
    public class JPSPlusBakedMap
    {
        /// <summary>
        /// 查找表
        /// </summary>
        public readonly int[,] BlockLUT;

        public readonly JPSPlusBakedMapBlock[] Blocks;

        public int Width => BlockLUT.GetLength(0);
        public int Height => BlockLUT.GetLength(1);

        public JPSPlusBakedMap(int[,] blockLUT, JPSPlusBakedMapBlock[] blocks)
        {
            BlockLUT = blockLUT;
            Blocks = blocks;
        }

        public class JPSPlusBakedMapBlock
        {
            /// <summary>
            /// 八方向距离
            /// </summary>
            public readonly int[] JumpDistances;

            public readonly Int2 Pos;

            public JPSPlusBakedMapBlock(in Int2 pos, int[] jumpDistances)
            {
                JumpDistances = jumpDistances;
                Pos = pos;
            }
        }
    }
}
