using System;

namespace FPLibrary {

    /**
     *  @brief Generates random numbers based on a deterministic approach.
     **/
    /// <summary>
    /// 确定性随机数生成器（FPRandom）。
    /// <para>用途：基于梅森旋转（Mersenne Twister）算法生成确定性随机数——相同种子产生完全相同的序列，</para>
    /// <para>保证网络对战各客户端随机结果一致（帧同步确定性）。</para>
    /// </summary>
    public class FPRandom {
        // From http://www.codeproject.com/Articles/164087/Random-Number-Generation
        // Class FPRandom generates random numbers
        // from a uniform distribution using the Mersenne
        // Twister algorithm.
        /// <summary>梅森旋转状态数组大小。</summary>
        private const int N = 624;
        /// <summary>梅森旋转偏移。</summary>
        private const int M = 397;
        /// <summary>矩阵 A 常数。</summary>
        private const uint MATRIX_A = 0x9908b0dfU;
        /// <summary>高位移位掩码。</summary>
        private const uint UPPER_MASK = 0x80000000U;
        /// <summary>低位移位掩码。</summary>
        private const uint LOWER_MASK = 0x7fffffffU;
        /// <summary>最大随机整数。</summary>
        private const int MAX_RAND_INT = 0x7fffffff;
        /// <summary>梅森旋转魔法数组。</summary>
        private uint[] mag01 = { 0x0U, MATRIX_A };
        /// <summary>梅森旋转状态数组。</summary>
        private uint[] mt = new uint[N];
        /// <summary>当前状态索引。</summary>
        private int mti = N + 1;

        /**
         *  @brief Static instance of {@link FPRandom} with seed 1.
         **/
        /// <summary>静态默认实例（种子 1，网络对战共用）。</summary>
        public static FPRandom instance;

        /// <summary>
        /// 初始化静态实例（种子 1）。
        /// </summary>
        internal static void Init() {
            instance = New(1);
        }

        /**
         *  @brief Generates a new instance based on a given seed.
         **/
        /// <summary>
        /// 按指定种子创建新实例。
        /// </summary>
        /// <param name="seed">随机种子。</param>
        /// <returns>新的随机数生成器。</returns>
        public static FPRandom New(int seed) {
            FPRandom r = new FPRandom(seed);

            return r;
        }

        /// <summary>
        /// 私有构造函数（以当前毫秒为种子）。
        /// </summary>
        private FPRandom() {
            init_genrand((uint)DateTime.Now.Millisecond);
        }

        /// <summary>
        /// 私有构造函数（以指定整数为种子）。
        /// </summary>
        /// <param name="seed">随机种子。</param>
        private FPRandom(int seed) {
            init_genrand((uint)seed);
        }

        /// <summary>
        /// 私有构造函数（以整数数组初始化种子）。
        /// </summary>
        /// <param name="init">种子数组。</param>
        private FPRandom(int[] init) {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i)
                initArray[i] = (uint)init[i];
            init_by_array(initArray, (uint)initArray.Length);
        }

        /// <summary>
        /// 最大随机整数（0x7fffffff）。
        /// </summary>
        public static int MaxRandomInt { get { return 0x7fffffff; } }

        /**
         *  @brief Returns a random integer.
         **/
        /// <summary>
        /// 返回一个随机整数。
        /// </summary>
        /// <returns>随机整数。</returns>
        public int Next() {
            return genrand_int31();
        }

        /**
         *  @brief Returns a random integer.
         **/
        /// <summary>
        /// 通过静态实例返回随机整数。
        /// </summary>
        /// <returns>随机整数。</returns>
        public static int CallNext() {
            return instance.Next();
        }

        /**
         *  @brief Returns a integer between a min value [inclusive] and a max value [exclusive].
         **/
        /// <summary>
        /// 返回 [minValue, maxValue) 范围内的随机整数（自动交换越界参数）。
        /// </summary>
        /// <param name="minValue">最小值（含）。</param>
        /// <param name="maxValue">最大值（不含）。</param>
        /// <returns>随机整数。</returns>
        public int Next(int minValue, int maxValue) {
            if (minValue > maxValue) {
                int tmp = maxValue;
                maxValue = minValue;
                minValue = tmp;
            }

            int range = maxValue - minValue;

            return minValue + Next() % range;
        }

        /**
         *  @brief Returns a {@link FP} between a min value [inclusive] and a max value [inclusive].
         **/
        /// <summary>
        /// 返回 [minValue, maxValue] 范围内的随机定点数（含两端，千分位精度）。
        /// </summary>
        /// <param name="minValue">最小值（含）。</param>
        /// <param name="maxValue">最大值（含）。</param>
        /// <returns>随机定点数。</returns>
        public Fix64 Next(float minValue, float maxValue) {
            int minValueInt = (int)(minValue * 1000), maxValueInt = (int)(maxValue * 1000);

            if (minValueInt > maxValueInt) {
                int tmp = maxValueInt;
                maxValueInt = minValueInt;
                minValueInt = tmp;
            }

            return (Fix64.Floor((maxValueInt - minValueInt + 1) * NextFP() +
                minValueInt)) / 1000;
        }

        /**
         *  @brief Returns a integer between a min value [inclusive] and a max value [exclusive].
         **/
        /// <summary>
        /// 通过静态实例返回 [minValue, maxValue) 范围内的随机整数。
        /// </summary>
        /// <param name="minValue">最小值（含）。</param>
        /// <param name="maxValue">最大值（不含）。</param>
        /// <returns>随机整数。</returns>
        public static int Range(int minValue, int maxValue) {
            return instance.Next(minValue, maxValue);
        }

        /**
         *  @brief Returns a {@link FP} between a min value [inclusive] and a max value [inclusive].
         **/
        /// <summary>
        /// 通过静态实例返回 [minValue, maxValue] 范围内的随机定点数。
        /// </summary>
        /// <param name="minValue">最小值（含）。</param>
        /// <param name="maxValue">最大值（含）。</param>
        /// <returns>随机定点数。</returns>
        public static Fix64 Range(float minValue, float maxValue) {
            return instance.Next(minValue, maxValue);
        }

        /**
         *  @brief Returns a {@link FP} between 0.0 [inclusive] and 1.0 [inclusive].
         **/
        /// <summary>
        /// 返回 [0, 1] 范围内的随机定点数。
        /// </summary>
        /// <returns>随机定点数。</returns>
        public Fix64 NextFP() {
            return ((Fix64) Next()) / (MaxRandomInt);
        }

        /**
         *  @brief Returns a {@link FP} between 0.0 [inclusive] and 1.0 [inclusive].
         **/
        /// <summary>
        /// 静态属性：返回 [0, 1] 范围内的随机定点数。
        /// </summary>
        public static Fix64 value {
            get {
                return instance.NextFP();
            }
        }

        /**
         *  @brief Returns a random {@link FPVector} representing a point inside a sphere with radius 1.
         **/
        /// <summary>
        /// 静态属性：返回单位球内（各分量均在 [0,1]）的随机定点向量。
        /// </summary>
        public static FPVector insideUnitSphere {
            get {
                return new FPVector(value, value, value);
            }
        }

        /// <summary>返回 [0,1) 范围内的随机浮点数。</summary>
        private float NextFloat() {
            return (float)genrand_real2();
        }

        /// <summary>返回 [0,1] 或 [0,1) 范围内的随机浮点数（按 includeOne）。</summary>
        /// <param name="includeOne">是否包含 1。</param>
        private float NextFloat(bool includeOne) {
            if (includeOne) {
                return (float)genrand_real1();
            }
            return (float)genrand_real2();
        }

        /// <summary>返回 (0,1) 范围内的随机浮点数。</summary>
        private float NextFloatPositive() {
            return (float)genrand_real3();
        }

        /// <summary>返回 [0,1) 范围内的随机双精度数。</summary>
        private double NextDouble() {
            return genrand_real2();
        }

        /// <summary>返回 [0,1] 或 [0,1) 范围内的随机双精度数（按 includeOne）。</summary>
        /// <param name="includeOne">是否包含 1。</param>
        private double NextDouble(bool includeOne) {
            if (includeOne) {
                return genrand_real1();
            }
            return genrand_real2();
        }

        /// <summary>返回 (0,1) 范围内的随机双精度数。</summary>
        private double NextDoublePositive() {
            return genrand_real3();
        }

        /// <summary>返回 53 位精度的随机双精度数。</summary>
        private double Next53BitRes() {
            return genrand_res53();
        }

        /// <summary>以当前毫秒为种子初始化。</summary>
        public void Initialize() {
            init_genrand((uint)DateTime.Now.Millisecond);
        }

        /// <summary>以指定整数为种子初始化。</summary>
        /// <param name="seed">随机种子。</param>
        public void Initialize(int seed) {
            init_genrand((uint)seed);
        }

        /// <summary>以整数数组初始化种子。</summary>
        /// <param name="init">种子数组。</param>
        public void Initialize(int[] init) {
            uint[] initArray = new uint[init.Length];
            for (int i = 0; i < init.Length; ++i)
                initArray[i] = (uint)init[i];
            init_by_array(initArray, (uint)initArray.Length);
        }

		/// <summary>
		/// 用单个种子初始化梅森旋转状态数组。
		/// </summary>
		/// <param name="s">随机种子。</param>
        private void init_genrand(uint s) {
            mt[0] = s & 0xffffffffU;
            for (mti = 1; mti < N; mti++) {
                mt[mti] = (uint)(1812433253U * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + mti);
                mt[mti] &= 0xffffffffU;
            }
        }

		/// <summary>
		/// 用种子数组初始化梅森旋转状态。
		/// </summary>
		/// <param name="init_key">种子数组。</param>
		/// <param name="key_length">种子数组长度。</param>
        private void init_by_array(uint[] init_key, uint key_length) {
            int i, j, k;
            init_genrand(19650218U);
            i = 1;
            j = 0;
            k = (int)(N > key_length ? N : key_length);
            for (; k > 0; k--) {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525U)) + init_key[j] + j);
                mt[i] &= 0xffffffffU;
                i++;
                j++;
                if (i >= N) {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
                if (j >= key_length)
                    j = 0;
            }
            for (k = N - 1; k > 0; k--) {
                mt[i] = (uint)((uint)(mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) *
                    1566083941U)) - i);
                mt[i] &= 0xffffffffU;
                i++;
                if (i >= N) {
                    mt[0] = mt[N - 1];
                    i = 1;
                }
            }
            mt[0] = 0x80000000U;
        }

		/// <summary>
		/// 生成一个 32 位随机无符号整数（梅森旋转核心算法）。
		/// </summary>
		/// <returns>随机无符号整数。</returns>
        uint genrand_int32() {
            uint y;
            if (mti >= N) {
                int kk;
                if (mti == N + 1)
                    init_genrand(5489U);
                for (kk = 0; kk < N - M; kk++) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + M] ^ (y >> 1) ^ mag01[y & 0x1U];
                }
                for (; kk < N - 1; kk++) {
                    y = (mt[kk] & UPPER_MASK) | (mt[kk + 1] & LOWER_MASK);
                    mt[kk] = mt[kk + (M - N)] ^ (y >> 1) ^ mag01[y & 0x1U];
                }
                y = (mt[N - 1] & UPPER_MASK) | (mt[0] & LOWER_MASK);
                mt[N - 1] = mt[M - 1] ^ (y >> 1) ^ mag01[y & 0x1U];
                mti = 0;
            }
            y = mt[mti++];
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680U;
            y ^= (y << 15) & 0xefc60000U;
            y ^= (y >> 18);
            return y;
        }

		/// <summary>
		/// 生成 31 位随机整数（正整数）。
		/// </summary>
		/// <returns>随机整数。</returns>
        private int genrand_int31() {
            return (int)(genrand_int32() >> 1);
        }

		/// <summary>
		/// 生成 [0,1] 范围内的随机定点数。
		/// </summary>
		/// <returns>随机定点数。</returns>
        Fix64 genrand_FP() {
            return (Fix64)genrand_int32() * (Fix64.One / (Fix64)4294967295);
        }

		/// <summary>生成 [0,1] 范围内的随机双精度数。</summary>
        double genrand_real1() {
            return genrand_int32() * (1.0 / 4294967295.0);
        }
		/// <summary>生成 [0,1) 范围内的随机双精度数。</summary>
        double genrand_real2() {
            return genrand_int32() * (1.0 / 4294967296.0);
        }

		/// <summary>生成 (0,1) 范围内的随机双精度数。</summary>
        double genrand_real3() {
            return (((double)genrand_int32()) + 0.5) * (1.0 / 4294967296.0);
        }

		/// <summary>生成 53 位精度的随机双精度数。</summary>
        double genrand_res53() {
            uint a = genrand_int32() >> 5, b = genrand_int32() >> 6;
            return (a * 67108864.0 + b) * (1.0 / 9007199254740992.0);
        }
    }

}