using System;

namespace Ionic.Zlib;

internal sealed class Tree
{
	private static readonly int HEAP_SIZE = 2 * InternalConstants.L_CODES + 1;

	internal static readonly int[] ExtraLengthBits = new int[29]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 1, 1,
		1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
		4, 4, 4, 4, 5, 5, 5, 5, 0
	};

	internal static readonly int[] ExtraDistanceBits = new int[30]
	{
		0, 0, 0, 0, 1, 1, 2, 2, 3, 3,
		4, 4, 5, 5, 6, 6, 7, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 13, 13
	};

	internal static readonly int[] extra_blbits = new int[19]
	{
		0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
		0, 0, 0, 0, 0, 0, 2, 3, 7
	};

	internal static readonly sbyte[] bl_order = new sbyte[19]
	{
		16, 17, 18, 0, 8, 7, 9, 6, 10, 5,
		11, 4, 12, 3, 13, 2, 14, 1, 15
	};

	internal const int Buf_size = 16;

	private static readonly sbyte[] _dist_code = new sbyte[512]
	{
		0, 1, 2, 3, 4, 4, 5, 5, 6, 6,
		6, 6, 7, 7, 7, 7, 8, 8, 8, 8,
		8, 8, 8, 8, 9, 9, 9, 9, 9, 9,
		9, 9, 10, 10, 10, 10, 10, 10, 10, 10,
		10, 10, 10, 10, 10, 10, 10, 10, 11, 11,
		11, 11, 11, 11, 11, 11, 11, 11, 11, 11,
		11, 11, 11, 11, 12, 12, 12, 12, 12, 12,
		12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
		12, 12, 12, 12, 12, 12, 12, 12, 12, 12,
		12, 12, 12, 12, 12, 12, 13, 13, 13, 13,
		13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
		13, 13, 13, 13, 13, 13, 13, 13, 13, 13,
		13, 13, 13, 13, 13, 13, 13, 13, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 14, 14, 14, 14, 14, 14, 14, 14,
		14, 14, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 15, 15, 15, 15,
		15, 15, 15, 15, 15, 15, 0, 0, 16, 17,
		18, 18, 19, 19, 20, 20, 20, 20, 21, 21,
		21, 21, 22, 22, 22, 22, 22, 22, 22, 22,
		23, 23, 23, 23, 23, 23, 23, 23, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		24, 24, 24, 24, 25, 25, 25, 25, 25, 25,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 28, 28,
		28, 28, 28, 28, 28, 28, 28, 28, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29, 29, 29, 29, 29, 29, 29, 29, 29,
		29, 29
	};

	internal static readonly sbyte[] LengthCode = new sbyte[256]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 8,
		9, 9, 10, 10, 11, 11, 12, 12, 12, 12,
		13, 13, 13, 13, 14, 14, 14, 14, 15, 15,
		15, 15, 16, 16, 16, 16, 16, 16, 16, 16,
		17, 17, 17, 17, 17, 17, 17, 17, 18, 18,
		18, 18, 18, 18, 18, 18, 19, 19, 19, 19,
		19, 19, 19, 19, 20, 20, 20, 20, 20, 20,
		20, 20, 20, 20, 20, 20, 20, 20, 20, 20,
		21, 21, 21, 21, 21, 21, 21, 21, 21, 21,
		21, 21, 21, 21, 21, 21, 22, 22, 22, 22,
		22, 22, 22, 22, 22, 22, 22, 22, 22, 22,
		22, 22, 23, 23, 23, 23, 23, 23, 23, 23,
		23, 23, 23, 23, 23, 23, 23, 23, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		24, 24, 24, 24, 24, 24, 24, 24, 24, 24,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		25, 25, 25, 25, 25, 25, 25, 25, 25, 25,
		25, 25, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 26, 26, 26, 26, 26, 26,
		26, 26, 26, 26, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 27, 27, 27, 27, 27,
		27, 27, 27, 27, 27, 28
	};

	internal static readonly int[] LengthBase = new int[29]
	{
		0, 1, 2, 3, 4, 5, 6, 7, 8, 10,
		12, 14, 16, 20, 24, 28, 32, 40, 48, 56,
		64, 80, 96, 112, 128, 160, 192, 224, 0
	};

	internal static readonly int[] DistanceBase = new int[30]
	{
		0, 1, 2, 3, 4, 6, 8, 12, 16, 24,
		32, 48, 64, 96, 128, 192, 256, 384, 512, 768,
		1024, 1536, 2048, 3072, 4096, 6144, 8192, 12288, 16384, 24576
	};

	internal short[] dyn_tree;

	internal int max_code;

	internal StaticTree staticTree;

	/// <summary>
	/// Map from a distance to a distance code.
	/// </summary>
	/// <remarks> 
	/// No side effects. _dist_code[256] and _dist_code[257] are never used.
	/// </remarks>
	internal static int DistanceCode(int dist)
	{
		if (dist >= 256)
		{
			return _dist_code[256 + SharedUtils.URShift(dist, 7)];
		}
		return _dist_code[dist];
	}

	internal void gen_bitlen(DeflateManager s)
	{
		short[] tree = dyn_tree;
		short[] stree = staticTree.treeCodes;
		int[] extra = staticTree.extraBits;
		int base_Renamed = staticTree.extraBase;
		int max_length = staticTree.maxLength;
		int overflow = 0;
		for (int bits = 0; bits <= InternalConstants.MAX_BITS; bits++)
		{
			s.bl_count[bits] = 0;
		}
		tree[s.heap[s.heap_max] * 2 + 1] = 0;
		int h;
		for (h = s.heap_max + 1; h < HEAP_SIZE; h++)
		{
			int n = s.heap[h];
			int bits = tree[tree[n * 2 + 1] * 2 + 1] + 1;
			if (bits > max_length)
			{
				bits = max_length;
				overflow++;
			}
			tree[n * 2 + 1] = (short)bits;
			if (n <= max_code)
			{
				s.bl_count[bits]++;
				int xbits = 0;
				if (n >= base_Renamed)
				{
					xbits = extra[n - base_Renamed];
				}
				short f = tree[n * 2];
				s.opt_len += f * (bits + xbits);
				if (stree != null)
				{
					s.static_len += f * (stree[n * 2 + 1] + xbits);
				}
			}
		}
		if (overflow == 0)
		{
			return;
		}
		do
		{
			int bits = max_length - 1;
			while (s.bl_count[bits] == 0)
			{
				bits--;
			}
			s.bl_count[bits]--;
			s.bl_count[bits + 1] = (short)(s.bl_count[bits + 1] + 2);
			s.bl_count[max_length]--;
			overflow -= 2;
		}
		while (overflow > 0);
		for (int bits = max_length; bits != 0; bits--)
		{
			int n = s.bl_count[bits];
			while (n != 0)
			{
				int m = s.heap[--h];
				if (m <= max_code)
				{
					if (tree[m * 2 + 1] != bits)
					{
						s.opt_len = (int)(s.opt_len + ((long)bits - (long)tree[m * 2 + 1]) * tree[m * 2]);
						tree[m * 2 + 1] = (short)bits;
					}
					n--;
				}
			}
		}
	}

	internal void build_tree(DeflateManager s)
	{
		short[] tree = dyn_tree;
		short[] stree = staticTree.treeCodes;
		int elems = staticTree.elems;
		int max_code = -1;
		s.heap_len = 0;
		s.heap_max = HEAP_SIZE;
		for (int n = 0; n < elems; n++)
		{
			if (tree[n * 2] != 0)
			{
				max_code = (s.heap[++s.heap_len] = n);
				s.depth[n] = 0;
			}
			else
			{
				tree[n * 2 + 1] = 0;
			}
		}
		int node;
		while (s.heap_len < 2)
		{
			node = (s.heap[++s.heap_len] = ((max_code < 2) ? (++max_code) : 0));
			tree[node * 2] = 1;
			s.depth[node] = 0;
			s.opt_len--;
			if (stree != null)
			{
				s.static_len -= stree[node * 2 + 1];
			}
		}
		this.max_code = max_code;
		for (int n = s.heap_len / 2; n >= 1; n--)
		{
			s.pqdownheap(tree, n);
		}
		node = elems;
		do
		{
			int n = s.heap[1];
			s.heap[1] = s.heap[s.heap_len--];
			s.pqdownheap(tree, 1);
			int m = s.heap[1];
			s.heap[--s.heap_max] = n;
			s.heap[--s.heap_max] = m;
			tree[node * 2] = (short)(tree[n * 2] + tree[m * 2]);
			s.depth[node] = (sbyte)(Math.Max((byte)s.depth[n], (byte)s.depth[m]) + 1);
			tree[n * 2 + 1] = (tree[m * 2 + 1] = (short)node);
			s.heap[1] = node++;
			s.pqdownheap(tree, 1);
		}
		while (s.heap_len >= 2);
		s.heap[--s.heap_max] = s.heap[1];
		gen_bitlen(s);
		gen_codes(tree, max_code, s.bl_count);
	}

	internal static void gen_codes(short[] tree, int max_code, short[] bl_count)
	{
		short[] next_code = new short[InternalConstants.MAX_BITS + 1];
		short code = 0;
		for (int bits = 1; bits <= InternalConstants.MAX_BITS; bits++)
		{
			code = (next_code[bits] = (short)(code + bl_count[bits - 1] << 1));
		}
		for (int n = 0; n <= max_code; n++)
		{
			int len = tree[n * 2 + 1];
			if (len != 0)
			{
				tree[n * 2] = (short)bi_reverse(next_code[len]++, len);
			}
		}
	}

	internal static int bi_reverse(int code, int len)
	{
		int res = 0;
		do
		{
			res |= code & 1;
			code >>= 1;
			res <<= 1;
		}
		while (--len > 0);
		return res >> 1;
	}
}
